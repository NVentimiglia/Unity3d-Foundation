using System;
using System.Collections.Generic;
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
    public partial class AppSession
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string UserId { get; set; }
        
        public DateTime UpdatedOn { get; set; }

        public AppSession()
        {
            UpdatedOn = DateTime.UtcNow;
        }

        public AppSession(AppUser user)
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

            OnUpdate(user);
        }

        /// <summary>
        /// Sign out
        /// </summary>
        public void SignOut()
        {
            UserId = String.Empty;
            OnDelete();
        }
    }

    public partial class AppSession
    {
        /// <summary>
        /// TODO : Sync with Event Bus
        /// </summary>
        static Dictionary<string, DateTime> ExpiredUsers = new Dictionary<string, DateTime>();

        /// <summary>
        /// Read from cookie or Create
        /// </summary>
        /// <returns></returns>
        public static AppSession GetSession()
        {
            var json = HttpContext.Current.Request.Headers[AccountConstants.SESSION];
            if (String.IsNullOrEmpty(json))
            {
                return new AppSession();
            }

            var txt = AESHelper.Decrypt(json, AppConfig.Settings.AesKey);
            var model = JsonConvert.DeserializeObject<AppSession>(txt);
            var userId = model.UserId;
            
            if (ExpiredUsers.ContainsKey(model.UserId) && ExpiredUsers[model.UserId] > model.UpdatedOn)
                return Create(userId);

            return model;
        }

        /// <summary>
        /// Save to cookie
        /// </summary>
        public void SetResponse()
        {
            HttpContext.Current.Response.Headers.Remove(AccountConstants.SESSION);
            HttpContext.Current.Response.AddHeader(AccountConstants.SESSION, ToAesJson());
        }


        public string ToAesJson()
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

        /// <summary>
        /// Forces reload of all sessions by userid
        /// </summary>
        /// <remarks>
        /// Session changed by another user
        /// </remarks>
        public static void ExpireUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            ExpiredUsers.Remove(id);
            ExpiredUsers.Add(id, DateTime.UtcNow);
        }
    }
}