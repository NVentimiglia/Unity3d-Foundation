// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using Foundation.Server.Api;
using Foundation.Tasks;

namespace Foundation.Server
{
    /// <summary>
    /// Service for Realtime Messaging Authentication
    /// </summary>
    public class RealtimeService : ServiceClientBase
    {
        #region Internal

        public static readonly RealtimeService Instance = new RealtimeService();

        RealtimeService() : base("Realtime") { }

        #endregion

        #region Public Method
        /// <summary>
        /// New account created in memory
        /// </summary>
        /// <returns>Realtime Authentication Token to use</returns>
        public void SignIn(Dictionary<string, string[]> channels, Action<Response<RealtimeToken>> callback)
        {
            if (!IsAuthenticated)
            {
                callback(new Response<RealtimeToken>(new Exception("Not authenticated")));
                return;
            }

            HttpPostAsync("SignIn", new RealtimeSignIn { Channels = channels }, callback);
        }

        /// <summary>
        /// New account created in memory
        /// </summary>
        /// <returns>Realtime Authentication Token to use</returns>
        public void SignIn(Action<Response<RealtimeToken>> callback)
        {
            SignIn(null, callback);
        }

        /// <summary>
        /// New account created in memory
        /// </summary>
        /// <returns>Realtime Authentication Token to use</returns>
        public UnityTask<RealtimeToken> SignIn(Dictionary<string, string[]> channels = null)
        {
            var task = new UnityTask<RealtimeToken>(TaskStrategy.Custom);
            SignIn(channels, task.FromResponse());
            return task;
        }

        #endregion
    }
}