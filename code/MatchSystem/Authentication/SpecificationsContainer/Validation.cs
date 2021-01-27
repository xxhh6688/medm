using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public class Validation
    {
        string _entityName;

        public Validation(string entityName)
        {
            _entityName = entityName;
        }

        public bool IsMatchEntityName(string entityName)
        {
            return _entityName == entityName;
        }

        public bool IsValid(AuthorithenContext context)
        {
            var res = false;
            switch (context.HttpMethodName.ToLower())
            {
                case "get":
                    {
                        if (context.OdataProperties.Path.PathTemplate.ToLower().Equals("~/entityset/key"))
                        {
                            // get one
                            if(null != GetOneSpecification)
                            {
                                res = GetOneSpecification.IsAuthenticated(context);
                            }
                        }
                        else if(context.OdataProperties.Path.PathTemplate.ToLower().Equals("~/entityset"))
                        {
                            // get list
                            if(null != GetListSpecification)
                            {
                                res = GetListSpecification.IsAuthenticated(context);
                            }
                        }
                        break;
                    }
                case "insert":
                    {
                        if(null != AddOneSpecification)
                        {
                            res = AddOneSpecification.IsAuthenticated(context);
                        }
                        break;
                    }
                case "update":
                    {
                        if(null != UpdateSpecification)
                        {
                            res = UpdateSpecification.IsAuthenticated(context);
                        }
                        break;
                    }
                case "remove":
                    {
                        if(null != RemoveSpecification)
                        {
                            res = RemoveSpecification.IsAuthenticated(context);
                        }
                        break;
                    }
                default:
                    // ~/entityset/key/navigation  is not support
                    break;
            }

            return res;
        }

        private IAuthSpecification getOneSpec;

        public IAuthSpecification GetOneSpecification
        {
            get { return getOneSpec; }
            set { getOneSpec = value; }
        }

        private IAuthSpecification addOneSpec;

        public IAuthSpecification AddOneSpecification
        {
            get { return addOneSpec; }
            set { addOneSpec = value; }
        }

        private IAuthSpecification getListSpec;

        public IAuthSpecification GetListSpecification
        {
            get { return getListSpec; }
            set { getListSpec = value; }
        }

        private IAuthSpecification updateSpec;

        public IAuthSpecification UpdateSpecification
        {
            get { return updateSpec; }
            set { updateSpec = value; }
        }

        private IAuthSpecification removeSpec;

        public IAuthSpecification RemoveSpecification
        {
            get { return removeSpec; }
            set { removeSpec = value; }
        }

    }
}