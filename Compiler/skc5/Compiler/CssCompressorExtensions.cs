using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpKit.Compiler
{
    static class CssCompressorExtensions
    {
        public static int AppendReplacement(this Capture capture,
                                            StringBuilder value,
                                            string input,
                                            string replacement,
                                            int index)
        {
            string preceding = input.Substring(index,
                                               capture.Index - index);

            value.Append(preceding);
            value.Append(replacement);

            return capture.Index + capture.Length;
        }

        public static void AppendTail(this StringBuilder value,
                                      string input,
                                      int index)
        {
            value.Append(input.Substring(index));
        }

        public static string RegexReplace(this string input,
                                          string pattern,
                                          string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }

        public static string RegexReplace(this string input,
                                          string pattern,
                                          string replacement,
                                          RegexOptions options)
        {
            return Regex.Replace(input,
                                 pattern,
                                 replacement,
                                 options);
        }

        public static string Fill(this string format,
                                  params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 format,
                                 args);
        }

        public static string RemoveRange(this string input,
                                         int startIndex,
                                         int endIndex)
        {
            return input.Remove(startIndex,
                                endIndex - startIndex);
        }

        public static bool EqualsIgnoreCase(this string left,
                                            string right)
        {
            return String.Compare(left,
                                  right,
                                  StringComparison.OrdinalIgnoreCase) == 0;
        }

        // NOTE: To check out some decimal -> Hex converstions,
        //       goto http://www.openstrike.co.uk/cgi-bin/decimalhex.cgi
        public static string ToHexString(this int value)
        {
            return value.ToString("X");
        }

        public static string ToPluralString(this int value)
        {
            return value == 1 ? string.Empty : "s";
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> value)
        {
            return value == null ||
                   value.Count() <= 0
                       ? true
                       : false;
        }

        public static IList<T> ToListIfNotNullOrEmpty<T>(this IList<T> value)
        {
            return value.IsNullOrEmpty() ? null : value;
        }

        public static string Replace(this string value, int startIndex, int endIndex, string newContent)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            // Chop the string into two parts, the before and then the after.
            string before = value.Substring(0, startIndex);
            string after = value.Substring(endIndex);
            return before + newContent + after;
        }
    }
}