// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;

namespace Foundation.Messenging
{
    /// <summary>
    /// Marks the message as cached
    /// </summary> 
    public class CachedMessage : Attribute
    {
        /// <summary>
        /// Only one message of this type should exist.
        /// Setting this to true will clear the cache for this message type.
        /// </summary>
        public bool OnePerType { get; set; }
    }
}