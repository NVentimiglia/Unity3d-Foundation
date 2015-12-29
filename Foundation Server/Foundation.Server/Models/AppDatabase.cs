using System.Data.Entity;
using Foundation.Server.Api;

namespace Foundation.Server.Models
{
    /// <summary>
    /// Database
    /// </summary>
    public class AppDatabase : DbContext
    {
        public DbSet<UserAccount> Users { get; set; }
        public DbSet<UserFacebookClaim> UserFacebookClaims { get; set; }

        public DbSet<StorageObject> Storage { get; set; }
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
}