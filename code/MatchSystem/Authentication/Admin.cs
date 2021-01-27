using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public class Admin : BaseAuthSpecification
    {
        public override bool IsAuthenticated(AuthorithenContext actx = null)
        {
            return Utilities.GetCurrentUser()!= null && (Utilities.GetCurrentUser().Type & 16) != 0;
        }
    }
}