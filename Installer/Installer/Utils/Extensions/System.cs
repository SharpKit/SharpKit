//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace System
//{
//    internal static class Extensions
//    {
//        public static string F(this string format, params object[] args)
//        {
//            return String.Format(format, args);
//        }
//        public static string F(this string format, object arg0)
//        {
//            return String.Format(format, arg0);
//        }
//        public static string F(this string format, object arg0, object arg1)
//        {
//            return String.Format(format, arg0, arg1);
//        }
//        public static string F(this string format, object arg0, object arg1, object arg2)
//        {
//            return String.Format(format, arg0, arg1, arg2);
//        }

//        public static T IfTrue<T>(this bool x, T value)
//        {
//            if (x)
//                return value;
//            return default(T);
//        }
//        public static T IfTrue<T>(this bool x, T value, T elseValue)
//        {
//            if (x)
//                return value;
//            return elseValue;
//        }
//        public static T IfFalse<T>(this bool x, T value)
//        {
//            if (!x)
//                return value;
//            return default(T);
//        }

//        public static string ToStringOrEmpty<T>(this T? value) where T : struct
//        {
//            if (!value.HasValue)
//                return String.Empty;
//            return value.ToString();
//        }
//        public static string ToStringOrEmpty<T>(this T value) where T : class
//        {
//            if (value == null)
//                return String.Empty;
//            return value.ToString();
//        }
//        public static bool IsNullOrEmpty<T>(this T[] array)
//        {
//            return array == null || array.Length == 0;
//        }

//        public static bool IsNotNullOrEmpty<T>(this T[] array)
//        {
//            return array != null && array.Length > 0;
//        }

//        public static bool IsNullOrEmpty(this string s)
//        {
//            return s == null || s.Length == 0;
//        }

//        public static bool IsNotNullOrEmpty(this string s)
//        {
//            return s != null && s.Length > 0;
//        }

//        public static string ReplaceFirst(this string s, string search, string replace)
//        {
//            return ReplaceFirst(s, search, replace, StringComparison.CurrentCulture);
//        }

//        public static string ReplaceFirst(this string s, string search, string replace, StringComparison comparisonType)
//        {
//            int index = s.IndexOf(search, comparisonType);
//            if (index != -1)
//            {
//                string final = String.Concat(s.Substring(0, index), replace, s.Substring(search.Length + index));
//                return final;
//            }
//            return s;
//        }

//        public static string RemoveLast(this string s, int count)
//        {
//            return s.Substring(0, s.Length - count);
//        }

//        public static string TrimEnd(this string s, string trimText)
//        {
//            if (s.EndsWith(trimText))
//                return RemoveLast(s, trimText.Length);
//            return s;
//        }

//        public static bool EqualsIgnoreCase(this string s1, string s2)
//        {
//            return String.Compare(s1, s2, true) == 0;
//        }

//        public static List<string> SplitAt(this string text, int index)
//        {
//            return SplitAt(text, index, false);
//        }

//        public static List<string> SplitAt(this string text, int index, bool removeIndexChar)
//        {
//            string s1 = text.Substring(0, index);
//            if (removeIndexChar)
//                index++;
//            string s2 = text.Substring(index);
//            return new List<string>{s1, s2};
//        }


//        public static string SubstringBetween(this string s, int fromIndex, int toIndex)
//        {
//            return s.Substring(fromIndex, toIndex - fromIndex);
//        }
//        public static string ReplaceBetween(this string s, string from, string to, string value)
//        {
//            int index1, index2;
//            if (s.IndexesBetween(from, to, out index1, out index2))
//            {
//                var s2 = s.Remove(index1, index2 - index1);
//                s2 = s2.Insert(index1, value);
//                return s2;
//            }
//            return s;
//        }
//        public static string SubstringBetween(this string s, string from, string to)
//        {
//            int index1, index2;
//            if (s.IndexesBetween(from, to, out index1, out index2))
//                return s.SubstringBetween(index1, index2);
//            return null;
//        }
//        public static bool IndexesBetween(this string s, string from, string to, out int index, out int index2)
//        {
//            index = s.IndexOf(from);
//            if (index >= 0)
//            {
//                index += from.Length;
//                index2 = s.IndexOf(to, index);
//                if (index2 >= 0)
//                    return true;
//            }
//            index2 = -1;
//            return false;
//        }
//        public static IEnumerable<int> AllIndexesOf(this string s, string find, StringComparison comparisonType=StringComparison.CurrentCulture)
//        {
//            var index = 0;
//            var list = new List<int>();
//            while (index >= 0 && index < s.Length)
//            {
//                index = s.IndexOf(find, index, comparisonType);
//                if (index >= 0)
//                {
//                    yield return index;
//                    index++;
//                }
//            }
//        }


//    }
//}
