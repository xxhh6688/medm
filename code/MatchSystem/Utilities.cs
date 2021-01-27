using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Configuration;

namespace MatchSystem
{
    using Models;
    using System.Collections.Concurrent;
    using System.Runtime.Remoting.Messaging;

    public static class Utilities
    {
        public static user GetCurrentUser()
        {
            var token = Utilities.GetTokenFormCookie();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            Models.user usr = null;
            if (!Utilities.TryGetUserFromCache(token, out usr))
            {
                // find token from db
                var res = Utilities.GetDBContext().token.Where(tk => tk.Token1 == token).ToArray();
                if (res.Length < 1)
                {
                    return null;
                }

                usr = Utilities.Copy(res.First().user, new string[] { });
                Utilities.SetUserIntoCache(token, usr);
            }
            return usr;
        }

        public static Models.user GetLoginUser(string token)
        {
            Models.user usr = null;
            if (string.IsNullOrEmpty(token))
            {
                return usr;
            }

            if (!Utilities.TryGetUserFromCache(token, out usr))
            {
                // find token from db
                var res = Utilities.GetDBContext().token.Where(tk => tk.Token1 == token).ToArray();
                if (res.Length < 1)
                {
                    return usr;
                }

                usr = Utilities.Copy(res.First().user, new string[] { });
                Utilities.SetUserIntoCache(token, usr);
            }

            return usr;
        }

        public static T Copy<T>(T obj, string[] doNotCopyPropNames) where T : new()
        {
            if(obj == null)
            {
                throw new ArgumentNullException("Copy<T>(T obj)");
            }

            var res = new T();
            var typ = typeof(T);
            foreach (var info in typ.GetProperties())
            {
                if (!doNotCopyPropNames.Contains(info.Name)
                    &&
                    !info.GetMethod.IsVirtual
                    )
                {
                    info.SetValue(res, info.GetValue(obj));
                }
            }

            return res;
        }

        public static bool AreSame<T>(T left, T right, string[] ignoredPropNames)
        {
            var props = typeof(T).GetProperties();

            var res = true;
            foreach (var prop in props)
            {
                if(!prop.GetMethod.IsVirtual && !ignoredPropNames.Contains(prop.Name))
                {
                    var l = prop.GetValue(left);
                    var r = prop.GetValue(right);
                    if (!object.Equals(l, r))
                    {
                        res = false;
                        break;
                    }
                }
            }

            return res;
        }

        public static string GetTokenFormCookie()
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(ConfigurationManager.AppSettings["token"]);
            if (null != cookie)
            {
                return cookie.Value;
            }

            return string.Empty;
        }

        public static bool TryGetUserFromCache(string token, out Models.user usr)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var obj = HttpContext.Current.Cache.Get(token);
                if (null != obj)
                {
                    usr = (Models.user)obj;
                    return true;
                }
            }

            usr = null;
            return false;
        }

        public static bool NotNullOfProperties<T>(T obj, params string[] excepts) where T : class
        {
            var res = true;
            var props = typeof(T).GetProperties();
            foreach (var t in props)
            {
                if (!excepts.Contains(t.Name))
                {
                    if (!t.PropertyType.IsValueType && !t.GetMethod.IsVirtual && !t.PropertyType.IsGenericType)
                    {
                        if (null == t.GetValue(obj))
                        {
                            res = false;
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// caste T to T1, they have the same properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T1 Cast<T,T1>(T s) where T1: new()
        {
            T1 res = new T1();
            var tProps = typeof(T).GetProperties();
            var t1Type = typeof(T1);
            foreach (var prop in tProps)
            {
                var p = t1Type.GetProperty(prop.Name);
                if(p != null)
                {
                    var val = prop.GetValue(s);
                    p.SetValue(res, val);
                }
            }

            return res;
        }

        public static T1 PartialCopy<T, T1>(T s, T1 res, string[] ignore) 
        {
            if(res == null)
            {
                throw new ArgumentNullException("res");
            }
            if(s == null)
            {
                throw new ArgumentNullException("s");
            }

            var tProps = typeof(T).GetProperties();
            var t1Type = typeof(T1);
            foreach (var prop in tProps)
            {
                var p = t1Type.GetProperty(prop.Name);
                if (p != null 
                    && 
                    !p.GetMethod.IsVirtual 
                    && 
                    !ignore.Contains(p.Name) 
                    && 
                    p.PropertyType == prop.PropertyType)
                {
                    var val = prop.GetValue(s);
                    p.SetValue(res, val);
                }
            }

            return res;
        }

        public static int UnixtTimeStamp(DateTime windowsTime)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        public static void SetUserIntoCache(string token, Models.user usr)
        {
            HttpContext.Current.Cache.Add(token, usr, null, System.Web.Caching.Cache.NoAbsoluteExpiration,
                new TimeSpan(0, 15, 0),
                System.Web.Caching.CacheItemPriority.Default,
                null);
        }

        public static void RemoveUserFromCache(string token)
        {
            HttpContext.Current.Cache.Remove(token);
        }

        public static string CreateNewTokenString(string seed)
        {
            return GetMd5Hash(seed + DateTime.Now.Ticks);
        }

        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        public static Models.Ctx GetDBContext()
        {
            // error of "There is already an open DataReader associated with this Command which must be closed first."
            // so just return new one, do not use single one. -- 2017/2/9
            // return new MeeaDBContext();

            // the solution above is old fashioned, we use thread safe dictionary -- 2017/3/11
            var ctx = CallContext.GetData(typeof(Ctx).FullName) as Ctx;

            if(ctx == null)
            {
                ctx = new Ctx();
                CallContext.SetData(typeof(Ctx).FullName, ctx);
            }

            return ctx;
        }
    }
}