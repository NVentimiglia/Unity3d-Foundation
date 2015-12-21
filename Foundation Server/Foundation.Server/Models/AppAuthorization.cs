using System;
using System.Web;
using Foundation.Server.Api;
using Foundation.Server.Infrastructure;
using Foundation.Server.Infrastructure.Helpers;
using Newtonsoft.Json;

namespace Foundation.Server.Models
{
    /// <summary>
    /// Application Session. Saved as an encrypted header cookie.
    /// </summary>
    public class AppAuthorization
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Unique Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Reserved UserId
        /// </summary>
        public bool IsAuthenticated { get; set; }

        public DateTime UpdatedOn { get; set; }

        public AppAuthorization()
        {
            UpdatedOn = DateTime.UtcNow;
        }

        public AppAuthorization(AppUser user)
        {
            UpdateFrom(user);
        }

        /// <summary>
        /// Updates session from DB object
        /// </summary>
        /// <param name="user"></param>
        public void UpdateFrom(AppUser user)
        {
            UpdatedOn = DateTime.UtcNow;
            UserId = user.Id;
            Email = user.Email;
            IsAuthenticated = true;
        }

        /// <summary>
        /// Sign out
        /// </summary>
        public void SignOut()
        {
            Email= UserId = String.Empty;
        }
        
        /// <summary>
        /// Read from cookie or Create
        /// </summary>
        /// <returns></returns>
        public static AppAuthorization GetSession()
        {
            var json = HttpContext.Current.Request.Headers[AccountConstants.AUTHORIZATION];
            if (String.IsNullOrEmpty(json))
            {
                return new AppAuthorization();
            }

            var txt = AESHelper.Decrypt(json, AppConfig.Settings.AesKey);
            return JsonConvert.DeserializeObject<AppAuthorization>(txt);
        }

        /// <summary>
        /// Save to cookie
        /// </summary>
        public void SetResponse()
        {
            HttpContext.Current.Response.Headers.Remove(AccountConstants.AUTHORIZATION);
            HttpContext.Current.Response.AddHeader(AccountConstants.AUTHORIZATION, ToAesJson());
        }
        
        public string ToAesJson()
        {
            var json = JsonConvert.SerializeObject(this);
            return AESHelper.Encrypt(json, AppConfig.Settings.AesKey);
        }
    }
}