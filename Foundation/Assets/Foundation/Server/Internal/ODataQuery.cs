// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foundation.Server
{
    /// <summary>
    /// Tool for constructing OData queries
    /// </summary>
    public class ODataQuery<T> where T : class
    {
        // ReSharper disable InconsistentNaming
        protected int skip;
        protected int take;
        protected string orderBy;
        protected List<string> filters = new List<string>();

        protected void AddFilter(string format, params object[] values)
        {
            filters.Add(string.Format(format, values));
        }

        ///<summary>
        /// Where the property contains
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">String, Number</param>
        /// <returns></returns>
        public ODataQuery<T> WhereContains(string key, object value)
        {
            AddFilter("substringof({1}, {0})", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the property string value starts with
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">String, Number</param>
        /// <returns></returns>
        public ODataQuery<T> WhereStartsWith(string key, object value)
        {
            AddFilter("{0} sw {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the property string value ends with
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">String, Number</param>
        /// <returns></returns>
        public ODataQuery<T> WhereEndsWiths(string key, object value)
        {
            AddFilter("{0} ew {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the property value equals
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">String, Number</param>
        /// <returns></returns>
        public ODataQuery<T> WhereEquals(string key, object value)
        {
            AddFilter("{0} eq {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the property value does not equal
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">String, Number</param>
        /// <returns></returns>
        public ODataQuery<T> WhereNotEquals(string key, object value)
        {
            AddFilter("{0} ne {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the (numeric) property value is greater than
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">Number, DateTime</param>
        /// <returns></returns>
        public ODataQuery<T> WhereGreaterThan(string key, object value)
        {
            AddFilter("{0} gt {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the (numeric) property value is less than
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">Number, DateTime</param>
        /// <returns></returns>
        public ODataQuery<T> WhereLessThan(string key, object value)
        {
            AddFilter("{0} lt {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the (numeric) property value is greater than or equal to
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">Number, DateTime</param>
        /// <returns></returns>
        public ODataQuery<T> WhereGreaterThanOrEqualTo(string key, object value)
        {
            AddFilter("{0} ge {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Where the (numeric) property value is less than or equal to
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">Number, DateTime</param>
        /// <returns></returns>
        public ODataQuery<T> WhereLessThanOrEqualTo(string key, object value)
        {
            AddFilter("{0} le {1}", key, ReadValue(value));
            return this;
        }

        ///<summary>
        /// Sorts with the highest value first
        /// </summary>
        /// <summary>
        /// Orders the selection by the property
        /// </summary>
        /// <param name="key">property name</param>
        /// <returns></returns>
        public ODataQuery<T> OrderByDescending(string key)
        {
            orderBy = string.Format("orderby={0} desc", key);
            return this;
        }

        /// <summary>
        /// Sorts with the lowest value first
        /// </summary>
        /// <param name="key">property name</param>
        /// <returns></returns>
        public ODataQuery<T> OrderBy(string key)
        {
            orderBy = string.Format("orderby={0}", key);
            return this;
        }

        /// <summary>
        /// Skip, For paging.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public ODataQuery<T> Skip(int count)
        {
            skip = count;
            return this;
        }

        /// <summary>
        /// Truncate, For paging.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public ODataQuery<T> Take(int count)
        {
            take = count;
            return this;
        }

        #region internal

        public int GetTake()
        {
            return skip;
        }

        public int GetSkip()
        {
            return skip;
        }

        protected string ReadValue(object value)
        {
            if (value is DateTime)
            {
                //  return new DateTimeOffset(((DateTime)value).ToUniversalTime()).ToString();
                return (new DateTimeOffset((DateTime)value)).ToString("yyyy-MM-ddTHH:mm");
            }
            if (value is string)
            {
                return string.Format("'{0}'", value);
            }
            if (value is bool)
            {
                return ((bool)value) ? "true" : "false";
            }
            if (value == null)
            {
                return "''";
            }
            return value.ToString();
        }

        public override string ToString()
        {
            var first = true;
            var sb = new StringBuilder();

            if (filters.Any())
            {
                ClausePrefix(sb, true);
                first = false;
                sb.AppendFormat("filter=");

                for (int i = 0;i < filters.Count;i++)
                {
                    if (i > 0)
                        sb.AppendFormat(" and ");

                    sb.Append(filters[i]);
                }
            }


            if (!string.IsNullOrEmpty(orderBy))
            {
                ClausePrefix(sb, first);
                first = false;
                sb.Append(orderBy);
            }

            if (skip > 0)
            {
                ClausePrefix(sb, first);
                first = false;
                sb.AppendFormat("skip={0}", skip);
            }

            if (take > 0)
            {
                ClausePrefix(sb, first);
                sb.AppendFormat("top={0}", take);
            }

            // Unity will throw a 400 BadRequest if there are spaces in the query string.
            // Convert them to %20 (ascii equivalent) to get around this.
            return sb.ToString().Replace(" ", "%20");
        }

        void ClausePrefix(StringBuilder sb, bool first)
        {
            sb.Append(first ? "?$" : "&$");
        }

        #endregion
    }
}
