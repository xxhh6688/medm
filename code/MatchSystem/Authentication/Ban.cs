using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public class Ban : BaseAuthSpecification
    {
        public override bool IsAuthenticated(AuthorithenContext actx)
        {
            return false;
        }
    }
}