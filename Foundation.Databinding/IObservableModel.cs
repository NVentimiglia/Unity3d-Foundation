// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using UnityEngine;

namespace Foundation.Databinding
{ 
    /// <summary>
    /// Like INotifyPropertyChanged. Contains property name, value and sender.
    /// </summary>
    [Serializable]
    public class ObservableMessage : IDisposable
    {
        /// <summary>
        /// The sender of this message
        /// </summary>
        [SerializeField]
        public object Sender;

        /// <summary>
        /// Property, Field, or Member name
        /// </summary>
        [SerializeField]
        public string Name;

        /// <summary>
        /// Property,Field or Method arguments
        /// </summary>
        [SerializeField]
        public object Value;

        /// <summary>
        /// Casts as value helper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CastValue<T>()
        {
            return (T)Value;
        }

        public void Dispose()
        {
            Name = null;
            Value = Sender = null;
        }

        public override string ToString()
        {
            return "BindingMessage " + Name + " " + Value;
        }
    }

    /// <summary>
    /// Like INotifyPropertyChanged but with a more detailed event.
    /// </summary>
    public interface IObservableModel
    {
        /// <summary>
        /// Raised when a property is changed.
        /// </summary>
        event Action<ObservableMessage> OnBindingUpdate;

        /// <summary>
        /// Raises OnBindingMessage
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="paramater"></param>
        void RaiseBindingUpdate(string memberName, object paramater);
 
        /// <summary>
        /// Gets the value of the property of field.
        /// Calls the method with return value
        /// </summary>
        /// <param name="memberName"></param>
        object GetValue(string memberName);

        /// <summary>
        /// Calls the method with argument and return value
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="paramater"></param>
        object GetValue(string memberName, object paramater);

        /// <summary>
        /// Calls the method.
        /// Starts coroutine.
        /// </summary>
        /// <param name="memberName"></param>
        void Command(string memberName);
        
        /// <summary>
        /// Sets the value of a property or field.
        /// Calls the method.
        /// Starts coroutine.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="paramater"></param>
        void Command(string memberName, object paramater);
    }
}