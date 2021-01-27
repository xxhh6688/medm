using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MatchSystem.Controllers
{
    using Qcloud.Sms;
    using Authentication;
    using System.Configuration;

    public class ShortMessageController : ApiController
    {

        int sdkappid = int.Parse(ConfigurationManager.AppSettings["shortMessageAppId"]);
        int tempReviewId = int.Parse(ConfigurationManager.AppSettings["shortMessageReviewTemplId"]);
        string appkey = ConfigurationManager.AppSettings["shortMessageAppKey"];

        //[HttpGet]
        //public object SendTestMessage()
        //{
        //    string phoneNumber1 = "18612934282";

        //    var param = new List<string>();
        //    param.Add("许亮");
        //    param.Add("专家");
        //    param.Add("机械工程学会");
        //    param.Add("2017年1月10日12点30分");
        //    param.Add("http://meea.cmes.org");

        //    SmsSingleSender singleSender = new SmsSingleSender(sdkappid, appkey);
        //    return singleSender.SendWithParam("86", phoneNumber1, tempMeetingId, param, null, null, null);
        //}

        [HttpGet]
        public object SendReviewMessageToUser(string userIds)
        {
            Admin admin = new Admin();
            if (!admin.IsAuthenticated(null))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var res = new List<object>();
            var uids = userIds.Trim().Split(',');
            foreach (var uid in uids)
            {
                int userId;
                if (int.TryParse(uid, out userId))
                {
                    res.Add(InnserSendMessageToUser(userId, tempReviewId));
                }
            }

            return res;
        }

        private object InnserSendMessageToUser(int userId, int tempId)
        {
            var ctx = Utilities.GetDBContext();
            var usr = ctx.user.Find(userId);

            if (null == usr)
            {
                return new
                {
                    errorCode = 404,
                    message = string.Format("Specified user (id={0}) not found.", userId)
                };
            }

            SmsSingleSender singleSender = new SmsSingleSender(sdkappid, appkey);
            return singleSender.SendWithParam("86", usr.CellPhone, tempId, new List<string>(), "毕业设计大赛", null, null);
        }
    }
}
