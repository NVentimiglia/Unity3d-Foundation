// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace Foundation.Messenging
{
    /// <summary>
    /// A message broker for global message propagation. (global events, pub/sub pattern.)
    /// </summary>
    public class Messenger
    {
        #region settings
        /// <summary>
        /// Will use message's sub classes when looking for delegates
        /// </summary>
        public static bool CheckSubclasses = true;

        #endregion

        #region internal objects
        /// <summary>
        /// represents a messenger subscriber
        /// </summary>
        protected class Subscription
        {
            /// <summary>
            /// ValueType of message this is listening to
            /// </summary>
            public readonly Type MessageType;

            /// <summary>
            /// subscribing delegate
            /// </summary>
            public readonly Delegate Handler;

            /// <summary>
            /// Object that is subscribing
            /// </summary>
            /// <remarks>
            /// Handler target
            /// </remarks>
            public readonly object Instance;

            public Subscription(Delegate handler, Type mtype, object instance)
            {
                Handler = handler;
                MessageType = mtype;
                Instance = instance;
            }
        }

        /// <summary>
        /// a cached message for late subscribers
        /// </summary>
        protected class Cache
        {
            /// <summary>
            /// ValueType of message this is
            /// </summary>
            public readonly Type MessageType;

            /// <summary>
            /// Message instance
            /// </summary>
            public readonly object Message;

            public Cache(Type mtype, object m)
            {
                MessageType = mtype;
                Message = m;
            }
        }

        /// <summary>
        /// delegates
        /// </summary>
        static readonly List<Subscription> Subscriptions = new List<Subscription>();

        /// <summary>
        /// SaveAsync of historical messages
        /// </summary>
        static readonly List<Cache> Cached = new List<Cache>();
        #endregion

        #region shared

        static void Invoke(Delegate handler, object message)
        {
#if UNITY_WSA && !UNITY_EDITOR
            var method = handler.GetMethodInfo();
            if (method.ReturnType == typeof(IEnumerator))
            {
                var mono = handler.Target as MonoBehaviour;

                if (mono != null)
                {
                    mono.StartCoroutine(method.Name, message);
                }
                else
                {
                    MessengerHandler.Instance.StartCoroutine((IEnumerator)method.Invoke(handler.Target, new[] { message }));
                }

            }
            else
            {
                handler.DynamicInvoke(message);
            }
#else
            if (handler.Method.ReturnType == typeof(IEnumerator))
            {
                var mono = handler.Target as MonoBehaviour;

                if (mono != null)
                {
                    mono.StartCoroutine(handler.Method.Name, message);
                }
                else
                {
                    MessengerHandler.Instance.StartCoroutine((IEnumerator)handler.Method.Invoke(handler.Target, new[] { message }));
                }

            }
            else
            {
                handler.DynamicInvoke(message);
            }
#endif
        }

        /// <summary>
        /// determines if the subscription should be invoked
        /// </summary>
        /// <param name="subscriptionType">subscription type</param>
        /// <param name="messagetype">message type</param>
        /// <returns></returns>
        static bool IsMatch(Type subscriptionType, Type messagetype)
        {
            return (
                // is message type 
                subscriptionType == messagetype
                // or handler is an interface of message
#if UNITY_WSA && !UNITY_EDITOR
                || (CheckSubclasses && messagetype.GetTypeInfo().IsSubclassOf(subscriptionType))
                || (CheckSubclasses && messagetype.GetTypeInfo().ImplementedInterfaces.Contains(subscriptionType))
#else
                || (CheckSubclasses && subscriptionType.IsAssignableFrom(messagetype))
#endif
                );
        }
        #endregion

        #region Subscribe

        /// <summary>
        /// Adds the delegate as a listener for the message event
        /// </summary>
        /// <param name="handler">handler</param>
        /// <param name="messageType">ValueType of message</param>
        /// <param name="instance">sending instance</param>
        public static void Subscribe(Delegate handler, Type messageType, object instance)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (Subscriptions)
            {
                Subscriptions.Add(new Subscription(handler, messageType, instance));
            }

            // send caches messages
            InvokeCache(messageType, handler);
        }

        /// <summary>
        /// Searches the instance for subscription attributes and adds the annotated methods as subscribers.
        /// </summary>
        /// <param name="instance"></param>
        public static void Subscribe(object instance)
        {
            lock (Subscriptions)
            {
#if UNITY_WSA && !UNITY_EDITOR
                var methods = instance.GetType().GetRuntimeMethods().Where(o => HasAttribute<SubscribeAttribute>(o)).ToArray();
#else
                var methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Where(o => HasAttribute<SubscribeAttribute>(o)).ToArray();
#endif

                foreach (var methodInfo in methods)
                {
                    var ps = methodInfo.GetParameters();
                    if (ps.Length != 1)
                    {
                        Debug.LogError("Subscription aborted. Invalid parameter length.");
                        continue;
                    }

                    MethodInfo info = methodInfo;

                    if (info.ReturnType == typeof(IEnumerator))
                    {
                        Func<object, IEnumerator> action;
                        action = o => (IEnumerator)info.Invoke(instance, new[] { o });
                        var type = ps[0].ParameterType;
                        Subscribe(action, type, instance);
                    }
                    else
                    {
                        Action<object> action;
                        action = o => info.Invoke(instance, new[] { o });
                        var type = ps[0].ParameterType;
                        Subscribe(action, type, instance);
                    }
                }
            }
        }
        #endregion

        #region Unsubscribe
        /// <summary>
        /// Unsubscribes from single delegate from message
        /// </summary>
        /// <param name="handler">handler</param>
        public static void Unsubscribe(Delegate handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (Subscriptions)
            {
                Subscriptions.RemoveAll(o => o.Handler == handler);
            }
        }

        /// <summary>
        /// UnSubscribes all listeners member to this instance
        /// </summary>
        /// <param name="instance"></param>
        public static void Unsubscribe(object instance)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (Subscriptions)
            {
                Subscriptions.RemoveAll(o => o.Instance == instance);
            }
        }

        /// <summary>
        /// Removes all subscriptions
        /// </summary>
        public static void UnsubscribeAll()
        {
            Subscriptions.Clear();
        }

        #endregion

        #region Publishing
        /// <summary>
        /// Publishes the message to all delegates listening to this message
        /// </summary>
        /// <param name="m">message to send</param>
        /// <returns>number of methods called</returns>
        public static void Publish(object m)
        {
            Publish(m.GetType(), m, null);
        }

        /// <summary>
        /// Publishes the message to all delegates listening to this message
        /// </summary>
        /// <param name="mType">ValueType of message</param>
        /// <param name="m">message to send</param>
        /// <returns>number of methods called</returns>
        public static void Publish(Type mType, object m)
        {
            Publish(mType, m, null);
        }

        /// <summary>
        /// Publishes the message to most delegates listening to this message.
        /// Will ignore members instance to the ignore argument.
        /// </summary>
        /// <param name="mType">ValueType of message</param>
        /// <param name="m">parameter to send</param>
        /// <param name="ignore">Ignores all delegates member to this instance.</param>
        /// <returns>number of methods called</returns>
        public static void Publish(Type mType, object m, object ignore)
        {
            lock (Subscriptions)
            {
                var subs = Subscriptions.Where(o => IsMatch(o.MessageType, mType) && o.Instance != ignore).ToArray();

                for (int j = 0;j < subs.Length;j++)
                {
                    Invoke(subs[j].Handler, m);
                }
            }
#if UNITY_WSA
            if (HasAttribute<CachedMessage>(mType))
            {
                if (GetAttribute<CachedMessage>(mType).OnePerType)
                    ClearCache(mType);
                else
                    RemoveCache(m);
                AddCache(mType, m);
            }
#else
            if (HasAttribute<CachedMessage>(mType))
            {
                if (GetAttribute<CachedMessage>(mType).OnePerType)
                    ClearCache(mType);
                else
                    RemoveCache(m);
                AddCache(mType, m);
            }
#endif

        }
        #endregion

        #region caching

        /// <summary>
        /// Publishes the message to most delegates listening to this message.
        /// Caches the message for late subscribers
        /// </summary>
        /// <param name="m">parameter to send</param>
        /// <returns>number of methods called</returns>
        public static void PublishAndCache(object m)
        {
            //remove instance if it already exists
            RemoveCache(m);

            Publish(m.GetType(), m);
            AddCache(m.GetType(), m);

#if UNITY_EDITOR
            if (HasAttribute<CachedMessage>(m.GetType()))
                Debug.LogError("Messages with the CachedMessage attribute should use Publish.");
#endif
        }

        /// <summary>
        /// Publishes the message to most delegates listening to this message.
        /// Caches the message for late subscribers
        /// </summary>
        /// <param name="m">parameter to send</param>
        /// <param name="ignore">Ignores all delegates member to this instance.</param>
        /// <returns>number of methods called</returns>
        public static void PublishAndCache(object m, object ignore)
        {
            //remove instance if it already exists
            RemoveCache(m);

            Publish(m.GetType(), m, ignore);
            AddCache(m.GetType(), m);

#if UNITY_EDITOR
            if (HasAttribute<CachedMessage>(m.GetType()))
                Debug.LogError("Messages with the CachedMessage attribute should use Publish.");
#endif
        }

        /// <summary>
        /// Publishes cached message to the delegate
        /// </summary>
        /// <param name="mType">cached message type</param>
        /// <param name="handler">delegate receiving the cached messages</param>
        static void InvokeCache(Type mType, Delegate handler)
        {
            lock (Cached)
            {
                var m = Cached.Where(o => IsMatch(mType, o.MessageType));

                foreach (var cach in m)
                {
                    Invoke(handler, cach.Message);
                }
            }
        }

        /// <summary>
        /// Adds message instance to the cache for resending to late subscribers
        /// </summary>
        public static void AddCache(Type mType, object m)
        {
            lock (Cached)
            {
                Cached.Add(new Cache(mType, m));
            }
        }

        /// <summary>
        /// removes message instance from the cache
        /// </summary>
        public static void RemoveCache(object message)
        {
            lock (Cached)
            {
                Cached.RemoveAll(o => o.Message == message);
            }
        }

        /// <summary>
        /// Removes all cached messages of a specific type
        /// </summary>
        public static void ClearCache(Type mType)
        {
            lock (Cached)
            {
                Cached.RemoveAll(o => o.MessageType == mType);
            }
        }

        /// <summary>
        /// Removes all cached messages
        /// </summary>
        public static void ClearCache()
        {
            lock (Cached)
            {
                Cached.Clear();
            }
        }
        #endregion

        #region Misc

        /// <summary>
        /// Returns true if the subscription exists
        /// </summary>
        /// <param name="handler">handler</param>
        /// <param name="messageType">ValueType of message</param>
        /// <param name="instance">sending instance</param>
        public static bool HasSubscription(Delegate handler, Type messageType, object instance)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (Subscriptions)
            {
                return Subscriptions.Any(o => o.Instance == instance && messageType == o.MessageType && handler == o.Handler);
            }
        }

        /// <summary>
        /// Returns subscription count
        /// </summary>
        public static int SubscriptionCount()
        {
            return Subscriptions.Count;
        }

        /// <summary>
        /// Returns cache count
        /// </summary>
        public static int CacheCount()
        {
            return Cached.Count;
        }

        /// <summary>
        /// Returns cache count
        /// </summary>
        public static int CacheCount(Type messageType)
        {
            return Cached.Count(o => IsMatch(messageType, o.MessageType));
        }

        #endregion

        #region protected
        /// <summary>
        /// return Attribute.IsDefined(m, typeof(T));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        static bool HasAttribute<T>(MemberInfo m) where T : Attribute
        {
#if UNITY_WSA  && !UNITY_EDITOR
            return m.CustomAttributes.Any(o => o.AttributeType == typeof(T));
#else
            return Attribute.IsDefined(m, typeof(T));
#endif
        }

        /// <summary>
        /// return Attribute.IsDefined(m, typeof(T));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        static bool HasAttribute<T>(Type m) where T : Attribute
        {
#if UNITY_WSA  && !UNITY_EDITOR
            return m.GetTypeInfo().CustomAttributes.Any(o => o.AttributeType == typeof(T));
#else
            return Attribute.IsDefined(m, typeof(T));
#endif
        }

        /// <summary>
        ///  return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        static T GetAttribute<T>(MemberInfo m) where T : Attribute
        {
#if UNITY_WSA  && !UNITY_EDITOR
            return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
#else
            return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
#endif
        }

        /// <summary>
        ///  return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        static T GetAttribute<T>(Type m) where T : Attribute
        {
#if UNITY_WSA  && !UNITY_EDITOR
            return m.GetTypeInfo().GetCustomAttribute<T>();
#else
            return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
#endif
        }
        #endregion
    }

    /// <summary>
    /// A generic message broker for global messages.
    /// </summary>
    /// <remarks>
    /// Extends the non generic messenger
    /// </remarks>
    public class Messenger<T>
    {
        /// <summary>
        /// Subscribes to the message
        /// </summary>
        /// <param name="handler">handler</param>
        public static void Subscribe(Action<T> handler)
        {
            Messenger.Subscribe(handler, typeof(T), handler.Target);
        }

        /// <summary>
        /// Unsubscribes from the message
        /// </summary>
        /// <param name="handler">handler</param>
        public static void Unsubscribe(Action<T> handler)
        {
            Messenger.Unsubscribe(handler);
        }

        /// <summary>
        /// Subscribes to the message
        /// </summary>
        /// <param name="handler">handler</param>
        public static void SubscribeCoroutine(Func<T, IEnumerator> handler)
        {
            Messenger.Subscribe(handler, typeof(T), handler.Target);
        }

        /// <summary>
        /// Unsubscribes from the message
        /// </summary>
        /// <param name="handler">handler</param>
        public static void UnsubscribeCoroutine(Func<T, IEnumerator> handler)
        {
            Messenger.Unsubscribe(handler);
        }

        /// <summary>
        /// Publishes the message to all delegates listening to this message
        /// </summary>
        /// <param name="arg1">parameter to send</param>
        /// <returns>number of methods called</returns>
        public static void Publish(T arg1)
        {
            Messenger.Publish(typeof(T), arg1);
        }

        /// <summary>
        /// Publishes the message to all delegates listening to this message
        /// </summary>
        /// <param name="ignore">ignores members of the ignore instance</param>
        /// <param name="arg1">parameter to send</param>
        /// <returns>number of methods called</returns>
        public static void Publish(T arg1, object ignore)
        {
            Messenger.Publish(typeof(T), arg1, ignore);
        }

        /// <summary>
        /// Adds message to cache to resending to late subscribers
        /// </summary>
        public static void AddCache(T arg1)
        {
            Messenger.AddCache(typeof(T), arg1);
        }

        /// <summary>
        /// Removes all cached messages of a specific type
        /// </summary>
        public static void ClearCache()
        {
            Messenger.ClearCache(typeof(T));

        }

        /// <summary>
        /// Returns true if the subscription exists
        /// </summary>
        /// <param name="handler">handler</param>
        public static bool HasSubscription(Action<T> handler)
        {
            return Messenger.HasSubscription(handler, typeof(T), handler.Target);
        }

        /// <summary>
        /// Returns cache count
        /// </summary>
        public static int CacheCount()
        {
            return Messenger.CacheCount(typeof(T));
        }
    }
}