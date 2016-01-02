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

namespace Foundation.Server
{
    /// <summary>
    /// A http client which returns UnityTasks's
    /// </summary>
    public class HttpService
    {
        #region Settings
        /// <summary>
        /// content type Header. Default value of "application/json"
        /// </summary>
        public string ContentType = "application/json";

        /// <summary>
        /// Accept Header. Default value of "application/json"
        /// </summary>
        public string Accept = "application/json";

        /// <summary>
        /// timeout in seconds
        /// </summary>
        public int Timeout = 11;
        #endregion

        #region session

        /// <summary>
        /// Key used to acquire the session server side. Add to header.
        /// </summary>
        public static string SessionToken { get; set; }

        /// <summary>
        /// Key used to acquire the session server side. Add to header.
        /// </summary>
        public static string AuthorizationToken { get; set; }

        /// <summary>
        /// Is authorized
        /// </summary>
        public static bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(AuthorizationToken); }
        }

        /// <summary>
        /// Load session from disk
        /// </summary>
        public static void LoadSession()
        {
            SessionToken = PlayerPrefs.GetString("HttpSession:SessionToken", string.Empty);
            AuthorizationToken = PlayerPrefs.GetString("HttpSession:AuthorizationToken", string.Empty);
        }

        /// <summary>
        /// Saves to prefs
        /// </summary>
        public static void SaveSession()
        {
            PlayerPrefs.SetString("HttpSession:SessionToken", SessionToken);
            PlayerPrefs.SetString("HttpSession:AuthorizationToken", AuthorizationToken);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Delete session
        /// </summary>
        public static void ClearSession()
        {
            SessionToken = AuthorizationToken = string.Empty;
            SaveSession();
        }

        static HttpService()
        {
            LoadSession();
        }
        #endregion

        #region public interface

        //Callbacks

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void GetAsync(string url, Action<Response> callback)
        {
            TaskManager.StartRoutine(GetAsync(callback.FromJsonResponse(), url));
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void GetAsync<T>(string url, Action<Response<T>> callback)
        {
            TaskManager.StartRoutine(GetAsync(callback.FromJsonResponse(), url));
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void PostAsync(string url, Action<Response> callback)
        {
            TaskManager.StartRoutine(PostAsync(callback.FromJsonResponse(), url, null));
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public void PostAsync(string url, string content, Action<Response> callback)
        {
            TaskManager.StartRoutine(PostAsync(callback.FromJsonResponse(), url, content));
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void PostAsync<T>(string url, Action<Response<T>> callback)
        {
            TaskManager.StartRoutine(PostAsync(callback.FromJsonResponse(), url, null));
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void PostAsync<T>(string url, string content, Action<Response<T>> callback)
        {
            TaskManager.StartRoutine(PostAsync(callback.FromJsonResponse(), url, content));
        }

        // Async

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public UnityTask GetAsync(string url)
        {
            var state = new UnityTask();

            TaskManager.StartRoutine(GetAsync(state.FromJsonResponse(), url));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public UnityTask<T> GetAsync<T>(string url)
        {
            var state = new UnityTask<T>();

            TaskManager.StartRoutine(GetAsync(state.FromJsonResponse(), url));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public UnityTask PostAsync(string url)
        {
            var state = new UnityTask();

            TaskManager.StartRoutine(PostAsync(state.FromJsonResponse(), url, null));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public UnityTask PostAsync(string url, string content)
        {
            var state = new UnityTask();

            TaskManager.StartRoutine(PostAsync(state.FromJsonResponse(), url, content));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public UnityTask<T> PostAsync<T>(string url)
        {
            var state = new UnityTask<T>();

            TaskManager.StartRoutine(PostAsync(state.FromJsonResponse(), url, null));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public UnityTask<T> PostAsync<T>(string url, string content)
        {
            var state = new UnityTask<T>();

            TaskManager.StartRoutine(PostAsync(state.FromJsonResponse(), url, content));

            return state;
        }
        #endregion

        #region internal
        
        Dictionary<WWW, Action<Response<string>>> _pendingCalls = new Dictionary<WWW, Action<Response<string>>>();

        // Callbacks
        
        IEnumerator GetAsync(Action<Response<string>> task, string url)
        {

            WWW www;
            try
            {
                www = new WWW(url);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                task(new Response<string>
                {
                    Exception = ex
                });
                yield break;
            }

            yield return TaskManager.StartRoutine(HandleWWWAsync(task, www));
        }

        IEnumerator PostAsync(Action<Response<string>> task, string url, string content)
        {
            var headers = new Dictionary<string, string>
            {
                {"Accept", Accept},
                {"Content-Type", ContentType},
                {APIConstants.AUTHORIZATION, AuthorizationToken},
                {APIConstants.SESSION, SessionToken},
                {APIConstants.APPLICATIONID, ServerConfig.Instance.Key}
            };

            WWW www;
            try
            {
                www = new WWW(url, content == null ? new byte[1] : Encoding.UTF8.GetBytes(content), headers);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                task(new Response<string>
                {
                    Exception = ex
                });
                yield break;
            }


            yield return TaskManager.StartRoutine(HandleWWWAsync(task, www));
        }

        IEnumerator TimeoutResponse(Action<Response<string>> task, WWW www)
        {
            yield return new WaitForSeconds(Timeout);

            if (_pendingCalls.ContainsKey(www))
            {
                _pendingCalls.Remove(www);

                task(new Response<string>
                {
                    Exception = new Exception("Request timed out"),
                    Metadata = new HttpMetadata {Message = "Request timed out", StatusCode = HttpStatusCode.RequestTimeout}
                });
            }
        }

        IEnumerator HandleWWWAsync(Action<Response<string>> task, WWW www)
        {
            if (!www.isDone)
            {
                _pendingCalls.Add(www, task);

                TaskManager.StartRoutine(TimeoutResponse(task, www));

                yield return www;
            }

            //Timeout
            if (!_pendingCalls.ContainsKey(www))
            {
                yield break;
            }

            _pendingCalls.Remove(www);

            HttpMetadata meta;

            if (www.responseHeaders.ContainsKey("MESSAGE"))
            {
                var error = www.responseHeaders["MESSAGE"];
                meta = JsonSerializer.Deserialize<HttpMetadata>(error);
                meta.StatusCode = GetCode(www);
            }
            else
            {
                meta = new HttpMetadata {StatusCode = GetCode(www) };
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                if (!string.IsNullOrEmpty(meta.Message))
                {
                    task(new Response<string>
                    {
                        Metadata = meta,
                        Exception = new HttpException(meta.Message, meta.StatusCode, meta.ModelState)
                    });
                }
                else
                {
                    task(new Response<string>
                    {
                        Metadata = meta,
                        Exception = new Exception(RemoveReturn(www.error))
                    });
                }
            }
            
            else
            {
                if (www.responseHeaders.ContainsKey(APIConstants.AUTHORIZATION))
                {
                    AuthorizationToken = www.responseHeaders[APIConstants.AUTHORIZATION];
                }

                if (www.responseHeaders.ContainsKey(APIConstants.SESSION))
                {
                    SessionToken = www.responseHeaders[APIConstants.SESSION];
                }

                SaveSession();


                task(new Response<string>
                {
                   Metadata = meta,
                   Result = www.text,
                });
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
            return s.Replace("\r", "");
        }
        #endregion
    }
}