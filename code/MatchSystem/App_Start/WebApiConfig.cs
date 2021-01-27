using MatchSystem.App_Start;
using MatchSystem.Filters;
using Microsoft.Restier.Publishers.OData;
using Microsoft.Restier.Publishers.OData.Batch;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MatchSystem
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.EnableCors(new EnableCorsAttribute("*", "accept, content-type, cookie", "GET,POST,DELETE,PATCH,PUT,OPTIONS"));
            config.MessageHandlers.Add(new PreflightRequestsHandler());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.MapRestierRoute<OdataApi>(
                "model",
                "tables",
                new RestierBatchHandler(GlobalConfiguration.DefaultServer));

            config.Filters.Add(new Authentication.QueryAuthoritherFilter());
        }
    }
}
