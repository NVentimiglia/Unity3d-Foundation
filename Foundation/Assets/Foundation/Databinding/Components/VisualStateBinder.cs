// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Linq;
using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Sets a GameObject's Visual State (SetActive) by comparing to a property's string value
    /// </summary>
    /// <remarks>
    ///     bool's converted to true / false
    /// </remarks>
    [AddComponentMenu("Foundation/Databinding/VisualStateBinder")]
    public class VisualStateBinder : BindingBase
    {
        protected bool IsInit;
        public GameObject[] Targets;
        public string ValidState = "true";

        [HideInInspector] public BindingInfo ValueBinding = new BindingInfo {BindingName = "State"};

        private void Awake()
        {
            Init();
        }

        private void UpdateState(object arg)
        {
            var valid = arg != null && ValidState.ToLower() == arg.ToString().ToLower();

            foreach (var target in Targets.ToArray())
            {
                target.SetActive(valid);
            }
        }

        public override void Init()
        {
            if (IsInit)
                return;
            IsInit = true;

            ValueBinding.Action = UpdateState;
            ValueBinding.Filters = BindingFilter.Properties;
            ValueBinding.FilterTypes = new[] {typeof (bool), typeof (string), typeof (int), typeof (Enum)};
        }
    }
}