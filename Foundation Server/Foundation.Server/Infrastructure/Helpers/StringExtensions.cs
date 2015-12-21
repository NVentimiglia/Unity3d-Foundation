using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Foundation.Server.Infrastructure.Helpers
{
    public static class StringExtensions
    {

        /// <summary>
        /// If the value is longer than max length, the string is truncated and appended with ...
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string TrimIfLongerThan(this string value, int maxLength)
        {
            if (value.Length > maxLength)
            {
                return value.Substring(0, maxLength) + "...";
            }

            return value;
        }


        /// <summary>
        /// Null if the string is empty, otherwise the original string.
        /// (Useful to use with with null coalesce, e.g. myString.AsNullIfEmpty() ?? defaultString
        /// </summary>
        public static string AsNullIfEmpty(this string items)
        {
            return string.IsNullOrEmpty(items) ? null : items;
        }

        /// <summary>
        /// Null if the string is empty or whitespace, otherwise the original string.
        /// (Useful to use with with null coalesce, e.g. myString.AsNullIfWhiteSpace() ?? defaultString
        /// </summary>
        public static string AsNullIfWhiteSpace(this string items)
        {
            return string.IsNullOrWhiteSpace(items) ? null : items;
        }


        /// <summary>
        /// Creates a URL friendly slug from a string
        /// </summary>
        public static string ToUrlSlug(this string str)
        {
            string originalValue = str;

            // Repalce any characters that are not alphanumeric with hypen
            str = Regex.Replace(str, "[^a-z^0-9]", "-", RegexOptions.IgnoreCase);

            // Replace all double hypens with single hypen
            string pattern = "--";
            while (Regex.IsMatch(str, pattern))
                str = Regex.Replace(str, pattern, "-", RegexOptions.IgnoreCase);

            // Remove leading and trailing hypens ("-")
            pattern = "^-|-$";
            str = Regex.Replace(str, pattern, "", RegexOptions.IgnoreCase);

            return str.ToLower();
        }

        /// <summary>
        /// return str.Replace(Environment.NewLine, "<br />");
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToHtml(this string str)
        {
            return str.Replace(Environment.NewLine, "<br />");
        }

        /// <summary>
        /// Combines two parts of a Uri similiar to Path.Combine
        /// </summary>
        /// <param name="val"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public static string UriCombine(this string val, string append)
        {
            if (String.IsNullOrEmpty(val))
            {
                return append;
            }

            if (String.IsNullOrEmpty(append))
            {
                return val;
            }

            return val.TrimEnd('/') + "/" + append.TrimStart('/');
        }


        public const string EmailRegex = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        public static bool IsEmail(this string str)
        {
            return Regex.IsMatch(str, EmailRegex);

        }

        /// <summary>
        /// Uppercase
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string UppercaseFirst(this string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static byte[] GetBytes(this string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(this byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string PrettyJson(this string s)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(s);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}