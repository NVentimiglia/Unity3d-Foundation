// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Foundation.Databinding
{
    /// <summary>
    /// A List with on change delegates
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableList<T> :  IList<T>
    {
        protected List<T> InternalList = new List<T>();

        public event Action<T> OnAdd;
        public event Action<int, T> OnInset;
        public event Action<T> OnRemove;
#pragma warning disable 0067
        public event Action OnClear;
#pragma warning restore 0067


        public IEnumerator<T> GetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return InternalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            InternalList.Insert(index, item);

            if (OnInset != null)
                OnInset(index, item);
        }

        public void RemoveAt(int index)
        {
            InternalList.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return InternalList[index]; }
            set { InternalList[index] = value; }
        }

        public void Add(T item)
        {
            InternalList.Add(item);

            if (OnAdd != null)
                OnAdd(item);
        }

        public void Clear()
        {
            InternalList.Clear();

            if (OnClear != null)
                OnClear();
        }

        public bool Contains(T item)
        {
            return InternalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InternalList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return InternalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            if (InternalList.Remove(item))
            {
                if (OnRemove != null)
                    OnRemove(item);

                return true;
            }

            return false;

        }
    }
}