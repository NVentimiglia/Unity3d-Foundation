// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WSA
using System.Runtime.CompilerServices;
#else
using System.Diagnostics;
using System.Linq;
#endif

namespace Foundation.Databinding
{
    /// <summary>
    /// Implements the IObservableModel for mono behavior objects
    /// </summary>
    public abstract class ObservableBehaviour : MonoBehaviour, IObservableModel
    {
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

        private ModelBinder _binder;

        private bool _isDisposed;

        private ObservableMessage _bindingMessage;

        private ModelBinder Binder
        {
            get
            {
                if (_binder == null && !_isDisposed)
                    _binder = new ModelBinder(this);
                return _binder;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsApplicationQuit { get; private set; }

        /// <summary>
        /// Virtual, Initializes Binder
        /// </summary>
        protected virtual void Awake()
        {
            if (_bindingMessage == null)
                _bindingMessage = new ObservableMessage { Sender = this };
            if (_binder == null)
                _binder = new ModelBinder(this);
        }

        /// <summary>
        /// Virtual, Disposes
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (IsApplicationQuit)
                return;

            Dispose();
        }

        [HideInInspector]
        public virtual void Dispose()
        {
            _isDisposed = true;

            if (_binder != null)
            {
                _binder.Dispose();
            }

            if (_bindingMessage != null)
            {
                _bindingMessage.Dispose();
            }

            _bindingMessage = null;
            _binder = null;
        }

        public void RaiseBindingUpdate(string memberName, object paramater)
        {
            if (_bindingMessage == null)
                _bindingMessage = new ObservableMessage { Sender = this };

            Binder.RaiseBindingUpdate(memberName, paramater);

            if (_onBindingEvent != null)
            {
                _bindingMessage.Name = memberName;
                _bindingMessage.Value = paramater;
                _onBindingEvent(_bindingMessage);
            }
        }


        [HideInInspector]
        public void Command(string memberName)
        {
            _binder.Command(memberName);
        }

        public void Command(string memberName, object paramater)
        {
            _binder.Command(memberName, paramater);
        }

        [HideInInspector]
        public object GetValue(string memberName)
        {
            return Binder.GetValue(memberName);
        }

        public object GetValue(string memberName, object paramater)
        {

            return Binder.GetValue(memberName, paramater);
        }

        public virtual void NotifyProperty(string memberName, object paramater)
        {
            RaiseBindingUpdate(memberName, paramater);
        }

        protected virtual void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

#if !UNITY_WSA
        /// <summary>
        /// Mvvm light set method
        /// </summary>
        /// <remarks>
        /// https://github.com/NVentimiglia/Unity3d-Databinding-Mvvm-Mvc/issues/3
        /// https://github.com/negue
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueHolder"></param>
        /// <param name="value"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T valueHolder, T value, string propName = null)
        {
            var same =  EqualityComparer<T>.Default.Equals(valueHolder, value);

            if (!same)
            {
                if (string.IsNullOrEmpty(propName))
                {
                    // get call stack
                    var stackTrace = new StackTrace();
                    // get method calls (frames)
                    var stackFrames = stackTrace.GetFrames().ToList();

                    if (propName == null && stackFrames.Count > 1)
                    {
                        propName = stackFrames[1].GetMethod().Name.Replace("set_", "");
                    }
                }

                valueHolder = value;

                NotifyProperty(propName, value);

                return true;
            }

            return false;
        }
#else
        /// <summary>
        /// Mvvm light set method
        /// </summary>
        /// <remarks>
        /// https://github.com/NVentimiglia/Unity3d-Databinding-Mvvm-Mvc/issues/3
        /// https://github.com/negue
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueHolder"></param>
        /// <param name="value"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T valueHolder, T value, [CallerMemberName] string propName = null)
        {
            var same = EqualityComparer<T>.Default.Equals(valueHolder, value);

            if (!same)
            {
                NotifyProperty(propName, value);
                valueHolder = value;
                return true;
            }

            return false;
        }
#endif
    }
}