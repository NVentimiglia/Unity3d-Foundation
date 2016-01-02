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
        /// Working
        /// </summary>
        Pending,
        /// <summary>
        /// Exception as thrown or otherwise stopped early
        /// </summary>
        Faulted,
        /// <summary>
        /// Complete without error
        /// </summary>
        Success,
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
    ///    yield return task;
    ///
    ///    // check exceptions
    ///    if(task.IsFaulted)
    ///        Debug.LogException(task.Exception)
    ///</code>
    ///</example>
    public partial class UnityTask : CustomYieldInstruction, IDisposable
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
        
        #region properties

        /// <summary>
        /// Run execution path
        /// </summary>
        public TaskStrategy Strategy;

        /// <summary>
        /// Error
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Run State
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// Bag for additional http meta data
        /// </summary>
        public HttpMetadata Metadata { get; set; }

        /// <summary>
        /// Custom Yield
        /// </summary>
        public override bool keepWaiting
        {
            get { return !IsCompleted; }
        }

        public bool IsRunning
        {
            get { return Status == TaskStatus.Pending; }
        }

        public bool IsCompleted
        {
            get { return (Status == TaskStatus.Success || Status == TaskStatus.Faulted) && !HasContinuations; }
        }

        public bool IsFaulted
        {
            get { return Status == TaskStatus.Faulted; }
        }

        public bool IsSuccess
        {
            get { return Status == TaskStatus.Success; }
        }

        public bool HasContinuations { get; protected set; }
        #endregion

        #region private

        protected TaskStatus _status;
        protected Action _action;
        protected IEnumerator _routine;
        List<Delegate> _completeList;

        #endregion

        #region constructor

        static UnityTask()
        {
            TaskManager.ConfirmInit();
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        public UnityTask()
        {
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        public UnityTask(TaskStrategy mode)
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

        #endregion

        #region Private

        protected virtual void Execute()
        {
            try
            {
                if (_action != null)
                {
                    _action();
                }
                Status = TaskStatus.Success;
                OnTaskComplete();
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
#if UNITY_WSA
        protected async void RunOnBackgroundThread()
        {
            Status = TaskStatus.Running;
            await ThreadPool.RunAsync(o => Execute());
#else
        protected void RunOnBackgroundThread()
        {
            Status = TaskStatus.Pending;
            ThreadPool.QueueUserWorkItem(state => Execute());
#endif
        }
#endif

        protected void RunOnCurrentThread()
        {
            Status = TaskStatus.Pending;
            Execute();
        }

        protected void RunOnMainThread()
        {
            Status = TaskStatus.Pending;
#if UNITY_WEBGL
            Execute();
#else
            TaskManager.RunOnMainThread(Execute);
#endif
        }

        protected void RunAsCoroutine()
        {
            Status = TaskStatus.Pending;

            TaskManager.StartRoutine(new TaskManager.CoroutineCommand
            {
                Coroutine = _routine,
                OnComplete = OnRoutineComplete
            });
        }

        protected virtual void OnTaskComplete()
        {
            if (_completeList != null)
            {
                foreach (var d in _completeList)
                {
                    if (d != null)
                        d.DynamicInvoke(this);
                }
                _completeList = null;
            }
            HasContinuations = false;
        }

        protected void OnRoutineComplete()
        {
            if (Status == TaskStatus.Pending)
            {
                Status = TaskStatus.Success;
                OnTaskComplete();
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// For HTTP
        /// </summary>
        /// <param name="json"></param>
        public virtual void DeserializeResult(string json)
        {
            
        } 

        /// <summary>
        /// Runs complete logic, for custom tasks
        /// </summary>
        public virtual void Complete(Exception ex = null)
        {
            if (ex == null)
            {
                Exception = null;
                Status = TaskStatus.Success;
                OnTaskComplete();
            }
            else
            {
                Exception = ex;
                Status = TaskStatus.Faulted;
                OnTaskComplete();
            }
        }

        /// <summary>
        /// Executes the task
        /// </summary>
        public virtual void Start()
        {
            Status = TaskStatus.Pending;

            switch (Strategy)
            {

                case TaskStrategy.Custom:
                    break;
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
            Status = TaskStatus.Pending;
            Exception = null;
            _action = null;
            _routine = null;
            _completeList = null;
            HasContinuations = false;
        }

        public void AddContinue(Delegate action)
        {
            HasContinuations = true;
            if (_completeList == null)
            {
                _completeList = new List<Delegate>();
            }

            _completeList.Add(action);
        }
        #endregion
    }
}
