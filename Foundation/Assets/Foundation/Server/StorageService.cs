// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
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
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Query<T>(ODataQuery<T> query, Action<Response<T[]>> callback) where T : class
        {
            var meta = StorageMetadata.GetMetadata<T>();

            HttpPostAsync(meta.TableName, query, callback);
        }

        /// <summary>
        /// Reads a single of object from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Get<T>(string id, Action<Response<T>> callback) where T : class
        {
            if (!IsAuthenticated)
            {
                callback(new Response<T>(new Exception("Not authenticated")));
                return;
            }
            
            StorageMetadata.RegisterType<T>();

            HttpPostAsync("Get", id, callback);
        }

        /// <summary>
        /// Reads a set of objects from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void GetSet<T>(string[] ids, Action<Response<T[]>> callback) where T : class
        {
            if (!IsAuthenticated)
            {
                callback(new Response<T[]>(new Exception("Not authenticated")));
                return;
            }


            StorageMetadata.RegisterType<T>();

            HttpPostAsync("GetSet", ids, callback);
        }

        /// <summary>
        /// Saves a new object server side.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Create<T>(T entity, Action<Response> callback) where T : class
        {
            Create(entity, StorageACL.Public, null, callback);
        }

        /// <summary>
        /// Saves a new object server side. Includes write protection (AVL).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="acl">protection group</param>
        /// <param name="param">User name</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Create<T>(T entity, StorageACL acl, string param, Action<Response> callback) where T : class
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }
            
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

            HttpPostAsync("Create", model, callback);
        }

        /// <summary>
        /// Saves an existing object server side 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Update<T>(T entity, Action<Response> callback) where T : class
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }
            
            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                ModifiedOn = meta.GetModified(entity),
            };

            HttpPostAsync("Update", model, callback);
        }

        /// <summary>
        /// Syncs with DB. returns newest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Sync<T>(T entity, Action<Response<T>> callback) where T : class
        {
            if (!IsAuthenticated)
            {
                callback(new Response<T>(new Exception("Not authenticated")));
                return;
            }


            var meta = StorageMetadata.GetMetadata<T>();

            var model = new StorageRequest
            {
                ObjectId = meta.GetId(entity),
                ObjectScore = float.Parse(meta.GetScore(entity)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(entity),
                ModifiedOn = meta.GetModified(entity),
            };

            HttpPostAsync("Sync", model, callback);
        }


        /// <summary>
        /// Saves an existing object set server side 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void UpdateSet<T>(T[] entities, Action<Response> callback) where T : class
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }
            
            var meta = StorageMetadata.GetMetadata<T>();

            var model = entities.Select(o => new StorageRequest
            {
                ObjectId = meta.GetId(o),
                ObjectScore = float.Parse(meta.GetScore(o)),
                ObjectType = meta.TableName,
                ObjectData = JsonSerializer.Serialize(o),
            }).ToArray();

            HttpPostAsync("UpdateSet", model, callback);
        }

        /// <summary>
        /// Updates a single property on an existing object
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void UpdateProperty(string id, string propertyName, string propertyValue, Action<Response> callback)
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }

            var model = new StorageProperty
            {
                ObjectId = id,
                PropertyName = propertyName,
                PropertyValue = propertyValue,
            };

            HttpPostAsync("UpdateProperty", model, callback);
        }

        /// <summary>
        /// Increments / Decrements a single property.
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="delta">change</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void UpdateDelta(string id, string propertyName, float delta,  Action<Response> callback)
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (delta == 0)
            {
                UnityEngine.Debug.LogWarning("Delta of 0 is no delta at all. please use +/- 1 minimum.");
                callback(new Response());
                return;
            }

            var model = new StorageDelta
            {
                ObjectId = id,
                PropertyName = propertyName,
                Delta = delta,
                IsFloat = true,
            };

            HttpPostAsync("UpdateDelta", model, callback);
        }

        /// <summary>
        /// Increments / Decrements a single property.
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="delta">change</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void UpdateDelta(string id, string propertyName, int delta, Action<Response> callback)
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }

            if (delta == 0)
            {
                UnityEngine.Debug.LogWarning("Delta of 0 is no delta at all. please use +/- 1 minimum.");
                callback(new Response());
                return;
            }

            var model = new StorageDelta
            {
                ObjectId = id,
                PropertyName = propertyName,
                Delta = delta,
                IsFloat = false,
            };

            HttpPostAsync("UpdateDelta", model, callback);
        }

        /// <summary>
        /// Deletes the entity serverside
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Delete<T>(T entity, Action<Response> callback) where T : class
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }

            var meta = StorageMetadata.GetMetadata<T>();

            HttpPostAsync("Delete", meta.GetId(entity), callback);
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Reads a collection of objects which match a cloud query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public UnityTask<T[]> Query<T>(ODataQuery<T> query) where T : class
        {
            var task = new UnityTask<T[]>(TaskStrategy.Custom);
            Query(query, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Reads a single of object from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public UnityTask<T> Get<T>(string id) where T : class
        {
            var task = new UnityTask<T>(TaskStrategy.Custom);
            Get(id, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Reads a set of objects from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        public UnityTask<T[]> GetSet<T>(string[] ids) where T : class
        {
            var task = new UnityTask<T[]>(TaskStrategy.Custom);
            GetSet(ids, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Saves a new object serverside.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public UnityTask Create<T>(T entity) where T : class
        {
            var task = new UnityTask(TaskStrategy.Custom);
            Create(entity, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Saves a new object serverside. Includes write protection (AVL).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="acl">protection group</param>
        /// <param name="param">User name</param>
        /// <returns></returns>
        public UnityTask Create<T>(T entity, StorageACL acl, string param) where T : class
        {
            var task = new UnityTask(TaskStrategy.Custom);
            Create(entity, acl, param, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Saves an existing object serverside 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public UnityTask Update<T>(T entity) where T : class
        {
            var task = new UnityTask(TaskStrategy.Custom);
            Update(entity, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Syncs with DB. returns newest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public UnityTask<T> Sync<T>(T entity) where T : class
        {
            var task = new UnityTask<T>(TaskStrategy.Custom);
            Sync(entity, task.FromResponse());
            return task;
        }
        
        /// <summary>
        /// Saves an existing object set serverside 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public UnityTask UpdateSet<T>(T[] entities) where T : class
        {
            var task = new UnityTask(TaskStrategy.Custom);
            UpdateSet(entities, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Updates a single property on an existing object
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public UnityTask UpdateProperty(string id, string propertyName, string propertyValue)
        {
            var task = new UnityTask(TaskStrategy.Custom);
            UpdateProperty(id, propertyName, propertyValue, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Increments / Decrements a single property.
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="delta">change</param>
        /// <returns></returns>
        public UnityTask UpdateDelta(string id, string propertyName, float delta = 1)
        {
            var task = new UnityTask(TaskStrategy.Custom);
            UpdateDelta(id, propertyName, delta, task.FromResponse());
            return task;
        }
    

        /// <summary>
        /// Increments / Decrements a single property.
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="propertyName"></param>
        /// <param name="delta">change</param>
        /// <returns></returns>
        public UnityTask UpdateDelta(string id,  string propertyName, int delta = 1)
        {
            var task = new UnityTask(TaskStrategy.Custom);
            UpdateDelta(id, propertyName, delta, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Deletes the entity serverside
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public UnityTask Delete<T>(T entity) where T : class
        {
            var task = new UnityTask(TaskStrategy.Custom);
            Delete(entity, task.FromResponse());
            return task;
        }

        #endregion
    }
}