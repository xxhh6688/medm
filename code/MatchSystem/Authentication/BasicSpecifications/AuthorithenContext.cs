using Microsoft.Restier.Core.Submit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;

namespace MatchSystem.Authentication
{
    public class AuthorithenContext
    {
        public string HttpMethodName { get; set; }

        public string EntityTypeName { get; set; }

        public HttpActionContext ActionContext { get; set; }

        public DataModificationItem ModifyItem { get; set; }

        public HttpRequestMessageProperties OdataProperties { get; set; }

        public object GetTargetFromOdataProperties()
        {
            if(OdataProperties == null)
            {
                return null;
            }

            var templateTags = OdataProperties.Path.PathTemplate.TrimStart('~').Split('/');

            return null;
        }
    }
}