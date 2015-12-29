using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.OData.Query;
using Foundation.Server.Api;
using Foundation.Server.Infrastructure.Filters;
using Foundation.Server.Infrastructure.Helpers;
using Foundation.Server.Models;
using Newtonsoft.Json.Linq;

namespace Foundation.Server.Controllers
{
    /// <summary>
    /// Interface for dynamic object storage
    /// </summary>
    [Authorize]
    [Route("api/Storage")]
    public class StorageController : ApiControllerBase
    {
        /// <summary>
        /// OData Query. Max query size 50.
        /// </summary>
        /// <remarks>
        /// Query is applied against object data not metadata
        /// </remarks>
        /// <param name="type">Object Type</param>
        /// <param name="options">Verbs limited to Equals, NotEquals, Skip, Take and OrderBy</param>
        /// <returns>OData Query</returns>
        [HttpPost]
        [Route("api/Storage/Query/{type}")]
        public async Task<IEnumerable<JObject>> Get(string type, ODataQueryOptions options)
        {
            // Select Data
            var objects = await AppDatabase.Storage
                .OrderBy(o => o.ObjectScore)
                .ToArrayAsync();

            // Get Inner Object
            var a = objects.Select(o => o.GetData());

            // filter
            if (options.Filter != null)
            {
                var helper = new ODataFilterHelper();
                var items = helper.GetFilters(options.Filter.RawValue);

                a = items.Aggregate(a, helper.ApplyFilter);
            }

            // Orderby
            if (options.OrderBy != null)
            {
                var desc = options.OrderBy.RawValue.Contains("desc");
                var orderBy = options.OrderBy.RawValue.Split(' ').First();

                if (desc)
                    a = a.OrderByDescending(o => o.GetValue(orderBy));
                else
                    a = a.OrderBy(o => o.GetValue(orderBy));
            }

            var take = options.Top == null ? 25 : (options.Top.Value > 25 ? 25 : options.Top.Value);
            var skip = options.Skip == null ? 0 : options.Skip.Value;

            var b = a.Skip(skip).Take(take);

            return b;
        }

        /// <summary>
        /// Returns a single storage object by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [ApiCheckModelForNull]
        [Route("api/Storage/Get/{id}")]
        public async Task<IHttpActionResult> Get(string id)
        {
            var cloudObject = await AppDatabase.Storage.FindAsync(id);
            if (cloudObject == null)
            {
                return Ok();
            }

            return Ok(cloudObject.GetData());
        }

        /// <summary>
        /// Returns a set of storage object by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(JObject))]
        [HttpPost]
        [ApiValidateModelState]
        [ApiCheckModelForNull]
        [Route("api/Storage/GetSet/")]
        public async Task<IHttpActionResult> GetSet(string[] id)
        {

            var data = new List<JObject>();

            foreach (var i in id)
            {
                var cloudObject = await AppDatabase.Storage.FindAsync(i);
                if (cloudObject != null)
                {
                    data.Add(cloudObject.GetData());
                }

            }

            return Ok(data);
        }

        /// <summary>
        /// Creates the Storage Object
        /// </summary>
        /// <param name="cloudObject"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [ApiCheckModelForNull]
        [Authorize]
        [Route("api/Storage/Create")]
        public async Task<IHttpActionResult> Create(StorageObject cloudObject)
        {
            cloudObject.ModifiedOn = cloudObject.CreatedOn = DateTime.UtcNow;

            var entity = await AppDatabase.Storage.FindAsync(cloudObject.ObjectId);
            if (entity != null)
                return BadRequest("ObjectId is in use");

            AppDatabase.Storage.Add(cloudObject);
            await AppDatabase.SaveChangesAsync();

            return Ok(cloudObject.GetData());
        }

        /// <summary>
        /// Updates the Storage Object
        /// </summary>
        /// <param name="cloudObject"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [ApiCheckModelForNull]
        [Authorize]
        [Route("api/Storage/Update")]
        public async Task<IHttpActionResult> Update(StorageObject cloudObject)
        {
            var entity = await AppDatabase.Storage.FindAsync(cloudObject.ObjectId);

            if (entity == null)
                return await Create(cloudObject);

            // ACL
            switch (entity.AclType)
            {
                case StorageACLType.Admin:
                    if (!User.Identity.IsAuthenticated)
                        return Unauthorized();
                    break;
                case StorageACLType.User:
                    if (UserId != entity.AclParam)
                        return Unauthorized();
                    break;
            }

            if (entity.ObjectType != cloudObject.ObjectType)
            {
                //would cause caching errors
                return BadRequest("Can not change object type");
            }

            entity.ModifiedOn = DateTime.UtcNow;
            entity.ObjectData = cloudObject.ObjectData;
            entity.ObjectScore = cloudObject.ObjectScore;

            //  Context.Update(entity);
            await AppDatabase.SaveChangesAsync();

            return Ok(entity.GetData());
        }

        /// <summary>
        /// Updates if newer, returns latest
        /// </summary>
        /// <param name="cloudObject"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [ApiCheckModelForNull]
        [Authorize]
        [Route("api/Storage/Sync")]
        public async Task<IHttpActionResult> Sync(StorageObject cloudObject)
        {
            var entity = await AppDatabase.Storage.FindAsync(cloudObject.ObjectId);

            // create ?
            if (entity == null)
                return await Create(cloudObject);
            
            // Update if newer
            if (entity.ModifiedOn < cloudObject.ModifiedOn)
                return await Update(cloudObject);

            // return old
            return Ok(entity.GetData());
        }

        /// <summary>
        /// Updates set of Storage Object
        /// </summary>
        /// <param name="cloudObjects"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [ApiCheckModelForNull]
        [Authorize]
        [Route("api/Storage/UpdateSet")]
        public async Task<IHttpActionResult> UpdateSet(StorageObject[] cloudObjects)
        {
            if (!cloudObjects.Any())
                return Ok();
            
            foreach (var cloudObject in cloudObjects)
            {
                var entity = await AppDatabase.Storage.FindAsync(cloudObject.ObjectId);

                if (entity == null)
                {
                    cloudObject.ModifiedOn = cloudObject.CreatedOn = DateTime.UtcNow;

                    AppDatabase.Storage.Add(cloudObject);
                }
                else
                {
                    // ACL
                    switch (entity.AclType)
                    {
                        case StorageACLType.Admin:
                            if (!User.Identity.IsAuthenticated)
                                return Unauthorized();
                            break;
                        case StorageACLType.User:
                            if (UserId != entity.AclParam)
                                return Unauthorized();
                            break;
                    }

                    if (entity.ObjectType != cloudObject.ObjectType)
                    {
                        //would cause caching errors
                        return BadRequest("Can not change object type");
                    }

                    entity.ModifiedOn = DateTime.UtcNow;
                    entity.ObjectData = cloudObject.ObjectData;
                    entity.ObjectScore = cloudObject.ObjectScore;
                }
            }
            
            await AppDatabase.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Updates a Storage Object property
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [ApiCheckModelForNull]
        [Authorize]
        [Route("api/Storage/UpdateProperty")]
        public async Task<IHttpActionResult> UpdateProperty(StorageProperty model)
        {
            var entity = await AppDatabase.Storage.FindAsync(model.ObjectId);

            if (entity == null)
                return Ok();

            // ACL
            switch (entity.AclType)
            {
                case StorageACLType.Admin:
                    if (!User.Identity.IsAuthenticated)
                        return Unauthorized();
                    break;
                case StorageACLType.User:
                    if (UserId != entity.AclParam)
                        return Unauthorized();
                    break;
            }

            var data = entity.GetData();

            data[model.PropertyName] = model.PropertyValue;

            entity.ObjectData = data.ToString();
            entity.ModifiedOn = DateTime.UtcNow;

            //  Context.Update(entity);
            await AppDatabase.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Increment / Decrement Update of a  Storage Object
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [Authorize]
        [ApiCheckModelForNull]
        [Route("api/Storage/UpdateDelta")]
        public async Task<IHttpActionResult> UpdateDelta(StorageDelta model)
        {
            var entity = await AppDatabase.Storage.FindAsync(model.ObjectId);

            if (entity == null)
                return Ok();

            // ACL
            switch (entity.AclType)
            {
                case StorageACLType.Admin:
                    if (!User.Identity.IsAuthenticated)
                        return Unauthorized();
                    break;
                case StorageACLType.User:
                    if (UserId != entity.AclParam)
                        return Unauthorized();
                    break;
            }

            var data = entity.GetData();

            var token = data.GetValue(model.PropertyName);

            if (model.IsFloat)
            {
                data[model.PropertyName] = token.Value<float>() + model.Delta;
            }
            else
            {
                data[model.PropertyName] = token.Value<int>() + (int)model.Delta;
            }

            entity.ObjectData = data.ToString();
            entity.ModifiedOn = DateTime.UtcNow;

            //  Context.Update(entity);
            await AppDatabase.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes the Storage Object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ApiValidateModelState]
        [HttpPost]
        [ApiCheckModelForNull]
        [Authorize]
        [Route("api/Storage/Delete/{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            var entity = await AppDatabase.Storage.FindAsync(id);

            if (entity == null)
                return Ok();

            // ACL
            switch (entity.AclType)
            {
                case StorageACLType.Admin:
                    if (!User.Identity.IsAuthenticated)
                        return Unauthorized();
                    break;
                case StorageACLType.User:
                    if (UserId != entity.AclParam)
                        return Unauthorized();
                    break;
            }

            AppDatabase.Storage.Remove(entity);

            await AppDatabase.SaveChangesAsync();

            return Ok();
        }
    }
}