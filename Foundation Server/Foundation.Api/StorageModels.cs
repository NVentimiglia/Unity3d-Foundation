// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.ComponentModel.DataAnnotations;

namespace Foundation.Server.Api
{
    /// <summary>
    /// Defines the protection level for a Storage Object
    /// </summary>
    public enum StorageACLType
    {
        Public,
        User,
        Admin,
    }

    /// <summary>
    /// Describes a storage object. Object saved in ObjectData as Json.
    /// </summary>
    public class StorageRequest
    {
        /// <summary>
        /// Unique object Id
        /// </summary>
        [Required]
        public string ObjectId { get; set; }

        /// <summary>
        /// Object Table Name, computed
        /// </summary>
        [Required]
        public string ObjectType { get; set; }

        /// <summary>
        /// Object Sort Index, computed
        /// </summary>
        public float ObjectScore { get; set; }

        /// <summary>
        /// Object Table Name, computed
        /// </summary>
        [Required]
        public string ObjectData { get; set; }

        /// <summary>
        /// Book keeping
        /// </summary>
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// Book keeping
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Security. Current Write Protection
        /// </summary>
        public StorageACLType AclType { get; set; }

        /// <summary>
        /// Security Parameter. Current Write Protection (UserId / GroupId)
        /// </summary>
        public string AclParam { get; set; }

        #region ==
        public override int GetHashCode()
        {
            return (ObjectId != null ? ObjectId.GetHashCode() : 0);
        }
        #endregion
    }

    /// <summary>
    /// Increment/Decrement a storage object property
    /// </summary>
    public class StorageDelta
    {
        /// <summary>
        /// Unique object Id
        /// </summary>
        [Required]
        public string ObjectId { get; set; }

        /// <summary>
        /// Unique property Name
        /// </summary>
        [Required]
        public string PropertyName { get; set; }

        /// <summary>
        /// Is the object not an int
        /// </summary>
        public bool IsFloat { get; set; }

        /// <summary>
        /// Change
        /// </summary>
        [Required]
        public float Delta { get; set; }

        #region ==
        public override int GetHashCode()
        {
            return (ObjectId != null ? ObjectId.GetHashCode() : 0);
        }
        #endregion
    }
    
    /// <summary>
    /// Updates a single storage object property
    /// </summary>
    public class StorageProperty
    {
        /// <summary>
        /// Unique object Id
        /// </summary>
        [Required]
        public string ObjectId { get; set; }

        /// <summary>
        /// Unique property Name
        /// </summary>
        [Required]
        public string PropertyName { get; set; }

        /// <summary>
        /// Is the object not an int
        /// </summary>
        public string PropertyValue { get; set; }

        #region ==
        public override int GetHashCode()
        {
            return (ObjectId != null ? ObjectId.GetHashCode() : 0);
        }
        #endregion
    }
}