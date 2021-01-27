using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public abstract class BaseAuthSpecification : IAuthSpecification
    {
        public IAuthSpecification And(IAuthSpecification spec)
        {
            return new AndAuthentication(this, spec);
        }

        public IAuthSpecification Not(IAuthSpecification spec)
        {
            return new NotAuthentication(this, spec);
        }

        public IAuthSpecification Or(IAuthSpecification spec)
        {
            return new OrAuthentication(this, spec);
        }


        public abstract bool IsAuthenticated(AuthorithenContext actx);
    }
}