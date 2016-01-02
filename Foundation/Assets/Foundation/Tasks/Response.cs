using System;
using FullSerializer;

namespace Foundation.Tasks
{
    /// <summary>
    /// Callback response
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Optional, may be null on successful  calls
        /// </summary>
        public HttpMetadata Metadata;

        /// <summary>
        /// Not null if faulted
        /// </summary>
        public Exception Exception;

        public bool IsFaulted
        {
            get { return Exception != null; }
        }

        public bool IsSuccess
        {
            get { return !IsFaulted; }
        }

        public Response()
        {

        }

        public Response(Exception ex)
        {
            Exception = ex;
        }

        public Response(Response<string> jsonResponse)
        {
            Exception = jsonResponse.Exception;
            Metadata = jsonResponse.Metadata;
        }
    }

    /// <summary>
    /// Callback response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T> : Response
    {
        /// <summary>
        /// Response for success
        /// </summary>
        public T Result;

        public Response()
        {

        }

        public Response(T result)
        {
            Result = result;
        }

        public Response(Exception ex)
        {
            Exception = ex;
        }

        public Response(Response<string> jsonResponse)
        {
            if (!string.IsNullOrEmpty(jsonResponse.Result))
                Result = JsonSerializer.Deserialize<T>(jsonResponse.Result);
            Exception = jsonResponse.Exception;
            Metadata = jsonResponse.Metadata;
        }
    }

    public static class ResponseExt
    {
        public static Action<Response<string>> FromJsonResponse(this Action<Response> callback)
        {
            return (response) =>
            {
                callback(new Response(response));
            };
        }

        public static Action<Response<string>> FromJsonResponse<T>(this Action<Response<T>> callback)
        {
            return (response) =>
            {
                callback(new Response<T>(response));
            };
        }

        public static Action<Response<string>> FromJsonResponse(this UnityTask task)
        {
            return (response) =>
            {
                task.Metadata = response.Metadata;
                task.Complete(response.Exception);
            };
        }

        public static Action<Response<string>> FromJsonResponse<T>(this UnityTask<T> task)
        {
            return (response) =>
            {
                task.Metadata = response.Metadata;
                if (response.IsFaulted)
                    task.Complete(response.Exception);
                else
                    task.Complete(JsonSerializer.Deserialize<T>(response.Result));
            };
        }

        public static Action<Response> FromResponse(this UnityTask task)
        {
            return (response) =>
            {
                task.Metadata = response.Metadata;
                task.Complete(response.Exception);
            };
        }

        public static Action<Response<T>> FromResponse<T>(this UnityTask<T> task)
        {
            return (response) =>
            {
                task.Metadata = response.Metadata;
                if (response.IsFaulted)
                    task.Complete(response.Exception);
                else
                    task.Complete(response.Result);

            };
        }
    }
}