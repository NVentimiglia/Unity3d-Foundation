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
#if !UNITY_STANDALONE || UNITY_EDITOR
            FB.Init(() =>
            {
                Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
            },
             isGameShown =>
             {
                 FBHideState = isGameShown;
                 OnFBHideState(isGameShown);
             });
#endif
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
        /// Hit server, get up to date account profile
        /// </summary>
        /// <returns>true if authenticated</returns>
        public void Get(Action<Response> callback)
        {
            if (IsAuthenticated)
            {
                //Refresh
                HttpPostAsync<AccountDetails>("Get", o =>
                {
                    if (o.IsSuccess && o.Result != null)
                    {
                        ReadDetails(o.Result);
                    }
                    else if (o.Metadata != null && o.Metadata.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        SignOut();
                    }
                });
            }
            else
            {
                callback(new Response(new Exception("Not authorized")));
            }
        }


        /// <summary>
        /// Requests a new guest account. Use for 'Skip Sign In' option.
        /// </summary>
        /// <returns></returns>
        public void Guest(Action<Response> callback)
        {
            // Is Authenticated
            if (IsAuthenticated)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    callback(new Response());
                    return;
                }

                //Refresh
                HttpPostAsync<AccountDetails>("Get", o =>
                    {
                        //reload details
                        if (o.IsSuccess && o.Result != null)
                        {
                            ReadDetails(o.Result);
                        }
                    });

                return;
            }

            if (Account == null || string.IsNullOrEmpty(Account.Id))
            {
                Account = new AccountDetails { Id = Guid.NewGuid().ToString() };
            }

            //No Internet ? 
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                callback(new Response(new Exception("Not signed in, no internet.")));
                return;
            }

            //Save serverside in background
            HttpPostAsync<AccountDetails>("Guest", new AccountGuestSignIn
            {
                UserId = Account.Id,
            }, o =>
             {
                 if (o.IsSuccess && o.Result != null)
                 {
                     ReadDetails(o.Result);
                 }
                 else
                 {
                     Debug.LogException(o.Exception);

                    //sign out
                    HttpService.ClearSession();
                     HttpService.SaveSession();
                 }

                 callback(new Response());
             });
        }

        /// <summary>
        /// Sign in request
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void SignIn(string email, string password, Action<Response> callback)
        {
            HttpPostAsync<AccountDetails>("SignIn", new AccountEmailSignIn
            {
                Email = email,
                Password = password,
                UserId = Account.Id,
            }, o =>
             {
                 if (o.IsSuccess && o.Result != null)
                 {
                     ReadDetails(o.Result);
                     callback(new Response());
                 }
                 else
                 {
                     callback(new Response(o.Exception));
                 }
             });
        }

        /// <summary>
        /// Update account details
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Update(string email, string password, Action<Response> callback)
        {
            if (!IsAuthenticated)
            {
                // Happens if start as guest offline
                // must sign in.
                SignIn(email, password, callback);
                return;
            }

            HttpPostAsync<AccountDetails>("Update", new AccountEmailUpdate
            {
                NewEmail = email,
                NewPassword = password,
            }, o =>
             {
                 if (o.IsSuccess && o.Result != null)
                 {
                     ReadDetails(o.Result);
                     callback(new Response());
                 }
                 else
                 {
                     callback(new Response(o.Exception));
                 }
             });
        }

        /// <summary>
        /// Tells the server to send out an recovery email. This email will contain a reset token.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Reset(string email, Action<Response> callback)
        {
            HttpPostAsync<AccountDetails>("Reset", new AccountEmailReset
            {
                Email = email
            },o =>
            {
                if (o.IsSuccess && o.Result != null)
                {
                    ReadDetails(o.Result);
                    callback(new Response());
                }
                else
                {
                    callback(new Response(o.Exception));
                }
            });
        }

        /// <summary>
        /// Deletes the current account
        /// </summary>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Delete(string password, Action<Response> callback)
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception("Not authenticated")));
                return;
            }

            HttpPostAsync<AccountDetails>("Delete", new AccountEmailDelete
            {
                Password = password,
            },o =>
            {
                if (o.IsSuccess)
                {
                    SignOut();
                    callback(new Response());
                }
                else
                {
                    callback(new Response(o.Exception));
                }
            });
        }

        //

        /// <summary>
        /// Hit server, get up to date account profile
        /// </summary>
        /// <returns>true if authenticated</returns>
        public UnityTask Get()
        {
            var task = new UnityTask(TaskStrategy.Custom);
            Get(task.FromResponse());
            return task;
        }


        /// <summary>
        /// Requests a new guest account. Use for 'Skip Sign In' option.
        /// </summary>
        /// <returns></returns>
        public UnityTask Guest()
        {

            var task = new UnityTask(TaskStrategy.Custom);
            Guest(task.FromResponse());
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
            var task = new UnityTask(TaskStrategy.Custom);
            SignIn(email, password, task.FromResponse());
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
            var task = new UnityTask(TaskStrategy.Custom);
            Update(email, password, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Tells the server to send out an recovery email. This email will contain a reset token.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public UnityTask Reset(string email)
        {
            var task = new UnityTask(TaskStrategy.Custom);
            Reset(email, task.FromResponse());
            return task;
        }

        /// <summary>
        /// Deletes the current account
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public UnityTask Delete(string password)
        {
            var task = new UnityTask(TaskStrategy.Custom);
            Delete(password, task.FromResponse());
            return task;
        }
        #endregion

        #region facebook

        /// <summary>
        /// Sign in request
        /// </summary>
        /// <returns></returns>
        public void FacebookConnect(Action<Response> callback)
        {
            FB.LogInWithPublishPermissions(new[] { "public_profile", "user_friends", "user_birthday", "user_email" }, result =>
            {
                if (!string.IsNullOrEmpty(result.Error))
                {
                    callback(new Response(new Exception(result.Error)));
                }
                else
                {
                    FacebookConnect(result.AccessToken, callback);
                }
            });
        }

        /// <summary>
        /// Sign in request
        /// </summary>
        /// <returns></returns>
        public void FacebookConnect(AccessToken token, Action<Response> callback)
        {
            HttpPostAsync<AccountDetails>("FacebookConnect", new AccountFacebookConnect
            {
                AccessToken = token.TokenString,
            },
            response =>
            {
                if (response.IsSuccess && response.Result != null)
                {
                    FBToken = token;
                    ReadDetails(response.Result);
                }
            });
        }

        /// <summary>
        /// Removal
        /// </summary>
        /// <returns></returns>
        public void FacebookDisconnect(Action<Response> callback)
        {
            if (!IsAuthenticated)
            {
                callback(new Response(new Exception(("Not authenticated"))));
                return;
            }

            HttpPostAsync<AccountDetails>("FacebookDisconnect", new AccountFacebookDisconnect(),
            response =>
            {
                if (response.IsSuccess && response.Result != null)
                {
                    ReadDetails(response.Result);
                    FBToken = null;
                }
            });
        }

        /// <summary>
        /// Sign in request
        /// </summary>
        /// <returns></returns>
        public UnityTask FacebookConnect()
        {
            var task = new UnityTask { Strategy = TaskStrategy.Custom };

            FacebookConnect(task.FromResponse());

            return task;
        }

        /// <summary>
        /// Sign in request
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public UnityTask FacebookConnect(AccessToken token)
        {
            var task = new UnityTask { Strategy = TaskStrategy.Custom };

            FacebookConnect(token, task.FromResponse());

            return task;
        }

        /// <summary>
        /// Removal
        /// </summary>
        /// <returns></returns>
        public UnityTask FacebookDisconnect()
        {
            var task = new UnityTask { Strategy = TaskStrategy.Custom };

            FacebookDisconnect(task.FromResponse());

            return task;
        }

        #endregion
    }
}