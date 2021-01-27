namespace MatchSystem.Controllers
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Net.Http.Headers;
    using Models;
    using Newtonsoft.Json;

    public class UsersController : ApiController
    {
        /// <summary>
        /// just return whether the user is 审核通过, if login failed, also return true;
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool TryLogInMeuee(LoginObj obj)
        {
            try
            {
                HttpWebRequest req = WebRequest.CreateHttp("http://www.meuee.org/member/app/common/member");
                req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                req.Method = "POST";
                using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
                {
                    writer.Write($"loginName={obj.Email}&passWd={obj.Password}");
                }

                var response = req.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    var resStr = reader.ReadToEnd();
                    var returnValue = JsonConvert.DeserializeObject<MeueeMajorContactResponseType>(resStr);
                    if (!returnValue.result)
                    {
                        return true;
                    }

                    if (returnValue.data.status != "审核通过")
                    {
                        // could not allow none "审核通过" user login.
                        return false;
                    }

                    var ctx = Utilities.GetDBContext();
                    var targetUser = ctx.user.FirstOrDefault(u => u.Mail == obj.Email);
                    if (targetUser != default(user))
                    {
                        targetUser.Name = returnValue.data.majorResponsible;
                        targetUser.CellPhone = returnValue.data.majorMobile;
                        targetUser.UpdateTime = DateTime.UtcNow;
                        targetUser.Type = targetUser.Type | 4;
                    }
                    else
                    {
                        targetUser = new user
                        {
                            Name = returnValue.data.majorResponsible,
                            Mail = obj.Email,
                            Type = 4,
                            CellPhone = returnValue.data.majorMobile,
                            Disabled = "N",
                            CreateTime = DateTime.UtcNow,
                            UpdateTime = DateTime.UtcNow
                        };
                        ctx.user.Add(targetUser);
                    }
                    ctx.SaveChanges();

                    var targetContact = targetUser.majorcontact.FirstOrDefault();
                    if (targetContact != default(majorcontact))
                    {
                        targetContact.Job = returnValue.data.majorName;
                    }
                    else
                    {
                        targetContact = new majorcontact();
                        targetContact.Job = returnValue.data.majorName;
                        targetContact.UserId = targetUser.Id;
                        ctx.majorcontact.Add(targetContact);
                    }
                    ctx.SaveChanges();

                    var targetSecurity = ctx.user_security.FirstOrDefault(uS => uS.UserId == targetUser.Id);
                    if (targetSecurity != default(user_security))
                    {
                        targetSecurity.Password = Utilities.GetMd5Hash(obj.Password);
                    }
                    else
                    {
                        targetSecurity = new user_security();
                        targetSecurity.UserId = targetUser.Id;
                        targetSecurity.Password = Utilities.GetMd5Hash(obj.Password);
                        ctx.user_security.Add(targetSecurity);
                    }
                    ctx.SaveChanges();
                }

                return true;
            }
            catch(Exception e)
            {
                string msg = e.Message;
                return true;
            }
        }

        /// <summary>
        /// User log in, name or email is ok
        /// <para>{</para>
        /// <para>  Phone:"13**********"</para>
        /// <para>  Email:"****@xxx.com"</para>
        /// <para>  Password:"*********"</para>
        /// <para>}</para>
        /// </summary>
        /// <param name="obj">{Phone:"",Email:"",Password:""}</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Login([FromBody] LoginObj obj)
        {
            if (obj == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (!TryLogInMeuee(obj))
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }

            // check usr exist or enabled
            var ctx = Utilities.GetDBContext();
            var psw = Utilities.GetMd5Hash(obj.Password);
            var res = ctx.user.Where((usr) =>
                (usr.CellPhone == obj.Phone || usr.Mail == obj.Email)
            ).ToArray();

            if (res.Length < 1)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound) { Source = "Not registered user" };
            }

            var tempU = res.First();

            // check security
            var security = ctx.user_security.Where(se => se.UserId == tempU.Id).ToArray();
            if (security.Length < 1)
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable) { Source = "Invalid user" };
            }

            if (security.First().Password != psw)
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable) { Source = "Password not Match" };
            }

            // create token for usr
            var token = ctx.token.Create();
            token.Token1 = Utilities.CreateNewTokenString(tempU.Name);
            token.UserId = tempU.Id;
            ctx.token.Add(token);
            ctx.SaveChanges();

            // set usr to cache and response
            var user = CopyAndSetUserIntoCache(tempU, token.Token1);
            var resp = new HttpResponseMessage();
            var cookie = new CookieHeaderValue(ConfigurationManager.AppSettings["token"], token.Token1);
            cookie.Path = "/";
            cookie.Expires = new DateTimeOffset(DateTime.Now.AddYears(1));
            resp.Headers.AddCookies(new[] { cookie });
            resp.Content = new StringContent(
                JsonConvert.SerializeObject(user),
                System.Text.Encoding.UTF8);
            return resp;
        }

        /// <summary>
        /// Get current user by cookie token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object GetCurrentUserInfo()
        {
            return Utilities.GetCurrentUser();
        }

        /// <summary>
        /// User log out and delete current cache, token
        /// </summary>
        [HttpGet]
        public void Logout()
        {
            // get client cookie
            var cookie = Request.Headers.GetCookies(ConfigurationManager.AppSettings["token"]).FirstOrDefault();
            if (null == cookie)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            // remove cache
            var tkValue = cookie[ConfigurationManager.AppSettings["token"]].Value;
            RemoveToken(tkValue);
        }

        [HttpPost]
        public void UpdatePassword([FromBody] PswParam psw, [FromUri]int userId)
        {
            var ctx = Utilities.GetDBContext();
            var usr = ctx.user.Find(userId);
            if (null == usr)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (string.IsNullOrEmpty(psw.NewPassword) || string.IsNullOrWhiteSpace(psw.NewPassword))
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }

            var admin = new Authentication.Admin();
            if (admin.IsAuthenticated())
            {
                InnerUpdatePassword(usr.Id, psw);
            }
            else if (null != Utilities.GetCurrentUser())
            {
                var currUser = Utilities.GetCurrentUser();
                if (currUser.Id != usr.Id)
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
                }
                InnerUpdatePassword(usr.Id, psw);
            }
        }

        [HttpGet]
        public void ResetPassword(int userId)
        {
            var admin = new Authentication.Admin();
            if (!admin.IsAuthenticated())
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var ctx = Utilities.GetDBContext();
            var usr = ctx.user.Find(userId);
            if (null == usr)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var uS = ctx.user_security.FirstOrDefault(u => u.UserId == userId);
            var psw = Utilities.GetMd5Hash(usr.CellPhone.Substring(usr.CellPhone.Length - 6, 6));
            if (uS == default(user_security))
            {
                uS = new user_security()
                {
                    UserId = userId,
                    Password = psw
                };
                ctx.user_security.Add(uS);
            }
            else
            {
                uS.Password = psw;
            }
            ctx.SaveChanges();
        }

        private void InnerUpdatePassword(int uid, PswParam psw)
        {
            var ctx = Utilities.GetDBContext();
            var sc = ctx.user_security.Where(s => s.UserId == uid).FirstOrDefault();
            if (sc == null)
            {
                var newOne = ctx.user_security.Create();
                newOne.Password = Utilities.GetMd5Hash(psw.NewPassword);
                newOne.UserId = uid;
            }
            else
            {
                var oldPsw = Utilities.GetMd5Hash(psw.OldPassword);
                if (oldPsw != sc.Password)
                {
                    throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                }

                sc.Password = Utilities.GetMd5Hash(psw.NewPassword);
            }
            ctx.SaveChanges();
        }

        public static void RemoveToken(string tkValue)
        {
            Utilities.RemoveUserFromCache(tkValue);

            // remove token in db
            var ctx = Utilities.GetDBContext();
            var tkn = ctx.token.FirstOrDefault(t => t.Token1 == tkValue);
            if (null != tkn)
            {
                ctx.token.Remove(tkn);
                ctx.SaveChanges();
            }
        }

        private user CopyAndSetUserIntoCache(user usrFromEf, string token)
        {
            var user = Utilities.Copy(usrFromEf, new string[] { });

            Utilities.SetUserIntoCache(token, user);

            return user;
        }

        public class LoginObj
        {
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class PswParam
        {
            public string OldPassword { get; set; }

            public string NewPassword { get; set; }
        }

        public class MeueeMajorContactResponseType
        {
            public bool result { get; set; }
            public MeueeMajorContactResponseData data { get; set; }
        }

        public class MeueeMajorContactResponseData
        {
            public string fee { get; set; }
            public string status { get; set; }
            public string majorResponsible { get; set; }
            public string majorMobile { get; set; }
            public string majorEmail { get; set; }
            public string majorNumber { get; set; }
            public string youxiaoqi { get; set; }
            public string majorName { get; set; }
            public string majorCode { get; set; }
            public string miaoshu { get; set; }
        }
    }
}
