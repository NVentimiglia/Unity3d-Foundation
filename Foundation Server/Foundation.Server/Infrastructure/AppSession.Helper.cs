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
    public partial class AppSession
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string UserId { get; set; }

        public DateTime UpdatedOn { get; set; }

        protected bool IsSignedOut;

        public AppSession()
        {
            UpdatedOn = DateTime.UtcNow;
        }

        public AppSession(UserAccount user)
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

            OnUpdate(user);
        }

        /// <summary>
        /// Sign out
        /// </summary>
        public void SignOut()
        {
            IsSignedOut = true;
            UserId = String.Empty;
            HttpContext.Current.Request.Headers.Remove(APIConstants.SESSION);
            HttpContext.Current.Response.Headers.Remove(APIConstants.SESSION);
            OnDelete();
        }
    }

    public partial class AppSession
    {
        /// <summary>
        /// Read from cookie or Create
        /// </summary>
        /// <returns></returns>
        public static AppSession GetSession()
        {
            var json = HttpContext.Current.Request.Headers[APIConstants.SESSION];
            if (String.IsNullOrEmpty(json))
            {
                return new AppSession();
            }

            var txt = AESHelper.Decrypt(json, AppConfig.Settings.AesKey);
            var model = JsonConvert.DeserializeObject<AppSession>(txt);

            if (string.IsNullOrEmpty(model.UserId))
                return new AppSession();
            
            return model;
        }

        /// <summary>
        /// Save to cookie
        /// </summary>
        public void SetResponse()
        {
            HttpContext.Current.Response.Headers.Remove(APIConstants.SESSION);
            if (!IsSignedOut)
                HttpContext.Current.Response.AddHeader(APIConstants.SESSION, ToAesJson());
        }


        string ToAesJson()
        {
            var json = JsonConvert.SerializeObject(this);
            return AESHelper.Encrypt(json, AppConfig.Settings.AesKey);
        }

        /// <summary>
        /// Creates a new Session from the DB
        /// </summary>
        /// <returns></returns>
        public static AppSession Create(string userId)
        {
            if (HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
                return new AppSession();

            using (var db = new AppDatabase())
            {
                var account = db.Users.Find(userId);

                if (account == null)
                {
                    return new AppSession();
                }

                var session = new AppSession(account);

                session.SetResponse();

                return session;
            }
        }
    }
}