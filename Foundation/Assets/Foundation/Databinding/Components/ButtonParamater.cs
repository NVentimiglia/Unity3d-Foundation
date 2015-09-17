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
    ///     Adds a Paramater to the Button Command
    /// </summary>
    [RequireComponent(typeof (ButtonBinder))]
    [AddComponentMenu("Foundation/Databinding/ButtonParamater")]
    public class ButtonParamater : BindingBase
    {
        public enum ParamaterTypeEnum
        {
            Context,
            Static,
            Binding
        }

        protected bool IsInit;
        public ParamaterTypeEnum ParamaterType;

        [HideInInspector] public BindingInfo ParameterBinding = new BindingInfo {BindingName = "Parameter"};

        public string StaticParamater;

        private void Awake()
        {
            Init();
        }

        public override void Init()
        {
            if (IsInit)
                return;

            IsInit = true;

            ParameterBinding.Filters = BindingFilter.Properties;
            ParameterBinding.ShouldShow = HasParamaterBinding;
        }

        public object GetValue()
        {
            switch (ParamaterType)
            {
                case ParamaterTypeEnum.Binding:
                    return GetValue(ParameterBinding.MemberName);
                case ParamaterTypeEnum.Context:
                    return Context.DataInstance;
                case ParamaterTypeEnum.Static:
                    return StaticParamater;
            }

            return null;
        }

        private bool HasParamaterBinding()
        {
            return ParamaterType == ParamaterTypeEnum.Binding;
        }
    }
}