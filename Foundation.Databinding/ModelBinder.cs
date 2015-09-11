// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    /// Implements IObservableModel on behalf of other objects.
    /// Change notification requires IObservableModel implementation
    /// </summary>
    public class ModelBinder : IObservableModel, IDisposable
    {
        Type _myType;
        ObservableMessage _bindingMessage = new ObservableMessage();
        object _instance;
        MonoBehaviour _insanceBehaviour;
        IObservableModel _bindableInstance;
        INotifyPropertyChanged _notifyInstance;

        public ModelBinder(object instance)
        {
            _instance = instance;
            _myType = _instance.GetType();

            _insanceBehaviour = instance as MonoBehaviour;
            _bindableInstance = instance as IObservableModel;
            _notifyInstance = instance as INotifyPropertyChanged;

            if (_bindableInstance != null)
                _bindableInstance.OnBindingUpdate += _bindableInstance_OnBindingUpdate;
            else if (_notifyInstance != null)
                _notifyInstance.PropertyChanged += _notifyInstance_PropertyChanged;

        }

        void _notifyInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_onBindingEvent != null)
            {
                _bindingMessage.Name = e.PropertyName;
                _bindingMessage.Value = GetValue(e.PropertyName);
                _onBindingEvent(_bindingMessage);
            }
        }

        void _bindableInstance_OnBindingUpdate(ObservableMessage obj)
        {
            if (_onBindingEvent != null)
            {
                _onBindingEvent(obj);
            }
        }

        /// <summary>
        /// Raises property changed on all listeners
        /// </summary>
        /// <param name="propertyName">property to change</param>
        /// <param name="propValue">value to pass</param>
        public virtual void NotifyProperty(string propertyName, object propValue)
        {
            RaiseBindingUpdate(propertyName, propValue);
        }

        #region interface
        private Action<ObservableMessage> _onBindingEvent = delegate { };
        public event Action<ObservableMessage> OnBindingUpdate
        {
            add
            {
                _onBindingEvent = (Action<ObservableMessage>)Delegate.Combine(_onBindingEvent, value);
            }
            remove
            {
                _onBindingEvent = (Action<ObservableMessage>)Delegate.Remove(_onBindingEvent, value);
            }
        }

        public void RaiseBindingUpdate(string memberName, object paramater)
        {
            if (_onBindingEvent != null)
            {
                _bindingMessage.Name = memberName;
                _bindingMessage.Sender = this;
                _bindingMessage.Value = paramater;

                _onBindingEvent(_bindingMessage);
            }
        }

        [HideInInspector]
        public object GetValue(string memberName)
        {
            var member = _myType.GetRuntimeMember(memberName);

            if (member == null)
            {
                Debug.LogError("Member not found ! " + memberName + " " + _myType);
                return null;
            }

            return member.GetMemberValue(_instance);
        }

        public object GetValue(string memberName, object paramater)
        {
            var member = _myType.GetRuntimeMember(memberName);

            if (member == null)
            {
                Debug.LogError("Member not found ! " + memberName + " " + _myType);
                return null;
            }

            if (member is MethodInfo)
            {
                var meth = (member as MethodInfo);
                if (paramater != null)
                {
                    var p = meth.GetParameters().FirstOrDefault();
                    if (p == null)
                    {
                        return GetValue(memberName);
                    }

                    var converted = ConverterHelper.ConvertTo(p.GetType(), paramater);
                    return meth.Invoke(_instance, new[] { converted });
                }

                return meth.Invoke(_instance, null);
            }
            if (member is PropertyInfo)
            {
#if UNITY_WSA
                return (member as PropertyInfo).GetValue(_instance);
#else
                return (member as PropertyInfo).GetValue(_instance, null);
#endif
            }

            return (member as FieldInfo).GetValue(_instance);
        }

        [HideInInspector]
        public void Command(string memberName)
        {
            Command(memberName, null);
        }

        public void Command(string memberName, object paramater)
        {
            var member = _myType.GetRuntimeMember(memberName);

            if (member == null)
            {
                Debug.LogError("Member not found ! " + memberName + " " + _myType);
                return;
            }

            // convert to fit signature
            var converted = ConverterHelper.ConvertTo(member.GetParamaterType(), paramater);

            if (member is MethodInfo)
            {
                var method = member as MethodInfo;
                if (method.ReturnType == typeof(IEnumerator))
                {
                    if (_insanceBehaviour)
                    {
                        if (!_insanceBehaviour.gameObject.activeSelf)
                        {
                            Debug.LogError("Behavior is inactive ! " + _insanceBehaviour);
                        }

                        // via built in
                        if (converted == null)
                            _insanceBehaviour.StartCoroutine(method.Name);
                        else
                            _insanceBehaviour.StartCoroutine(method.Name, converted);

                    }
                    else
                    {
                        // via helper
                        var routine = method.Invoke(_instance, converted == null ? null : new[] { converted });
                        ObservableHandler.Instance.StartCoroutine((IEnumerator)routine);
                    }
                    return;
                }
            }

            member.SetMemberValue(_instance, converted);
        }

        [HideInInspector]
        public void Dispose()
        {
            _bindingMessage.Dispose();

            if (_bindableInstance != null)
            {
                _bindableInstance.OnBindingUpdate -= _bindableInstance_OnBindingUpdate;
            }
            if (_notifyInstance != null)
            {
                _notifyInstance.PropertyChanged -= _notifyInstance_PropertyChanged;
            }
            _myType = null;
            _instance = null;
            _insanceBehaviour = null;
            _bindableInstance = null;
            _bindingMessage = null;
            _notifyInstance = null;
        }
        #endregion



    }
}