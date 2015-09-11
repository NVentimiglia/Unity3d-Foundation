using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WSA
using Windows.System.Threading;
#elif !UNITY_WEBGL
using System.Threading;
#endif

namespace Foundation.Tasks
{
    /// <summary>
    /// Describes the Tasks State
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Ready to run
        /// </summary>
        Created,
        /// <summary>
        /// Working
        /// </summary>
        Running,
        /// <summary>
        /// Exception as thrown or otherwise stopped early
        /// </summary>
        Faulted,
        /// <summary>
        /// Complete without error
        /// </summary>
        Success,
        /// <summary>
        /// Dispose has been called
        /// </summary>
        Disposed,
    }

    /// <summary>
    /// Execution strategy for the Task
    /// </summary>
    public enum TaskStrategy
    {
#if !UNITY_WEBGL
        /// <summary>
        /// Dispatches the task to a background thread
        /// </summary>
        BackgroundThread,
#endif
        /// <summary>
        /// Dispatches the task to the main thread
        /// </summary>
        MainThread,
        /// <summary>
        /// Dispatches the task to the current thread
        /// </summary>
        CurrentThread,
        /// <summary>
        /// Runs the task as a coroutine
        /// </summary>
        Coroutine,
        /// <summary>
        /// Does nothing. For custom tasks.
        /// </summary>
        Custom,
    }

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
    public partial class UnityTask : IDisposable
    {
        #region options
        /// <summary>
        /// Forces use of a single thread for debugging
        /// </summary>
        public static bool DisableMultiThread = false;

        /// <summary>
        /// Logs Exceptions
        /// </summary>
        public static bool LogErrors = false;
        #endregion

        #region fields
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Input Parameter
        /// </summary> 
        public object Paramater { get; set; }

        /// <summary>
        /// Execution option
        /// </summary>
        public TaskStrategy Strategy;

        Action _action;
        Delegate _action2;
        protected IEnumerator _routine;

        protected List<Delegate> CompleteList = new List<Delegate>();
        protected List<Delegate> SuccessList = new List<Delegate>();

        #endregion

        #region properties

        private TaskStatus _status;
        public TaskStatus Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;
                _status = value;
                
                if (IsCompleted)
                    OnTaskComplete();
            }
        }

        public Exception Exception { get; set; }
        
        #endregion

        #region computed properties
        public bool IsRunning
        {
            get { return Status == TaskStatus.Running; }
        }

        public bool IsCompleted
        {
            get { return Status == TaskStatus.Success || Status == TaskStatus.Faulted; }
        }

        public bool IsFaulted
        {
            get { return Status == TaskStatus.Faulted; }
        }

        public bool IsSuccess
        {
            get { return Status == TaskStatus.Success; }
        }
        #endregion

        #region constructor

        static UnityTask()
        {
            TaskManager.ConfirmInit();
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        protected UnityTask()
        {
            Status = TaskStatus.Created;
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        public UnityTask(TaskStrategy mode)
            : this()
        {
            Strategy = mode;
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
        /// Creates a new background task
        /// </summary>
        /// <param name="action"></param>
        public UnityTask(Action action)
            : this()
        {
            _action = action;
#if UNITY_WEBGL
            Strategy = TaskStrategy.MainThread;
#else
            Strategy = TaskStrategy.BackgroundThread;
#endif
        }

        /// <summary>
        /// Creates a new Task 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="mode"></param>
        public UnityTask(Action action, TaskStrategy mode)
            : this()
        {
            if (mode == TaskStrategy.Coroutine)
                throw new ArgumentException("Action tasks may not be coroutines");

            _action = action;
            Strategy = mode;
        }

        /// <summary>
        /// Creates a new Coroutine Task
        /// </summary>
        /// <param name="action"></param>
        public UnityTask(IEnumerator action)
            : this()
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _routine = action;
            Strategy = TaskStrategy.Coroutine;
        }


        /// <summary>
        /// Creates a new Coroutine Task
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        public UnityTask(IEnumerator action, object param)
            : this()
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _routine = action;
            Strategy = TaskStrategy.Coroutine;
            Paramater = param;
        }

        /// <summary>
        /// Creates a new background task with a parameter
        /// </summary>
        /// <param name="action"></param>
        /// <param name="paramater"></param>
        public UnityTask(Delegate action, object paramater)
            : this()
        {
            _action2 = action;
#if UNITY_WEBGL
            Strategy = TaskStrategy.MainThread;
#else
            Strategy = TaskStrategy.BackgroundThread;
#endif
            Paramater = paramater;
        }

        /// <summary>
        /// Creates a new Task with a parameter
        /// </summary>
        /// <param name="action"></param>
        /// <param name="paramater"></param>
        /// <param name="mode"></param>
        public UnityTask(Delegate action, object paramater, TaskStrategy mode)
            : this()
        {
            if (mode == TaskStrategy.Coroutine)
                throw new ArgumentException("Action tasks may not be coroutines");

            _action2 = action;
            Strategy = mode;
            Paramater = paramater;
        }

        #endregion

        #region Private

        protected virtual void Execute()
        {
            try
            {
                if (_action2 != null)
                {
                    _action2.DynamicInvoke(Paramater);
                }
                else if (_action != null)
                {
                    _action();
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
#if !UNITY_WEBGL
        /// <summary>
        /// Executes the task in background thread
        /// </summary>
#if UNITY_WSA
        protected async void RunOnBackgroundThread()
        {
            Status = TaskStatus.Running;
            await ThreadPool.RunAsync(o => Execute());
#else
        protected void RunOnBackgroundThread()
        {
            Status = TaskStatus.Running;
            ThreadPool.QueueUserWorkItem(state => Execute());
#endif
        }
#endif

        /// <summary>
        /// Executes the task in background thread
        /// </summary>
        protected void RunOnCurrentThread()
        {
            Status = TaskStatus.Running;
            Execute();
        }

        /// <summary>
        /// Executes the task on the main thread
        /// </summary>
        protected void RunOnMainThread()
        {
            Status = TaskStatus.Running;
#if UNITY_WEBGL
            Execute();
#else
            TaskManager.RunOnMainThread(Execute);
#endif
        }

        /// <summary>
        /// Executes the task in a coroutine
        /// </summary>
        protected void RunAsCoroutine()
        {
            Status = TaskStatus.Running;

            TaskManager.StartRoutine(new TaskManager.CoroutineCommand
            {
                Coroutine = _routine,
                OnComplete = OnRoutineComplete
            });
        }

        protected virtual void OnTaskComplete()
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
                        d.DynamicInvoke();
                }
            }
            SuccessList.Clear();
        }

        protected void OnRoutineComplete()
        {
            if (Status == TaskStatus.Running || Status == TaskStatus.Created)
                Status = TaskStatus.Success;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Executes the task
        /// </summary>
        public void Start()
        {
            if (IsCompleted)
            {
                return;
            }

            switch (Strategy)
            {

                case TaskStrategy.Coroutine:
                    RunAsCoroutine();
                    break;
#if UNITY_WEBGL
                default:
                    RunOnCurrentThread();
                    break;
#else
                case TaskStrategy.BackgroundThread:
                    if (DisableMultiThread)
                        RunOnCurrentThread();
                    else
                        RunOnBackgroundThread();
                    break;
                case TaskStrategy.CurrentThread:
                    RunOnCurrentThread();
                    break;
                case TaskStrategy.MainThread:
                    RunOnMainThread();
                    break;
#endif
            }
        }

        /// <summary>
        /// will throw if faulted
        /// </summary>
        /// <returns></returns>
        public UnityTask ThrowIfFaulted()
        {
            if (IsFaulted)
                throw Exception;
            return this;
        }

        /// <summary>
        /// Thread.Sleep
        /// </summary>
        /// <param name="millisecondTimeout"></param>
#if UNITY_WSA
        public async static System.Threading.Tasks.Task Delay(int millisecondTimeout)
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(millisecondTimeout));
        }
#elif UNITY_WEBGL
        public static void Delay(int millisecondTimeout)
        {
            Debug.LogError("Delay not supported on WebGL");
        }
#else
        public static void Delay(int millisecondTimeout)
        {
            Thread.Sleep(millisecondTimeout);
        }
#endif

        public virtual void Dispose()
        {
            Status = TaskStatus.Created;
            Paramater = null;
            Exception = null;
            _action = null;
            _action2 = null;
            _routine = null;
            CompleteList.Clear();
            SuccessList.Clear();
        }

        #endregion

        #region wait
        /// <summary>
        /// Wait for the task to complete in an iterator coroutine
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitRoutine()
        {
            while (IsRunning || CompleteList.Count > 0)
            {
                yield return 1;
            }
        }

        /// <summary>
        /// Waits for the task to complete
        /// </summary>
        public UnityTask Wait()
        {
            if (TaskManager.IsMainThread && !DisableMultiThread)
            {
                Debug.LogWarning("Use WaitRoutine in coroutine to wait in main thread");
            }

            Delay(10);

            while (IsRunning || CompleteList.Count > 0)
            {
                Delay(10);
            }

            return this;
        }
        #endregion

        #region continue with
        /// <summary>
        /// Called after the task is complete
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public UnityTask ContinueWith(Action<UnityTask> action)
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
        /// Called after the task is complete
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public K ContinueWith<K>(Action<K> action) where K : UnityTask
        {
            if (IsCompleted)
            {
                action((K)this);
            }
            else
            {
                CompleteList.Add(action);
            }
            return (K)this;
        }

        /// <summary>
        /// Called after a successful task execution
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public UnityTask OnSuccess(Action action)
        {
            if (IsCompleted && IsSuccess)
            {
                action();
            }
            else
            {
                SuccessList.Add(action);
            }

            return this;
        }
        #endregion
    }
}
