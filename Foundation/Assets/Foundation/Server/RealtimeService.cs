// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

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

        RealtimeService() : base("Realtime"){}

        #endregion

        #region Public Method

        /// <summary>
        /// New account created in memory
        /// </summary>
        /// <returns>Realtime Authentication Token to use</returns>
        public UnityTask<RealtimeToken> SignIn(Dictionary<string, string[] >channels = null)
        {
            if (!IsAuthenticated)
                return UnityTask.FailedTask<RealtimeToken>("Not authenticated");

            return HttpPost<RealtimeToken>("SignIn", new RealtimeSignIn
            {
                Channels = channels

            });
        }

        #endregion
    }
}