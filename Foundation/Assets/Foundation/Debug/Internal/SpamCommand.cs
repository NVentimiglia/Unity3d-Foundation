// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Collections;
using UnityEngine;

namespace Foundation.Debuging.Internal
{
    /// <summary>
    /// Test script
    /// </summary>
    [AddComponentMenu("Foundation/Terminal/SpamCommand")]
    public class SpamCommand : MonoBehaviour
    {
        protected void Awake()
        {
            Terminal.Add(new TerminalCommand
            {
                Label = "Spam",
                Method = () => StartCoroutine(DoSpam())
            });
        }

        IEnumerator DoSpam()
        {
            for (int i = 0; i < 50; i++)
            {
                yield return new WaitForSeconds(.1f);

                Terminal.Log(DateTime.UtcNow.ToLongTimeString());
            }
        }
    }
}