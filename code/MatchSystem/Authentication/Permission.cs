using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem.Authentication
{
    public class Permission<TEntity> : HttpMethodsAuthSpecification where TEntity : class, new()
    {
        protected Func<TEntity, TEntity, Models.user, bool> _condition;

        /// <summary>
        /// Ctor for permission.
        /// </summary>
        /// <param name="condition">
        /// <para>Condition of the permission.</para>
        /// <para>There are three param in this func.</para>
        /// <para>First is the original entity used for getOne, addOne, deleteOne, or updateOne</para>
        /// <para>Second is the changed/new entity used for addOne or updateOne</para>
        /// <para>Third is the current user</para>
        /// </param>
        public Permission(Func<TEntity, TEntity, Models.user, bool> condition)
            : base()
        {
            this._condition = condition;
        }

        public override bool IsAuthenticated(AuthorithenContext actx)
        {
            // check signin
            //
            if (null == Utilities.GetCurrentUser())
            {
                return false;
            }

            return base.IsAuthenticated(actx);
        }

        public override bool AddOneAvailable(AuthorithenContext actx)
        {
            var uid = Utilities.GetCurrentUser().Id;
            var ctx = Utilities.GetDBContext();
            var usr = ctx.user.Find(uid);

            var newOne = new TEntity();
            newOne = CopyProperty(newOne, actx.ModifyItem.LocalValues);
            return _condition(null, newOne, usr);
        }

        public override bool GetListAvaliable(AuthorithenContext actx)
        {
            var uid = Utilities.GetCurrentUser().Id;
            var ctx = Utilities.GetDBContext();
            var usr = ctx.user.Find(uid);
            return _condition(null, null, usr);
        }

        public override bool GetOneAvailable(AuthorithenContext actx)
        {
            var tid = GetTargetIdForGetOneAndChange(actx);
            return CompareGetDelete(tid,actx); 
        }

        public override bool RemoveAvailable(AuthorithenContext actx)
        {
            var tid = actx.ModifyItem.ResourceKey.First().Value;
            return CompareGetDelete(tid, actx);
        }

        public override bool UpdateAvailable(AuthorithenContext actx)
        {
            var tid = actx.ModifyItem.ResourceKey.First().Value;
            var target = Utilities.GetDBContext().Set<TEntity>().Find(tid);
            var uid = Utilities.GetCurrentUser().Id;
            var ctx = Utilities.GetDBContext();
            var usr = ctx.user.Find(uid);

            var updateOne = Utilities.Copy(target, new string[] { });
            updateOne = CopyProperty(updateOne, actx.ModifyItem.LocalValues);

           return _condition(target, updateOne, usr);
        }

        public bool CompareGetDelete(object tid, AuthorithenContext actx)
        {
            var target = Utilities.GetDBContext().Set<TEntity>().Find(tid);
            var uid = Utilities.GetCurrentUser().Id;
            var ctx = Utilities.GetDBContext();
            var usr = ctx.user.Find(uid);

            return _condition(target, null, usr);
        }

        private TEntity CopyProperty(TEntity entity, IReadOnlyDictionary<string, object> localValues)
        {
            var t = typeof(TEntity);
            foreach (var item in localValues)
            {
                var p = t.GetProperty(item.Key);
                if (null != p)
                {
                    if (p.PropertyType == typeof(DateTime) && item.Value is DateTimeOffset)
                    {
                        var val = ((DateTimeOffset)item.Value).DateTime;
                        p.SetValue(entity, val);
                        continue;
                    }
                    p.SetValue(entity, item.Value);
                }
            }
            return entity;
        }

    }
}