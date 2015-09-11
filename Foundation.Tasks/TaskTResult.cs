using System;
using System.Collections;
using UnityEngine;


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
    ///    yield return StartCoroutine(task.WaitRoutine());
    ///
    ///    // check exceptions
    ///    if(task.IsFaulted)
    ///        Debug.LogException(task.Exception)
    ///</code>
    ///</example>
    public class UnityTask<TResult> : UnityTask
    {
        #region public fields
        Func<TResult> _function;
        Delegate _function2;


        /// <summary>
        /// get the result of the task. Blocking. It is recommended you yield on the wait before accessing this value
        /// </summary>
        public TResult Result;

        #endregion

        #region ctor

        public UnityTask()
        {

        }

        /// <summary>
        /// Returns the task in the Success state.
        /// </summary>
        /// <param name="result"></param>
        public UnityTask(TResult result)
            : this()
        {
            Status = TaskStatus.Success;
            Strategy = TaskStrategy.Custom;
            Result = result;
        }

        /// <summary>
        /// Creates a new background Task strategy
        /// </summary>
        /// <param name="function"></param>
        public UnityTask(Func<TResult> function)
            : this()
        {
            if (function == null)
                throw new ArgumentNullException("function");

            _function = function;
        }

        /// <summary>
        /// Creates a new background Task strategy
        /// </summary>
        public UnityTask(Delegate function, object param)
            : this()
        {
            if (function == null)
                throw new ArgumentNullException("function");

            _function2 = function;
            Paramater = param;
        }

        /// <summary>
        /// Creates a new task with a specific strategy
        /// </summary>
        public UnityTask(Func<TResult> function, TaskStrategy mode)
            : this()
        {
            if (function == null)
                throw new ArgumentNullException("function");

            if (mode == TaskStrategy.Coroutine)
                throw new ArgumentException("Mode can not be coroutine");

            _function = function;
            Strategy = mode;
        }


        /// <summary>
        /// Creates a new task with a specific strategy
        /// </summary>
        public UnityTask(Delegate function, object param, TaskStrategy mode)
            : this()
        {
            if (function == null)
                throw new ArgumentNullException("function");

            if (mode == TaskStrategy.Coroutine)
                throw new ArgumentException("Mode can not be coroutine");

            _function2 = function;
            Paramater = param;
            Strategy = mode;
        }

        /// <summary>
        /// Creates a new Coroutine  task
        /// </summary>
        public UnityTask(IEnumerator routine)
        {
            if (routine == null)
                throw new ArgumentNullException("routine");


            _routine = routine;
            Strategy = TaskStrategy.Coroutine;
        }

        /// <summary>
        /// Creates a new Task in a Faulted state
        /// </summary>
        /// <param name="ex"></param>
        public UnityTask(Exception ex)
        {
            Exception = ex;
            Strategy = TaskStrategy.Custom;
            Status = TaskStatus.Faulted;
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        public UnityTask(TaskStrategy mode)
            : this()
        {
            Strategy = mode;
        }
        #endregion

        #region protected methods
        
        protected override void OnTaskComplete()
        {
            foreach (var d in CompleteList)
            {
                if (d != null)
                    d.DynamicInvoke(this);
            }
            CompleteList.Clear();

            if (IsSuccess)
            {
                foreach (var d in SuccessList)
                {
                    if (d != null)
                    {
                        d.DynamicInvoke(Result);
                    }
                }
            }
            SuccessList.Clear();
        }

        protected override void Execute()
        {
            try
            {
                if (_function2 != null)
                {
                    Result = (TResult)_function2.DynamicInvoke(Paramater);
                }
                else if (_function != null)
                {
                    Result = _function();
                }
                Status = TaskStatus.Success;
            }
            catch (Exception ex)
            {
                Exception = ex;
                Status = TaskStatus.Faulted;
                if (LogErrors)
                    Debug.LogException(ex);
            }
        }

        #endregion

        #region continue with
        /// <summary>
        /// Called after the task is complete
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public UnityTask<TResult> ContinueWith(Action<UnityTask<TResult>> action)
        {
            if (IsCompleted)
            {
                action(this);
            }
            else
            {
                CompleteList.Add(action);
            }

            return this;
        }

        /// <summary>
        /// Called after a successful task execution
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public UnityTask<TResult> OnSuccess(Action<TResult> action) 
        {
            if (IsCompleted && IsSuccess)
            {
                action(Result);
            }
            else
            {
                SuccessList.Add(action);
            }

            return this;
        }

        #endregion


        #region ConvertTo
        /// <summary>
        /// Like continue runs after the host task is complete but returns a new task with a converted result
        /// </summary>
        /// <typeparam name="T">new conversion type</typeparam>
        /// <param name="func">conversion function</param>
        /// <returns></returns>
        public UnityTask<T> ConvertTo<T>(Func<UnityTask<TResult>, T> func)
        {
            var task = new UnityTask<T>(TaskStrategy.Custom);
            if (IsCompleted)
            {
                task.Result = func(this);
                task.Exception = Exception;
                task.Status = Status;
            }
            else
                TaskManager.StartRoutine(ConvertToAsync(task, func));
            return task;
        }

        private IEnumerator ConvertToAsync<T>(UnityTask<T> task, Func<UnityTask<TResult>, T> func)
        {
            while (!IsCompleted)
                yield return 1;

            task.Result = func(this);
            task.Exception = Exception;
            task.Status = Status;
        }
        #endregion
    }
}
