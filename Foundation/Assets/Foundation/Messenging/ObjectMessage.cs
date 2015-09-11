// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Foundation.Messenging
{
    /// <summary>
    /// an abstract weak-event message for object signals.
    /// Replaces SendMessage. Faster preformence, weak references. 
    /// Allows for external (controller) event listening.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectMessage<T> {

        /// <summary>
        /// Dictionary of all Methods subscribing to the damage message
        /// </summary>
        public static readonly Dictionary<GameObject, List<Action<T>>> Messenger = new Dictionary<GameObject, List<Action<T>>>();

        /// <summary>
        /// Submits the message to the target if subscribed
        /// </summary>
        /// <param name="obj">the object</param>
        /// <param name="msg">The message</param>
        /// <returns>true if this is a subscriber</returns>
        public static bool SendMessage(GameObject obj, T msg)
        {
            if (Messenger.ContainsKey(obj))
            {
                var list = Messenger[obj];
                foreach (var o in list)
                {
                    o.Invoke(msg);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Subscribes the object to the messenger
        /// </summary>
        /// <param name="obj">the object</param>
        /// <param name="handler"> </param>
        public static void Subscribe(GameObject obj, Action<T> handler)
        {
            if (!Messenger.ContainsKey(obj))
            {
                Messenger.Add(obj, new List<Action<T>>());
            }

            Messenger[obj].Add(handler);
        }

        /// <summary>
        /// Unsubscribes the action
        /// </summary>
        /// <param name="obj">the object</param>
        /// <param name="handler"> </param>
        public static void Unsubscribe(GameObject obj, Action<T> handler)
        {
            if (Messenger.ContainsKey(obj))
            {
                var l = Messenger[obj];

                l.Remove(handler);

                if(l.Count == 0)
                    Messenger.Remove(obj);
            }
        }

        /// <summary>
        /// Unsubscribes all listeners for an object 
        /// </summary>
        /// <param name="obj">the object</param>
        public static void Unsubscribe(GameObject obj)
        {
            if (Messenger.ContainsKey(obj))
            {
                Messenger.Remove(obj);
            }
        }
    }
}
