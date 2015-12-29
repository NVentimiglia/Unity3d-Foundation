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
    /// A http client which returns HttpTasks's
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
        /// timeout
        /// </summary>
        public float Timeout = 11f;
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

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpTask GetAsync(string url)
        {
            var state = new HttpTask();

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
            var state = new HttpTask<T>();

            TaskManager.StartRoutine(GetAsync(state, url));

            return state;
        }

        /// <summary>
        /// Begins the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpTask PostAsync(string url)
        {
            var state = new HttpTask();

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
            var state = new HttpTask();

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
            var state = new HttpTask<T>();

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
            var state = new HttpTask<T>();

            TaskManager.StartRoutine(PostAsync(state, url, content));

            return state;
        }
        #endregion

        #region internal
        
        // List of disposed WWW. Shorten around 60 second Android Timeout
        List<WWW> _disposed = new List<WWW>();

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
                task.Complete(ex);
                yield break;
            }
            
            yield return TaskManager.StartRoutine(HandleWWWAsync(task, www));
        }

        IEnumerator PostAsync(IHttpTask task, string url, string content)
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
                task.Complete(ex);
                yield break;
            }


            yield return TaskManager.StartRoutine(HandleWWWAsync(task, www));
        }

        IEnumerator HandleWWWAsync(IHttpTask task, WWW www)
        {
            if (!www.isDone)
            {
                TaskManager.StartRoutine(StartTimeout(www, Timeout));
                yield return www;
            }

            if (_disposed.Contains(www))
            {
                task.StatusCode = HttpStatusCode.RequestTimeout;
                task.Complete(new Exception("Request timed out"));
                task.IsWebException = true;
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
                    task.Complete(new HttpException(task.Metadata.Message, task.StatusCode, task.Metadata.ModelState));
                }
                else
                {
                    task.Complete(new Exception(RemoveReturn(www.error)));
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

                task.Content = www.text;
                task.DeserializeResult();
                task.Complete();
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
            return s.Replace("\r", "");
        }
        #endregion
    }
}