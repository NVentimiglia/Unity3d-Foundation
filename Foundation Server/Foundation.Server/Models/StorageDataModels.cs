using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Foundation.Server.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Foundation.Server.Models
{
    /// <summary>
    /// Untyped saved object.
    /// </summary>
    public class StorageObject
    {

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