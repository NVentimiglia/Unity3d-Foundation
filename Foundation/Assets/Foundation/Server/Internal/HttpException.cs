// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using System.Net;

namespace Foundation.Server
{
    /// <summary>
    /// Extends with ModelState Errors
    /// </summary>
    public class HttpException : Exception
    {
        /// <summary>
        /// Validation Errors. apiEntity.PropertyName by errors[]
        /// </summary>
        public Dictionary<string, string[]> ModelState { get; set; }

        /// <summary>
        /// Status Code
        /// </summary>
        public HttpStatusCode Status { get; set; }

        public HttpException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public HttpException(string message, Exception inner, HttpStatusCode code, Dictionary<string, string[]> modelState)
            : base(message, inner)
        {
            ModelState = modelState;
            Status = code;
        }

        public HttpException(string message, HttpStatusCode code, Dictionary<string, string[]> modelState)
            : base(message)
        {
            ModelState = modelState;
            Status = code;
        }

        public IEnumerable<string> GetErrors()
        {
            yield return Message;

            if (ModelState != null && ModelState.Count > 0)
            {
                foreach (var key in ModelState.Keys)
                {
                    foreach (var s in ModelState[key])
                    {
                        yield return String.Format("{0} '{1}'", s, key);
                    }
                }
            }
        }
    }
}