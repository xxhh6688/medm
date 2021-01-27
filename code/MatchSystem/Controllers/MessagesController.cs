using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace MatchSystem.Controllers
{
    public class MessagesController : ApiController
    {
        [HttpGet]
        public Models.message[] GetMessages(string receivedMessageIds)
        {
            var sign = new Authentication.SignIn();
            if (!sign.IsAuthenticated())
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var usr = Utilities.GetCurrentUser();
            var ids = ParseIntList(receivedMessageIds);

            var uid = usr.Id;
            var ctx = Utilities.GetDBContext();
            var currentUserMessage = ctx.message_user_ref
                .Where(mu => (mu.UserId == uid) && (!mu.IsRead)).ToList();

            var result = new List<Models.message>();
            foreach (var item in currentUserMessage)
            {
                if (!ids.Contains(item.MessageId))
                {
                    var msg = ctx.message.Find(item.MessageId);
                    if (msg != null)
                    {
                        result.Add(msg);
                    }
                }
            }

            if (result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    result[i] = Utilities.Copy(result[i], new string[] { });
                }

                return result.ToArray();
            }
            else
            {
                var wtn = new Message.WaitAbleContent(uid, HttpContext.Current);
                var tkn = Utilities.GetTokenFormCookie();
                var key = Message.MessageCache.MakeKey(uid, tkn);
                Message.MessageCache.GetCurrent().RegistListen(key, wtn);

                // wait until new message comes.
                wtn.Wait();
                if (null != wtn.ReceivedMessage)
                    result.Add(wtn.ReceivedMessage);

                for (int i = 0; i < result.Count; i++)
                {
                    result[i] = Utilities.Copy(result[i], new string[] { "message_user" });
                }

                return result.ToArray();
            }
        }



        [HttpPost]
        public async void SendMessage([FromBody]MessageParam param)
        {
            var admin = new Authentication.UserTypeOf(8 & 16);
            if (!admin.IsAuthenticated())
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var ctx = Utilities.GetDBContext();
            var msg = ctx.message.Create();
            msg.Title = param.Title;
            msg.Description = param.Content;
            msg.CreateTime = DateTime.Now;

            ctx.message.Add(msg);
            await ctx.SaveChangesAsync();

            var ids = ParseIntList(param.UserIds);
            var mus = new List<Models.message_user_ref>();
            foreach (var id in ids)
            {
                if (null != ctx.user.Find(id))
                {
                    var mu = ctx.message_user_ref.Create();
                    mu.IsRead = false;
                    mu.UserId = id;
                    mu.MessageId = msg.Id;

                    ctx.message_user_ref.Add(mu);
                    mus.Add(mu);
                }
            }

            await ctx.SaveChangesAsync();
            Message.MessageCache.GetCurrent().SetNewMessage(msg, mus.ToArray());
        }

        /// <summary>
        /// Not used currently
        /// </summary>
        /// <param name="MessageIds"></param>
        /// <param name="allAsRead"></param>
        private async void MarkMessageAsRead(string MessageIds, bool allAsRead)
        {
            var sign = new Authentication.SignIn();
            if (!sign.IsAuthenticated())
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var user = Utilities.GetCurrentUser();

            var uid = user.Id;
            var ids = ParseIntList(MessageIds);
            var ctx = Utilities.GetDBContext();
            var mus = ctx.message_user_ref
                .Where(mu => (!mu.IsRead) && (mu.UserId == uid)).ToArray();
            for (int i = 0; i < mus.Length; i++)
            {
                if (allAsRead || ids.Contains(mus[i].MessageId))
                {
                    mus[i].IsRead = true;
                }
            }

            await ctx.SaveChangesAsync();
        }

        public class MessageParam
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string UserIds { get; set; }
        }

        private List<int> ParseIntList(string ints)
        {
            var ids = new List<int>();
            if (!string.IsNullOrEmpty(ints))
            {
                var idStrs = ints.Split(',');
                foreach (var idStr in idStrs)
                {
                    int id;
                    if (int.TryParse(idStr, out id))
                    {
                        ids.Add(id);
                    }
                }
            }

            return ids;
        }

        private List<int> GetAllUserIds(Models.Ctx ctx)
        {
            var res = new List<int>();
            foreach (var usr in ctx.user)
            {
                res.Add(usr.Id);
            }

            return res;
        }
    }
}