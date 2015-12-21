using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foundation.Server.Api
{
    public class GameScore
    {
        [Key]
        public string UserId { get; set; }

        public string UserName { get; set; }

        public int Score { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime CreatedOn { get; set; }
    }
}
