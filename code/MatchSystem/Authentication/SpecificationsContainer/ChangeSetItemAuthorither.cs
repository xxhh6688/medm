using Microsoft.Restier.Core.Submit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace MatchSystem.Authentication
{
    using System.Net.Http;
    using System.Web.OData.Extensions;

    public class ChangeSetItemAuthorither : IChangeSetItemAuthorizer
    {
        // The inner Authorizer will call CanUpdate/Insert/Delete<EntitySet> method
        private IChangeSetItemAuthorizer InnerAuthorizer { get; set; }

        public async Task<bool> AuthorizeAsync(
            SubmitContext context,
            ChangeSetItem item,
            CancellationToken cancellationToken)
        {

            var x = await InnerAuthorizer.AuthorizeAsync(
                context,
                item,
                cancellationToken);

            if (x)
            {
                var modifyItem = item as DataModificationItem;
                return ValidationContainer.Instance.Verify(
                    new AuthorithenContext {
                        EntityTypeName = modifyItem.ResourceSetName,
                        HttpMethodName = modifyItem.DataModificationItemAction.ToString().ToLower(),
                        ModifyItem = modifyItem
                    });
            }

            return false;
        }
    }
}