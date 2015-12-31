// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;

namespace Foundation.Server
{
    /// <summary>
    /// Configuration for Foundation Cloud Services
    /// </summary>
    public class ServerConfig :ScriptableObject
    {
        /// <summary>
        /// Is Valid flag
        /// </summary>
        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(Path); }
        }

        /// <summary>
        /// URL to the Web API
        /// </summary>
        public string Path = "http://{yourdomain.com}";

        /// <summary>
        /// Application Key
        /// </summary>
        public string Key;

        private static ServerConfig _instance;
        public static ServerConfig Instance
        {
            get { return _instance ?? (_instance = Create()); }
        }

        static ServerConfig Create()
        {
            return Resources.Load<ServerConfig>("ServerConfig");
        }

    }
}