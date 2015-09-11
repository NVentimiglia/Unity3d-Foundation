// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foundation.Databinding
{
    /// <summary>
    /// A collection that supports change notification.
    /// Simplified to use objects.
    /// </summary>
    public interface IObservableCollection
    {
        event Action<object> OnObjectAdd;

        event Action<object> OnObjectRemove;

        event Action<int, object> OnObjectInsert;

        event Action OnClear;

        IEnumerable<object> GetObjects();
    }


    /// <summary>
    /// A collection that supports change notification
    /// </summary>
    /// <typeparam name="T">ValueType of a collection item</typeparam>
    public class ObservableCollection<T> : IEnumerable<T>, IObservableCollection
    {

        #region events
        /// <summary>
        /// For data binding
        /// </summary>
        private Action<object> _onObjectAdd = delegate { };
        public event Action<object> OnObjectAdd
        {
            add
            {
                _onObjectAdd = (Action<object>)Delegate.Combine(_onObjectAdd, value);
            }
            remove
            {
                _onObjectAdd = (Action<object>)Delegate.Remove(_onObjectAdd, value);
            }
        }
        /// <summary>
        /// For data binding
        /// </summary>
        private Action<int, object> _onObjectInsert = delegate { };
        public event Action<int, object> OnObjectInsert
        {
            add
            {
                _onObjectInsert = (Action<int, object>)Delegate.Combine(_onObjectInsert, value);
            }
            remove
            {
                _onObjectInsert = (Action<int, object>)Delegate.Remove(_onObjectInsert, value);
            }
        }

        /// <summary>
        /// For data binding
        /// </summary>
        private Action<object> _onObjectRemove = delegate { };
        public event Action<object> OnObjectRemove
        {
            add
            {
                _onObjectRemove = (Action<object>)Delegate.Combine(_onObjectRemove, value);
            }
            remove
            {
                _onObjectRemove = (Action<object>)Delegate.Remove(_onObjectRemove, value);
            }
        }
        
        private Action<T> _onAdd = delegate { };
        public event Action<T> OnAdd
        {
            add
            {
                _onAdd = (Action<T>)Delegate.Combine(_onAdd, value);
            }
            remove
            {
                _onAdd = (Action<T>)Delegate.Remove(_onAdd, value);
            }
        }
        
        private Action<int,T> _onInsert = delegate { };
        public event Action<int, T> OnInsert
        {
            add
            {
                _onInsert = (Action<int, T>)Delegate.Combine(_onInsert, value);
            }
            remove
            {
                _onInsert = (Action<int, T>)Delegate.Remove(_onInsert, value);
            }
        }

        
        private Action<T> _onRemove = delegate { };
        public event Action<T> OnRemove
        {
            add
            {
                _onRemove = (Action<T>)Delegate.Combine(_onRemove, value);
            }
            remove
            {
                _onRemove = (Action<T>)Delegate.Remove(_onRemove, value);
            }
        }
        
        private Action _onClear = delegate { };
        public event Action OnClear
        {
            add
            {
                _onClear = (Action)Delegate.Combine(_onClear, value);
            }
            remove
            {
                _onClear = (Action)Delegate.Remove(_onClear, value);
            }
        }
        #endregion


        private readonly List<T> _list = new List<T>();


        public ObservableCollection()
        {

        }

        public ObservableCollection(IEnumerable<T> set)
        {
            Add(set);
        }

        /// <summary>
        /// Returns all items cast as object
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetObjects()
        {
            if (_list.Count == 0)
                return new object[0];
            return _list.Cast<object>();
        }


        /// <summary>
        /// Convenience function. I recommend using .buffer instead.
        /// </summary>
        public T this[int i]
        {
            get { return _list[i]; }
            set { _list[i] = value; }
        }

        /// <summary>
        /// Returns 'true' if the specified item is within the list.
        /// </summary>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// Add the specified item to the end of the list.
        /// </summary>
        public void Add(T o)
        {
            _list.Add(o);

            if (_onAdd != null)
                _onAdd(o);

            if (_onObjectAdd != null)
                _onObjectAdd(o);
        }

        /// <summary>
        /// Add the specified items to the end of the list.
        /// </summary>
        public void Add(IEnumerable<T> o)
        {
            var s = o.ToArray();

            for (int i = 0;i < s.Length;i++)
            {
                Add(s[i]);
            }
        }

        /// <summary>
        /// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
        /// </summary>
        public void Remove(T o)
        {
            if (_list.Remove(o))
            {
                if (_onRemove != null)
                    _onRemove(o);

                if (_onObjectRemove != null)
                    _onObjectRemove(o);
            }
        }

        /// <summary>
        /// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
        /// </summary>
        public void Remove(IEnumerable<T> o)
        {
            var s = o.ToArray();

            for (int i = 0;i < s.Length;i++)
            {
                Remove(s[i]);
            }
        }

        /// <summary>
        /// Insert an item at the specified index, pushing the entries back.
        /// </summary>
        public void Insert(int index, T o)
        {
            _list.Insert(index, o);

            if (_onInsert != null)
                _onInsert(index, o);

            if (_onInsert != null)
                _onObjectInsert(index, o);

        }

        /// <summary>
        /// Clear the array by resetting its size to zero. Note that the memory is not actually released.
        /// </summary>
        public void Clear()
        {
            _list.Clear();

            if (_onClear != null)
                _onClear();

        }


        /// <summary>
        /// Clear the array and release the used memory.
        /// </summary>
        public void Release()
        {
                Clear();
        }

        /// <summary>
        /// returns count
        /// </summary>
        /// <returns></returns>
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
        /// </summary>

        public T[] ToArray()
        {
            return _list.ToArray();
        }
    }

}