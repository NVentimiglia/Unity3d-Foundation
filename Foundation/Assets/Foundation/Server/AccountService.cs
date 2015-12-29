// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using System.Net;
using Facebook.Unity;
using Foundation.Server.Api;
using Foundation.Tasks;
using FullSerializer;
using UnityEngine;

namespace Foundation.Server
{
    /// <summary>
    /// Static service for communicating with the Accounts / Authentication Service
    /// </summary>
    public class AccountService : ServiceClientBase
    {
        #region Static

        public static readonly AccountService Instance = new AccountService();
        public const string PrefKey = "Account:Profile";
        public const string FBPrefKey = "Account:FB";

        AccountService() : base("Account")
        {
            Load();

            FB.Init(() =>
            {
                Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
            },
             isGameShown =>
             {
                 FBHideState = isGameShown;
                 OnFBHideState(isGameShown);
             });
        }

        #endregion

        #region props

        /// <summary>
        /// Current User Account
        /// </summary>
        public AccountDetails Account { get; protected set; }

        /// <summary>
        /// FB Token
        /// </summary>
        public AccessToken FBToken { get; protected set; }

        /// <summary>
        /// Raised by FB
        /// </summary>
        public bool FBHideState { get; protected set; }

        /// <summary>
        /// Raised by FB
        /// </summary>
        public event Action<bool> OnFBHideState = delegate { };

        #endregion

        #region private Methods

        void ReadDetails(AccountDetails model)
        {
            Account = model;
            Save();
        }

        /// <summary>
        /// Loads session from prefs
        /// Called automatically in constructor.
        /// </summary>
        public void Load()
        {
            if (PlayerPrefs.HasKey(PrefKey))
            {
                Account = JsonSerializer.Deserialize<AccountDetails>(PlayerPrefs.GetString(PrefKey));
            }
            else
            {
                Account = new AccountDetails
                {
                    Id = Guid.NewGuid().ToString()
                };
            }

            if (PlayerPrefs.HasKey(FBPrefKey))
            {
                FBToken = JsonSerializer.Deserialize<AccessToken>(PlayerPrefs.GetString(FBPrefKey));
            }
        }

        /// <summary>
        /// Saves to prefs
        /// </summary>
        public void Save()
        {
            var json = JsonSerializer.Serialize(Account);
            PlayerPrefs.SetString(PrefKey, json);

            var json2 = JsonSerializer.Serialize(FBToken);
            PlayerPrefs.SetString(FBPrefKey, json2);

            PlayerPrefs.Save();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Hit server, get up to date account profile
        /// </summary>
        /// <returns>true if authenticated</returns>
        public UnityTask Get()
        {
            if (IsAuthenticated)
            {
                //Refresh
                return HttpPost<AccountDetails>("Get")
                .ContinueWith(o =>
                {
                    //reload details
                    if (o.IsSuccess && o.Result != null)
                    {
                        ReadDetails(o.Result);
                    }
                    else if (o.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //
                        SignOut();
                    }
                });
            }

            return Guest();

        }

        /// <summary>
        /// Sign out and clear cache
        /// </summary>
        public void SignOut()
        {
            HttpService.ClearSession();
            HttpService.SaveSession();

            Account = new AccountDetails
            {
                Id = Guid.NewGuid().ToString()
            };

            Save();
        }

        /// <summary>
        /// Requests a new guest account. Use for 'Skip Sign In' option.
        /// </summary>
        /// <returns></returns>
        public UnityTask Guest()
        {
            // Is Authenticated
            if (IsAuthenticated)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                    return UnityTask.SuccessTask();

                //Refresh
                return HttpPost<AccountDetails>("Get")
                    .ContinueWith(o =>
                    {
                        //reload details
                        if (o.IsSuccess && o.Result != null)
                        {
                            ReadDetails(o.Result);
                        }
                    });
            }


            //No Internet ? 
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return UnityTask.SuccessTask();
            
            //Save serverside in background
            var task = HttpPost<AccountDetails>("Guest", new AccountGuestSignIn
            {
                UserId = Account.Id,
            })
            .ContinueWith(o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                }
                else
                {
                    // Reboot local id
                    Account = new AccountDetails
                    {
                        Id = Guid.NewGuid().ToString()
                    };

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
            var task = HttpPost<AccountDetails>("SignIn", new AccountEmailSignIn
            {
                Email = email,
                Password = password,
                UserId = Account.Id,
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
            if (!IsAuthenticated)
                return UnityTask.FailedTask("Not authenticated");

            var task = HttpPost<AccountDetails>("Update", new AccountEmailUpdate
            {
                NewEmail = email,
                NewPassword = password,
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
        public UnityTask Reset(string email)
        {
            var task = HttpPost<AccountDetails>("Reset", new AccountEmailReset
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
        /// Deletes the current account
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public UnityTask Delete(string password)
        {
            if (!IsAuthenticated)
                return UnityTask.FailedTask("Not authenticated");

            var task = HttpPost<AccountDetails>("Delete", new AccountEmailDelete
            {
                Password = password,
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
        public UnityTask FacebookConnect(AccessToken token)
        {
            var task = HttpPost<AccountDetails>("FacebookConnect", new AccountFacebookConnect
            {
                AccessToken = token.TokenString,
            }).ContinueWith(o =>
           {
               if (o.IsSuccess && o.Result != null)
               {
                   FBToken = token;
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
        public UnityTask FacebookDisconnect()
        {
            if(!IsAuthenticated)
                return UnityTask.FailedTask("Not authenticated");

            var task = HttpPost<AccountDetails>("FacebookDisconnect", new AccountFacebookDisconnect())
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