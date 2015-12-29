using System;

namespace Foundation.Tasks
{
    public static class TaskExtensions
    {
        /// <summary>
        /// will throw if faulted
        /// </summary>
        /// <returns></returns>
        public static T ThrowIfFaulted<T>(this T self) where T : UnityTask
        {
            if (self.IsFaulted)
                throw self.Exception;
            return self;
        }

        /// <summary>
        /// Waits for the task to complete
        /// </summary>
        public static T Wait<T>(this T self) where T : UnityTask
        {
            UnityTask.Delay(10);

            while (self.keepWaiting)
            {
                UnityTask.Delay(10);
            }

            return self;
        }

        /// <summary>
        /// Waits for the task to complete
        /// </summary>
        public static T ContinueWith<T>(this T self, Action<T> continuation) where T : UnityTask
        {
            if (self.IsCompleted)
            {
                continuation(self);
            }
            else
            {
                self.AddContinue(continuation);
            }
            return self;
        }
    }
}