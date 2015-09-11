using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Foundation.Ioc;
using UnityEditor;
using UnityEngine;

namespace Foundation.Core.Editor
{
    /// <summary>
    /// Tool for instantiating scriptable object services initialized by the injector
    /// </summary>
    public class InstantiateScriptableObjects
    {
        [MenuItem("Tools/Foundation/Instantiate ScriptableObjects")]
        public static void ShowWindow()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(o => HasAttribute<InjectorInitialized>(o))).ToArray();
            
            foreach (var type in types)
            {
                if (!typeof(ScriptableObject).IsAssignableFrom(type))
                    continue;

                var deco = GetAttribute<InjectorInitialized>(type);

                if (deco.AbortLoad)
                    continue;

                // note Object has the responsibility of exporting itself to the injector
                var resource = Resources.Load(deco.ResourceName);

                // already an instance
                if (resource != null)
                    continue;

                var path = Application.dataPath + "/Resources";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var asset = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(asset, "Assets/Resources/"+deco.ResourceName+".asset");
                AssetDatabase.SaveAssets();

                Debug.Log(type.Name + " Created");
            }
        }

        /// <summary>
        /// return Attribute.IsDefined(m, typeof(T));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        static bool HasAttribute<T>(MemberInfo m) where T : Attribute
        {
            return Attribute.IsDefined(m, typeof(T));
        }

        /// <summary>
        ///  return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        static T GetAttribute<T>(MemberInfo m) where T : Attribute
        {
            return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        } 
    }
}