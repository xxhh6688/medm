using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MatchSystem.Controllers
{
    using System.Net.Mail;
    using System.Configuration;

    public class EmailsController : ApiController
    {
        private void InnerSendMail(string addresses, string subject, string body)
        {
            using (var client = new SmtpClient())
            using (var mail = new MailMessage(ConfigurationManager.AppSettings["mailSender"], addresses))
            {
                mail.Subject = subject;
                mail.Body = body;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Port = 25;
                client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailSender"],
                    ConfigurationManager.AppSettings["mailSenderPsw"]);
                client.Host = ConfigurationManager.AppSettings["mailHost"];
                client.Send(mail);
            }
        }

        public void SendExpertReviewRequestMail(string userIds)
        {
            var res = new List<object>();
            var uids = userIds.Trim().Split(',');
            foreach (var uid in uids)
            {
                int userId;
                if (int.TryParse(uid, out userId))
                {
                    string subject = "毕业设计大赛评审通知";
                    string body = @"尊敬的评委：
    您好！
    首届中国机械行业卓越工程师教育联盟“恒星杯”毕业设计大赛截止5月10日，共收到了244项参赛作品，其中221项参赛作品通过了形式审查，函评工作现已启动。
    请您访问主页www.meuee.org，点击下方的“毕业设计大赛”模块，使用您的手机号作为账号，手机号后六位作为密码登录，点击“我的审阅”开始您的函评工作，具体的函评操作方法详见附件或下载中心中的“作品评审帮助文档 - 毕设大赛”，评审时如需打开二维、三维图纸，请使用下载中心的ABViewer 11软件。
    本届大赛函评时间截止到2017年5月13日17: 00，请您认真、负责、公正的审阅给您分配的每一篇论文。

首届中国机械行业卓越工程师教育联盟“恒星杯”毕业设计大赛执委会
主办单位：中国机械行业卓越工程师教育联盟
承办单位：大连理工大学
联系人：刘新    
联系电话：13840991139 0411-84708421
联系邮箱：bysjds@cmes.org 
联盟/大赛网站：www.meuee.org
";
                    var ctx = Utilities.GetDBContext();
                    var usr = ctx.user.Find(userId);
                    if(usr !=null && string.IsNullOrWhiteSpace(usr.Mail))
                    {
                        InnerSendMail(usr.Mail, subject, body);
                    }
                }
            }

        }

        [HttpGet]
        public void SendMail([FromBody] MailParam mail)
        {
            InnerSendMail(mail.Address, mail.Subject, mail.Body);
        }

        public class MailParam
        {
            public string Address { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
    }
}
