using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Restier.Core.Submit;
using System.Web.OData.Extensions;
using Microsoft.Restier.Core.Query;
using System.Net.Http;
using System.Web.OData.Query;
using System.Linq.Expressions;
using System.Web.Http.Filters;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.Net;

namespace MatchSystem.Authentication
{
    public class QueryAuthoritherFilter : IAuthorizationFilter
    {
        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var odataProps = actionContext.Request.ODataProperties();

            if(actionContext.Request.Method.Method.ToLower() == "get")
            {
                if (odataProps != null && odataProps.Path != null &&
                    odataProps.Path.NavigationSource != null)
                {
                    var verified = ValidationContainer.Instance.Verify(
                        new AuthorithenContext()
                        {
                            EntityTypeName = odataProps.Path.NavigationSource.Name,
                            HttpMethodName = "get",
                            ActionContext = actionContext,
                            OdataProperties = odataProps
                        });

                    if (!verified)
                    {
                        throw new HttpResponseException(HttpStatusCode.Forbidden);
                    }
                }
            }
            
            return continuation();
        }
    }
}