// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Desktopkeyboard input to command
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/KeyBinder")]
    public class KeyBinder : BindingBase
    {
        public enum PlatformEnum
        {
            All,
            Mobile,
            Desktop
        }

        [HideInInspector] public BindingInfo CommandBinding = new BindingInfo {BindingName = "Command"};

        public KeyCode Key;
        protected float lastHit;
        public PlatformEnum Platform;
        public bool RequireDouble = false;

        private void Awake()
        {
            Init();

            enabled = enabled && ValidPlatform();
        }

        private bool ValidPlatform()
        {
            switch (Platform)
            {
                case PlatformEnum.Mobile:
                    return Application.isMobilePlatform;
                case PlatformEnum.Desktop:
                    return !Application.isMobilePlatform;
                default:
                    return true;
            }
        }

        public void Call()
        {
            SetValue(CommandBinding.MemberName, null);
        }

        public override void Init()
        {
            CommandBinding.Filters = BindingFilter.Commands;
        }

        private void Update()
        {
            if (Input.GetKeyUp(Key))
            {
                if (RequireDouble)
                {
                    if (lastHit + .2f > Time.time)
                    {
                        Call();
                        lastHit = 0;
                    }
                    lastHit = Time.time;
                }
                else
                {
                    Call();
                }
            }
        }
    }
}