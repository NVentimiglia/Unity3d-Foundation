using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    /// Coroutine runner for Databinding to Coroutines
    /// </summary>
    public class ObservableHandler : MonoBehaviour
    {
        public static ObservableHandler Instance = new ObservableHandler();

        static ObservableHandler()
        {
            var o = new GameObject("_ObservableHandler");
            DontDestroyOnLoad(o);
            Instance = o.AddComponent<ObservableHandler>();
        }
    }
}
