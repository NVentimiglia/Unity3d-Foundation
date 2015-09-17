// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Sets a GameObject's Visual State (SetActive) by comparing to a property's bool value
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/AnimatedVisibilityBinder")]
    public class AnimatedVisibilityBinder : BindingBase
    {
        public Animation[] Animators;
        public AnimationClip CloseClip;
        public bool DeactivateObject = true;
        public bool Inverse;
        protected bool IsInit;
        public AnimationClip OpenClip;

        [HideInInspector] public BindingInfo ValueBinding = new BindingInfo
        {
            Filters = BindingFilter.Properties,
            FilterTypes = new[] {typeof (bool)},
            BindingName = "Value"
        };

        private void Awake()
        {
            Init();
        }

        public override void Init()
        {
            if (IsInit)
                return;
            IsInit = true;

            ValueBinding.Action = UpdateState;
        }

        public void UpdateState(object arg)
        {
            var b = arg != null && (bool) arg;

            StopAllCoroutines();

            foreach (var target in Animators)
            {
                target.Stop();
            }

            if (Inverse ? !b : b)
                OpenAsync();
            else
                CloseAsync();
        }

        public void OpenAsync()
        {
            foreach (var target in Animators)
            {
                if (!target)
                    continue;

                target.gameObject.SetActive(true);

                if (OpenClip)
                {
                    target.clip = OpenClip;
                    target.Play();
                }
            }
        }

        public void CloseAsync()
        {
            foreach (var target in Animators)
            {
                if (!target)
                    continue;

                if (CloseClip)
                {
                    target.clip = CloseClip;
                    target.Play();
                    // Deactivate object when done !
                    if (DeactivateObject)
                        StartCoroutine(DeactivateViewAsync(target, CloseClip));
                }
                else
                {
                    target.gameObject.SetActive(false);
                }
            }
        }

        public IEnumerator DeactivateViewAsync(Animation target, AnimationClip clip)
        {
            yield return new WaitForSeconds(clip.length);
            target.gameObject.SetActive(false);
        }

        [ContextMenu("Add Animations")]
        public void UpdateAnimators()
        {
            foreach (var target in Animators)
            {
                if (!target)
                    continue;

                target.playAutomatically = false;

                target.AddClip(OpenClip, OpenClip.name);
                target.AddClip(CloseClip, CloseClip.name);
            }
        }
    }
}