using UnityEngine;

namespace Foundation.Databinding
{
    public static class BindingExtensions
    {
        /// <summary>
        ///     Looks for component at or above source in hierarchy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindInParent<T>(this GameObject obj) where T : Component
        {
            var t = obj.transform;

            for (var i = 0; i < 25; i++)
            {
                if (t == null)
                    return null;

                var found = t.GetComponent<T>();

                if (found != null)
                {
                    return found;
                }

                if (t.parent == null)
                {
                    return null;
                }

                t = t.parent;
            }

            return null;
        }
    }
}