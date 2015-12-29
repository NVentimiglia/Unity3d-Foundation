using System;
using System.Collections;
using Foundation.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Example
{
    /// <summary>
    /// Example of how to use the UnityTask library
    /// </summary>
    [AddComponentMenu("Foundation/Examples/TaskTests")]
    public class TaskTests : MonoBehaviour
    {
        public Text Output;
        protected int Counter = 0;

        string log;

        void Awake()
        {
            Application.logMessageReceived += Application_logMessageReceived;
        }

        void Assert(Func<bool> compare, string testName)
        {
            if (!compare.Invoke())
                throw new Exception(string.Format("Test {0} Failed", testName));
            Debug.Log("Test " + testName + " Passed");
        }

     
        void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (Output)
            {
                log += (Environment.NewLine + condition);
            }
        }

        void Update()
        {
            Output.text = log;
        }

        public IEnumerator Start()
        {
            log = string.Empty;
            Debug.Log("Test Starting");
            Counter = 0;
            yield return new WaitForSeconds(2);

            // Action
            Debug.Log("Action...");
            var mtask = UnityTask.Run(() => { Counter++; });
            yield return mtask;
            Assert(() => Counter == 1, "Action");
            yield return new WaitForSeconds(2);

            // Action with 
            Debug.Log("Action w/ Continue...");
            yield return new WaitForSeconds(1);
            yield return UnityTask.Run(() =>
            {
                Counter++;
            }).ContinueWith(task =>
            {
                Counter++;
            });
            Assert(() => Counter == 3, "Action w/ Continue");

            // Coroutine
            Debug.Log("Continue...");
            yield return new WaitForSeconds(1);
            yield return UnityTask.RunCoroutine(DemoCoroutine);
            Assert(() => Counter == 4, "Coroutine");

            // Coroutine with Continue
            Debug.Log("Coroutine w/ Continue...");
            yield return new WaitForSeconds(1);
            yield return UnityTask.RunCoroutine(DemoCoroutine).ContinueWith(task => { Counter++; });
            Assert(() => Counter == 6, "Coroutine w/ Continue");

            // Coroutine with result
            Debug.Log("Coroutine w/ result...");
            yield return new WaitForSeconds(1);
            var ctask = UnityTask.RunCoroutine<string>(DemoCoroutineWithResult);
            yield return ctask;
            Assert(() => Counter == 7, "Coroutine w/ result (A)");
            Assert(() => ctask.Result == "Hello", "Coroutine w/ result (B)");


            // Coroutine with result and Continue
            Debug.Log("CoroutineCoroutine w/ result and continue...");
            yield return new WaitForSeconds(1);
            var ctask2 = UnityTask.RunCoroutine<string>(DemoCoroutineWithResult).ContinueWith(t =>
            {
                Assert(() => t.Result == "Hello", "Coroutine w/ result and continue (A)");
                t.Result = "Goodbye";
                Counter++;

            });
            yield return ctask2;
            Assert(() => Counter == 9, "Coroutine w/ result and continue (B)");
            Assert(() => ctask2.Result == "Goodbye", "Coroutine w/ result and continue (C)");


            // Function
            Debug.Log("Function...");
            yield return new WaitForSeconds(1);
            var ftask = UnityTask.Run(() =>
            {
                Counter++;
                return "Hello";
            });

            yield return ftask;
            Assert(() => Counter == 10, "Function");
            Assert(() => ftask.Result == "Hello", "Function");

            //Exception
            Debug.Log("Exception...");
            yield return new WaitForSeconds(1);
            var etask = UnityTask.Run(() => { Counter++; throw new Exception("Hello"); });
            yield return etask;
            Assert(() => Counter == 11, "Exception");
            Assert(() => etask.IsFaulted && etask.Exception.Message == "Hello", "Exception");

            //Main Thread
            log = string.Empty;
            Debug.Log("Basic Tests Passed");
            Debug.Log("Threading Tests...");
            Debug.Log("Background....");
            yield return new WaitForSeconds(1);
            yield return UnityTask.Run(() =>
            {
                UnityTask.Delay(50);
                Debug.Log("Sleeping...");
                UnityTask.Delay(2000);
                Debug.Log("Slept");
            });
            yield return 1;
            Debug.Log("BackgroundToMain");
            yield return new WaitForSeconds(1);
            yield return UnityTask.Run(() =>
            {
                UnityTask.Delay(100);

                var task = UnityTask.RunOnMain(() =>
                {
                    Debug.Log("Hello From Main");
                });

                while (task.IsRunning)
                {
                    log += ".";
                    UnityTask.Delay(100);
                }
            });
            yield return 1;
            Debug.Log("BackgroundToRotine");
            yield return new WaitForSeconds(1);
            yield return UnityTask.Run(() =>
            {
                var task = UnityTask.RunCoroutine(LongCoroutine());

                while (task.IsRunning)
                {
                    log += ".";
                    UnityTask.Delay(500);
                }
            });
            yield return 1;
            Debug.Log("BackgroundToBackground");
            yield return new WaitForSeconds(1);
            yield return UnityTask.Run(() =>
             {
                 Debug.Log("1 Sleeping...");

                 UnityTask.Run(() =>
                 {
                     Debug.Log("2 Sleeping...");
                     UnityTask.Delay(2000);
                     Debug.Log("2 Slept");
                 });
                 UnityTask.Delay(2000);
                 Debug.Log("1 Slept");
             });
            yield return 1;

            Debug.Log("Success All");
        }

        IEnumerator DemoCoroutine()
        {
            yield return 1;
            Counter++;
        }

        IEnumerator DemoCoroutineWithResult(UnityTask<string> task)
        {
            yield return 1;
            Counter++;
            task.Result = "Hello";
        }


        public IEnumerator LongCoroutine()
        {
            Debug.Log("LongCoroutine");
            yield return new WaitForSeconds(5);
            Debug.Log("/LongCoroutine");
        }
    }
}
