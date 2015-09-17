// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;

namespace Foundation.Internal
{
    /// <summary>
    /// Static helper for passing messages to coroutines
    /// </summary>
    /// <remarks>
    /// Do not initialize, internal
    /// </remarks>
    [AddComponentMenu("Foundation/Internal/MessengerHandler")]
    public class MessengerHandler : MonoBehaviour
    {
        public static MessengerHandler Instance { get; protected set; }

        static MessengerHandler()
        {
            if (Instance == null)
            {
                var go = new GameObject("_MessengerHandler");
                Instance = go.AddComponent<MessengerHandler>();
                DontDestroyOnLoad(go);
            }
        }
    }
}