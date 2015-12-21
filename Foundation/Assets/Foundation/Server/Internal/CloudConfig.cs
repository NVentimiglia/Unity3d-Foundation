// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// Configuration for Foundation Cloud Services
    /// </summary>
    public class CloudConfig :ScriptableObject
    {
        /// <summary>
        /// Is Valid flag
        /// </summary>
        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(Key); }
        }

        /// <summary>
        /// URL to the Web API
        /// </summary>
        public string Path = "http://{yourdomain.com}";

        /// <summary>
        /// Application Key
        /// </summary>
        public string Key;

        private static CloudConfig _instance;
        public static CloudConfig Instance
        {
            get { return _instance ?? (_instance = Create()); }
        }

        static CloudConfig Create()
        {
            return Resources.Load<CloudConfig>("CloudConfig");
        }

    }
}