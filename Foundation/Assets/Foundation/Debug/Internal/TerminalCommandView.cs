// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Debuging.Internal
{
    /// <summary>
    /// Presentation script for a Terminal Button Item
    /// </summary>
    public class TerminalCommandView : MonoBehaviour
    {
        public TerminalCommand Model { get; set; }

        public Text Label;

        public Action Handler;

        public void OnClick()
        {
            if (Handler != null)
                Handler();
        }

    }
}