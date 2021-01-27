using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    using Models;

    public class Owner<T> : Relation<T> where T: class
    {
        public Owner(string relationDescription)
            :base(relationDescription, null)
        {
        }

        public override bool IsAuthenticated(AuthorithenContext actx)
        {
            // check signin
            //
            if (null == Utilities.GetCurrentUser())
            {
                return false;
            }

            tar = Utilities.GetCurrentUser().Id;

            return base.IsAuthenticated(actx);
        }
    }
}