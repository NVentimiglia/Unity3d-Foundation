using System;
using System.Collections;
using Foundation.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Foundation.Example
{
    /// <summary>
    /// Example of how to use the UnityTask library
    /// </summary>
    [AddComponentMenu("Foundation/Examples/TaskTests")]
    public class TaskTests : MonoBehaviour
    {
        public Text Output;
        protected int Counter = 0;

        void Assert(Func<bool> compare, int c)
        {
            if (!compare.Invoke())
                throw new Exception(string.Format("Test {0} Failed",c));
        }

        public IEnumerator Start()
        {
            Output.text = string.Empty;
            Counter = 0;
            Application.logMessageReceived += Application_logMessageReceived;
            yield return 1;


            UnityTask.Run(() =>
            {
                Counter++;
                Debug.Log("1 Run");
            });
            yield return new WaitForSeconds(1);
            Assert(() => Counter == 1,1);

            UnityTask.Run(Test2, "2 Run With Param");
            yield return new WaitForSeconds(1);
            Assert(() => Counter == 2,2);

            UnityTask.RunCoroutine(Test3);
            yield return new WaitForSeconds(1);
            Assert(() => Counter == 3,3);

            var t4 =UnityTask.RunCoroutine(Test4()).ContinueWith(t =>
            {
                Counter++;
                Debug.Log("5 Coroutine with Continue");
            });
            yield return StartCoroutine(t4.WaitRoutine());
            Assert(() => Counter == 5,5);
            yield return new WaitForSeconds(1);

            var t5 =UnityTask.RunCoroutine(Test5).ContinueWith(t =>
            {
                Counter++;
                Debug.Log("5 Continued");
            });

            yield return StartCoroutine(t5.WaitRoutine());
            Assert(() => Counter == 7,7);
            yield return new WaitForSeconds(1);

            var t6 = UnityTask.Run(() => { return "6 Run with Result And Continue"; }).ContinueWith(t => { Counter++; Debug.Log(t.Result); });
            yield return StartCoroutine(t6.WaitRoutine());
            Assert(() => Counter == 8,8);
            yield return new WaitForSeconds(1);

            var t7 = UnityTask.Run<string, string>(Test7, "7 Run with Param and Result And Continue").ContinueWith(t => { Counter++; Debug.Log(t.Result); });
            yield return StartCoroutine(t7.WaitRoutine());
            yield return new WaitForSeconds(1);
            Assert(() => Counter == 10, 10);

            var t1 = UnityTask.RunCoroutine<string>(Test8);
            yield return StartCoroutine(t1.WaitRoutine());
            Debug.Log(t1.Result);
            Assert(() => Counter == 11, 11);
            yield return new WaitForSeconds(1);

            var t2 = UnityTask.RunCoroutine<string>(Test9).ContinueWith(t => { Counter++; Debug.Log(t.Result); });
            yield return StartCoroutine(t2.WaitRoutine());
            Assert(() => Counter == 13, 13);


            var t12 = UnityTask.Run(() => { return "1"; }).ConvertTo<int>(task =>
            {
                Debug.Log("10 ConvertTo Extension");
                Counter++; 
                return int.Parse(task.Result);
            });

            Assert(() => t12.Result == 1, 14);
            Assert(() => Counter == 14, 14);
            
            Debug.Log("Success");
        }

        void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (Output)
            {
                Output.text += (Environment.NewLine + condition);
            }
        }

        void Test2(string param)
        {
            Debug.Log(param);
            Counter++;
        }

        IEnumerator Test3()
        {
            yield return 1;
            Counter++;
            Debug.Log("3 Coroutine");
        }

        IEnumerator Test5(UnityTask UnityTask)
        {
            yield return 1;
            Counter++;
            Debug.Log("5 Coroutine with UnityTask");
        }

        IEnumerator Test4()
        {
            yield return 1;
            Debug.Log("4 Coroutine");
            Counter++;
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(1);
        }

        string Test7(string param)
        {
            Counter++;
            return param;
        }

        IEnumerator Test8(UnityTask<string> UnityTask)
        {
            yield return 1;
            Counter++;
            UnityTask.Result = "8 Coroutine With Result";
        }
        IEnumerator Test9(UnityTask<string> UnityTask)
        {
            yield return 1;
            UnityTask.Result = ("9 Coroutine with UnityTask State Complete");
            Counter++;
        }


        void MainTest()
        {
            UnityTask.RunOnMain(() =>
            {
                Debug.Log("Sleeping...");
                UnityTask.Delay(2000);
                Debug.Log("Slept");
            });
        }

        void Background()
        {
            UnityTask.Run(() =>
            {
                Debug.Log("Sleeping...");
                UnityTask.Delay(2000);
                Debug.Log("Slept");
            });
        }

        void Routine()
        {
            UnityTask.RunCoroutine(RoutineFunction());
        }

        IEnumerator RoutineFunction()
        {
            Debug.Log("Sleeping...");
            yield return new WaitForSeconds(2);
            Debug.Log("Slept");
        }



        void BackgroundToMain()
        {
            UnityTask.Run(() =>
            {

                Debug.Log("Thread A Running");

                var task = UnityTask.RunOnMain(() =>
                   {
                       Debug.Log("Sleeping...");
                       UnityTask.Delay(2000);
                       Debug.Log("Slept");
                   });

                while (task.IsRunning)
                {
                    Debug.Log(".");
                    UnityTask.Delay(100);
                }

                Debug.Log("Thread A Done");
            });
        }


        void BackgroundToRotine()
        {
            UnityTask.Run(() =>
            {
                Debug.Log("Thread A Running");

                var task = UnityTask.RunCoroutine(RoutineFunction());

                while (task.IsRunning)
                {
                    Debug.Log(".");
                    UnityTask.Delay(500);
                }

                Debug.Log("Thread A Done");
            });

        }

        void BackgroundToBackground()
        {
            UnityTask.Run(() =>
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

        }

        void BackgroundToBackgroundException()
        {
            var task1 = UnityTask.Run(() =>
            {
                Debug.Log("1 Go");

                var task2 = UnityTask.Run(() =>
                {
                    UnityTask.Delay(100);
                    Debug.Log("2 Go");
                    throw new Exception("2 Fail");
                });

                task2.Wait();

                if (task2.IsFaulted)
                    throw task2.Exception;
            });

            task1.Wait();

            Debug.Log(task1.Status + " " + task1.Exception.Message);

        }

        void BackgroundException()
        {
            var task1 = UnityTask.Run(() =>
            {
                throw new Exception("Hello World");
            });

            task1.Wait();

            Debug.Log(task1.Status + " " + task1.Exception.Message);

        }


        void CoroutineUnityTaskState()
        {
            UnityTask.RunCoroutine<string>(CoroutineUnityTaskStateAsync).ContinueWith(o => Debug.Log(o.Result));
        }

        IEnumerator CoroutineUnityTaskStateAsync(UnityTask<string> UnityTask)
        {
            yield return 1;

            UnityTask.Result = "Hello World";
        }

    }
}
