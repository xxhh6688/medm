using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public abstract class ChangeGetOneSpec : HttpMethodsAuthSpecification
    {
        public override bool AddOneAvailable(AuthorithenContext actx)
        {
            throw new NotImplementedException();
        }

        public override bool GetListAvaliable(AuthorithenContext actx)
        {
            throw new NotImplementedException();
        }
        
    }
}