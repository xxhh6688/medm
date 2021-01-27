using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace MatchSystem.Authentication
{
    public class Relation<T> : ChangeGetOneSpec where T:class
    {
        protected string des;
        protected object tar;

        /// <summary>
        /// check userId property or relation type userId property
        /// </summary>
        /// <param name="relationDescription">"UserId" or "shop.UserId"</param>
        public Relation(string relationDescription, object targetValue)
            :base()
        {
            des = relationDescription;
            tar = targetValue;
        }

        public static object GetPropertyByName(object source, string propName)
        {
            var type = source.GetType();
            var prop = type.GetProperty(propName);
            return prop.GetValue(source);
        }

        public static object GetRelationObj(T target, string[] props)
        {
            object obj = target;
            foreach (var prop in props)
            {
                obj = GetPropertyByName(obj, prop);
            }
            
            return obj;
        }

        public bool Compare(AuthorithenContext actx, object targetValue)
        {
            var id = GetTargetIdForGetOneAndChange(actx);
            var target = Utilities.GetDBContext().Set<T>().Find(id);

            if (string.IsNullOrEmpty(des.Trim()))
            {
                return false;
            }


            var props = des.Split('.');
            var actualValue = GetRelationObj(target, props);
            return actualValue.Equals(targetValue);
        }

        public override bool GetOneAvailable(AuthorithenContext actx)
        {
            return Compare(actx, tar);
        }

        public override bool RemoveAvailable(AuthorithenContext actx)
        {
            return Compare(actx, tar);
        }

        public override bool UpdateAvailable(AuthorithenContext actx)
        {
            return Compare(actx, tar);
        }
    }
}