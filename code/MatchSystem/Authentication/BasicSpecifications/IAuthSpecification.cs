using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MatchSystem.Authentication
{
    public interface IAuthSpecification
    {
        IAuthSpecification And(IAuthSpecification spec);

        IAuthSpecification Or(IAuthSpecification spec);

        IAuthSpecification Not(IAuthSpecification spec);

        bool IsAuthenticated(AuthorithenContext actx);
    }
}
