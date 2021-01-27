using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public class SignIn : BaseAuthSpecification
    {
        public override bool IsAuthenticated(AuthorithenContext actx = null)
        {
            return null != Utilities.GetCurrentUser();
        }
    }
}