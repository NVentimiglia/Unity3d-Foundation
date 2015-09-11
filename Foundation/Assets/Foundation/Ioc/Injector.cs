// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_WSA && !UNITY_EDITOR
using System.Threading.Tasks;
#endif

namespace Foundation.Ioc
{
    /// <summary>
    /// The Injector is a static application service for resolving dependencies by asking for a components type or interface
    /// </summary>
    public class Injector
    {
        #region container

        /// <summary>
        /// represents a injection subscriber
        /// </summary>
        protected class InjectSubscription
        {
            /// <summary>
            /// ValueType of the export
            /// </summary>
            public readonly Type MemberType;

            /// <summary>
            /// Optional lookup key
            /// </summary>
            public string InjectKey { get; private set; }

            /// <summary>
            /// Is there an optional lookup key ?
            /// </summary>
            public bool HasKey
            {
                get
                {
                    return !string.IsNullOrEmpty(InjectKey);
                }
            }

            /// <summary>
            /// Importing Member
            /// </summary>
            public readonly MemberInfo Member;

            /// <summary>
            /// Importing instance
            /// </summary>
            /// <remarks>
            /// Handler target
            /// </remarks>
            public readonly object Instance;

            public InjectSubscription(Type mtype, object instance, MemberInfo member)
            {
                MemberType = mtype;
                Member = member;
                Instance = instance;
            }

            public InjectSubscription(Type mtype, object instance, MemberInfo member, string key)
            {
                MemberType = mtype;
                Member = member;
                Instance = instance;
                InjectKey = key;
            }
        }

        /// <summary>
        /// represents a injection export
        /// </summary>
        protected class InjectExport
        {
            /// <summary>
            /// ValueType of the export
            /// </summary>
            public readonly Type MemberType;

            /// <summary>
            /// Importing instance
            /// </summary>
            /// <remarks>
            /// Handler target
            /// </remarks>
            public readonly object Instance;

            /// <summary>
            /// Optional lookup key
            /// </summary>
            public string InjectKey { get; private set; }

            /// <summary>
            /// Is there an optional lookup key ?
            /// </summary>
            public bool HasKey
            {
                get
                {
                    return !string.IsNullOrEmpty(InjectKey);
                }
            }

            public InjectExport(Type mtype, object instance)
            {
                MemberType = mtype;
                Instance = instance;
            }

            public InjectExport(Type mtype, object instance, string key)
            {
                MemberType = mtype;
                Instance = instance;
                InjectKey = key;
            }
        }

        /// <summary>
        /// delegates
        /// </summary>
        static readonly List<InjectSubscription> Subscriptions = new List<InjectSubscription>();

        /// <summary>
        /// delegates
        /// </summary>
        static readonly List<InjectExport> Exports = new List<InjectExport>();

        /// <summary>
        /// determines if the subscription should be invoked
        /// </summary>
        /// <returns></returns>
        static IEnumerable<InjectExport> GetExports(Type memberType, string key)
        {

            if (!string.IsNullOrEmpty(key))
            {
                return Exports.Where(o => (o.HasKey && o.InjectKey == key));
            }

            return
                Exports.Where(o =>
                    // is message type 
                    memberType == o.MemberType
                        // or handler is an interface of message
#if UNITY_WSA && !UNITY_EDITOR
                    || (memberType.GetTypeInfo().IsAssignableFrom(o.MemberType.GetTypeInfo())));
#else
                    || (memberType.IsAssignableFrom(o.MemberType)));
#endif
        }

        /// <summary>
        /// determines if the subscription should be invoked
        /// </summary>
        /// <returns></returns>
        static IEnumerable<InjectSubscription> GetSubscriptionsFor(InjectExport export)
        {

            if (export.HasKey)
            {
                return Subscriptions.Where(o => (o.HasKey && o.InjectKey == export.InjectKey));
            }
#if UNITY_WSA && !UNITY_EDITOR
            return
                Subscriptions.Where(o =>
                    // is message type 
                    o.MemberType == export.MemberType
                        // or handler is an interface of message
                    || (o.MemberType.GetTypeInfo().IsAssignableFrom(export.MemberType.GetTypeInfo()))
                        // support for GetAll
                        // ReSharper disable once PossibleNullReferenceException
                    || (o.MemberType.IsArray && o.MemberType.GetElementType().GetTypeInfo().IsAssignableFrom(export.MemberType.GetTypeInfo()))
                    || (o.MemberType.GetTypeInfo().IsGenericType && o.MemberType.GetTypeInfo().GenericTypeArguments.First().GetTypeInfo().IsAssignableFrom(export.MemberType.GetTypeInfo())));
#else
            return
                Subscriptions.Where(o =>
                    // is message type 
                    o.MemberType == export.MemberType
                        // or handler is an interface of message
                    || (o.MemberType.IsAssignableFrom(export.MemberType))
                        // support for GetAll
                        // ReSharper disable once PossibleNullReferenceException
                    || (o.MemberType.IsArray && o.MemberType.GetElementType().IsAssignableFrom(export.MemberType))
                    || (o.MemberType.IsGenericType && o.MemberType.GetGenericArguments().First().IsAssignableFrom(export.MemberType)));
#endif
        }

        #endregion

        #region ctor

        static Injector()
        {
            InjectorInitialized.LoadServices();
        }


        /// <summary>
        /// Initializes the injector. 
        /// </summary>
        public static void ConfirmInit()
        {

        }


        #endregion

        #region exporting
        /// <summary>
        /// Adds the instance to the export container.
        /// Will publish to pending imports
        /// </summary>
        /// <param name="instance">The instance to add</param>
        public static void AddExport(object instance)
        {
            // add as self
            var type = instance.GetType();

#if UNITY_WSA && !UNITY_EDITOR
            if (type.IsGenericParameter || type.IsArray)
#else
            if (type.IsGenericType || type.IsArray)
#endif
            {
                Debug.LogError("Generics and Arrays are not valid exports. Export individually or a container.");
                return;
            }

            if (Exports.Any(o => o.Instance == instance))
                Debug.LogWarning("Export is being added multiple times ! " + instance.GetType());

            var key = type.GetAttribute<ExportAttribute>();

            // add to container
            var e = new InjectExport(type, instance, key == null ? null : key.InjectKey);

            Exports.Add(e);

            // notify imports
            foreach (var sub in GetSubscriptionsFor(e))
            {
                Import(sub.Instance, sub.Member, sub.InjectKey);
            }
        }

        /// <summary>
        /// Removes the instance to the container.
        /// Will publish changes to imports
        /// </summary>
        /// <param name="instance">The instance to remove</param>
        public static void RemoveExport(object instance)
        {
            // get exports
            var es = Exports.Where(o => o.Instance == instance).ToArray();

            //remove
            Exports.RemoveAll(o => o.Instance == instance);

            // clean up publish
            for (int index = 0;index < es.Length;index++)
            {
                var export = es[index];

                // notify imports
                foreach (var sub in GetSubscriptionsFor(export))
                {
                    Clear(sub.Instance, sub.Member);
                }
            }
        }

        #endregion

        #region importing

        /// <summary>
        /// Will import to member fields/properties with Import Attribute
        /// </summary>
        /// <param name="instance"></param>
        public static void Import(object instance)
        {
#if UNITY_WSA && !UNITY_EDITOR
            var fields = instance.GetType().GetTypeInfo().DeclaredFields.Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
            var props = instance.GetType().GetTypeInfo().DeclaredProperties.Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
#else
            var fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
            var props = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
#endif
            for (int i = 0;i < fields.Length;i++)
            {
                Import(instance, fields[i], fields[i].GetAttribute<ImportAttribute>().InjectKey);
            }
            for (int i = 0;i < props.Length;i++)
            {
                Import(instance, props[i], props[i].GetAttribute<ImportAttribute>().InjectKey);
            }
        }

        /// <summary>
        /// Will inject instance into member
        /// </summary>
        static void Import(object instance, MemberInfo member, string key)
        {
            // get member type
            var memberType = member.GetMemberType();

            if (memberType.IsArray)
            {
                // get array type
                var type = memberType.GetElementType();

                // ReSharper disable once AssignNullToNotNullAttribute
                var arg = GetExports(type, key).Select(o => ConvertTo(o.Instance, type)).ToArray();

                // ReSharper disable once AssignNullToNotNullAttribute
                var a = Array.CreateInstance(type, arg.Length);

                Array.Copy(arg, a, arg.Length);

                member.SetMemberValue(instance, a);

            }
#if UNITY_WSA && !UNITY_EDITOR
            else if (memberType.GetTypeInfo().IsGenericType)
#else
            else if (memberType.IsGenericType)
#endif
            {
#if UNITY_WSA && !UNITY_EDITOR
                var type = memberType.GetTypeInfo().GenericTypeArguments.First();
#else
                var type = memberType.GetGenericArguments().First();
#endif
                //get enumerable generic type
#if UNITY_WSA && !UNITY_EDITOR
                if (memberType.GetTypeInfo().IsInterface)
#else
                if (memberType.IsInterface)
#endif
                {
                    var arg = GetExports(type, key).Select(o => ConvertTo(o.Instance, type)).ToArray();

                    var a = Array.CreateInstance(type, arg.Length);

                    Array.Copy(arg, a, arg.Length);

                    member.SetMemberValue(instance, a);
                }
                else
                {
                    Debug.LogError("Injecting into non-Array / non-IEnumerable not supported.");
                }
            }
            else
            {
                var arg = GetExports(memberType, key).FirstOrDefault();

                if (arg != null)
                    member.SetMemberValue(instance, arg.Instance);
            }
        }

        static object ConvertTo(object instance, Type type)
        {
#if UNITY_WSA && !UNITY_EDITOR
            if (type.GetTypeInfo().IsInterface)
#else
            if (type.IsInterface)
#endif
            {
                return instance;
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            return Convert.ChangeType(instance, type);

        }

        /// <summary>
        /// Will set value to null
        /// </summary>
        public static void Clear(object instance, MemberInfo member)
        {
            member.SetMemberValue(instance, null);
        }
        #endregion

        #region Resolving

        /// <summary>
        /// Returns true if the container contains any T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasExport<T>()
        {
            var t = typeof(T);

            return HasExport(t);
        }

        /// <summary>
        /// Returns true if the container contains any T
        /// </summary>
        /// <returns></returns>
        public static bool HasExport(Type t)
        {
#if UNITY_WSA && !UNITY_EDITOR
            return Exports.Any(o => o.MemberType == t || (o.MemberType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())));
#else
            return Exports.Any(o => o.MemberType == t || (t.IsAssignableFrom(o.MemberType)));
#endif
        }

        /// <summary>
        /// returns the first instance of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFirst<T>()
        {
            return (T)GetFirst(typeof(T));
        }

        /// <summary>
        /// returns the first instance of T
        /// </summary>
        /// <returns></returns>
        public static object GetFirst(Type t)
        {
            var es = GetExports(t, null).ToArray();

            if (es.Count() > 1)
            {
                Debug.LogWarning("Multiple exports of type : " + t);
            }

            if (!es.Any())
            {
                return null;
            }

            return es.First().Instance;
        }

        /// <summary>
        /// Returns all instances of T
        /// </summary>
        ///  <returns></returns>
        public static IEnumerable<T> GetAll<T>()
        {
            return GetAll(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Returns all instances of T
        /// </summary>
        ///  <returns></returns>
        public static IEnumerable<object> GetAll(Type t)
        {
            return GetExports(t, null).Select(o => o.Instance).ToArray();
        }

        /// <summary>
        /// Returns true if the container contains any T
        /// </summary>
        /// <returns></returns>
        public static bool HasExport(string key)
        {
            return Exports.Any(o => o.HasKey && o.InjectKey == key);
        }

        /// <summary>
        /// returns the first instance of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFirst<T>(string key)
        {
            return (T)GetFirst(key);
        }

        /// <summary>
        /// returns the first instance of T
        /// </summary>
        /// <returns></returns>
        public static object GetFirst(string key)
        {
            var es = Exports.Where(o => o.HasKey && o.InjectKey == key).ToArray();

            if (es.Count() > 1)
            {
                Debug.LogWarning("Multiple exports of key : " + key);
            }

            if (!es.Any())
            {
                return null;
            }

            return es.First().Instance;
        }

        /// <summary>
        /// Returns all instances of T
        /// </summary>
        ///  <returns></returns>
        public static IEnumerable<T> GetAll<T>(string key)
        {
            return GetAll(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Returns all instances of T
        /// </summary>
        ///  <returns></returns>
        public static IEnumerable<object> GetAll(string key)
        {
            return Exports.Where(o => o.HasKey && o.InjectKey == key).Select(o => o.Instance).ToArray();
        }
        #endregion

        #region subscribing
        /// <summary>
        /// Will subscribe members for late injection
        /// </summary>
        /// <param name="instance"></param>
        public static void Subscribe(object instance)
        {
#if UNITY_WSA && !UNITY_EDITOR
            var fields = instance.GetType().GetRuntimeFields().Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
            var props = instance.GetType().GetRuntimeProperties().Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
#else
            var fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
            var props = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
#endif
            for (int i = 0;i < fields.Length;i++)
            {
                Subscribe(instance, fields[i]);
            }
            for (int i = 0;i < props.Length;i++)
            {
                Subscribe(instance, props[i]);
            }
        }

        /// <summary>
        /// Will subscribe instance into member
        /// </summary>
        public static void Subscribe(object instance, MemberInfo member)
        {
            //Import First
            var key = member.GetAttribute<ImportAttribute>();

            Import(instance, member, key == null ? null : key.InjectKey);

            var a = member.GetCustomAttributes(typeof(ImportAttribute), true).Cast<ImportAttribute>().FirstOrDefault();

            // add subscription
            Subscriptions.Add(new InjectSubscription(member.GetMemberType(), instance, member, a == null ? null : a.InjectKey));
        }

        /// <summary>
        /// Will unsubscribe from late injection
        /// </summary>
        /// <param name="instance"></param>
        public static void UnSubscribe(object instance)
        {
#if UNITY_WSA && !UNITY_EDITOR
            var fields = instance.GetType().GetRuntimeFields().Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
            var props = instance.GetType().GetRuntimeProperties().Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
#else
            var fields = instance.GetType().GetFields().Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
            var props = instance.GetType().GetProperties().Where(o => o.HasAttribute<ImportAttribute>()).ToArray();
#endif
            for (int i = 0;i < fields.Length;i++)
            {
                fields[i].SetValue(instance, null);
            }
            for (int i = 0;i < props.Length;i++)
            {
                props[i].SetValue(instance, null, null);
            }

            Subscriptions.RemoveAll(o => o.Instance == instance);
        }

        /// <summary>
        /// Will remove instance member subscription
        /// </summary>
        public static void UnSubscribe(object instance, MemberInfo member)
        {
            //Remove Imports First
            Clear(instance, member);

            //remove
            Subscriptions.RemoveAll(o => o.Instance == instance && Equals(o.Member, member));
        }

        #endregion
    }
}