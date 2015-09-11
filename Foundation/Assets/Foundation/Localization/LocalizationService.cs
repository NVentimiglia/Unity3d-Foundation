// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Foundation.Localization
{
    /// <summary>
    /// Language Translation Service.
    /// 
    /// Features :
    /// - Partitions languages by sub folder
    /// - Reads from csv. (key, value\n)
    /// - Support for multiple files for logical partitioning of 'keys'
    /// - Win32 Translator and converter available on the website avariceonline.com
    /// - Support for fall-back value if not found
    /// - White list code variables with annotation and use GetForMembers() to automatically resolve content 
    /// Use :
    /// Add LocalizationService script to scene
    /// Subscribe to LocalizationService as a BindingMessage for language change notification
    /// Or import via IOC
    /// </summary>
    public class LocalizationService : ScriptableObject
    {
        #region static
        private static LocalizationService _instance;
        public static LocalizationService Instance
        {
            get { return _instance ?? (_instance = Create()); }
        }

        static LocalizationService Create()
        {
            return Resources.Load<LocalizationService>("LocalizationService");
        }

        #endregion

        #region event
        /// <summary>
        /// Raised when the language dictionary changes
        /// </summary>
        public static event Action<LocalizationService> OnLanguageChanged;
        #endregion

        #region properties
        /// <summary>
        /// Dictionary of all loaded strings partitioned by file
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> StringsByFile { get; set; }

        /// <summary>
        /// Dictionary of all loaded strings
        /// </summary>
        public Dictionary<string, string> Strings { get; set; }

        /// <summary>
        /// Listing of all files
        /// </summary>
        public List<string> Files { get; set; }

        /// <summary>
        /// Default
        /// </summary>
        [HideInInspector]
        public LanguageInfo DefaultLanguage = LanguageInfo.English;

        /// <summary>
        /// Current language
        /// </summary>
        [HideInInspector]
        private LanguageInfo _language = LanguageInfo.English;
        public LanguageInfo Language
        {
            get { return _language; }
            set
            {
                if (!HasLanguage(value))
                {
                    Debug.LogError("Invalid Language " + value);
                }

                _language = value;

                RaiseLanguageChanged();

                SaveToPrefs();
            }
        }

        /// <summary>
        /// all supported languages
        /// </summary>
        [SerializeField]
        public LanguageInfo[] Languages = LanguageInfo.All;
        #endregion

        #region LocalizationService

        public void OnEnable()
        {
            LoadLanguage();

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            ReadFiles();
        }

        void LoadLanguage()
        {
            var raw = PlayerPrefs.GetString("LocalizationService.Current");
            if (string.IsNullOrEmpty(raw))
            {
                Language = DefaultLanguage;
                return;
            }


            var lan = Languages.FirstOrDefault(o => o.Abbreviation == raw);
            if (lan == null)
            {
                Debug.LogError("Unknown language saved to prefs : " + raw);
                Language = DefaultLanguage;
                SaveToPrefs();
            }
            else
            {
                Language = lan;
            }
        }

        void SaveToPrefs()
        {
            PlayerPrefs.SetString("LocalizationService.Current", Language.Abbreviation);
            PlayerPrefs.Save();
        }

        #endregion

        #region internal

        /// <summary>
        /// Contains() crashes on IOS
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        bool HasLanguage(LanguageInfo language)
        {
            foreach (var systemLanguage in Languages)
            {
                if (systemLanguage.Equals(language))
                    return true;
            }
            return false;
        }

        private void RaiseLanguageChanged()
        {
            ReadFiles();

            //notify of change
            if (Application.isPlaying)
            {
                if (OnLanguageChanged != null)
                    OnLanguageChanged(this);
            }
        }

        void ReadFiles()
        {
            Strings = new Dictionary<string, string>();
            StringsByFile = new Dictionary<string, Dictionary<string, string>>();
            Files = new List<string>();

            var path = "Localization/" + Language.Name + "/";

            var resources = Resources.LoadAll<TextAsset>(path);

            if (!resources.Any())
            {
                Debug.LogError("Localization Files Not Found : " + Language.Name);
            }


            foreach (var resource in resources)
            {
                ReadCSVAsset(resource);
            }
        }

        void ReadCSVAsset(TextAsset resource)
        {
            try
            {
                Files.Add(resource.name);
                StringsByFile.Add(resource.name, new Dictionary<string, string>());

                var rows = CsvReader.ReadCSV(resource.text);

                foreach (var row in rows)
                {
                    var key = row[0];
                    var value = row[1].TrimEnd('\r');

                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.TrimEnd('"').TrimStart('"');
                    }

                    StringsByFile[resource.name].Add(key, value);

                    if (Strings.ContainsKey(key))
                        Debug.LogWarning("Duplicate string : " + resource + " : " + key);
                    else
                        Strings.Add(key, value);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogError(string.Format("Failed to read file : {0}/{1}  ", Language, resource.name));
            }

        }

        public static T GetAttribute<T>(MemberInfo m) where T : Attribute
        {
            return m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }

        public static bool HasAttribute<T>(MemberInfo m) where T : Attribute
        {
#if UNITY_WSA && !UNITY_EDITOR
            return m.CustomAttributes.Any(o => o.AttributeType == typeof (T));
#else
            return Attribute.IsDefined(m, typeof(T));
#endif
        }
        #endregion

        #region public

        /// <summary>
        /// Reload text assets. Used by editor
        /// </summary>
        public void LoadTextAssets()
        {
            ReadFiles();
        }

        /// <summary>
        /// Gets the string from the dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            return Get(key, string.Empty);
        }

        /// <summary>
        /// Gets the string from the dictionary with a fall-back if not found
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"> if not found</param>
        /// <returns></returns>
        public string Get(string key, string fallback)
        {
            if (!Strings.ContainsKey(key))
            {
                Debug.LogWarning(string.Format("Localization Key Not Found {0} : {1} ", Language.Name, key));
                return fallback;
            }

            return Strings[key];
        }

        /// <summary>
        /// Gets the string from the group dictionary with a fall-back if not found
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetFromFile(string groupId, string key)
        {
            return GetFromFile(groupId, key, string.Empty);
        }

        /// <summary>
        /// Gets all strings per a file
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetFile(string groupId)
        {
            return StringsByFile[groupId];
        }

        /// <summary>
        /// Gets the string from the group dictionary with a fall-back if not found
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="key"></param>
        /// <param name="fallback"> if not found</param>
        /// <returns></returns>
        public string GetFromFile(string groupId, string key, string fallback)
        {
            if (!StringsByFile.ContainsKey(groupId))
            {
                Debug.LogWarning("Localization File Not Found : " + groupId);
                return fallback;
            }

            var group = StringsByFile[groupId];

            if (!group.ContainsKey(key))
            {
                Debug.LogWarning("Localization Key Not Found : " + key);
                return fallback;
            }

            return group[key];
        }

        /// <summary>
        /// Sets localized strings using the Localized attribute
        /// </summary>
        /// <param name="instance"></param>
        public void Localize(object instance)
        {

#if UNITY_WSA && !UNITY_EDITOR
            var typeInfo = instance.GetType().GetTypeInfo();
            var fields = typeInfo.DeclaredFields.Where(o => HasAttribute<LocalizedAttribute>(o));
#else
            var fields = instance.GetType().GetFields().Where(o => Attribute.IsDefined(o, typeof(LocalizableAttribute)));
#endif
            foreach (var member in fields)
            {
                if (member.FieldType != typeof(string))
                {
                    Debug.LogError("ValueType must be string.");
                    continue;
                }

                var attr = GetAttribute<LocalizedAttribute>(member);

                // get key
                var key = attr.Key;
                var group = attr.Group;

                // get fall back value
                if (string.IsNullOrEmpty(attr.FallbackValue))
                {
                    attr.FallbackValue = (string)member.GetValue(instance);
                }

                // query
                var val = string.IsNullOrEmpty(group) ? Get(key, attr.FallbackValue) : GetFromFile(group, key, attr.FallbackValue);

                member.SetValue(instance, val);
            }

#if UNITY_WSA && !UNITY_EDITOR
            var props = typeInfo.DeclaredProperties.Where(o => HasAttribute<LocalizedAttribute>(o));
#else
            var props = instance.GetType().GetProperties().Where(o => HasAttribute<LocalizedAttribute>(o));
#endif
            foreach (var member in props)
            {
                if (member.PropertyType != typeof(string))
                {
                    Debug.LogError("ValueType must be string.");
                    continue;
                }

                var attr = GetAttribute<LocalizedAttribute>(member);

                // get key
                var key = attr.Key;
                var group = attr.Group;

                // get fall back value
                if (string.IsNullOrEmpty(attr.FallbackValue))
                {
                    attr.FallbackValue = (string)member.GetValue(instance, null);
                }

                // query
                var val = string.IsNullOrEmpty(group) ? Get(key, attr.FallbackValue) : GetFromFile(group, key, attr.FallbackValue);

                member.SetValue(instance, val, null);
            }
        }

        /// <summary>
        /// Deletes the Saved Localization key
        /// </summary>
        public void ClearPref()
        {
            PlayerPrefs.DeleteKey("LocalizationService");
            PlayerPrefs.Save();
        }
        #endregion

        #region debug

        [ContextMenu("TestAllLanguages")]
        public void TestAllLanguages()
        {
            foreach (var systemLanguage in Languages)
            {
                ReadFiles();
                Debug.Log(string.Format("Localization Loaded. {0} : {1} files, {2} string", systemLanguage, StringsByFile.Count, Strings.Count));
            }

            ReadFiles();
        }
        #endregion
    }
}