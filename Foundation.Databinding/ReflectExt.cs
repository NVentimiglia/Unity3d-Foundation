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

namespace Foundation.Databinding
{

    /// <summary>
    /// Caches TypeInfo
    /// </summary>
    public class ReflectionCache
    {
        public Type Type;
#if UNITY_WSA
        public TypeInfo TypeInfo;
#endif
        public MemberInfo[] Members;

        public ReflectionCache(Type t)
        {
            Type = t;
#if UNITY_WSA
            TypeInfo = t.GetTypeInfo();  var a1 = t.GetRuntimeFields().ToArray();
            var a2 = t.GetRuntimeProperties().ToArray();
            var a3 = t.GetRuntimeMethods().ToArray();
            var a = new MemberInfo[a1.Length + a2.Length + a3.Length];
            a1.CopyTo(a, 0);
            a2.CopyTo(a, a1.Length);
            a3.CopyTo(a, a1.Length + a2.Length);
            Members = a;
#else
            Members = t.GetMembers().ToArray();
#endif
        }

        public MemberInfo GetMember(string name)
        {
            return Members.FirstOrDefault(o => o.Name == name);
        }

        private static Dictionary<Type, ReflectionCache> Cache = new Dictionary<Type, ReflectionCache>();

        public static ReflectionCache Get<T>()
        {
            return Get(typeof(T));
        }

        public static ReflectionCache Get(Type type)
        {
            if (Cache.ContainsKey(type))
                return Cache[type];

            Cache.Add(type, new ReflectionCache(type));
            return Cache[type];
        }
    }

    public static class ReflectionExt
    {
        /// <summary>
        /// return Attribute.IsDefined(m, typeof(T));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this MemberInfo m) where T : Attribute
        {
#if UNITY_WSA
            return GetAttribute<T>(m) != null;
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
        public static T GetAttribute<T>(this MemberInfo m) where T : Attribute
        {
            return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }

        /// <summary>
        /// return Attribute.IsDefined(m, typeof(T));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttribute<T>(this object m, string memberName) where T : Attribute
        {

            var member = ReflectionCache.Get<T>().GetMember(memberName); 

            if (member == null)
                return null;

            return member.GetAttribute<T>();

        }

        /// <summary>
        /// Returns the Return ValueType of the member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is MethodInfo)
                return ((MethodInfo)member).ReturnType;

            if (member is PropertyInfo)
                return ((PropertyInfo)member).PropertyType;

            return ((FieldInfo)member).FieldType;
        }

        /// <summary>
        /// Returns the Return ValueType of the member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetParamaterType(this MemberInfo member)
        {
            if (member is MethodInfo)
            {
                var p = ((MethodInfo)member).GetParameters().FirstOrDefault();
                if (p == null)
                    return null;
                return p.ParameterType;
            }

            if (member is PropertyInfo)
                return ((PropertyInfo)member).PropertyType;

            if (member is FieldInfo)
                return ((FieldInfo)member).FieldType;

            return null;
        }


        /// <summary>
        /// Set the member's instances value
        /// </summary>
        /// <returns></returns>
        public static void SetMemberValue(this MemberInfo member, object instance, object value)
        {
            if (member is MethodInfo)
            {
                var method = ((MethodInfo)member);

                if (method.GetParameters().Any())
                {
                    method.Invoke(instance, new[] { value });
                }
                else
                {
                    method.Invoke(instance, null);
                }
            }
            else if (member is PropertyInfo)
            {
                ((PropertyInfo)member).SetValue(instance, value, null);
            }
            else
            {
                ((FieldInfo)member).SetValue(instance, value);
            }
        }



        /// <summary>
        /// Set the member's instances value
        /// </summary>
        /// <returns></returns>
        public static object GetMemberValue(this MemberInfo member, object instance)
        {
            if (member is MethodInfo)
                return ((MethodInfo)member).Invoke(instance, null);
            if (member is PropertyInfo)
                return ((PropertyInfo)member).GetValue(instance, null);


            return ((FieldInfo)member).GetValue(instance);

        }

        /// <summary>
        /// Set the member's instances value
        /// </summary>
        /// <returns></returns>
        public static object GetMemberValue(this object instance, string propertyName)
        {
            var member = ReflectionCache.Get(instance.GetType()).GetMember(propertyName); 

            if (member == null)
                return null;

            if (member is MethodInfo)
                return ((MethodInfo)member).Invoke(instance, null);
            if (member is PropertyInfo)
                return ((PropertyInfo)member).GetValue(instance, null);

            return ((FieldInfo)member).GetValue(instance);

        }


        /// <summary>
        /// Set the member's instances value
        /// </summary>
        /// <returns></returns>
        public static T GetMemberValue<T>(this MemberInfo member, object instance)
        {
            return (T)GetMemberValue(member, instance);

        }

        /// <summary>
        /// Get all Members
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static MemberInfo[] GetRuntimeMembers(this Type t)
        {
            return ReflectionCache.Get(t).Members;
        }

        /// <summary>
        /// Get Member
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MemberInfo GetRuntimeMember(this Type t, string name)
        {
            return ReflectionCache.Get(t).GetMember(name);
        }

        /// <summary>
        /// WSA support
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsEnum(this Type t)
        {
#if UNITY_WSA
            return ReflectionCache.Get(t).TypeInfo.IsEnum;
#else
            return t.IsEnum;
#endif
        }

        /// <summary>
        /// WSA support
        /// </summary>
        /// <returns></returns>
        public static bool IsAssignable(this Type desiredType, object param)
        {
#if UNITY_WSA
            var d = ReflectionCache.Get(desiredType).TypeInfo;
            var i = ReflectionCache.Get(param.GetType()).TypeInfo;
            return d.IsAssignableFrom(i);
#else
            return desiredType.IsInstanceOfType(param);
#endif
        }
    }
}