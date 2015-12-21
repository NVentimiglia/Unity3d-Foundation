// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using Foundation.Server.Api;
using FullSerializer;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// Encapsulates Api Http Communication
    /// </summary>
    /// <remarks>
    /// Inherent to access your own strongly typed server side controllers
    /// </remarks>
    public class ServiceClient
    {
        #region Shared

        protected CloudConfig Config
        {
            get { return CloudConfig.Instance; }
        }

        protected AccountService AccountService
        {
            get { return AccountService.Instance; }
        }

        private HttpTaskService _client;
        protected HttpTaskService Client
        {
            get
            {

                if (_client == null)
                {
                    _client = new HttpTaskService();
                }
                return _client;
            }
        }

        public string ControllerName { get; private set; }
        
        public ServiceClient(string controllerName)
        {
            ControllerName = controllerName;
        }

        void AddHeaders()
        {
            Client.RequestHeaders[AccountConstants.APPLICATIONID] = Config.Key;

            if (!string.IsNullOrEmpty(AccountService.SessionToken))
                Client.RequestHeaders[AccountConstants.SESSION] = AccountService.SessionToken;

            if (!string.IsNullOrEmpty(AccountService.AuthorizationToken))
                Client.RequestHeaders[AccountConstants.AUTHORIZATION] = AccountService.AuthorizationToken;
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Posts a get request against a IQueryable OData data source
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="query">odata query</param>
        /// <returns>found entity array of type T</returns>
        public HttpTask<T[]> Post<T>(ODataQuery<T> query) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T[]>(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{1}/Query/{0}", query, ControllerName);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync<T[]>(url);
        }

        /// <summary>
        /// Posts a get request against a IQueryable OData data source
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="prefix">prefix to odata query</param>
        /// <param name="query">odata query</param>
        /// <returns>found entity array of type T</returns>
        public HttpTask<T[]> Post<T>(string prefix, ODataQuery<T> query) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T[]>(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{0}/Query/{1}{2}", ControllerName, prefix, query);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync<T[]>(url);
        }
        
        
        /// <summary>
        /// Posts the HTTP request to the server
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="method">controller method to call</param>
        /// <returns>response of type T</returns>
        public HttpTask<T> Post<T>(string method) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T>(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{0}/{1}", ControllerName, method);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync<T>(url);
        }

        /// <summary>
        /// Posts the HTTP request to the server
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="method">controller method to call</param>
        /// <param name="id">id paramater</param>
        /// <returns>response of type T</returns>
        public HttpTask<T> Post<T>(string method, string id) where T : class 
        {
            if (!Config.IsValid)
                return new HttpTask<T>(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{0}/{1}/{2}", ControllerName, method, id);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync<T>(url);
        }

        /// <summary>
        /// Posts the HTTP request to the server
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="method">controller method to call</param>
        /// <param name="entity">dto</param>
        /// <returns>response of type T</returns>
        public HttpTask<T> Post<T>(string method, object entity) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T>(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{0}/{1}", ControllerName, method);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync<T>(url, JsonSerializer.Serialize(entity));
        }


        /// <summary>
        /// Posts the HTTP request to the server
        /// </summary>
        /// <param name="method">controller method to call</param>
        /// <param name="entity">dto</param>
        /// <returns>Metadata</returns>
        public HttpTask Post(string method, object entity)
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{0}/{1}", ControllerName, method);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync(url, JsonSerializer.Serialize(entity));
        }

        /// <summary>
        /// Posts the HTTP request to the server
        /// </summary>
        /// <param name="method">controller method to call</param>
        /// <param name="id">id paramater</param>
        /// <returns>Metadata</returns>
        public HttpTask Post(string method, string id)
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{0}/{1}/{2}", ControllerName, method, id);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync(url);
        }

        /// <summary>
        /// Posts the HTTP request to the server
        /// </summary>
        /// <param name="method">controller method to call</param>
        /// <returns>Metadata</returns>
        public HttpTask Post(string method)
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            AddHeaders();

            var action = string.Format("api/{0}/{1}", ControllerName, method);

            var url = Config.Path.EndsWith("/") ? Config.Path + action : Config.Path + "/" + action;

            return Client.PostAsync(url);
        }

        //

        #endregion
    }
}