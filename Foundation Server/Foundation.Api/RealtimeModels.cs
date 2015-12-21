// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections.Generic;

namespace Foundation.Server.Api
{
    /// <summary>
    /// Authorizes the global Messenger
    /// </summary>
    /// <returns></returns>
    public class RealtimeSignIn
    {
        /// <summary>
        /// Channels by Permission
        /// </summary>
        public Dictionary<string, string[]> Channels { get; set; }
    }

    /// <summary>
    /// Authorizes the global Messenger
    /// </summary>
    /// <returns></returns>
    public class RealtimeToken
    {
        /// <summary>
        /// Token used
        /// </summary>
        public string AuthenticationToken { get; set; }
    }
}