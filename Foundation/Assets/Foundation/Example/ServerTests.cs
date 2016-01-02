using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Foundation.Server;
using Foundation.Server.Api;
using Foundation.Tasks;
using UnityEngine;

namespace Foundation.Example
{
    public class ServerTests : MonoBehaviour
    {
        // I/O
        public UnityEngine.UI.Text Log;
        public UnityEngine.UI.InputField TokenInput;
        public GameObject InputRoot;


        //Props
        public bool Waiting { get; set; }
        public string Token;
        public string Email1 = "";
        public string Email2 = "";
        public string Password = "password1";
        public string RealtimeChannel = "myChannel";

        // Demo client-defined Storage Object
        [StorageTable("DemoObject")]
        class DemoObject
        {
            [StorageIdentity]
            public string Id { get; set; }
            public string String { get; set; }
            public int Number { get; set; }
            public Color Color { get; set; }
            public Vector3 Vector { get; set; }

            private static int Counter;
            public static DemoObject Create()
            {
                return new DemoObject
                {
                    Id = (Counter++).ToString(),
                    String = UnityEngine.Random.value.ToString(),
                    Number = UnityEngine.Random.Range(0, 10),
                    Vector = UnityEngine.Random.insideUnitSphere,
                    Color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value)
                };
            }
        }

        #region internal stuff
        
        IEnumerator TaskAsync(UnityTask task, bool hault = true)
        {
            yield return 1;

            yield return task;

            if (task.IsFaulted)
            {
                if (task.Exception is HttpException)
                {
                    var hte = task.Exception as HttpException;
                    foreach (var error in hte.GetErrors())
                    {
                        Debug.LogError(error);
                    }
                }
                else
                {
                    Debug.LogError(task.Exception.Message);
                }

                if (hault)
                {
                    Debug.LogError("Haulting");
                    StopAllCoroutines();

                    while (true)
                    {
                        yield return 1;
                    }
                }

            }

            yield return 1;
        }
        #endregion


        public void StartAccountTests()
        {
            StartCoroutine(AccountsAsync());
        }

        public void StartRealtimeTests()
        {
            StartCoroutine(RealtimeAsync());
        }

        public void StartStorageTests()
        {
            StartCoroutine(StorageAsync());
        }

        IEnumerator AccountsAsync()
        {
            Log.text = string.Empty;
            Token = string.Empty;
            Debug.Log("Account Tests");

            var account = AccountService.Instance;
            
            // auto sign out (cache)
            account.SignOut();

            // sign up
            Debug.Log("SignIn...");
            yield return StartCoroutine(TaskAsync(account.SignIn(Email1, Password)));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // sign out
            Debug.Log("SignOut...");
            account.SignOut();
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // sign in
            Debug.Log("SignIn...");
            yield return StartCoroutine(TaskAsync(account.SignIn(Email1, Password)));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));
            
            // out
            Debug.Log("Delete...");
            yield return StartCoroutine(TaskAsync(account.Delete(Password)));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // guest
            Debug.Log("Guest...");
            yield return StartCoroutine(TaskAsync(account.Guest()));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // update
            Debug.Log("Update...");
            yield return StartCoroutine(TaskAsync(account.Update(Email2, Password)));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // sign out
            Debug.Log("SignOut...");
            account.SignOut();
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // sign in
            Debug.Log("SignIn...");
            yield return StartCoroutine(TaskAsync(account.SignIn(Email2, Password)));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));
            
            // out
            Debug.Log("SignOut...");
            account.SignOut();
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // recover
            Debug.Log("Recovery...");
            yield return StartCoroutine(TaskAsync(account.Reset(Email2)));

            // wait
            Debug.Log("Waiting On Token...");
            InputRoot.SetActive(true);
            Waiting = true;
            while (Waiting)
            {
                yield return 1;
            }
            InputRoot.SetActive(false);

            // sign in
            Debug.Log("SignIn...");
            yield return StartCoroutine(TaskAsync(account.SignIn(Email2, TokenInput.text)));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            // update
            Debug.Log("Update...");
            yield return StartCoroutine(TaskAsync(account.Update(Email1, Password)));
            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));

            Debug.Log("Done");

        }


        IEnumerator StorageAsync()
        {

            Log.text = string.Empty;
            Debug.Log("Storage Tests");

            var storage = StorageService.Instance;

            //Make sure you are logged in.
            yield return StartCoroutine(SignIn());

            // make objects
            List<DemoObject> _demoObjects = new List<DemoObject>();
            for (int i = 0; i < 10; i++)
            {
                _demoObjects.Add(DemoObject.Create());
            }

            // post 1
            Debug.Log("Post 1...");
            yield return StartCoroutine(TaskAsync(storage.Update(_demoObjects.First())));

            // post 9
            Debug.Log("Post Set...");
            yield return StartCoroutine(TaskAsync(storage.UpdateSet(_demoObjects.ToArray())));

            // get 1
            Debug.Log("Get 1...");
            yield return StartCoroutine(TaskAsync(storage.Get<DemoObject>(_demoObjects.First().Id)));

            // getset
            Debug.Log("Get Set...");
            yield return StartCoroutine(TaskAsync(storage.GetSet<DemoObject>(_demoObjects.Select(o => o.Id).ToArray())));

            // query
            Debug.Log("Query Set...");
            var query = new ODataQuery<DemoObject>().WhereGreaterThan("Id", 5).OrderBy("String").Take(5);
            yield return StartCoroutine(TaskAsync(storage.Query(query)));

            var first = _demoObjects.First();

            // update prop
            Debug.Log("Update Prop...");
            yield return StartCoroutine(TaskAsync(storage.UpdateProperty(first.Id, "String", UnityEngine.Random.value.ToString())));

            // delta
            Debug.Log("Update Delta...");
            yield return StartCoroutine(TaskAsync(storage.UpdateDelta(first.Id, "Number", 1)));

            // delta
            Debug.Log("Sync...");
            yield return StartCoroutine(TaskAsync(storage.Sync(first)));

            // delete
            Debug.Log("Delete...");
            yield return StartCoroutine(TaskAsync(storage.Delete(first)));

            Debug.Log("Done");

        }

        public void SubmitToken()
        {
            Waiting = false;
        }


        public void DoSignIn()
        {
            StartCoroutine(SignIn());
        }

        public void DoSignOut()
        {
            Log.text = String.Empty;
            Debug.Log("Signed Out");
            AccountService.Instance.SignOut();
        }

        public void ConnectFacebook()
        {
            FB.LogInWithPublishPermissions(new []{ "public_profile", "user_friends", "user_birthday", "user_email" }, result =>
            {
                if (!string.IsNullOrEmpty(result.Error))
                {
                    Debug.LogError(result.Error);
                    return;
                }

                StartCoroutine(ConnectFacebookAsync2(result.AccessToken));
            });
        }
        
        IEnumerator ConnectFacebookAsync2(AccessToken accessToken)
        {
            var task = AccountService.Instance.FacebookConnect(accessToken);

            yield return task;

            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
            }
            else
            {
                Debug.Log("Facebook Connected");
            }
        }

        public void DiconnectFacebookAsync()
        {
            StartCoroutine(DisconnectFacebookAsync());
        }

        IEnumerator DisconnectFacebookAsync()
        {
            
            Log.text = String.Empty;
            
            Debug.Log("Facebook Disconnected");
            FB.LogOut();

            var task = AccountService.Instance.FacebookDisconnect();

            yield return StartCoroutine(TaskAsync(task, false));
        }


        IEnumerator SignIn()
        {
            Log.text = String.Empty;
            var account = AccountService.Instance;
            
            if (!account.IsAuthenticated)
            {
                Debug.Log("SignIn...");
                yield return StartCoroutine(TaskAsync(account.SignIn(Email1, Password), false));
            }

            Debug.Log(string.Format("Account : {0} {1} {2}", account.IsAuthenticated, account.Account.Email, account.Account.Id));
        }

        IEnumerator RealtimeAsync()
        {

            Log.text = string.Empty;
            Token = string.Empty;
            Debug.Log("Realtime Tests");

            var realtime = RealtimeService.Instance;

            yield return StartCoroutine(SignIn());

            Debug.Log("SignIn...");
            var channels = new Dictionary<string, string[]>();
            channels.Add(RealtimeChannel, new[] { "r", "w", "p" });
            var task = realtime.SignIn(channels);
            yield return StartCoroutine(TaskAsync(task));

            Debug.Log("Auth Token : " + task.Result.AuthenticationToken);
        }
        
    }
}
