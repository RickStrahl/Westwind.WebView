using System;
using System.Globalization;
using System.Text;

namespace Westwind.WebView.HtmlToPdf.Utilities
{
    internal static class StringUtils
    {
        /// <summary>
        /// A helper to generate a JSON string from a string value
        /// 
        /// Use this to avoid bringing in a full JSON Serializer for
        /// scenarios of string serialization.
        ///         
        /// Note: Function includes surrounding quotes!
        /// </summary>
        /// <param name="text"></param>
        /// <returns>JSON encoded string ("text"), empty ("") or "null".</returns>
        internal static string ToJson(this string text, bool noQuotes = false)
        {
            if (text is null)
                return "null";

            var sb = new StringBuilder(text.Length);
            
            if (!noQuotes)
                sb.Append("\"");

            var ct = text.Length;

            for (int x = 0; x < ct; x++)
            {
                var c = text[x];

                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        uint i = c;
                        if (i < 32)  // || i > 255
                            sb.Append($"\\u{i:x4}");
                        else
                            sb.Append(c);
                        break;
                }
            }
            if (!noQuotes)
                sb.Append("\"");

            return sb.ToString();
        }

        internal static string ToJson(this double value, int maxDecimals = 2)
        {
            if (maxDecimals > -1)
                value = Math.Round(value, maxDecimals);

            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static string ToJson(this bool value)
        {
            return value ? "true" : "false";
        }

        internal static string ExtractString(string source,
            string beginDelim,
            string endDelim,
            bool caseSensitive = false,
            bool allowMissingEndDelimiter = false,
            bool returnDelimiters = false)
        {
            int at1, at2;

            if (string.IsNullOrEmpty(source))
                return string.Empty;

            if (caseSensitive)
            {
                at1 = source.IndexOf(beginDelim);
                if (at1 == -1)
                    return string.Empty;

                at2 = source.IndexOf(endDelim, at1 + beginDelim.Length);
            }
            else
            {
                //string Lower = source.ToLower();
                at1 = source.IndexOf(beginDelim, 0, source.Length, StringComparison.OrdinalIgnoreCase);
                if (at1 == -1)
                    return string.Empty;

                at2 = source.IndexOf(endDelim, at1 + beginDelim.Length, StringComparison.OrdinalIgnoreCase);
            }

            if (allowMissingEndDelimiter && at2 < 0)
            {
                if (!returnDelimiters)
                    return source.Substring(at1 + beginDelim.Length);

                return source.Substring(at1);
            }

            if (at1 > -1 && at2 > 1)
            {
                if (!returnDelimiters)
                    return source.Substring(at1 + beginDelim.Length, at2 - at1 - beginDelim.Length);

                return source.Substring(at1, at2 - at1 + endDelim.Length);
            }

            return string.Empty;
        }


    }
}
