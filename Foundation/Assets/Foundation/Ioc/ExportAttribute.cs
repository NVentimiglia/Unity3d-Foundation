// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;

namespace Foundation.Ioc
{
    /// <summary>
    /// Decorate a instance with this to define the export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportAttribute : Attribute
    {
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
        /// Uses standard type match lookup
        /// </summary>
        public ExportAttribute()
        {
            
        }

        /// <summary>
        /// Uses an Optional lookup key
        /// </summary>
        /// <param name="key"></param>
        public ExportAttribute(string key)
        {
            InjectKey = key;
        }
    }
}