// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using Foundation.Databinding;
using UnityEngine;

namespace Foundation.Debuging
{
    /// <summary>
    /// The Terminal API
    /// </summary>
    public class Terminal
    {
        #region static
        public static readonly Terminal Instance = new Terminal();
        #endregion

        #region props / fields

        // Default color of the standard display text.

        public Color LogColor = Color.white;
        public Color WarningColor = Color.yellow;
        public Color ErrorColor = Color.red;
        public Color SuccessColor = Color.green;
        public Color InputColor = Color.green;
        public Color ImportantColor = Color.cyan;

        public readonly ObservableList<TerminalItem> Items = new ObservableList<TerminalItem>();
        public readonly ObservableList<TerminalCommand> Commands = new ObservableList<TerminalCommand>();
        public readonly ObservableList<TerminalInterpreter> Interpreters = new ObservableList<TerminalInterpreter>();
        
        #endregion

        /// <summary>
        /// Add Command
        /// </summary>
        public static void Add(TerminalCommand arg)
        {
            Instance.Commands.Add(arg);
        }

        /// <summary>
        /// Add Interpreter
        /// </summary>
        public static void Add(TerminalInterpreter arg)
        {
            Instance.Interpreters.Add(arg);
        }


        /// <summary>
        /// write only
        /// </summary>
        public static void Add(TerminalItem message)
        {
            Instance.Items.Add(message);
        }

        /// <summary>
        /// write only
        /// </summary>
        public static void Add(object message, TerminalType type)
        {
            Add(new TerminalItem(type, message.ToString()));
        }

        /// <summary>
        /// write only
        /// </summary>
        public static void Log(object message)
        {
            Add(new TerminalItem(TerminalType.Log, message.ToString()));
        }

        /// <summary>
        /// write only
        /// </summary>
        public static void LogError(object message)
        {
            Add(new TerminalItem(TerminalType.Error, message.ToString()));
        }

        /// <summary>
        /// write only
        /// </summary>
        public static void LogWarning(object message)
        {
            Add(new TerminalItem(TerminalType.Warning, message.ToString()));
        }

        /// <summary>
        /// write only
        /// </summary>
        public static void LogSuccess(object message)
        {
            Add(new TerminalItem(TerminalType.Success, message.ToString()));
        }

        /// <summary>
        /// write only
        /// </summary>
        public static void LogImportant(object message)
        {
            Add(new TerminalItem(TerminalType.Important, message.ToString()));
        }

        /// <summary>
        /// write only
        /// </summary>
        public static void LogInput(object message)
        {
            Add(new TerminalItem(TerminalType.Input, message.ToString()));
        }


        /// <summary>
        /// Input for a command
        /// </summary>
        /// <param name="message"></param>
        public static void Submit(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                LogInput(string.Empty);
                return;
            }

            message = message.Trim();

            Add(new TerminalItem(TerminalType.Input, message));

            foreach (var interpreter in Instance.Interpreters)
            {
                interpreter.Method.Invoke(message);
            }
        }

        /// <summary>
        /// clear writes
        /// </summary>
        public static void Clear()
        {
            Instance.Items.Clear();
        }
    }
}