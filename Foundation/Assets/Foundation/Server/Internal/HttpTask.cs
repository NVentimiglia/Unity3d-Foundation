// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Foundation.Tasks;
using FullSerializer;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// Error Response
    /// </summary>
    public class HttpMetadata
    {
        /// <summary>
        /// General error title
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Validation Errors. apiEntity.PropertyName by errors[]
        /// </summary>
        public Dictionary<string, string[]> ModelState { get; set; }
    }


    public interface IHttpTask
    {
        /// <summary>
        /// Computed from WebResponse
        /// </summary>
        string Content { get; set; }

        /// <summary>
        /// Server Session
        /// </summary>
        string Session { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Specific error details
        /// </summary>
        HttpMetadata Metadata { get; set; }

        Exception Exception { get; set; }

        TaskStatus Status { get; set; }

        void DeserializeResult();
    }


    /// <summary>
    /// Extends Task with Web Values
    /// </summary>
    public class HttpTask : UnityTask, IHttpTask
    {
        /// <summary>
        /// Computed from WebResponse
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Server Session
        /// </summary>
        public string Session { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Specific error details
        /// </summary>
        public HttpMetadata Metadata { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpTask()
        {
            Strategy = TaskStrategy.Custom;
        }

        public HttpTask(Exception ex)
        {
            Strategy = TaskStrategy.Custom;
            Exception = ex;
            Status = TaskStatus.Faulted;
            StatusCode = HttpStatusCode.BadRequest;
        }

        public void DeserializeResult()
        {
            // no result
        }
        public UnityTask AsTask()
        {
            var task = new UnityTask(TaskStrategy.Custom);
            if (IsCompleted)
            {
                task.Exception = Exception;
                task.Status = Status;
            }
            else
                TaskManager.StartRoutine(AsTaskAsync(task));
            return task;
        }

        private IEnumerator AsTaskAsync(UnityTask task)
        {
            while (!IsCompleted)
                yield return 1;

            task.Exception = Exception;
            task.Status = Status;
        }
    }

    /// <summary>
    /// Extends Task with Web Values
    /// </summary>
    /// <typeparam name="T">Serialized Result Type</typeparam>
    public class HttpTask<T> : UnityTask<T>, IHttpTask
    {
        /// <summary>
        /// Computed from WebResponse
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Server Session
        /// </summary>
        public string Session { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Specific error details
        /// </summary>
        public HttpMetadata Metadata { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpTask()
        {
            Strategy = TaskStrategy.Custom;
        }

        public HttpTask(Exception ex)
        {
            Strategy = TaskStrategy.Custom;
            Exception = ex;
            Status = TaskStatus.Faulted;
            StatusCode = HttpStatusCode.BadRequest;
        }

        /// <summary>
        /// Hack for Unity5 generic error
        /// </summary>
        public void DeserializeResult()
        {
            Result = JsonSerializer.Deserialize<T>(Content);
        }
        
        public UnityTask<T> AsTask()
        {
            var task = new UnityTask<T>(TaskStrategy.Custom);
            if (IsCompleted)
            {
                task.Result = Result;
                task.Exception = Exception;
                task.Status = Status;
            }
            else
                TaskManager.StartRoutine(AsTaskAsync(task));
            return task;
        }

        private IEnumerator AsTaskAsync(UnityTask<T> task)
        {
            while (!IsCompleted)
                yield return 1;

            task.Result = Result;
            task.Exception = Exception;
            task.Status = Status;
        }
    }
}