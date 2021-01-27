using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace MatchSystem.Controllers
{
    using System.Collections.Generic;
    using Models;
    using System.Net.Http;
    using Newtonsoft.Json;
    using System.Data.Entity;

    public class MatchesController : ApiController
    {
        public enum ExpertOrManager
        {
            Expert = 1,
            Manager = 2
        }

        [HttpGet]
        public void AutoAsignReviewer(int matchId, int reviewStatus, int maxReviewCountEachPaper, ExpertOrManager expertOrManager)
        {
            var needToSendMessageUserIds = new List<int>();
            var ctx = Utilities.GetDBContext();
            var mth = ctx.match.Find(matchId);
            if (null == mth)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Func<int, bool> judgeMent;
            if (expertOrManager == ExpertOrManager.Expert)
            {
                judgeMent = IsExpert;
            }
            else
            {
                judgeMent = IsManager;
            }

            // in same sataus, taht means paper.currentstatus == paper_review_expert.StatusType
            //
            var papers = mth.paper.Where(p => p.CurrentStatus == reviewStatus);

            // get all sepcified user(expert|manager)
            //
            var allSpecifiedUser = ctx.user.Where(u => judgeMent(u.Type));

            foreach (var pap in papers)
            {
                // check wether alread assigned enough.
                //
                var cnt = maxReviewCountEachPaper - pap.paper_review_expert.Count(x => x.StatusType == reviewStatus);
                if (cnt < 1)
                {
                    continue;
                }

                // remove assigned users, but get assigned users first, then filter
                // 
                var assignedUserIds = ctx.paper_review_expert
                    .Where(pre => pre.StatusType == reviewStatus && pre.PaperId == pap.Id && pre.ExpertId != null)
                    .Select(pre => pre.ExpertId);

                var papSchoolName = pap.school == null ? pap.school.Name : "_";
                var leftUsers = allSpecifiedUser.Where(
                    u =>
                    !assignedUserIds.Contains(u.Id)
                    && pap.CreateBy != u.Id
                    && (u.expert.Count == 0 || u.expert.First().Company != papSchoolName)   // 如果是专家，那么专家的 推荐单位 不能是论文的 学校
                ).ToList();

                leftUsers.Sort(LessInFront);

                // when it's expert, we should compare their (paper.major, expert.ReviewArea) major
                //
                if (expertOrManager == ExpertOrManager.Expert)
                {
                    leftUsers = leftUsers
                        .Where(u => u.expert.Any() && u.expert.First().ReviewArea.Split(',').Contains(pap.major.Name))
                        .Take(cnt).ToList();
                }

                foreach (var usr in leftUsers)
                {
                    var n = new paper_review_expert()
                    {
                        CreateTime = DateTime.UtcNow,
                        Updatetime = DateTime.UtcNow,
                        StatusType = reviewStatus,
                        ExpertId = usr.Id,
                        PaperId = pap.Id,
                        Comment = "",
                        Score = 0
                    };
                    ctx.paper_review_expert.Add(n);
                    needToSendMessageUserIds.Add(usr.Id);
                }

                ctx.SaveChanges();
            }

            //TODO: send message to needToSendMessageUserIds
        }

        [HttpGet]
        public void AutoAsignReviewerByPaper(string paperIds, int maxReviewCountEachPaper = 4)
        {
            var ctx = Utilities.GetDBContext();
            var paperIdArray = paperIds.Split(',').Select(s => int.Parse(s));
            var papers = ctx.paper
                .Include("paper_review_expert")
                .Where(p => paperIdArray.Contains(p.Id)).ToList();
            var needToSendMessageUsers = new Dictionary<int, user>();
            foreach (var paper in papers)
            {
                var cnt = maxReviewCountEachPaper - paper.paper_review_expert.Count(x => x.StatusType == paper.CurrentStatus);
                if (cnt < 1)
                {
                    continue;
                }

                var userList = ctx.user.Where(x => (x.Type & 8) == 8).ToList();

                List<UserAssignment> uaList = new List<UserAssignment>();
                if (paper.CurrentStatus > 1)
                {
                    var papSchoolName = paper.school != null ? paper.school.Name : "_";
                    userList = ctx.user.Include("expert").Where(
                        x =>
                        ((x.Type & 1) == 1 || (x.Type & 2) == 2)
                        && x.Id != paper.CreateBy
                    ).ToList();

                    //  需要用户是专家，并且专家的 推荐单位 不能是论文的 学校
                    //
                    var tempList = new List<user>();
                    foreach (var usr in userList)
                    {
                        if (usr.expert.Count != 0 && usr.expert.First().SchoolName != papSchoolName)
                        {
                            tempList.Add(usr);
                        }
                    }
                    userList = tempList.ToList();
                }

                // 获取当前所有的专家 （由于之前的paper有新分配的专家，所以此处要刷新重新获取）
                var paper_review_expert = ctx.paper_review_expert.ToArray();
                foreach (var u in userList)
                {
                    // 获取 专家 已经分配的工作量。
                    var count = paper_review_expert.Where(x => (x.paper.MatchId == paper.MatchId && x.ExpertId == u.Id && x.StatusType == paper.CurrentStatus)).Count();
                    if (paper.CurrentStatus != 1)
                    {
                        // 根据专业筛选专家
                        if (u.expert.Count > 0 && paper.major.Name.IndexOf(u.expert.First().ReviewArea) != -1)
                        {
                            uaList.Add(new UserAssignment(u.Id, count));
                        }
                    }
                    else
                    {
                        uaList.Add(new UserAssignment(u.Id, count));
                    }
                }

                uaList.Sort(LessAssignmentFront);
                for (int i = 0; i < cnt && i < uaList.Count; i++)
                {
                    var n = new paper_review_expert()
                    {
                        CreateTime = DateTime.UtcNow,
                        Updatetime = DateTime.UtcNow,
                        StatusType = paper.CurrentStatus,
                        ExpertId = (uaList.ToArray())[i].userId,
                        PaperId = paper.Id,
                        Comment = string.Empty,
                        Score = 0
                    };

                    if (paper_review_expert.Where(x => (x.ExpertId == n.ExpertId && x.PaperId == n.PaperId && x.StatusType == n.StatusType)).Count() > 0)
                    {
                        continue;
                    }

                    ctx.paper_review_expert.Add(n);
                    user usr = null;
                    foreach (var u in userList)
                    {
                        if (u.Id == n.ExpertId)
                        {
                            usr = u;
                        }
                    }
                    if (!needToSendMessageUsers.ContainsKey((int)n.ExpertId))
                    {
                        needToSendMessageUsers.Add((int)n.ExpertId, usr);
                    }
                    cnt--;  // 计算还有几个指标没有分配
                }
                ctx.SaveChanges();

                if (cnt > 0)
                {
                    userList.Sort(LessInFront);
                    foreach (var lef in userList)
                    {
                        var n = new paper_review_expert()
                        {
                            CreateTime = DateTime.UtcNow,
                            Updatetime = DateTime.UtcNow,
                            StatusType = paper.CurrentStatus,
                            ExpertId = lef.Id,
                            PaperId = paper.Id,
                            Comment = string.Empty,
                            Score = 0
                        };

                        if (paper_review_expert.Where(x => (x.ExpertId == n.ExpertId && x.PaperId == n.PaperId && x.StatusType == n.StatusType)).Count() > 0)
                        {
                            continue;
                        }
                        ctx.paper_review_expert.Add(n);
                        user usr = null;
                        foreach (var u in userList)
                        {
                            if (u.Id == n.ExpertId)
                            {
                                usr = u;
                            }
                        }
                        if (!needToSendMessageUsers.ContainsKey((int)n.ExpertId))
                        {
                            needToSendMessageUsers.Add((int)n.ExpertId, usr);
                        }
                        cnt--;
                        if (cnt == 0)
                        {
                            break;
                        }
                    }
                }

                ctx.SaveChanges();
            }

            ctx.SaveChanges();

            //start send sms
            var userIds = "";
            foreach (var i in needToSendMessageUsers)
            {
                userIds = userIds + "," + i.Key;
            }

            if (userIds.Length > 0)
            {
                userIds = userIds.Substring(1, userIds.Length - 1);
            }

            ShortMessageController sc = new ShortMessageController();
            sc.SendReviewMessageToUser(userIds);

            // send mail
            EmailsController ec = new EmailsController();
            ec.SendExpertReviewRequestMail(userIds);
        }

        class paperInfo
        {
            public int 序号 { get; set; }
            public string 论文标题 { get; set; }
            public string 学校 { get; set; }
            public string 专业 { get; set; }
            public string 联系人姓名 { get; set; }
            public string 联系人电话 { get; set; }
            public string 学生姓名 { get; set; }
            public string 学生电话 { get; set; }
            public string 作品题目 { get; set; }
            public string 作品类型 { get; set; }
            public string 学生性别 { get; set; }
            public string 学生邮箱 { get; set; }
            public string 高校指导老师姓名 { get; set; }
            public string 高校指导老师邮箱 { get; set; }
            public string 高校指导老师手机 { get; set; }
            public string 企业指导老师姓名 { get; set; }
            public string 企业指导老师单位 { get; set; }
            public string 企业指导老师邮箱 { get; set; }
            public string 企业指导老师手机 { get; set; }
        }

        class TeacherInfo
        {
            public string Name { get; set; }
            public string Company { get; set; }
            public string Title { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
        }

        [HttpGet]
        public HttpResponseMessage CreateExpertPaperScoreExcelReport(int matchId, int type)
        {
            var ctx = Utilities.GetDBContext();
            var papers = ctx.paper.Include("paper_review_expert").Include("user").Include("paper_review_expert").Where(x => x.MatchId == matchId).ToList();

            //
            // 此处有两种解决方案，一种是先生成一个默认的二维数组，根据对应的index（x，y）填入对应paper——user的分数
            // 第二种是直接填写datatable
            //

            // get all user
            var users = new List<user>();
            foreach (var paper in papers)
            {
                foreach (var pre in paper.paper_review_expert)
                {
                    if (pre.StatusType != type) continue;
                    if (!users.Contains(pre.user))
                    {
                        users.Add(pre.user);
                    }
                }
            }

            var original = new DataTable("原始数据");

            // columns
            original.Columns.Add("论文信息");
            int step = 1;
            foreach (var usr in users)
            {
                if (usr == null)
                {
                    original.Columns.Add("已删除用户");
                    continue;
                }
                original.Columns.Add(string.Format("{0}_{1}",usr.Name,step++));
            }

            // rows
            foreach (var paper in papers)
            {
                var ts = JsonConvert.DeserializeObject<TeacherInfo[]>(paper.Teachers);
                var row = original.NewRow();
                var paperInfo = new paperInfo()
                {
                    序号 = paper.Id,
                    专业 = paper.major.Name,
                    学校 = paper.school.Name,
                    学生姓名 = paper.StudentName,
                    学生电话 = paper.StudentCellPhone,
                    论文标题 = paper.Title,
                    联系人姓名 = paper.user.Name,
                    联系人电话 = paper.user.CellPhone,
                    企业指导老师单位 = ts?[1]?.Company,
                    企业指导老师姓名 = ts?[1]?.Name,
                    企业指导老师手机 = ts?[1]?.Mobile,
                    企业指导老师邮箱 = ts?[1]?.Email,
                    作品类型 = paper.Type == 1? "论文": "设计",
                    作品题目 = paper.question.Name,
                    学生性别 = paper.Gender == "M"? "男" : "女",
                    学生邮箱 = paper.StudentMail,
                    高校指导老师姓名 = ts?[0]?.Name,
                    高校指导老师手机 = ts?[0]?.Mobile,
                    高校指导老师邮箱 = ts?[0]?.Email
                };
                row[0] = JsonConvert.SerializeObject(paperInfo);
                foreach (var pre in paper.paper_review_expert)
                {
                    if (pre.StatusType != type) continue;
                    var idx = users.IndexOf(pre.user) + 1;
                    row[idx] = (double)pre.Score;
                }

                original.Rows.Add(row);
            }

            // 增加新的列（平均值）
            original.Columns.Add("平均值");
            for (int i = 0; i < original.Rows.Count; i++)
            {
                var count = 0;
                double sum = 0;
                for (int j = 1; j < original.Columns.Count - 1; j++)
                {
                    var val = original.Rows[i][j].ToString();
                    if (string.IsNullOrEmpty(val) || val == "0") continue;
                    sum += double.Parse(val);
                    count++;
                }
                if (count == 0) continue;
                original.Rows[i][original.Columns.Count - 1] = sum / count;
            }

            // 均一化
            var computed = original.Copy();
            computed.TableName = "均一化数据";
            for (int i = 1; i < computed.Columns.Count; i++)
            {
                // 计算平均值。
                var average = 0.0;
                var count = 0;
                for (int j = 0; j < computed.Rows.Count; j++)
                {
                    if (string.IsNullOrEmpty(computed.Rows[j][i].ToString()) || computed.Rows[j][i].ToString() == "0") continue;
                    var value = double.Parse(computed.Rows[j][i].ToString());
                    if(value < 40) // 论文问题严重 不计入
                    {
                        continue;
                    }

                    average += value;
                    if (value != 0)
                    {
                        count++;
                    }
                }

                // 计算80分的均一值
                average = count == 0 ? 0 : 80 / (average / count);
                if (average == 0)
                {
                    continue;
                }

                // 为每一列的数据计算分数
                for (int j = 0; j < computed.Rows.Count; j++)
                {
                    if (string.IsNullOrEmpty(computed.Rows[j][i].ToString()) || computed.Rows[j][i].ToString() == "0") continue;
                    var value = double.Parse(computed.Rows[j][i].ToString());
                    if (value != 0)
                    {
                        computed.Rows[j][i] = value * average;
                    }
                }
            }

            // 计算均一表平均值
            for (int i = 0; i < computed.Rows.Count; i++)
            {
                var count = 0;
                double sum = 0;
                for (int j = 1; j < computed.Columns.Count - 1; j++)
                {
                    var val = computed.Rows[i][j].ToString();
                    if (string.IsNullOrEmpty(val) || val == "0") continue;
                    sum += double.Parse(val);
                    count++;
                }
                if (count == 0) continue;
                computed.Rows[i][computed.Columns.Count - 1] = sum / count;
            }

            // 记录数据库
            List<paper> paper_record = ctx.paper.ToList();
            foreach (DataRow row in computed.Rows)
            {
                var info = JsonConvert.DeserializeObject<paperInfo>(row[0].ToString());
                if (info != null)
                {
                    //var p = ctx.paper.Find(info.序号);
                    var p = paper_record.Where(x=>x.Id == info.序号).FirstOrDefault();
                    if (p != null)
                    {
                        var isEmpty = string.IsNullOrEmpty(row[computed.Columns.Count - 1].ToString());
                        double score;
                        if (isEmpty)
                        {
                            score = 0;
                        }
                        else
                        {
                            score = Math.Round(double.Parse(row[computed.Columns.Count - 1].ToString()), 2);
                        }

                        switch (type)
                        {
                            case 1:
                                p.Score1 = score;
                                break;
                            case 2:
                                p.Score2 = score;
                                break;
                            case 3:
                                p.Score3 = score;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            ctx.SaveChanges();

            var name = "论文分数汇总.xlsx";
            var stream = ExcelHelper.CreateExcel(name, new DataTable[] { original, computed });
            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = name;
            return result;
        }

        /// <summary>
        /// 为指导老师建立账号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string CreateInstructorUser([FromUri]int matchId)
        {
            var ctx = Utilities.GetDBContext();
            var papers = ctx.paper.Where(x => x.MatchId==matchId).ToList();
            var users = ctx.user.ToList();
            foreach(var p in papers)
            {
                string teacherString = p.Teachers;
                List<Teacher> teachers = JsonConvert.DeserializeObject<List<Teacher>>(teacherString);
                Teacher t = null;
                if(teachers.Count > 0)
                {
                    t = teachers[0];
                }

                if(t != null)
                {
                    user existUser = null;
                    foreach(var u in users)
                    {
                        if (u.CellPhone.Contains(t.Mobile.Trim()) || u.Mail.Equals(t.EMail.Trim())){
                            existUser = u;
                            break;
                        }
                    }

                    if(existUser == null)
                    {
                        user u = new user()
                        {
                            Name = t.Name.Replace(" ",""),
                            CellPhone = t.Mobile.Replace(" ", "").Trim(),
                            Mail = t.EMail.Trim().Equals("")?t.Mobile.Replace(" ", "").Trim(): t.EMail.Trim(),
                            CreateTime=DateTime.UtcNow,
                            Disabled="N"
                        };

                        users.Add(u);
                        ctx.user.Add(u);
                        ctx.SaveChanges();
                        user_security us = new user_security()
                        {
                            UserId = u.Id,
                            Password = Utilities.GetMd5Hash("1234")
                        };
                        ctx.user_security.Add(us);
                        ctx.SaveChanges();
                        System.Diagnostics.Debug.WriteLine(string.Format("new create user Id:{0} Name:{1} Cellphone:{2}",u.Id,u.Name,u.CellPhone));
                    }
                    else
                    {
                        existUser.Type = existUser.Type | 32;
                        ctx.SaveChanges();
                        System.Diagnostics.Debug.WriteLine(string.Format("update user Id:{0} Name:{1} Cellphone:{2}", existUser.Id, existUser.Name, existUser.CellPhone));
                    }
                }
            }

            return "done";
        }

        /// <summary>
        /// 为指导老师分配非本校论文
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        [HttpGet]
        public string AssignReviewPaperForInstructor([FromUri]int matchId)
        {
            var ctx = Utilities.GetDBContext();
            var papers = ctx.paper.Where(x => x.MatchId == matchId).ToList();
            var pres = ctx.paper_review_expert.Where(x => x.Id > 9540).ToList();
            var users = ctx.user.ToList();
            List<user> instructors = new List<user>();
            Dictionary<int, int> userAssignPaperCount = new Dictionary<int, int>();
            List<string> userAssignPaper = new List<string>();
            
            foreach (var u in users)
            {
                if((u.Type & 32) == 32 && papers.Where(x=>x.Teachers.Contains(u.CellPhone)).Count() > 0)
                {
                    instructors.Add(u);
                }
            }

            int time = 3;

            foreach (var p in papers)
            {
                int assignCount = 3;
                int existCount = pres.Where(x => x.PaperId == p.Id).Count();
                if (existCount >= 3)
                {
                    continue;
                }
                else
                {
                    assignCount = assignCount - existCount;
                }

                
                while(assignCount > 0)
                {
                    assignCount--;
                    user assignUser = null;
                    int step = 10;
                    do
                    {
                        step--;

                        foreach (var i in instructors)
                        {
                            int? userSchoolId = 0;
                            var p1 = papers.Where(x => x.Teachers.Contains(i.CellPhone)).FirstOrDefault();
                            if (p1 != null)
                            {
                                userSchoolId = p1.SchoolId;
                            }

                            int? paperShcoolId = 0;
                            paperShcoolId = p.SchoolId==null?0:p.SchoolId;

                            if(paperShcoolId == userSchoolId && userSchoolId != 0)
                            {
                                continue;
                            }

                            int assignUserCount = pres.Where(x => x.ExpertId == i.Id).Count();
                            if (assignUserCount > 5)
                            {
                                continue;
                            }

                            int count = 0;
                            if (userAssignPaperCount.TryGetValue(i.Id, out count))
                            {
                                if (count + assignUserCount < 6 && userAssignPaper.Contains(i.Id + ":" + p.Id) == false)
                                {
                                    userAssignPaperCount[i.Id] = count + 1;
                                    assignUser = i;
                                    userAssignPaper.Add(assignUser.Id + ":" + p.Id);
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                userAssignPaperCount.Add(i.Id, 1);
                                assignUser = i;
                                userAssignPaper.Add(assignUser.Id + ":" + p.Id);
                                break;
                            }
                        }

                        if (assignUser==null)
                        {
                            time++;
                        }
                    }
                    while (assignUser == null && step > 0);

                    if (assignUser != null)
                    {
                        paper_review_expert pre = new paper_review_expert()
                        {
                            ExpertId = assignUser.Id,
                            PaperId = p.Id,
                            CreateTime = DateTime.UtcNow,
                            StatusType = 2,
                            Updatetime = DateTime.UtcNow
                        };
                        ctx.paper_review_expert.Add(pre);
                        
                    }
                }
            }

            ctx.SaveChanges();

            return "done";
        }

        /// <summary>
        /// 用于删除被分配给本校错误记录
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        [HttpGet]
        public string AdjustAssignment([FromUri] int matchId)
        {
            var ctx = Utilities.GetDBContext();
            List<paper_review_expert> pres = ctx.paper_review_expert.Where(x => x.Id > 9540).ToList();
            List<paper> papers = ctx.paper.Where(x => x.MatchId == 10).ToList();
            List<user> users = ctx.user.ToList();
            foreach(var i in pres)
            {
                string cellphone = users.Where(x => x.Id == i.ExpertId).FirstOrDefault().CellPhone;
                paper p = papers.Where(x => x.Teachers.Contains(cellphone)).FirstOrDefault();
                int? schoolId = 0;
                if (p != null)
                {
                    schoolId = p.SchoolId;
                }

                p = papers.Where(x => x.Id == i.PaperId).FirstOrDefault();
                int? school2Id = 0;
                if(p != null)
                {
                    school2Id = p.SchoolId;
                }

                if(school2Id == schoolId && schoolId != 0)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("wrong id: {0}", i.Id));

                    //ctx.Entry(i).State = EntityState.Deleted;
                    //ctx.SaveChanges();
                }
            }

            return "";
        }

        /// <summary>
        /// 临时分配9个老师
        /// </summary>
        [HttpGet]
        public string AssignPaperToTeach()
        {
            int count = 1;
            var ctx = Utilities.GetDBContext();
            List<paper_review_expert> pres = ctx.paper_review_expert.Where(x => x.Id > 9540).ToList();
            List<paper> papers = ctx.paper.Where(x => x.MatchId == 10).ToList();
            List<user> users = ctx.user.ToList();
            List<UserQuota> userQuotas = new List<UserQuota>();
            userQuotas.Add(new UserQuota(131, 12));
            userQuotas.Add(new UserQuota(492, 12));
            userQuotas.Add(new UserQuota(620, 12));
            userQuotas.Add(new UserQuota(631, 12));
            userQuotas.Add(new UserQuota(711, 6));
            userQuotas.Add(new UserQuota(783, 12));
            userQuotas.Add(new UserQuota(789, 22));
            userQuotas.Add(new UserQuota(800, 6));
            userQuotas.Add(new UserQuota(811, 12));

            List<user> validUsers = new List<user>();
            foreach(var q in userQuotas)
            {
                validUsers.Add(users.Where(x => x.Id == q.UserId).FirstOrDefault());
            }


            foreach(var p in papers)
            {
                int newCount = 3 - pres.Where(x=>x.PaperId==p.Id).Count();

                foreach (var vu in validUsers)
                {
                    if(newCount == 0)
                    {
                        break;
                    }

                    if (userQuotas.Where(x => x.UserId == vu.Id).FirstOrDefault().LeftCount == 0)
                    {
                        continue;
                    }

                    if (p.Teachers.Contains(vu.CellPhone) || p.Teachers.Contains(vu.Mail))
                    {
                        continue;
                    }

                    paper_review_expert pre = new paper_review_expert()
                    {
                        PaperId = p.Id,
                        ExpertId = vu.Id,
                        CreateTime = DateTime.UtcNow,
                        Score = 0,
                        Updatetime = DateTime.UtcNow
                    };

                    ctx.paper_review_expert.Add(pre);
                    userQuotas.Where(x => x.UserId == vu.Id).FirstOrDefault().LeftCount--;
                    newCount--;
                    System.Diagnostics.Debug.WriteLine(string.Format("count:{2} paper:{0} user:{1}", p.Id, vu.Id, count++));
                }
            }

            ctx.SaveChanges();

            return "";
        }

        class UserQuota
        {
            public int UserId { get; set; }
            public int LeftCount { get; set; }

            public UserQuota(int userId, int q)
            {
                this.UserId = userId;
                this.LeftCount = q;
            }
        }

        private user FindUserByPhone(List<user> users, string cellphone)
        {
            foreach(var usr in users)
            {
                if (usr.CellPhone.Equals(cellphone))
                {
                    return usr;
                }
            }

            return null;
        }

        private bool IsExpert(int userType)
        {
            return (userType & 3) > 0;
        }

        private bool IsManager(int userType)
        {
            return (userType & 8) == 8;
        }

        private int LessInFront(user l, user r)
        {
            return l.paper_review_expert.Count - r.paper_review_expert.Count;
        }

        class UserAssignment
        {
            public int userId;
            public int taskCount;

            public UserAssignment(int userId, int taskCount)
            {
                this.userId = userId;
                this.taskCount = taskCount;
            }
        }

        private int LessAssignmentFront(UserAssignment a, UserAssignment b)
        {
            return a.taskCount - b.taskCount;
        }

        class Teacher
        {
            public string Name { get; set; }
            public string Company { get; set; }
            public string Title { get; set; }
            public string Mobile { get; set; }
            public string EMail { get; set; }
        }
    }
}
