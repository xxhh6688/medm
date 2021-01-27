using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public class NotAuthentication : BaseAuthSpecification
    {
        IAuthSpecification lSpec, rSpec;

        public NotAuthentication(IAuthSpecification left, IAuthSpecification right)
        {
            this.lSpec = left;
            this.rSpec = right;
        }

        public override bool IsAuthenticated(AuthorithenContext actx)
        {
            return this.lSpec.IsAuthenticated(actx) != this.rSpec.IsAuthenticated(actx);
        }
    }
}