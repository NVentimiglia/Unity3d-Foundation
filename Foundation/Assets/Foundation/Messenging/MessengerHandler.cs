// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;

namespace Foundation.Messenging
{
    /// <summary>
    /// For passing messages to coroutines
    /// </summary>
    [AddComponentMenu("Foundation/Messenger/MessengerHandler")]
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