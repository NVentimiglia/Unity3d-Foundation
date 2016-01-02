using System.Collections.Generic;
using System.Net;

namespace Foundation.Tasks
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
        /// HTTP Status Code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Validation Errors. apiEntity.PropertyName by errors[]
        /// </summary>
        public Dictionary<string, string[]> ModelState { get; set; }
    }
}