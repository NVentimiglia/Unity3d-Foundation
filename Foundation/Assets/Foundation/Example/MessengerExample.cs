using System;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Example
{
    [AddComponentMenu("Foundation/Examples/MessengerExample")]
    public class MessengerExample : MonoBehaviour
    {

        public interface IExampleMessage
        {
        }

        public class ExampleMessage : IExampleMessage
        {
        }

        [CachedMessage]
        public class CachedExampleMessage 
        {
        }

        public Text Logger;

        public string Log
        {
            set { Logger.text = value + Environment.NewLine + Logger.text; }
        }

        void Awake()
        {
            Application.logMessageReceived += Application_logMessageReceived;
        }

        void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            Log = condition;
        }

        public int Counts { get; set; }

        [Subscribe]
        public void Handler(ExampleMessage m)
        {
            Counts++;
            Debug.Log("Handling ExampleMessage");
        }

        [Subscribe]
        public void Handler(IExampleMessage m)
        {
            Counts++;
            Debug.Log("Handling IExampleMessage");
        }

        [Subscribe]
        public void Handler(CachedExampleMessage m)
        {
            Counts++;
            Debug.Log("Handling CachedExampleMessage");
        }


        public void ObjectHandler(object m)
        {
            Counts++;
            Debug.Log("Handling object");
        }

        void Start()
        {

            Logger.text = string.Empty;
            Debug.Log("Starting Messaging Test");
            Counts = 0;

            var m = new ExampleMessage();
            var m2 = new CachedExampleMessage();

            Messenger.Subscribe(this);
            Messenger.Publish(m);
            Assert(() => Counts == 2, "Sub Then Pub");
        
            Messenger.Unsubscribe(this);
            Messenger.Publish(m2);
            Messenger.Subscribe(this);
            Messenger.ClearCache();
            Assert(() => Counts == 3, "Cached Sub / Pub");
        
            Messenger.Unsubscribe(this);
            Messenger<object>.Subscribe(ObjectHandler);
            Messenger.Publish(m);
            Messenger.Publish(m2);
            Assert(() => Counts == 5, "Manual Sub / Pub");

            Debug.Log("All Done");
        }

        void Assert(Func<bool> func, string title)
        {
            if (func())
            {
                Debug.Log(title + " PASSED");
            }
            else
            {
                Debug.LogError(title + " FAILED");
                throw new Exception(title + " ASSERT FAILED");
            }
        }
    }
}
