// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    ///     UIToggle to bool or string. Use 'StaticValue' to pass an arbitrary string value if true.
    /// </summary>
    [RequireComponent(typeof (Toggle))]
    [AddComponentMenu("Foundation/Databinding/ToggleBinder")]
    public class ToggleBinder : BindingBase
    {
        public AudioClip ClickSound;

        [HideInInspector] public BindingInfo EnabledBinding = new BindingInfo
        {
            BindingName = "Enabled"
        };

        protected bool IsInit;
        protected float NextSound;
        public float SoundLag = .5f;

        /// <summary>
        ///     Value to pass if ture
        /// </summary>
        /// <remarks>
        ///     For group situations
        /// </remarks>
        public string StaticValue;

        [HideInInspector] public BindingInfo StaticValueBinding = new BindingInfo
        {
            BindingName = "StaticValue"
        };

        protected Toggle Target;

        /// <summary>
        ///     Will pass a static value when true
        /// </summary>
        /// <remarks>
        ///     For group situations
        /// </remarks>
        public bool UseStaticValue;

        [HideInInspector] public BindingInfo ValueBinding = new BindingInfo
        {
            BindingName = "Value"
        };

        private void Awake()
        {
            Init();
        }

        private void Call(bool value)
        {
            if (UseStaticValue)
            {
                if (value)
                {
                    if (ClickSound)
                    {
                        if (NextSound < Time.time)
                        {
                            Audio2DListener.PlayUI(ClickSound, 1);
                            NextSound = Time.time + SoundLag;
                        }
                    }

                    SetValue(StaticValueBinding.MemberName, StaticValue);
                }
            }
            else
            {
                if (ClickSound)
                {
                    if (NextSound < Time.time)
                    {
                        Audio2DListener.PlayUI(ClickSound, 1);
                        NextSound = Time.time + SoundLag;
                    }
                }

                SetValue(ValueBinding.MemberName, value);
            }
        }

        private void UpdateValue(object o)
        {
            if (UseStaticValue)
                return;

            Target.isOn = (bool) o;
        }

        private void UpdateState(object arg)
        {
            Target.interactable = (bool) arg;
        }

        public override void Init()
        {
            if (IsInit)
                return;
            IsInit = true;

            NextSound = Time.time + SoundLag;

            ValueBinding.Filters = BindingFilter.Properties;
            ValueBinding.FilterTypes = new[] {typeof (bool)};
            ValueBinding.ShouldShow = () => !UseStaticValue;
            ValueBinding.Action = UpdateValue;

            StaticValueBinding.Filters = BindingFilter.Properties;
            StaticValueBinding.ShouldShow = () => UseStaticValue;

            EnabledBinding.Action = UpdateState;
            EnabledBinding.Filters = BindingFilter.Properties;
            EnabledBinding.FilterTypes = new[] {typeof (bool)};

            Target = GetComponent<Toggle>();
            Target.onValueChanged.AddListener(Call);
        }
    }
}