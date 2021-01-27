using MatchSystem.Authentication;
using MatchSystem.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Restier.Core;
using Microsoft.Restier.Core.Submit;
using Microsoft.Restier.Providers.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.OData.Query;

namespace MatchSystem.App_Start
{
    public class OdataApi : EntityFrameworkApi<Models.Ctx>
    {
        protected override IServiceCollection ConfigureApi(IServiceCollection services)
        {
            Func<IServiceProvider, ODataValidationSettings> validationSettingFactory = (sp) => new ODataValidationSettings
            {
                MaxAnyAllExpressionDepth = 5,
                MaxExpansionDepth = 4,
            };

            return base.ConfigureApi(services)
                .AddService<IChangeSetItemAuthorizer, ChangeSetItemAuthorither>().AddSingleton<ODataValidationSettings>(validationSettingFactory); ;
        }
    }
}