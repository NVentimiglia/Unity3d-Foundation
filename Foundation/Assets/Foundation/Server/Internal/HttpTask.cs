// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using Foundation.Tasks;
using FullSerializer;

namespace Foundation.Server
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
        /// timeout
        /// </summary>
        bool IsWebException { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Specific error details
        /// </summary>
        HttpMetadata Metadata { get; set; }
        
        /// <summary>
        /// WWW Json to String
        /// </summary>
        void DeserializeResult();

        /// <summary>
        /// custom complete
        /// </summary>
        /// <param name="ex"></param>
        void Complete(Exception ex = null);
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
        /// timeout
        /// </summary>
        public bool IsWebException { get; set; }

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


        public static HttpTask Failure(string error)
        {
            return new HttpTask { Strategy = TaskStrategy.Custom, Status = TaskStatus.Faulted, Exception = new Exception(error) };
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
        /// timeout
        /// </summary>
        public bool IsWebException { get; set; }

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
        
        public static HttpTask<T> Failure(string error)
        {
          return new HttpTask<T> { Strategy = TaskStrategy.Custom, Status = TaskStatus.Faulted, Exception = new Exception(error) };
        }   
    }
}