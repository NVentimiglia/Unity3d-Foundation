// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System.Collections.Generic;
using Foundation.Server.Api;
using Foundation.Tasks;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// Service for Realtime Messaging Authentication
    /// </summary>
    public class RealtimeService
    {
        #region Internal

        public static readonly RealtimeService Instance = new RealtimeService();
        
        public CloudConfig Config
        {
            get { return CloudConfig.Instance; }
        }

        public readonly ServiceClient ServiceClient = new ServiceClient("Realtime");

        #endregion

        #region Public Method

        /// <summary>
        /// New account created in memory
        /// </summary>
        /// <returns>Realtime Authentication Token to use</returns>
        public UnityTask<RealtimeToken> SignIn(Dictionary<string, string[] >channels = null)
        {
            return ServiceClient.Post<RealtimeToken>("SignIn", new RealtimeSignIn
            {
                Channels = channels

            });
        }

        #endregion
    }
}