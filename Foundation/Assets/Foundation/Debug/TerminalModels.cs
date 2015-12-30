// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;
using UnityEngine;

namespace Foundation.Debuging
{
    /// <summary>
    /// Message Formatting
    /// </summary>
    public enum TerminalType
    {
        Log,
        Warning,
        Error,
        Exception,
        Success,
        Important,
        Input,
    }


    /// <summary>
    /// For processing Text input
    /// </summary>
    public class TerminalInterpreter
    {
        public string Label;
        public Action<string> Method;
    }

    /// <summary>
    /// Button to add to the menu
    /// </summary>
    public class TerminalCommand
    {
        public string Label;
        public Action Method;
    }

    /// <summary>
    /// A Terminal line item
    /// </summary>
    public struct TerminalItem
    {
        readonly string _text;
        public string Text
        {
            get { return _text; }
        }

        readonly string _formatted;
        public string Formatted
        {
            get { return _formatted; }
        }

        readonly TerminalType _type;
        public TerminalType Type
        {
            get { return _type; }
        }

        readonly Color _color;
        public Color Color
        {
            get { return _color; }
        }

        public TerminalItem(TerminalType type, string text)
        {
            _text = text;
            _type = type;
            switch (_type)
            {
                case TerminalType.Warning:
                    _formatted = string.Format("<< {0}", text);
                    _color = Terminal.Instance.WarningColor;
                    break;
                case TerminalType.Error:
                    _formatted = string.Format("<< {0}", text);
                    _color = Terminal.Instance.ErrorColor;
                    break;
                case TerminalType.Success:
                    _formatted = string.Format("<< {0}", text);
                    _color = Terminal.Instance.SuccessColor;
                    break;
                case TerminalType.Important:
                    _formatted = string.Format("<< {0}", text);
                    _color = Terminal.Instance.ImportantColor;
                    break;
                case TerminalType.Input:
                    _formatted = string.Format(">> {0}", text);
                    _color = Terminal.Instance.InputColor;
                    break;
                default:
                    _formatted = text;
                    _color = Terminal.Instance.LogColor;
                    break;
            }
        }
    }
}