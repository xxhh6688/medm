using System;
using System.Web.Http;

namespace MatchSystem.Controllers
{
    public class CommonController : ApiController
    {
        [HttpGet]
        public DateTime GetCurrentServerTime()
        {
            return DateTime.Now;
        }
    }
}
