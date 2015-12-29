using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace Foundation.Server.Models
{
    /// <summary>
    /// User Account
    /// </summary>
    public class UserAccount
    {
        [Required]
        [Key]
        public string Id { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public UserPassword EmailPassword { get; set; }

        public UserPassword PhonePassword { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime ModifiedOn { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime CreatedOn { get; set; }

        // User Claims

        public virtual ICollection<UserFacebookClaim> UserFacebookClaims { get; set; }
        
        #region ==
        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
        #endregion

    }


    [ComplexType]
    public class UserPassword
    {
        public byte[] Salt { get; set; }

        public byte[] PasswordHash { get; set; }

        public DateTime? Expiration { get; set; }

        /// <summary>
        /// checks against an input password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Compare(string password)
        {
            var pw = Hash(password, Salt);
            return (!Expiration.HasValue || Expiration.Value > DateTime.UtcNow) &&  CompareSaltedPasswordHashes(pw, PasswordHash);
        }

        /// <summary>
        /// create new from password
        /// </summary>
        /// <returns></returns>
        public static UserPassword Create(string password, DateTime? expiration = null)
        {
            byte[] salt = GenerateSalt();
            return new UserPassword
            {
                Expiration = expiration,
                Salt = salt,
                PasswordHash = Hash(password, salt)
            };
        }

        /// <summary>
        /// Password Hasher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static byte[] Hash(string plaintext, byte[] salt)
        {
            SHA512Cng hashFunc = new SHA512Cng();
            byte[] plainbytes = System.Text.Encoding.ASCII.GetBytes(plaintext);
            byte[] toHash = new byte[plainbytes.Length + salt.Length];
            plainbytes.CopyTo(toHash, 0);
            salt.CopyTo(toHash, plainbytes.Length);
            return hashFunc.ComputeHash(toHash);
        }

        /// <summary>
        /// Password salt generator
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[256];
            rng.GetBytes(salt);
            return salt;
        }

        /// <summary>
        /// IDK
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool CompareSaltedPasswordHashes(byte[] a, byte[] b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

    }

    public class UserFacebookClaim
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserAccount User { get; set; }

        public string FacebookId { get; set; }
        public string AccessToken { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Picture { get; set; }
        public string Link { get; set; }
        public string Gender { get; set; }
        public string Local { get; set; }
    }
}