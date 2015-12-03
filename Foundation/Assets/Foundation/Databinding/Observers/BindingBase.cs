// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Linq;
using UnityEngine;

#if UNITY_WSA && !UNITY_EDITOR
using System.Reflection;
#endif

namespace Foundation.Databinding
{
    /// <summary>
    ///     Base DataBinder. Used by presentation layer.
    ///     Depended on a (parent) BindingContext to mediate to the model
    /// </summary>
    /// <remarks>
    ///     If you want to write your own databinder, inherit from this.
    /// </remarks>
    [Serializable]
    [ExecuteInEditMode]
    public abstract class BindingBase : MonoBehaviour, IBindingElement
    {
        #region child

        public enum BindingFilter
        {
            /// <summary>
            ///     void Methods or Coroutines
            /// </summary>
            Commands,

            /// <summary>
            ///     Properties or Fields
            /// </summary>
            Properties
        }

        /// <summary>
        ///     PropertyBinder Child Item
        /// </summary>
        [Serializable]
        public class BindingInfo
        {
            /// <summary>
            ///     Control Property that is being bound
            /// </summary>
            public string BindingName;

            /// <summary>
            ///     Member Filter
            /// </summary>
            public BindingFilter Filters;

            /// <summary>
            ///     Return Type Filter
            /// </summary>
            public Type[] FilterTypes;

            /// <summary>
            ///     Model Property/Method bound to
            /// </summary>
            public string MemberName;

            /// <summary>
            ///     Action invoked with this binding
            /// </summary>
            public Action<object> Action { get; set; }

            /// <summary>
            ///     should show option
            /// </summary>
            public Func<bool> ShouldShow { get; set; }
        }

        #endregion

        #region settings

        /// <summary>
        ///     For binding to another object within the ui hierarchy
        /// </summary>
        /// <remarks>
        ///     For Master / Details situations.
        /// </remarks>
        public GameObject BindingProxy;

        /// <summary>
        ///     Prints Debug messages. This can get spammy.
        /// </summary>
        public bool DebugMode;

        #endregion

        #region Props

        [HideInInspector] private IObservableModel _model;

        /// <summary>
        ///     Bound Model
        /// </summary>
        public IObservableModel Model
        {
            get { return _model; }
            set
            {
                if (_model == value)
                    return;

                if (DebugMode && Application.isPlaying)
                {
                    Debug.Log(GetInstanceID() + ":" + name + ": " + "SetModel : " + value);
                }

                BeforeModelChanged();

                _model = value;

                OnModelChanged();
            }
        }

        [HideInInspector] private BindingContext _context;

        /// <summary>
        ///     Binding Root
        /// </summary>
        public BindingContext Context
        {
            get { return _context; }
            set
            {
                if (_context == value)
                    return;

                if (DebugMode && Application.isPlaying)
                {
                    Debug.Log(GetInstanceID() + ":" + name + ": " + "Context Set :" + value);
                }


                if (_context != null)
                    _context.UnsubscribeBinder(this);

                _context = value;

                if (_context != null)
                    _context.SubscribeBinder(this);
            }
        }

        /// <summary>
        ///     True if OnApplicationQuit was called
        /// </summary>
        public virtual bool IsApplicationQuit { get; protected set; }

        #endregion

        #region Internal

        /// <summary>
        ///     find _root and bind
        /// </summary>
        protected virtual void OnEnable()
        {
            if (DebugMode && Application.isPlaying)
            {
                Debug.Log(GetInstanceID() + ":" + name + ": " + "OnEnable");
            }

            FindContext();
        }

        /// <summary>
        ///     release from _root
        /// </summary>
        protected virtual void OnDisable()
        {
            // Dont reset in editor.
            if (!Application.isPlaying)
                return;

            if (IsApplicationQuit)
                return;

            if (DebugMode && Application.isPlaying)
            {
                Debug.Log(GetInstanceID() + ":" + name + ": " + "OnDisable");
            }

            Context = null;
            Model = null;
        }

        /// <summary>
        ///     Handles UnityEngine ApplicationQuit event
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

        /// <summary>
        ///     Finds the BindingContext in parent
        /// </summary>
        [ContextMenu("Find BindingContext")]
        public void FindContext()
        {
            Context = BindingProxy == null
                ? gameObject.FindInParent<BindingContext>()
                : BindingProxy.FindInParent<BindingContext>();

            if (BindingProxy != null && Context == null)
                Debug.LogError("Invalid BindingProxy. Please bind to a BindingContext or its child.");
        }


        [ContextMenu("Debug Info")]
        public virtual void DebugInfo()
        {
            Debug.Log("Context : " + Context);
            Debug.Log("Model   : " + (Model == null ? "null" : Model.ToString()));

            Debug.Log("Bindings");
            foreach (var info in GetBindingInfos())
            {
                Debug.Log("Member : " + info.MemberName + ", " + info.BindingName);
            }
        }

        #endregion

        #region protected

        /// <summary>
        ///     helper. Calls the GetValue method on the Model
        /// </summary>
        protected object GetValue(string memberName)
        {
            if (Model == null)
            {
                Debug.LogWarning("Model is null ! " + gameObject.name + " " + GetType());
                return null;
            }

            return Model.GetValue(memberName);
        }

        /// <summary>
        ///     helper. Calls the GetValue method on the Model
        /// </summary>
        protected object GetValue(string memberName, object argument)
        {
            if (Model == null)
            {
                Debug.LogWarning("Model is null ! " + gameObject.name + " " + GetType());
                return null;
            }

            return Model.GetValue(memberName);
        }

        /// <summary>
        ///     helper. Calls the SetValue method on the model
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="argument"></param>
        protected void SetValue(string memberName, object argument)
        {
            if (!enabled)
                return;

            if (IsApplicationQuit)
                return;

            if (string.IsNullOrEmpty(memberName))
                return;

            if (Model == null)
            {
                //  Debug.LogWarning("Model is null ! " + gameObject.name + " " + GetType() + GetInstanceID() + " / " + gameObject.transform.GetInstanceID());

                //  Debug.LogWarning("Where is this behavior ?");

                return;
            }
            if (argument == null)
            {
                Model.Command(memberName);
            }
            else
            {
                Model.Command(memberName, argument);
            }
        }

        #endregion

        #region virtual

        /// <summary>
        ///     Handle all change notification here
        /// </summary>
        /// <param name="m"></param>
        public virtual void OnBindingMessage(ObservableMessage m)
        {
            // ignore if editor
            if (!Application.isPlaying)
                return;

            if (Model == null)
                return;

            if (!enabled)
                return;

            var bindings = GetBindingInfos().Where(o => o.MemberName == m.Name && o.Action != null).ToArray();

            foreach (var binding in bindings)
            {
                if (binding.Action != null)
                    binding.Action(m.Value);
            }
        }

        /// <summary>
        ///     Any cleanup logic goes here
        /// </summary>
        protected virtual void BeforeModelChanged()
        {
        }

        /// <summary>
        ///     Any setup logic for the model goes here
        /// </summary>
        protected virtual void OnModelChanged()
        {
            // ignore if editor
            if (!Application.isPlaying)
                return;

            if (Model == null)
                return;

            if (!enabled)
                return;

            foreach (var binding in GetBindingInfos())
            {
                if (string.IsNullOrEmpty(binding.MemberName))
                    continue;

                if (binding.Action == null)
                    continue;

                binding.Action(GetValue(binding.MemberName));
            }
        }

        public void OnBindingRefresh()
        {
            OnModelChanged();
        }

        /// <summary>
        ///     And setup logic here
        /// </summary>
        /// <remarks>
        ///     Add Bindings
        /// </remarks>
        public abstract void Init();

        protected BindingInfo[] _infoCache;

        /// <summary>
        ///     Returns binding listeners
        /// </summary>
        /// <returns></returns>
        public BindingInfo[] GetBindingInfos()
        {
#if UNITY_EDITOR
            return GetType().GetFields()
                .Where(o => o.FieldType == typeof (BindingInfo))
                .Select(o => o.GetValue(this))
                .Cast<BindingInfo>()
                .ToArray();
#elif UNITY_WSA
            if(_infoCache == null)
                _infoCache = GetType().GetRuntimeFields()
                    .Where(o => o.FieldType == typeof(BindingInfo))
                    .Select(o => o.GetValue(this))
                    .Cast<BindingInfo>()
                    .ToArray();

            return _infoCache;
#else
            if (_infoCache == null)
                _infoCache = GetType().GetFields()
                    .Where(o => o.FieldType == typeof(BindingInfo))
                    .Select(o => o.GetValue(this))
                    .Cast<BindingInfo>()
                    .ToArray();

            return _infoCache;
#endif
        }

        #endregion
    }
}