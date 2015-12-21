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
using System.Text;
using Foundation.Server.Api;
using Foundation.Tasks;
using FullSerializer;
using UnityEngine;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// A http client which returns HttpTasks's
    /// </summary>
    public class HttpTaskService
    {
        /// <summary>
        /// content type Header. Default value of "application/json"
        /// </summary>
        public string ContentType = "application/json";

        /// <summary>
        /// Accept Header. Default value of "application/json"
        /// </summary>
        public string Accept = "application/json";

        /// <summary>
        /// timeout
        /// </summary>
        public float Timeout = 11f;
        
        /// <summary>
        /// Http Headers Collection
        /// </summary>
        public Dictionary<string, string> RequestHeaders = new Dictionary<string, string>();

        /// <summary>
        /// List of disposed WWW. Shorten around 60 second Android Timeout
        /// </summary>
        List<WWW> _disposed = new List<WWW>();

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpTask GetAsync(string url)
        {
            var state = new HttpTask
            {
                Status = TaskStatus.Running,
            };

            TaskManager.StartRoutine(GetAsync(state, url));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpTask<T> GetAsync<T>(string url)
        {
            var state = new HttpTask<T>
            {
                Status = TaskStatus.Running,
            };

            TaskManager.StartRoutine(GetAsync(state, url));

            return state;
        }

        IEnumerator GetAsync(IHttpTask task, string url)
        {

            WWW www;
            try
            {
                www = new WWW(url);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                task.Exception = ex;
                task.Status = TaskStatus.Faulted;
                yield break;
            }
            
            TaskManager.StartRoutine(StartTimeout(www, Timeout));

            yield return www;


            if (_disposed.Contains(www))
            {
                task.StatusCode = HttpStatusCode.RequestTimeout;
                task.Exception = new Exception("Request timed out");
                task.Status = TaskStatus.Faulted;
                _disposed.Remove(www);
                yield break;
            }

            task.StatusCode = GetCode(www);

            if (www.responseHeaders.ContainsKey("MESSAGE"))
            {
                var error = www.responseHeaders["MESSAGE"];
                task.Metadata = JsonSerializer.Deserialize<HttpMetadata>(error);
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                if (task.Metadata != null)
                {
                    task.Exception = new HttpException( task.Metadata.Message, task.StatusCode, task.Metadata.ModelState);
                    task.Status = TaskStatus.Faulted;
                }
                else
                {
                    task.Exception = new Exception(RemoveReturn(www.error));
                    task.Status = TaskStatus.Faulted;
                }
            }
            else
            {
                if (www.responseHeaders.ContainsKey(AccountConstants.AUTHORIZATION))
                {
                    task.Session = www.responseHeaders[AccountConstants.AUTHORIZATION];
                    AccountService.Instance.AuthorizationToken = task.Session;
                    AccountService.Instance.Save();
                }

                if (www.responseHeaders.ContainsKey(AccountConstants.SESSION))
                {
                    task.Session = www.responseHeaders[AccountConstants.SESSION];
                    AccountService.Instance.SessionToken = task.Session;
                    AccountService.Instance.Save();
                }

                task.Content = www.text;
                task.DeserializeResult();
                task.Status = TaskStatus.Success;
            }
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpTask PostAsync(string url)
        {
            var state = new HttpTask
            {
                Status = TaskStatus.Running,
            };

            TaskManager.StartRoutine(PostAsync(state, url, null));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public HttpTask PostAsync(string url, string content)
        {
            var state = new HttpTask
            {
                Status = TaskStatus.Running,
            };

            TaskManager.StartRoutine(PostAsync(state, url, content));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpTask<T> PostAsync<T>(string url) 
        {
            var state = new HttpTask<T>
            {
                Status = TaskStatus.Running,
            };

            TaskManager.StartRoutine(PostAsync(state, url, null));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public HttpTask<T> PostAsync<T>(string url, string content)
        {
            var state = new HttpTask<T>
            {
                Status = TaskStatus.Running,
            };

            TaskManager.StartRoutine(PostAsync(state, url, content));

            return state;
        }
        
        IEnumerator PostAsync(IHttpTask task, string url, string content)
        {
            if (!RequestHeaders.ContainsKey("Accept"))
                RequestHeaders.Add("Accept", Accept);
            if (!RequestHeaders.ContainsKey("Content-Type"))
                RequestHeaders.Add("Content-Type", ContentType);

            WWW www;
            try
            {
                www = new WWW(url, content == null ? new byte[1] : Encoding.UTF8.GetBytes(content), RequestHeaders);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                task.Exception = ex;
                task.Status = TaskStatus.Faulted;
                yield break;
            }

            if (!www.isDone)
            {
                TaskManager.StartRoutine(StartTimeout(www, Timeout));
                yield return www;
            }


            if (_disposed.Contains(www))
            {
                task.StatusCode = HttpStatusCode.RequestTimeout;
                task.Exception = new Exception("Request timed out");
                task.Status = TaskStatus.Faulted;
                _disposed.Remove(www);
                yield break;
            }

            task.StatusCode = GetCode(www);

            if (www.responseHeaders.ContainsKey("MESSAGE"))
            {
                var error = www.responseHeaders["MESSAGE"];
                task.Metadata = JsonSerializer.Deserialize<HttpMetadata>(error);
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                if (task.Metadata != null)
                {
                    task.Exception = new HttpException(task.Metadata.Message, task.StatusCode, task.Metadata.ModelState);
                    task.Status = TaskStatus.Faulted;
                }
                else
                {
                    task.Exception = new Exception(RemoveReturn(www.error));
                    task.Status = TaskStatus.Faulted;
                }
            }
            else
            {
                if (www.responseHeaders.ContainsKey(AccountConstants.AUTHORIZATION))
                {
                    task.Session = www.responseHeaders[AccountConstants.AUTHORIZATION];
                    AccountService.Instance.AuthorizationToken = task.Session;
                    AccountService.Instance.Save();
                }

                if (www.responseHeaders.ContainsKey(AccountConstants.SESSION))
                {
                    task.Session = www.responseHeaders[AccountConstants.SESSION];
                    AccountService.Instance.SessionToken = task.Session;
                    AccountService.Instance.Save();
                }

                task.Content = www.text;
                task.DeserializeResult();
                task.Status = TaskStatus.Success;
            }
        }
   
        IEnumerator StartTimeout(WWW www, float time)
        {
            yield return new WaitForSeconds(time);

            if (!www.isDone)
            {
                www.Dispose();
                _disposed.Add(www);
            }
        }

        HttpStatusCode GetCode(WWW www)
        {
            if (!www.responseHeaders.ContainsKey("STATUS"))
            {
                return 0;
            }

            var code = www.responseHeaders["STATUS"].Split(' ')[1];
            return (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), code);
        }

        string RemoveReturn(string s)
        {
            return s.Replace("\r","");
        }
    }
}