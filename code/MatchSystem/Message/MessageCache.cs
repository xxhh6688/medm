using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace MatchSystem.Message
{
    /// <summary>
    /// Use to Cache waitable content
    /// </summary>
    public class MessageCache
    {
        private MessageCache(){}

        private static MessageCache currentCache = new MessageCache();

        /// <summary>
        /// Get the current MessageCache
        /// </summary>
        /// <returns></returns>
        public static MessageCache GetCurrent()
        {
            return currentCache;
        }

        private ConcurrentDictionary<string, WaitAbleContent> cache = new ConcurrentDictionary<string, WaitAbleContent>();

        /// <summary>
        /// Start to listen message
        /// </summary>
        /// <param name="userIdWidthToken"></param>
        /// <param name="content"></param>
        public void RegistListen(string userIdWidthToken, WaitAbleContent content)
        {
            if (cache.Keys.Contains(userIdWidthToken))
            {
                // reset the content
                cache.AddOrUpdate(userIdWidthToken, content, (k, old) =>
                {
                    if(null != old)
                        old.SetOne(); // continue old one 
                    return content;
                });
            }
            else
            {
                cache.TryAdd(userIdWidthToken, content);
            }
            CleanOutTimeStream();
        }

        public static string GetMarkFromUser(int userId)
        {
            return string.Format("__uid:{0}__", userId);
        }

        /// <summary>
        /// make key different for one user login with different explore in the same time.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string MakeKey(int uid, string token)
        {
            var mark = GetMarkFromUser(uid);
            return mark + token;
        }

        /// <summary>
        /// New message is comming, continue the blocked thread
        /// </summary>
        /// <param name="message"></param>
        /// <param name="usrs"></param>
        public void SetNewMessage(Models.message message, Models.message_user_ref[] mus)
        {
            foreach (var mu in mus)
            {
                var mark = GetMarkFromUser(mu.UserId);
                var keys = cache.Keys.Where(s => s.Contains(mark));
                foreach (var key in keys)
                {
                    cache[key].ReceivedMessage = message;
                    cache[key].SetOne();
                    WaitAbleContent val;
                    cache.TryRemove(key, out val);
                }

            }
            CleanOutTimeStream();
        }

        private void CleanOutTimeStream()
        {
            var record = new List<string>();
            foreach (var item in cache)
            {
                if (!item.Value.Context.Response.IsClientConnected)
                {
                    record.Add(item.Key);
                }
            }

            foreach (var itemId in record)
            {
                WaitAbleContent val;
                cache.TryRemove(itemId, out val);
            }
        }
    }
}