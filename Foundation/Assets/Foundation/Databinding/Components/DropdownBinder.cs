// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Databinding
{
    /// <summary>
    /// Optional Interface for elements that may be selected using the DropdownBinder
    /// </summary>
    public interface IOptionData
    {
        string OptionLabel { get; set; }
        Sprite OptionImage { get; set; }
    }

    // <summary>
    ///     Dropdown binding controller
    /// </summary>
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("Foundation/Databinding/DropdownBinder")]
    public class DropdownBinder : BindingBase
    {
        [HideInInspector]
        public BindingInfo EnabledBinding = new BindingInfo
        {
            // Inspecter Name
            BindingName = "Enabled",
            // Properties are two way
            Filters = BindingFilter.Properties,
            // Filters Model Properties By Type
            FilterTypes = new[] { typeof(bool) }
        };

        [HideInInspector]
        public BindingInfo ItemsBinding = new BindingInfo
        {
            // Inspecter Name
            BindingName = "Items",
            // Properties are two way
            Filters = BindingFilter.Properties,
            // Filters Model Properties By Type
            FilterTypes = new[] { typeof(IEnumerable) }
        };


        [HideInInspector]
        public BindingInfo ValueBinding = new BindingInfo
        {
            // Inspecter Name
            BindingName = "Value",
            // Properties are two way
            Filters = BindingFilter.Properties,
            // Filters Model Properties By Type
            FilterTypes = new[] { typeof(int) }
        };



        [HideInInspector]
        public BindingInfo SelectedBinding = new BindingInfo
        {
            // Inspecter Name
            BindingName = "Selected",
            // Properties are two way
            Filters = BindingFilter.Properties,
        };


        protected Dropdown Element;
        protected IObservableCollection DataList;
        protected List<object> Data = new List<object>();
        public bool DelayLoad;
        protected int Index;
        protected bool IsInit;

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
            Element = GetComponent<Dropdown>();

            // Listen to button clicks
            Element.onValueChanged.AddListener(OnChange);

            // Handle the Model.Enabled Change
            EnabledBinding.Action = UpdateEnabled;

            if (!string.IsNullOrEmpty(ItemsBinding.MemberName))
            {
                ItemsBinding.Action = Bind;
            }

            // We cant have both !
            if (!string.IsNullOrEmpty(SelectedBinding.MemberName))
                SelectedBinding.Action = UpdateSelected;
            else if (!string.IsNullOrEmpty(ValueBinding.MemberName))
                ValueBinding.Action = UpdateValue;
        }

        void OnChange(int value)
        {
            // if button is disabled, no
            if (!Element.IsInteractable())
                return;

            if (!string.IsNullOrEmpty(SelectedBinding.MemberName))
            {
                if (value == -1)
                {
                    SetValue(SelectedBinding.MemberName, null);
                }
                else
                {
                    var model = Data[value];
                    SetValue(SelectedBinding.MemberName, model);
                }
            }
            if (!string.IsNullOrEmpty(ValueBinding.MemberName))
            {
                SetValue(ValueBinding.MemberName, value);
            }
        }

        private void UpdateEnabled(object arg)
        {
            // Disable The Button
            Element.interactable = (bool)arg;
        }

        private void UpdateSelected(object arg)
        {
            var index = Data.IndexOf(arg);
            if (index == -1)
            {
                Element.value = 0;
            }
            else
            {
                Element.value = index;
            }
        }

        private void UpdateValue(object arg)
        {
            var index = (int)arg;
            if (index == -1)
            {
                Element.value = 0;
            }
            else
            {
                Element.value = index;
            }
        }

        /// <summary>
        ///     Bind the ObservableCollection
        /// </summary>
        /// <param name="data"></param>
        public void Bind(object data)
        {
            if (DataList != null)
            {
                DataList.OnObjectAdd -= OnAdd;
                DataList.OnObjectInsert -= OnInsert;
                DataList.OnClear -= OnClear;
                DataList.OnObjectRemove -= OnRemove;
            }

            DataList = null;
            OnClear();
            
            if (data is IObservableCollection)
            {
                DataList = data as IObservableCollection;

                foreach (var d in DataList.GetObjects())
                {
                    OnAdd(d);
                }

                DataList.OnObjectAdd += OnAdd;
                DataList.OnObjectInsert += OnInsert;
                DataList.OnClear += OnClear;
                DataList.OnObjectRemove += OnRemove;
            }
            else if (data is IEnumerable)
            {
                var list = data as IEnumerable;
                foreach (var d in list)
                {
                    OnAdd(d);
                }
            }

            //Hack, not refreshing 
            var cache = Element.value;
            Element.value = cache;
        }

        private void OnClear()
        {
            Element.options.Clear();
            Data.Clear();
        }

        private void OnRemove(object obj)
        {
            var index = Data.IndexOf(obj);
            if (index == -1)
            {
                Debug.LogWarning("DropDown option not found :" + obj);
            }
            else
            {
                Data.Remove(obj);
                Element.options.RemoveAt(index);
            }
        }

        private void OnAdd(object obj)
        {
            Data.Add(obj);
            if (obj is Dropdown.OptionData)
            {
                Element.options.Add(obj as Dropdown.OptionData);
            }
            else if (obj is IOptionData)
            {
                var data = obj as IOptionData;
                Element.options.Add(new Dropdown.OptionData(data.OptionLabel, data.OptionImage));
            }
            else
            {
                Element.options.Add(new Dropdown.OptionData(obj.ToString()));
            }
        }

        private void OnInsert(int i, object obj)
        {
            Data.Insert(i, obj);
            if (obj is Dropdown.OptionData)
            {
                Element.options.Insert(i, obj as Dropdown.OptionData);
            }
            else if (obj is IOptionData)
            {
                var data = obj as IOptionData;
                Element.options.Insert(i, new Dropdown.OptionData(data.OptionLabel, data.OptionImage));
            }
            else
            {
                Element.options.Insert(i, new Dropdown.OptionData(obj.ToString()));
            }
        }
    }
}