using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Http.Filters;
using System.Web.Http;

namespace MatchSystem.Filters
{
    public class ExceptionFilters : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exp = actionExecutedContext.Exception;
            var logpath = "~/errorlog.txt";
            logpath = System.Web.Hosting.HostingEnvironment.MapPath(logpath);
            if (exp is HttpResponseException)
            {
                var temp = exp as HttpResponseException;
                File.AppendAllText(logpath, string.Format("{0}  {1}  {2}  {3}  {4}" + Environment.NewLine,
                    DateTime.Now,
                    temp.Response.StatusCode,
                    actionExecutedContext.Request.RequestUri,
                    exp.InnerException != null ? exp.InnerException.Message : exp.Message,
                    exp.InnerException != null ? exp.InnerException.StackTrace : exp.StackTrace));
            }
            else
            {
                File.AppendAllText(logpath, string.Format("{0}  {1}  {2}  {3}  {4}" + Environment.NewLine, 
                    DateTime.Now, 
                    "500", 
                    actionExecutedContext.Request.RequestUri,
                    exp.InnerException != null ? exp.InnerException.Message : exp.Message,
                    exp.InnerException != null ? exp.InnerException.StackTrace : exp.StackTrace));
            }

            base.OnException(actionExecutedContext);
        }
    }
}