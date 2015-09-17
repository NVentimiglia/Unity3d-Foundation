// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using System.Linq;
using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Sets a GameObject's Visual State (SetActive) by comparing to a property's bool value
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/VisibilityBinder")]
    public class VisibilityBinder : BindingBase
    {
        public GameObject[] InverseTargets;
        protected bool IsInit;
        public GameObject[] Targets;

        [HideInInspector] public BindingInfo ValueBinding = new BindingInfo
        {
            Filters = BindingFilter.Properties,
            FilterTypes = new[] {typeof (bool)},
            BindingName = "Value"
        };

        public bool WaitFrame;

        private void Awake()
        {
            Init();
        }

        [ContextMenu("DoDebug")]
        public void DoDebug()
        {
            Debug.Log(ValueBinding.BindingName);
        }

        private void UpdateState(object arg)
        {
            var b = arg != null && (bool) arg;

            if (WaitFrame)
            {
                StartCoroutine(UpdateAsync(b));
                return;
            }

            foreach (var target in Targets.ToArray())
            {
                if (target)
                    target.SetActive(b);
            }

            foreach (var target in InverseTargets.ToArray())
            {
                if (target)
                    target.SetActive(!b);
            }
        }

        private IEnumerator UpdateAsync(bool b)
        {
            yield return 1;

            foreach (var target in Targets.ToArray())
            {
                if (target)
                    target.SetActive(b);
            }

            foreach (var target in InverseTargets.ToArray())
            {
                if (target)
                    target.SetActive(!b);
            }
        }

        public override void Init()
        {
            if (IsInit)
                return;
            IsInit = true;

            ValueBinding.Action = UpdateState;
        }
    }
}