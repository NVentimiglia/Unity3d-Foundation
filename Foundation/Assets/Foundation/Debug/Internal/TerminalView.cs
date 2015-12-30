// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Debuging.Internal
{
    /// <summary>
    /// renders the Terminal using new 4.6 uGUI
    /// </summary>
    [AddComponentMenu("Foundation/Terminal/TerminalView")]
    public class TerminalView : MonoBehaviour
    {
        /// <summary>
        /// Option
        /// </summary>
        public bool DoDontDestoryOnLoad = true;

        /// <summary>
        /// For Hide / show
        /// </summary>
        public GameObject DisplayRoot;

        /// <summary>
        /// For Input
        /// </summary>
        public InputField TextInput;

        /// <summary>
        /// Button View Template
        /// </summary>
        public TerminalCommandView CommandPrototype;

        /// <summary>
        /// Log View Template
        /// </summary>
        public TerminalItemView ItemPrototype;

        /// <summary>
        /// Parent for Command Views
        /// </summary>
        public Transform CommandLayout;
        /// <summary>
        /// Parent for Log Views
        /// </summary>
        public Transform ItemLayout;

        /// <summary>
        /// Snap to bottom
        /// </summary>
        public Scrollbar ItemScrollBar;
        [HideInInspector]
        public List<TerminalCommandView> CommandItems = new List<TerminalCommandView>();
        [HideInInspector]
        public List<TerminalItemView> TextItems = new List<TerminalItemView>();

        public bool IsVisible
        {
            get
            {
                return DisplayRoot.activeSelf;

            }
            set
            {
                DisplayRoot.SetActive(value);
            }
        }

        public KeyCode VisiblityKey = KeyCode.BackQuote;

        public Color LogColor = Color.white;
        public Color WarningColor = Color.yellow;
        public Color ErrorColor = Color.red;

        public Color SuccessColor = Color.green;
        public Color InputColor = Color.cyan;
        public Color ImportantColor = Color.yellow;

        protected void Awake()
        {
            // Display
            Terminal.Instance.LogColor = LogColor;
            Terminal.Instance.WarningColor = WarningColor;
            Terminal.Instance.ErrorColor = ErrorColor;
            Terminal.Instance.SuccessColor = SuccessColor;
            Terminal.Instance.InputColor = InputColor;
            Terminal.Instance.ImportantColor = ImportantColor;

            //Hide prototypes
            CommandPrototype.gameObject.SetActive(false);
            ItemPrototype.gameObject.SetActive(false);

            //wire
            Terminal.Instance.Items.OnAdd += Items_OnAdd;
            Terminal.Instance.Items.OnClear += Items_OnClear;
            Terminal.Instance.Items.OnRemove += Items_OnRemove;

            Terminal.Instance.Commands.OnAdd += Commands_OnAdd;
            Terminal.Instance.Commands.OnClear += Commands_OnClear;
            Terminal.Instance.Commands.OnRemove += Commands_OnRemove;

            //add items preadded
            foreach (var item in Terminal.Instance.Items)
            {
                Items_OnAdd(item);
            }

            foreach (var item in Terminal.Instance.Commands)
            {
                Commands_OnAdd(item);
            }

            Application.logMessageReceived += HandlerLog;

            if (DoDontDestoryOnLoad)
            {
                var t = transform;
                while (t.parent != null)
                {
                    t = transform.parent;
                }
                DontDestroyOnLoad(t);
            }

            Debug.Log("Console Ready");
        }


        protected void OnDestroy()
        {
            //remove handlers
            Terminal.Instance.Items.OnAdd -= Items_OnAdd;
            Terminal.Instance.Items.OnClear -= Items_OnClear;
            Terminal.Instance.Items.OnRemove -= Items_OnRemove;

            Terminal.Instance.Commands.OnAdd -= Commands_OnAdd;
            Terminal.Instance.Commands.OnClear -= Commands_OnClear;
            Terminal.Instance.Commands.OnRemove -= Commands_OnRemove;

            Application.logMessageReceived -= HandlerLog;
        }

        private void HandlerLog(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    Terminal.LogError(condition);
                    break;
                case LogType.Warning:
                    Terminal.LogWarning(condition);
                    break;
                case LogType.Log:
                case LogType.Assert:
                    Terminal.Log(condition);
                    break;
            }
        }

        void Commands_OnRemove(TerminalCommand obj)
        {
            var item = CommandItems.FirstOrDefault(o => o.Model.Equals(obj));
            if (item != null)
            {
                CommandItems.Remove(item);
                Destroy(item.gameObject);
            }
        }

        void Commands_OnClear()
        {
            foreach (var item in CommandItems)
            {
                Destroy(item.gameObject);
            }
            CommandItems.Clear();
        }

        void Commands_OnAdd(TerminalCommand obj)
        {
            //inst
            var instance = Instantiate(CommandPrototype.gameObject);
            var script = instance.GetComponent<TerminalCommandView>();
            script.Label.text = obj.Label;
            script.Handler = obj.Method;
            script.Model = obj;

            //parent
            instance.transform.SetParent(CommandLayout);
            //wtf
            instance.transform.localScale = new Vector3(1, 1, 1);
            instance.SetActive(true);

            CommandItems.Add(script);
        }

        void Items_OnRemove(TerminalItem obj)
        {
            var item = TextItems.FirstOrDefault(o => o.Model.Equals(obj));
            if (item != null)
            {
                TextItems.Remove(item);
                Destroy(item.gameObject);
            }
        }

        void Items_OnClear()
        {
            foreach (var item in TextItems)
            {
                Destroy(item.gameObject);
            }
            TextItems.Clear();
        }

        void Items_OnAdd(TerminalItem obj)
        {
            StartCoroutine(AddItemAsync(obj));
        }

        IEnumerator AddItemAsync(TerminalItem obj)
        {

            //inst
            var instance = Instantiate(ItemPrototype.gameObject);
            var script = instance.GetComponent<TerminalItemView>();
            script.Label.text = obj.Text;
            script.Label.color = obj.Color;
            script.Model = obj;

            //parent
            instance.transform.SetParent(ItemLayout);
            instance.SetActive(true);
            //wtf
            instance.transform.localScale = new Vector3(1, 1, 1);

            TextItems.Add(script);

            yield return 1;

            if (ItemScrollBar)
                ItemScrollBar.value = 0;
        }


        protected void Update()
        {
            if (Input.GetKeyUp(VisiblityKey))
            {
                IsVisible = !IsVisible;
            }

        }

        public void DoSend()
        {
            var text = TextInput.text.Replace(Environment.NewLine, "");

            if (string.IsNullOrEmpty(text))
                return;

            Terminal.Submit(text);

            TextInput.text = string.Empty;
        }

        public void DoClear()
        {
            Terminal.Clear();
        }
    }
}