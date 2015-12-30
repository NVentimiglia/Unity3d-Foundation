// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Foundation
{
    public static class EnumerableExt
    {
        /// <summary>
        /// Random in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Random<T>(this IEnumerable<T> list)
        {
            var count = list.Count();

            if (count == 0)
                return default(T);

            return list.ElementAt(UnityEngine.Random.Range(0, count));
        }

        /// <summary>
        /// Next in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static T Next<T>(this T[] list, T current)
        {
            if (list == null || list.Length == 0)
                return current;

            if (current == null)
                return list[0];

            var index = Array.IndexOf(list, current);

            index++;

            if (index >= list.Length)
            {
                index = 0;
            }

            return list[index];
        }

        /// <summary>
        /// Next in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static T Back<T>(this T[] list, T current)
        {
            if (list == null || list.Length == 0)
                return current;

            if (current == null)
                return list[0];

            var index = Array.IndexOf(list, current);

            index--;

            if (index < 0)
            {
                index = list.Length - 1;
            }

            return list[index];
        }

        /// <summary>
        ///   Perform the <paramref name="action" /> on each item in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (var e in collection)
            {
                action.Invoke(e);
            }

            return collection;
        }
    }
}