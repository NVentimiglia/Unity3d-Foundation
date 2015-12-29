// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Foundation.Server
{

    /// <summary>
    /// Defines the protection level for a Storage Object
    /// </summary>
    public enum StorageACL
    {
        Public,
        User,
        Admin,
    }

    /// <summary>
    /// Decorate a class to make it usable by the storage system
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class StorageTable : Attribute
    {
        public string TableName { get; protected set; }

        public StorageTable(string tableName)
        {
            TableName = tableName;
        }
    }

    /// <summary>
    /// Decorate your unique id. This should be a GUID string
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StorageIdentity : Attribute
    {

    }

    /// <summary>
    /// Optional, OrderBy int Property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StorageScore : Attribute
    {

    }

    /// <summary>
    /// Optional, ModifiedOn DateTime Field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StorageModifiedOn : Attribute
    {

    }

    /// <summary>
    /// Utility class for serialization of clr objects using reflection
    /// </summary>
    public class StorageMetadata
    {
        #region static
        protected static Dictionary<Type, StorageMetadata> KnowenReflections = new Dictionary<Type, StorageMetadata>();

        /// <summary>
        /// Registers a type using Annotations 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterType<T>() where T : class
        {
            var type = typeof(T);

            if (KnowenReflections.ContainsKey(type))
                return;

#if UNITY_WSA && !UNITY_EDITOR
            var typeInfo = type.GetTypeInfo();
            var table = typeInfo.GetCustomAttribute<StorageTable>();
#else
            var table = (StorageTable)Attribute.GetCustomAttribute(type, typeof(StorageTable));
#endif
            if (table == null)
                throw new Exception("StorageTable Attribute is required");

            var meta = new StorageMetadata
            {
                TableName = table.TableName,
                ObjectType = type,
#if UNITY_WSA && !UNITY_EDITOR
                ObjectTypeInfo = typeInfo,
#endif
            };

#if UNITY_WSA && !UNITY_EDITOR
            foreach (var prop in typeInfo.DeclaredProperties)
#else
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
#endif
            {
                if (string.IsNullOrEmpty(meta.IdPropertyName))
                {
                    if (HasAttribute<StorageIdentity>(prop))
                    {
                        if (prop.PropertyType != typeof(string))
                            Debug.LogError("StorageIdentity must be string");

                        meta.IdPropertyName = prop.Name;
                    }
                }
                else if (string.IsNullOrEmpty(meta.ScorePropertyName))
                {
                    if (HasAttribute<StorageScore>(prop))
                    {
                        if (prop.PropertyType != typeof(int) &&
                            prop.PropertyType != typeof(uint) &&
                            prop.PropertyType != typeof(short) &&
                            prop.PropertyType != typeof(double) &&
                            prop.PropertyType != typeof(float))
                            Debug.LogError("StorageIdentity must be a number");

                        meta.ScorePropertyName = prop.Name;

                    }
                }
                else if (string.IsNullOrEmpty(meta.ModifiedPropertyName))
                {
                    if (HasAttribute<StorageModifiedOn>(prop))
                    {
                        if (prop.PropertyType != typeof(DateTime))
                            Debug.LogError("StorageModifiedOn must be a DateTime");

                        meta.ModifiedPropertyName = prop.Name;

                    }
                }
                else
                    break;
            }

            if (string.IsNullOrEmpty(meta.IdPropertyName))
                throw new Exception("StorageIdentity Attribute is required");

            KnowenReflections.Add(type, meta);

        }

        /// <summary>
        /// Registers a type without annotations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="meta"></param>
        public static void RegisterType<T>(StorageMetadata meta) where T : class
        {
            var type = typeof(T);

            if (string.IsNullOrEmpty(meta.IdPropertyName))
                throw new Exception("StorageIdentity Attribute is required");

            if (string.IsNullOrEmpty(meta.TableName))
                throw new Exception("StorageTable Attribute is required");

            if (KnowenReflections.ContainsKey(type))
                return;

            KnowenReflections.Add(type, meta);
        }

        /// <summary>
        /// Returns the metadata
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static StorageMetadata GetMetadata<T>() where T : class
        {

            RegisterType<T>();

            return KnowenReflections[typeof(T)];
        }

        #endregion

        #region fields
        public Type ObjectType;
#if UNITY_WSA && !UNITY_EDITOR
        public TypeInfo ObjectTypeInfo;
#endif
        public string TableName;
        public string IdPropertyName;
        public string ModifiedPropertyName;
        public string ScorePropertyName;
        #endregion

        #region methods
        public PropertyInfo GetProperty(string name)
        {
#if UNITY_WSA && !UNITY_EDITOR
            return ObjectTypeInfo.DeclaredProperties.First(o => o.Name == name);
#else
            return ObjectType.GetProperty(name);
#endif
        }

        public string GetId(object instance)
        {
            return (string)GetProperty(IdPropertyName).GetValue(instance, null);
        }


        public DateTime GetModified(object instance)
        {
            if (string.IsNullOrEmpty(ModifiedPropertyName))
                return DateTime.UtcNow;

            return (DateTime)GetProperty(ModifiedPropertyName).GetValue(instance, null);
        }

        public string GetScore(object instance)
        {
            if (string.IsNullOrEmpty(ScorePropertyName))
                return "0";

            return GetProperty(ScorePropertyName).GetValue(instance, null).ToString();
        }

        public static bool HasAttribute<T>(PropertyInfo info) where T : Attribute
        {
#if UNITY_WSA && !UNITY_EDITOR
            return  info.CustomAttributes.Any(o => o.AttributeType.Equals(typeof (T)));
#else
            return Attribute.IsDefined(info, typeof(T));
#endif
        }

        #endregion
    }
}
