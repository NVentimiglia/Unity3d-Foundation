// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Bind a buttons 'is pressed' state to a bool property on your model
    /// </summary>
    [RequireComponent(typeof (Button))]
    [AddComponentMenu("Foundation/Databinding/ButtonValueBinder")]
    public class ButtonValueBinder : BindingBase, IPointerDownHandler, IPointerUpHandler
    {
        protected Button Button;

        [HideInInspector] public BindingInfo EnabledBinding = new BindingInfo {BindingName = "Enabled"};

        protected bool IsInit;

        [HideInInspector] public BindingInfo IsPressedBinding = new BindingInfo {BindingName = "IsPressed"};

        protected ButtonParamater Paramater;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Button.interactable)
                SetValue(IsPressedBinding.MemberName, true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Button.interactable)
                SetValue(IsPressedBinding.MemberName, false);
        }

        private void Awake()
        {
            Init();
        }

        private void UpdateState(object arg)
        {
            Button.interactable = (bool) arg;
        }

        public override void Init()
        {
            if (IsInit)
                return;
            IsInit = true;

            Paramater = GetComponent<ButtonParamater>();
            Button = GetComponent<Button>();

            IsPressedBinding.Filters = BindingFilter.Properties;
            IsPressedBinding.FilterTypes = new[] {typeof (bool)};

            EnabledBinding.Action = UpdateState;
            EnabledBinding.Filters = BindingFilter.Properties;
            EnabledBinding.FilterTypes = new[] {typeof (bool)};
        }
    }
}