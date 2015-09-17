using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Foundation.Databinding;
using UnityEngine;

namespace Foundation.Editor
{
    public static class EditorMembersHelper
    {
        public static MemberInfo[] GetProperties(Type type)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;

            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface))
                            continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(flags)
                        .Where(o => !o.HasAttribute<HideInInspector>())
                        .Where(o => !o.Module.Name.Contains("UnityEngine"))
                        .OrderBy(o => o.Name);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }
            else
            {
                var propertyInfos = new List<MemberInfo>();
                var props =
                    type.GetProperties(flags)
                        .Where(o => !o.HasAttribute<HideInInspector>() && !o.Module.Name.Contains("UnityEngine"))
                        .OrderBy(o => o.Name);
                foreach (var prop in props)
                {
                    if (prop.IsSpecialName)
                        continue;

                    propertyInfos.Add(prop);
                }
                var fields =
                    type.GetFields(flags)
                        .Where(o => !o.HasAttribute<HideInInspector>() && !o.Module.Name.Contains("UnityEngine"))
                        .OrderBy(o => o.Name);
                foreach (var prop in fields)
                {
                    propertyInfos.Add(prop);
                }
                return propertyInfos.ToArray();
            }
        }

        public static MethodInfo[] GetMethods(Type type)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod;

            var ms = type.GetMethods(flags)
                .Where(o => !o.IsSpecialName)
                .Where(o => !o.HasAttribute<HideInInspector>())
                .Where(o => !o.Module.Name.Contains("UnityEngine"))
                .Where(o => o.GetParameters().Length < 2)
                .Where(o => o.ReturnType == typeof (void) || o.ReturnType == typeof (IEnumerator))
                .OrderBy(o => o.Name)
                .ToArray();

            return ms;
        }
    }
}