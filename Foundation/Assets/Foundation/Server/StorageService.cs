// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using System.Linq;
using Foundation.Server.Api;
using FullSerializer;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// CRUD service for untyped json objects
    /// </summary>
    public class StorageService 
    {
        #region Internal

        public static readonly StorageService Instance = new StorageService();

        public CloudConfig Config
        {
            get { return CloudConfig.Instance; }
        }

        public AccountService AccountService
        {
            get { return AccountService.Instance; }
        }

        public readonly ServiceClient ServiceClient = new ServiceClient("Storage");
        
        #endregion

        #region Public Method

        /// <summary>
        /// Reads a collection of objects which match a cloud query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public HttpTask<T[]> Query<T>(ODataQuery<T> query) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T[]>(new Exception("Configuration is invalid"));
            
            var meta = StorageMetadata.GetMetadata<T>();

            return ServiceClient.Post(meta.TableName, query);
        }

        /// <summary>
        /// Reads a single of object from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpTask<T> Get<T>(string id) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T>(new Exception("Configuration is invalid"));
            
            StorageMetadata.RegisterType<T>();

            return ServiceClient.Post<T>("Get", id);
        }

        /// <summary>
        /// Reads a set of objects from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        public HttpTask<T[]> GetSet<T>(string[] ids) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T[]>(new Exception("Configuration is invalid"));

            StorageMetadata.RegisterType<T>();

            return ServiceClient.Post<T[]>("GetSet", ids);
        }

        /// <summary>
        /// Saves a new object serverside.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask Create<T>(T entity) where T : class
        {
            return Create(entity, StorageACL.Public, null);
        }

        /// <summary>
        /// Saves a new object serverside. Includes write protection (AVL).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="acl">protection group</param>
        /// <param name="param">User name</param>
        /// <returns></returns>
        public HttpTask Create<T>(T entity, StorageACL acl, string param) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                AppId = Config.Key,
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                AclParam = param,
                AclType = (StorageACLType)acl,
                ModifiedOn = meta.GetModified(entity),
            };

            return ServiceClient.Post("Create", model);
        }

        /// <summary>
        /// Saves an existing object serverside 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask Update<T>(T entity) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                AppId = Config.Key,
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                ModifiedOn = meta.GetModified(entity),
            };

            return ServiceClient.Post("Update", model);
        }

        /// <summary>
        /// Syncs with DB. returns newest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask<T> Sync<T>(T entity) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask<T>(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask<T>(new Exception("Not Authenticated"));

            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                AppId = Config.Key,
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                ModifiedOn = meta.GetModified(entity),
            };

            return ServiceClient.Post<T>("Sync", model);
        }


        /// <summary>
        /// Saves an existing object set serverside 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public HttpTask UpdateSet<T>(T[] entities) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            var meta = StorageMetadata.GetMetadata<T>();

            var model = entities.Select(o => new StorageRequest
            {
                AppId = Config.Key,
                ObjectId = meta.GetId(o),
                ObjectScore = float.Parse(meta.GetScore(o)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(o),
            }).ToArray();

            return ServiceClient.Post("UpdateSet", model);
        }

        /// <summary>
        /// Updates a single property on an existing object
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public HttpTask UpdateProperty(string id, string propertyName, string propertyValue) 
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            var model = new StorageProperty
            {
                AppId = Config.Key,
                ObjectId = id,
                PropertyName = propertyName,
                PropertyValue = propertyValue,
            };

            return ServiceClient.Post("UpdateProperty", model);
        }

        /// <summary>
        /// Increments / Decrements a single property.
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="delta">change</param>
        /// <returns></returns>
        public HttpTask UpdateDelta(string id, string propertyName, float delta = 1) 
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            var model = new StorageDelta
            {
                AppId = Config.Key,
                ObjectId = id,
                PropertyName = propertyName,
                Delta = delta,
                IsFloat = true,
            };

            return ServiceClient.Post("UpdateDelta", model);
        }

        /// <summary>
        /// Increments / Decrements a single property.
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="delta">change</param>
        /// <returns></returns>
        public HttpTask UpdateDelta(string id,  string propertyName, int delta = 1) 
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            var model = new StorageDelta
            {
                AppId = Config.Key,
                ObjectId = id,
                PropertyName = propertyName,
                Delta = delta,
                IsFloat = false,
            };

            return ServiceClient.Post("UpdateDelta", model);
        }

        /// <summary>
        /// Deletes the entity serverside
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask Delete<T>(T entity) where T : class
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            var meta = StorageMetadata.GetMetadata<T>();

            return ServiceClient.Post("Delete", meta.GetId(entity));
        }

        #endregion
    }
}