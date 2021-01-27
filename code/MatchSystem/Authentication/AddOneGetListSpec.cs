using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public abstract class AddOneGetListSpec : HttpMethodsAuthSpecification
    {
        public override bool GetOneAvailable(AuthorithenContext actx)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveAvailable(AuthorithenContext actx)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateAvailable(AuthorithenContext actx)
        {
            throw new NotImplementedException();
        }
    }
}