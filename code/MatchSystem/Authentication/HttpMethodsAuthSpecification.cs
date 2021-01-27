using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public abstract class HttpMethodsAuthSpecification : BaseAuthSpecification
    {
        public override bool IsAuthenticated(AuthorithenContext actx)
        {
            // get
            //
            if (actx.HttpMethodName == "get")
            {
                if (actx.OdataProperties.Path.PathTemplate.ToLower().Contains("entityset/key"))
                {
                    // get entity
                    return GetOneAvailable(actx);
                }
                else
                {
                    // get entity set
                    return GetListAvaliable(actx);
                }
            }

            // remove 
            //
            if (actx.HttpMethodName == "remove")
            {
                return RemoveAvailable(actx);
            }

            // update
            if (actx.HttpMethodName == "update")
            {
                return UpdateAvailable(actx);
            }

            // insert
            return AddOneAvailable(actx);
        }

        protected int GetTargetIdForGetOneAndChange(AuthorithenContext actx)
        {
            return int.Parse(actx.OdataProperties.Path.Segments[1].ToString()); ;
        }

        public abstract bool RemoveAvailable(AuthorithenContext actx);

        public abstract bool GetOneAvailable(AuthorithenContext actx);

        public abstract bool UpdateAvailable(AuthorithenContext actx);

        public abstract bool AddOneAvailable(AuthorithenContext actx);

        public abstract bool GetListAvaliable(AuthorithenContext actx);
    }
}