// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System.Linq;
using Foundation.Server.Api;
using Foundation.Tasks;
using FullSerializer;

namespace Foundation.Server
{
    /// <summary>
    /// CRUD service for untyped json objects
    /// </summary>
    public class StorageService : ServiceClientBase
    {
        #region Internal

        public static readonly StorageService Instance = new StorageService();

        public StorageService() : base("Storage"){}
        
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
            var meta = StorageMetadata.GetMetadata<T>();

            return HttpPost(meta.TableName, query);
        }

        /// <summary>
        /// Reads a single of object from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpTask<T> Get<T>(string id) where T : class
        {
            if (!IsAuthenticated)
                return HttpTask<T>.Failure("Not authenticated");
            

            StorageMetadata.RegisterType<T>();

            return HttpPost<T>("Get", id);
        }

        /// <summary>
        /// Reads a set of objects from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        public HttpTask<T[]> GetSet<T>(string[] ids) where T : class
        {
            if (!IsAuthenticated)
                return HttpTask<T[]>.Failure("Not authenticated");

            StorageMetadata.RegisterType<T>();

            return HttpPost<T[]>("GetSet", ids);
        }

        /// <summary>
        /// Saves a new object serverside.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask Create<T>(T entity) where T : class
        {
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

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
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                AclParam = param,
                AclType = (StorageACLType)acl,
                ModifiedOn = meta.GetModified(entity),
            };

            return HttpPost("Create", model);
        }

        /// <summary>
        /// Saves an existing object serverside 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask Update<T>(T entity) where T : class
        {
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                ModifiedOn = meta.GetModified(entity),
            };

            return HttpPost("Update", model);
        }

        /// <summary>
        /// Syncs with DB. returns newest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask<T> Sync<T>(T entity) where T : class
        {
            if (!IsAuthenticated)
                return HttpTask<T>.Failure("Not authenticated");

            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                ModifiedOn = meta.GetModified(entity),
            };

            return HttpPost<T>("Sync", model);
        }


        /// <summary>
        /// Saves an existing object set serverside 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public HttpTask UpdateSet<T>(T[] entities) where T : class
        {
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

            var meta = StorageMetadata.GetMetadata<T>();

            var model = entities.Select(o => new StorageRequest
            {
                ObjectId = meta.GetId(o),
                ObjectScore = float.Parse(meta.GetScore(o)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(o),
            }).ToArray();

            return HttpPost("UpdateSet", model);
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
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

            var model = new StorageProperty
            {
                ObjectId = id,
                PropertyName = propertyName,
                PropertyValue = propertyValue,
            };

            return HttpPost("UpdateProperty", model);
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
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

            var model = new StorageDelta
            {
                ObjectId = id,
                PropertyName = propertyName,
                Delta = delta,
                IsFloat = true,
            };

            return HttpPost("UpdateDelta", model);
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
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

            var model = new StorageDelta
            {
                ObjectId = id,
                PropertyName = propertyName,
                Delta = delta,
                IsFloat = false,
            };

            return HttpPost("UpdateDelta", model);
        }

        /// <summary>
        /// Deletes the entity serverside
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask Delete<T>(T entity) where T : class
        {
            if (!IsAuthenticated)
                return HttpTask.Failure("Not authenticated");

            var meta = StorageMetadata.GetMetadata<T>();

            return HttpPost("Delete", meta.GetId(entity));
        }

        #endregion
    }
}