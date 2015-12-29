using System;
using System.Web;
using Foundation.Server.Api;
using Foundation.Server.Infrastructure.Helpers;
using Foundation.Server.Models;
using Newtonsoft.Json;

namespace Foundation.Server.Infrastructure
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

        public AppAuthorization(UserAccount user)
        {
            UpdateFrom(user);
        }

        /// <summary>
        /// Updates session from DB object
        /// </summary>
        /// <param name="user"></param>
        public void UpdateFrom(UserAccount user)
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
            Email = UserId = String.Empty;
            IsAuthenticated = false;
            HttpContext.Current.Request.Headers.Remove(APIConstants.AUTHORIZATION);
            HttpContext.Current.Response.Headers.Remove(APIConstants.AUTHORIZATION);
        }

        /// <summary>
        /// Read from cookie or Create
        /// </summary>
        /// <returns></returns>
        public static AppAuthorization GetSession()
        {
            var json = HttpContext.Current.Request.Headers[APIConstants.AUTHORIZATION];
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
            HttpContext.Current.Response.Headers.Remove(APIConstants.AUTHORIZATION);
            if (IsAuthenticated)
                HttpContext.Current.Response.AddHeader(APIConstants.AUTHORIZATION, ToAesJson());
        }

        string ToAesJson()
        {
            var json = JsonConvert.SerializeObject(this);
            return AESHelper.Encrypt(json, AppConfig.Settings.AesKey);
        }
    }
}