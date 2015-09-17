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
    ///     Button Click to Command. Invoke a method or run a coroutine !
    /// </summary>
    [RequireComponent(typeof (Button))]
    [AddComponentMenu("Foundation/Databinding/ButtonBinder")]
    public class ButtonBinder : BindingBase
    {
        //1) Local Dependencies (Our button and the param extension)
        protected Button Button;
        //2) Define a BindingInfo. These are found via reflection so
        //	 you can add as many as you like.Define t

        [HideInInspector] public BindingInfo EnabledBinding = new BindingInfo
        {
            // Inspecter Name
            BindingName = "Enabled",
            // Properties are two way
            Filters = BindingFilter.Properties,
            // Filters Model Properties By Type
            FilterTypes = new[] {typeof (bool)}
        };

        protected bool IsInit;

        [HideInInspector] public BindingInfo OnClickBinding = new BindingInfo
        {
            // Inspecter Name
            BindingName = "OnClick",
            // Commands are One way from the view
            Filters = BindingFilter.Commands
        };

        protected ButtonParamater Paramater;

        private void Awake()
        {
            Init();
        }

        public override void Init()
        {
            if (IsInit)
                return;

            IsInit = true;

            // Grab dependencies
            Paramater = GetComponent<ButtonParamater>();
            Button = GetComponent<Button>();

            // Listen to button clicks
            Button.onClick.AddListener(Call);

            // Handle the Model.Enabled Change
            EnabledBinding.Action = UpdateState;
        }

        public void Call()
        {
            // if button is disabled, no
            if (!Button.IsInteractable())
                return;
            // SetValue is the method for most View->Model value setting.
            // SetValue should work for all member types.
            SetValue(OnClickBinding.MemberName, Paramater == null ? null : Paramater.GetValue());
        }

        private void UpdateState(object arg)
        {
            // Disable The Button
            Button.interactable = (bool) arg;
        }
    }
}