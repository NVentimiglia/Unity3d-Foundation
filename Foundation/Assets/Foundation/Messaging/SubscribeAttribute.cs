// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;

namespace Foundation
{
    /// <summary>
    /// Attribute for identifying method handlers for automatic Messenger subscription
    /// </summary> 
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscribeAttribute : Attribute
    {
    }
}
