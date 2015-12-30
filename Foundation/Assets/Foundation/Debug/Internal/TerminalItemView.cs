// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Debuging.Internal
{
    /// <summary>
    /// Presentation script for a Terminal Text Item
    /// </summary>
    [AddComponentMenu("Foundation/Terminal/TerminalItemView")]
    public class TerminalItemView : MonoBehaviour
    {
        public TerminalItem Model { get; set; }

        public Text Label;
    }
}