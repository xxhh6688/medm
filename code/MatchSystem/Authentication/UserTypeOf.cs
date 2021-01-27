using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    using System.Runtime.InteropServices;

    public class UserTypeOf: BaseAuthSpecification
    {
        private readonly int _type;

        public UserTypeOf(int type)
            :base()
        {
            _type = type;
        }

        public override bool IsAuthenticated(AuthorithenContext actx = null)
        {
            return Utilities.GetCurrentUser() != null && (Utilities.GetCurrentUser().Type & _type) != 0;
        }
    }
}