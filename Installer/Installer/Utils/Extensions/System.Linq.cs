//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;

//namespace System.Linq
//{
//    internal static class Extensions
//    {

//        /// <summary>
//        /// Concatenates string values that are selected from an IEnumerable (e.g CSV parameter list, with ( and ) )
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="list"></param>
//        /// <param name="stringSelector"></param>
//        /// <param name="prefix"></param>
//        /// <param name="delim"></param>
//        /// <param name="suffix"></param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public static string StringConcat<T>(this IEnumerable<T> list, Func<T, string> stringSelector, string prefix, string delim, string suffix)
//        {
//            StringBuilder sb = new StringBuilder();
//            if (!String.IsNullOrEmpty(prefix))
//                sb.Append(prefix);
//            bool first = true, hasDelim = !String.IsNullOrEmpty(delim);
//            if (list != null)
//            {
//                foreach (T item in list)
//                {
//                    if (hasDelim)
//                    {
//                        if (first)
//                            first = false;
//                        else
//                            sb.Append(delim);
//                    }
//                    string s = stringSelector(item);
//                    if (!String.IsNullOrEmpty(s))
//                        sb.Append(s);
//                }
//            }
//            if (!String.IsNullOrEmpty(suffix))
//                sb.Append(suffix);
//            return sb.ToString();
//        }

//        /// <summary>
//        /// Concatenates an IEnumerable of strings
//        /// </summary>
//        /// <param name="list"></param>
//        /// <param name="prefix"></param>
//        /// <param name="delim"></param>
//        /// <param name="suffix"></param>
//        /// <returns></returns>
//        //[DebuggerStepThrough]
//        public static string StringConcat(this IEnumerable<string> list, string prefix, string delim, string suffix)
//        {
//            StringBuilder sb = new StringBuilder();
//            if (!String.IsNullOrEmpty(prefix))
//                sb.Append(prefix);
//            bool first = true, hasDelim = !String.IsNullOrEmpty(delim);
//            foreach (string item in list)
//            {
//                if (String.IsNullOrEmpty(item))
//                    continue;
//                if (hasDelim)
//                {
//                    if (first)
//                        first = false;
//                    else
//                        sb.Append(delim);
//                }
//                sb.Append(item);
//            }
//            if (!String.IsNullOrEmpty(suffix))
//                sb.Append(suffix);
//            return sb.ToString();
//        }
//        /// <summary>
//        /// Concatenates an IEnumerable of strings
//        /// </summary>
//        /// <param name="list"></param>
//        /// <param name="delim"></param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public static string StringConcat(this IEnumerable<string> list, string delim)
//        {
//            return StringConcat(list, null, delim, null);
//        }

//        /// <summary>
//        /// Returns true if the collection is empty
//        /// </summary>
//        /// <typeparam name="T">Collection item type</typeparam>
//        /// <param name="collection">a collection of items</param>
//        /// <returns>true if the collection contains no elements</returns>
//        public static bool IsEmpty<T>(this IEnumerable<T> collection)
//        {
//            foreach (var i in collection)
//            {
//                return false;
//            }
//            return true;
//        }

//        /// <summary>
//        /// Returns the symmetrical difference between two collections
//        /// </summary>
//        /// <remarks>
//        /// Symmetrical difference is defined as the subtraction of the intersection from the union of the two collections
//        /// </remarks>
//        /// <typeparam name="T">Collection item type</typeparam>
//        /// <param name="a">First collection</param>
//        /// <param name="b">Second collection</param>
//        /// <returns>a collection that has items that are exclusively in one of the collections</returns>
//        public static IEnumerable<T> SymetricalDifference<T>(this IEnumerable<T> a, IEnumerable<T> b)
//        {
//            var union = a.Union(b);
//            var intersection = a.Intersect(b);
//            return union.Except(intersection);
//        }

//        /// <summary>
//        /// Removes all items in Target that are not present in source, and Adds all items in Source that are not present in target
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="target"></param>
//        /// <param name="source"></param>
//        public static void SetItems<T>(this IList<T> target, IEnumerable<T> source)
//        {
//            var targetHash = new HashSet<T>(target);
//            var sourceHash = new HashSet<T>(source);
//            var toDelete = targetHash.Except(sourceHash).ToList();
//            var toAdd = sourceHash.Except(targetHash).ToList();
//            foreach (var d in toDelete)
//                target.Remove(d);
//            foreach (var a in toAdd)
//                target.Add(a);
//        }

//        /// <summary>
//        /// Reverses a list in-place
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="list"></param>
//        /// <returns></returns>
//        public static List<T> InPlaceReverse<T>(this List<T> list)
//        {
//            list.Reverse();
//            return list;
//        }

//        /// <summary>
//        /// Same as Map, but with a condition predicate
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <typeparam name="S"></typeparam>
//        /// <param name="source"></param>
//        /// <param name="mapping"></param>
//        /// <param name="condition"></param>
//        /// <returns></returns>
//        public static List<S> Map<T, S>(this IEnumerable<T> source, Func<T, S> mapping, Predicate<T> condition)
//        {
//            List<S> list = new List<S>();
//            foreach (T item in source)
//            {
//                if (!condition(item))
//                    continue;
//                S mappedItem = mapping(item);
//                if (mappedItem != null)
//                    list.Add(mappedItem);
//            }
//            return list;
//        }

//        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> list)
//        {
//            foreach (T item in list)
//                set.Add(item);
//        }

//        public static Dictionary<K, List<T>> ToDictionary<K, T>(this IEnumerable<IGrouping<K, T>> source)
//        {
//            var dic = new Dictionary<K, List<T>>();
//            foreach (var group in source)
//            {
//                var list = new List<T>(group);
//                dic[group.Key] = list;
//            }
//            return dic;
//        }

//        public static Dictionary<T, S> MapToDictionary<T, S>(this IEnumerable<T> source, Func<T, S> valueSelector)
//        {
//            Dictionary<T, S> ret = new Dictionary<T, S>();
//            foreach (T item in source)
//            {
//                ret.Add(item, valueSelector(item));
//            }
//            return ret;
//        }

//        public static void RemoveKeysWithValue<T, S>(this Dictionary<T, S> source, S value)
//        {
//            List<T> matchingKeys = new List<T>();
//            foreach (T key in source.Keys)
//            {
//                if (source[key].Equals(value))
//                    matchingKeys.Add(key);
//            }
//            foreach (T key in matchingKeys)
//                source.Remove(key);
//        }

//        public static void KeepOnlyKeysWithValue<T, S>(this IDictionary<T, S> source, S value)
//        {
//            List<T> matchingKeys = new List<T>();
//            foreach (T key in source.Keys)
//            {
//                if (!source[key].Equals(value))
//                    matchingKeys.Add(key);
//            }
//            foreach (T key in matchingKeys)
//                source.Remove(key);
//        }

//        [DebuggerStepThrough]
//        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
//        {
//            foreach (T item in items)
//                action(item);
//        }
//        public static void ForEachJoin<T>(this IEnumerable<T> items, Action<T> action, Action actionBetweenItems)
//        {
//            var first = true;
//            foreach (var item in items)
//            {
//                if (first)
//                    first = false;
//                else
//                    actionBetweenItems();
//                action(item);
//            }
//        }


//    }
//}
