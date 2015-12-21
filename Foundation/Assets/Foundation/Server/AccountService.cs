// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using Foundation.Server.Api;
using Foundation.Tasks;
using FullSerializer;
using UnityEngine;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// Static service for communicating with the Accounts / Authentication Service
    /// </summary>
    public class AccountService
    {
        #region Static
        public static readonly AccountService Instance = new AccountService();
        public const string PrefKey = "CloudAccount";

        #endregion

        #region props
        
        /// <summary>
        /// Unique Id (GUID) for Account
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// User's Email
        /// </summary>
        public string Email { get; protected set; }

        /// <summary>
        /// User's Email
        /// </summary>
        public string FacebookId { get; protected set; }

        /// <summary>
        /// Key used to acquire the session server side. Add to header.
        /// </summary>
        public string SessionToken { get; set; }

        /// <summary>
        /// Key used to acquire the session server side. Add to header.
        /// </summary>
        public string AuthorizationToken { get; set; }

        /// <summary>
        /// Has a Session
        /// </summary>
        public bool IsAuthenticated { get; protected set; }

        /// <summary>
        /// Unique Id for this application instance
        /// </summary>
        public static readonly string ClientId = Guid.NewGuid().ToString();

        #endregion

        #region private Methods
        
        public readonly ServiceClient ServiceClient = new ServiceClient("Account");

        void ReadDetails(AccountDetails model)
        {
            if (model != null)
            {
                IsAuthenticated = model.IsAuthenticated;
                AuthorizationToken = model.AuthorizationToken;
                SessionToken = model.SessionToken;
                Email = model.Email;
                FacebookId = model.FacebookId;
                Save();

            }
        }


        /// <summary>
        /// Loads session from prefs
        /// Called automatically in constructor.
        /// </summary>
        public void Load()
        {
            if (PlayerPrefs.HasKey(PrefKey))
            {
                var model = JsonSerializer.Deserialize<AccountService>(PlayerPrefs.GetString(PrefKey));

                IsAuthenticated = model.IsAuthenticated;
                AuthorizationToken = model.AuthorizationToken;
                SessionToken = model.SessionToken;
                Email = model.Email;
                FacebookId = model.FacebookId;
            }
        }

        /// <summary>
        /// Saves to prefs
        /// </summary>
        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            PlayerPrefs.SetString(PrefKey, json);
            PlayerPrefs.Save();

        }

        #endregion

        #region public methods

        /// <summary>
        /// SignIn from cache (remember me)
        /// </summary>
        /// <returns>true if authenticated</returns>
        public UnityTask SignIn()
        {
            Load();

            //Signed in fine
            if(IsAuthenticated)
                return UnityTask.SuccessTask();

            //Not signed in, but no error.
            if(string.IsNullOrEmpty(Id))
                return UnityTask.SuccessTask();

            //Try load guest account
            var task = ServiceClient.Post<AccountDetails>("Guest", new AccountGuestRequest
            {
                UserId = Id,

            }).AsTask().ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });

            return task;
        }

        /// <summary>
        /// Sign out and clear cache
        /// </summary>
        public void SignOut()
        {
            IsAuthenticated = false;
            Id = FacebookId = Email = SessionToken = AuthorizationToken = string.Empty;
            Save();
        }

        /// <summary>
        /// Requests a new guest account. Use for 'Skip Sign In' option.
        /// </summary>
        /// <returns></returns>
        public UnityTask Guest()
        {
            IsAuthenticated = false;
            Id = Guid.NewGuid().ToString();
            Id = FacebookId = Email = SessionToken = AuthorizationToken = string.Empty;
            Save();

            var task = ServiceClient.Post<AccountDetails>("Guest", new AccountGuestRequest
            {
                UserId = Id,

            }).AsTask().ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });
            return task;
        }


        /// <summary>
        /// Sign in request
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UnityTask SignIn(string email, string password)
        {
            var task = ServiceClient.Post<AccountDetails>("SignIn", new AccountEmailRequest
            {
                Email = email,
                Password = password,
                UserId = Id,
            }).ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });
            return task;
        }

        /// <summary>
        /// Update account details
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UnityTask Update(string email, string password)
        {
            var task = ServiceClient.Post<AccountDetails>("Update", new AccountEmailUpdateRequest
            {
                Email = email,
                Password = password,
            }).ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });
            return task;
        }

        /// <summary>
        /// Tells the server to send out an recovery email. This email will contain a reset token.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public UnityTask Recovery(string email)
        {
            var task = ServiceClient.Post<AccountDetails>("Recovery", new AccountRecoverRequest
            {
                Email = email
            }).ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });
            return task;
        }

        /// <summary>
        /// Updates the account with a new password. Token from Recovery Email.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UnityTask Reset(string token, string password)
        {
            var task = ServiceClient.Post<AccountDetails>("Reset", new AccountEmailResetRequest
            {
                Password = password,
                Token = token
            }).ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });
            return task;
        }

        /// <summary>
        /// Deletes the current account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UnityTask Delete(string email, string password)
        {
            var task = ServiceClient.Post<AccountDetails>("Delete", new AccountEmailDeleteRequest
            {
                Password = password,
                UserId = Id,
                Email = email
            }).ContinueWith(o =>
            {
                if (o.IsSuccess)
                {
                    SignOut();
                }
            });
            return task;
        }
        #endregion

        #region facebook

        /// <summary>
        /// Sign in request
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public UnityTask SocialConnect(string accessToken, string provider = "Facebook")
        {
            var task = ServiceClient.Post<AccountDetails>("Facebook", new AccountFacebookRequest
            {
                AccessToken = accessToken,

            }).ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });
            return task;
        }


        /// <summary>
        /// Removal
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public UnityTask SocialDisconnect(string provider = "Facebook")
        {
            var task = ServiceClient.Post<AccountDetails>("FacebookDelete", new AccountFacebookDeleteRequest())
            .ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
            });
            return task;
        }

        #endregion
    }
}