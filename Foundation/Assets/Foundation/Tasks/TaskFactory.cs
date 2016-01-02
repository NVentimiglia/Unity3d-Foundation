using System;
using System.Collections;

namespace Foundation.Tasks
{
    /// <summary>
    /// A task encapsulates future work that may be waited on.
    /// - Support running actions in background threads 
    /// - Supports running coroutines with return results
    /// - Use the WaitForRoutine method to wait for the task in a coroutine
    /// </summary>
    /// <example>
    /// <code>
    ///     var task = Task.Run(() =>
    ///     {
    ///        //Debug.Log does not work in
    ///        Debug.Log("Sleeping...");
    ///        Task.Delay(2000);
    ///        Debug.Log("Slept");
    ///    });
    ///    // wait for it
    ///    yield return task;
    ///
    ///    // check exceptions
    ///    if(task.IsFaulted)
    ///        Debug.LogException(task.Exception)
    ///</code>
    ///</example>
    public partial class UnityTask
    {
        #region Task
        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask Run(Action action)
        {
            var task = new UnityTask(action);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunOnMain(Action action)
        {
            var task = new UnityTask(action, TaskStrategy.MainThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunOnCurrent(Action action)
        {
            var task = new UnityTask(action, TaskStrategy.CurrentThread);
            task.Start();
            return task;
        }
        #endregion
        
        #region Coroutine

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunCoroutine(IEnumerator function)
        {
            var task = new UnityTask(function);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunCoroutine(Func<IEnumerator> function)
        {
            var task = new UnityTask(function());
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunCoroutine(Func<UnityTask, IEnumerator> function)
        {
            var task = new UnityTask();
            task.Strategy = TaskStrategy.Coroutine;
            task._routine = function(task);
            task.Start();
            return task;
        }
        #endregion

        #region Task With Result
        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = new UnityTask<TResult>(function);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask<TResult> RunOnMain<TResult>(Func<TResult> function)
        {
            var task = new UnityTask<TResult>(function, TaskStrategy.MainThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask<TResult> RunOnCurrent<TResult>(Func<TResult> function)
        {
            var task = new UnityTask<TResult>(function, TaskStrategy.CurrentThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask<TResult> RunCoroutine<TResult>(IEnumerator function)
        {
            var task = new UnityTask<TResult>(function);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a task which passes the task as a parameter
        /// </summary>
        public static UnityTask<TResult> RunCoroutine<TResult>(Func<UnityTask<TResult>, IEnumerator> function)
        {
            var task = new UnityTask<TResult>();
            task.Strategy = TaskStrategy.Coroutine;
            task._routine = function(task);
            task.Start();
            return task;
        }
        #endregion

        #region success / fails

        /// <summary>
        /// A default task in the success state
        /// </summary>
        static UnityTask _successTask = new UnityTask(TaskStrategy.Custom) { Status = TaskStatus.Success };
        
        /// <summary>
        /// A default task in the success state
        /// </summary>
        public static UnityTask<T> SuccessTask<T>(T result)
        {
            return new UnityTask<T>(TaskStrategy.Custom) { Status = TaskStatus.Success, Result = result };
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask SuccessTask()
        {
            return _successTask;
        }


        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask FailedTask(string exception)
        {
            return FailedTask(new Exception(exception));
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask FailedTask(Exception ex)
        {
            return new UnityTask(TaskStrategy.Custom) { Status = TaskStatus.Faulted, Exception = ex };
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask<T> FailedTask<T>(string exception)
        {
            return FailedTask<T>(new Exception(exception));
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask<T> FailedTask<T>(Exception ex) 
        {
            return new UnityTask<T>(TaskStrategy.Custom) { Status = TaskStatus.Faulted, Exception = ex };
        }
        #endregion

    }
}
