using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace MatchSystem.Message
{
    /// <summary>
    /// waitable content to record in MessageCache and block/continue thread
    /// </summary>
    public class WaitAbleContent
    {
        private AutoResetEvent controller;

        /// <summary>
        /// Create a waitable content
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ctx"></param>
        public WaitAbleContent(int userId, HttpContext ctx)
        {
            UserId = userId;
            Context = ctx;
            controller = new AutoResetEvent(false);
        }

        /// <summary>
        /// Id of user which the thread belongs to
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Message which will return to the user
        /// </summary>
        public Models.message ReceivedMessage { get; set; }

        /// <summary>
        /// HttpContext of the thread
        /// </summary>
        public HttpContext Context { get; set; }

        /// <summary>
        /// Start block the thread
        /// </summary>
        public void Wait()
        {
            controller.WaitOne();
        }

        /// <summary>
        /// Continue the thread
        /// </summary>
        public void SetOne()
        {
            controller.Set();
        }
    }
}