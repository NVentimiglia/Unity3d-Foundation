#if UNITY_WEBGL

namespace Foundation.Tasks
{
    public partial class TaskManager
    {       /// <summary>
        /// Checks if this is the main thread
        /// </summary>
        public static bool IsMainThread
        {
            get { return true; }
        }

        /// <summary>
        /// The Main Thread
        /// </summary>
        public static int MainThread { get; protected set; }
        
        /// <summary>
        /// The Current Thread
        /// </summary>
        public static int CurrentThread
        {
            get
            {
                return MainThread;
            }
        }
    }
}
#endif