// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;

namespace Foundation.Localization
{
    /// <summary>
    /// Place this on a class field / property that you wish to localize and call the LocalizationService.LocalizeObject() method.
    /// The string will be localized for you.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LocalizedAttribute : Attribute
    {
        /// <summary>
        /// Optional Lookup Group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The Key to import
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Set internally
        /// </summary>
        public string FallbackValue { get; set; }

        public LocalizedAttribute(string key)
        {
            Key = key;
        }


        public LocalizedAttribute(string key, string group)
        {
            Group = group;
            Key = key;
        }
    }
}