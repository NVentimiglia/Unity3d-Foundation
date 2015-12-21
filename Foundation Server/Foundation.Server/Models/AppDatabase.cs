using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Foundation.Server.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Foundation.Server.Models
{
    /// <summary>
    /// Database
    /// </summary>
    public class AppDatabase : DbContext
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<AppObject> Objects { get; set; }
        public DbSet<SocialProfile> Socials { get; set; }
        public DbSet<GameScore> Scores { get; set; }

        public AppDatabase()
            : base("AppDatabase")
        {

        }

        public static AppDatabase Create()
        {
            return new AppDatabase();
        }
    }
    

    /// <summary>
    /// User Account
    /// </summary>
    public class AppUser
    {
        [Required]
        [Key]
        public string Id { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string ResetToken { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime ResetTokenExpires { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime ModifiedOn { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Collection of OAuth login providers
        /// </summary>
        public virtual ICollection<SocialProfile> Socials { get; set; }

        #region ==
        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
        #endregion

    }
    
    public class SocialProfile
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        public string Provider { get; set; }
        public string AccessToken { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Picture { get; set; }
        public string Link { get; set; }
        public string Gender { get; set; }
        public string Local { get; set; }
    }

    
    /// <summary>
    /// Untyped saved object.
    /// </summary>
    public class AppObject
    {
        [Required]
        public string AppId { get; set; }


        // acl
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public StorageACLType AclType { get; set; }

        public string AclParam { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime ModifiedOn { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreatedOn { get; set; }

        [Required]
        [Key]
        public string ObjectId { get; set; }

        [Required]
        public string ObjectType { get; set; }
        [Required]
        public float ObjectScore { get; set; }

        [NotMapped]
        private string _objectData;
        public string ObjectData
        {
            get { return _objectData; }
            set
            {
                _objectData = value;
            }
        }


        public JObject GetData()
        {
           return (JObject)JsonConvert.DeserializeObject(ObjectData);
        }

        #region ==
        public override int GetHashCode()
        {
            return (ObjectId != null ? ObjectId.GetHashCode() : 0);
        }
        #endregion
    }
}