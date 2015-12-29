using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Foundation.Server.Infrastructure.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public enum ODataFilterOperation
    {
        NotEqual,
        Equal,
        GreaterThan,
        LessThan,
        LessThanOrEqualTo,
        GreaterThanOrEqualTo,
        Contains,
        StartsWith,
        EndsWith
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ODataFilterSplit
    {
        And,
        Or,
        Not,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ODataValueType
    {
        String,
        Number,
        DateTime
    }

    public class ODataFilterItem
    {
        public ODataFilterSplit Split { get; set; }
        public ODataFilterOperation Operation { get; set; }
        public ODataValueType Type { get; set; }
        public string Property { get; set; }
        public object Value { get; set; }

        public DateTime AsDateTime()
        {
            return (DateTime)Value;
        }
        public double AsNumber()
        {
            return (double)Value;
        }
        public string AsString()
        {
            return (string)Value;
        }
    }

    public class ODataFilterHelper
    {
        private static string[] splits = new[] { "and", "or", "not" };
        private static string[] ops = new[] { "ge", "le", "eq", "ne", "lt", "gt"/*custom*/ , "co", "sw", "ew" };

        public ODataFilterItem[] GetFilters(string raw)
        {
            var rawItems = raw.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            List<ODataFilterItem> items = new List<ODataFilterItem>();

            ODataFilterItem item = new ODataFilterItem
            {
                Split = ODataFilterSplit.And,
            };

            for (int i = 0;i < rawItems.Length;i++)
            {
                var r = rawItems[i];

                // start
                if (splits.Contains(r))
                {
                    // ReSharper disable PossibleNullReferenceException
                    item = new ODataFilterItem();

                    if (r == "and")
                        item.Split = ODataFilterSplit.And;
                    else if (r == "or")
                        item.Split = ODataFilterSplit.Or;
                    else if (r == "not")
                        item.Split = ODataFilterSplit.Not;
                    continue;
                }

                // operator
                if (ops.Contains(r))
                {
                    if (r == "eq")
                        item.Operation = ODataFilterOperation.Equal;
                    else if (r == "ne")
                        item.Operation = ODataFilterOperation.NotEqual;
                    else if (r == "gt")
                        item.Operation = ODataFilterOperation.GreaterThan;
                    else if (r == "lt")
                        item.Operation = ODataFilterOperation.LessThan;
                    else if (r == "le")
                        item.Operation = ODataFilterOperation.LessThanOrEqualTo;
                    else if (r == "ge")
                        item.Operation = ODataFilterOperation.GreaterThanOrEqualTo;
                    else if (r == "co")
                        item.Operation = ODataFilterOperation.Contains;
                    else if (r == "sw")
                        item.Operation = ODataFilterOperation.StartsWith;
                    else if (r == "ew")
                        item.Operation = ODataFilterOperation.EndsWith;
                    continue;
                }

                // propertyname

                if (string.IsNullOrEmpty(item.Property))
                {
                    item.Property = r;

                }
                else
                {
                    //value
                    DateTime date;
                    double number;
                    if (DateTime.TryParse(r, out date))
                    {
                        item.Value = date;
                        item.Type = ODataValueType.DateTime;
                    }
                    else if (Double.TryParse(r, out number))
                    {
                        item.Value = number;
                        item.Type = ODataValueType.Number;
                    }
                    else
                    {
                        item.Value = r.Replace("'","");
                    }

                    items.Add(item);

                    item = null;

                }
            }

            return items.ToArray();
        }


        public IEnumerable<JObject> ApplyFilter(IEnumerable<JObject> list, ODataFilterItem item)
        {
            switch (item.Operation)
            {
                case ODataFilterOperation.Equal:
                    return list.Where(o => WhereEquals(o.GetValue(item.Property), item));
                case ODataFilterOperation.NotEqual:
                    return list.Where(o => WhereNotEquals(o.GetValue(item.Property), item));
                case ODataFilterOperation.GreaterThan:
                    return list.Where(o => GreaterThan(o.GetValue(item.Property), item));
                case ODataFilterOperation.GreaterThanOrEqualTo:
                    return list.Where(o => GreaterThanOrEquals(o.GetValue(item.Property), item));
                case ODataFilterOperation.LessThan:
                    return list.Where(o => LessThan(o.GetValue(item.Property), item));
                case ODataFilterOperation.LessThanOrEqualTo:
                    return list.Where(o => LessThanOrEquals(o.GetValue(item.Property), item));
                // Custom
                case ODataFilterOperation.StartsWith:
                    return list.Where(o => StartsWith(o.GetValue(item.Property), item));
                case ODataFilterOperation.EndsWith:
                    return list.Where(o => EndsWith(o.GetValue(item.Property), item));
                case ODataFilterOperation.Contains:
                    return list.Where(o => Contains(o.GetValue(item.Property), item));
            }

            return list;
        }
        bool WhereEquals(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            switch (item.Type)
            {
                case ODataValueType.DateTime:
                    return token.ToObject<DateTime>() == item.AsDateTime();
                case ODataValueType.Number:
                    return token.ToObject<double>() == item.AsNumber();
                default:
                    return token.ToObject<string>() == item.AsString();
            }
        }

        bool WhereNotEquals(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            switch (item.Type)
            {
                case ODataValueType.DateTime:
                    return token.ToObject<DateTime>() != item.AsDateTime();
                case ODataValueType.Number:
                    return token.ToObject<double>() != item.AsNumber();
                default:
                    return token.ToObject<string>() != item.AsString();
            }
        }

        bool GreaterThan(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            switch (item.Type)
            {
                case ODataValueType.DateTime:
                    return token.ToObject<DateTime>() > item.AsDateTime();
                case ODataValueType.Number:
                    return token.ToObject<double>() > item.AsNumber();
                case ODataValueType.String:
                    return token.ToObject<string>().StartsWith(item.AsString());
                default:
                    return false;
            }
        }

        bool GreaterThanOrEquals(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            switch (item.Type)
            {
                case ODataValueType.DateTime:
                    return token.ToObject<DateTime>() >= item.AsDateTime();
                case ODataValueType.Number:
                    return token.ToObject<double>() >= item.AsNumber();
                case ODataValueType.String:
                    return token.ToObject<string>().StartsWith(item.AsString());
                default:
                    return false;
            }
        }

        bool LessThan(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            switch (item.Type)
            {
                case ODataValueType.DateTime:
                    return token.ToObject<DateTime>() < item.AsDateTime();
                case ODataValueType.Number:
                    return token.ToObject<double>() < item.AsNumber();
                case ODataValueType.String:
                    return System.String.Compare(token.ToObject<string>(), item.AsString(), StringComparison.OrdinalIgnoreCase) < 0;
                default:
                    return false;
            }
        }

        bool LessThanOrEquals(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            switch (item.Type)
            {
                case ODataValueType.DateTime:
                    return token.ToObject<DateTime>() <= item.AsDateTime();
                case ODataValueType.Number:
                    return token.ToObject<double>() <= item.AsNumber();
                case ODataValueType.String:
                    return String.Compare(token.ToObject<string>(), item.AsString(), StringComparison.OrdinalIgnoreCase) <= 0;
                default:
                    return false;
            }
        }

        bool Contains(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            return token.ToObject<string>().Contains(item.AsString());
        }

        bool StartsWith(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            return token.ToObject<string>().StartsWith(item.AsString());
        }
        bool EndsWith(JToken token, ODataFilterItem item)
        {
            if (token == null)
                return false;

            return token.ToObject<string>().EndsWith(item.AsString());
        }
    }
}