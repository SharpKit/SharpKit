using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;

namespace System.Collections.Generic
{
    static class CollectionExtensions
    {
        public static bool Parallel = true;
        public static Action ParallelPreAction { get; set; }
        public static void ForEachParallel<T>(this IEnumerable<T> items, Action<T> action, bool parallel)
        {
            if (parallel)
            {
                Action<T> action2 = action;
                if (ParallelPreAction != null)
                    action2 = t => { ParallelPreAction(); action(t); };
                items.AsParallel().ForAll(action2);
            }
            else
                items.ForEach(action);
        }
        public static void ForEachParallel<T>(this IEnumerable<T> items, Action<T> action)
        {
            items.ForEachParallel(action, Parallel);
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> list)
        {
            if (list == null)
                return Enumerable.Empty<T>();
            return list;
        }
        //public static T NotNull<T>(this T obj) where T : class, new()
        //{
        //    if (obj == null)
        //        return new T();
        //    return obj;
        //}
        public static R IfNotNull<T, R>(this T obj, Func<T, R> func) where T : class
        {
            if (obj != null)
                return func(obj);
            return default(R);
        }

        public static bool Matches<T>(this IEnumerable<T> list, IEnumerable<T> list2, Func<T, T, bool> match)
        {
            var x = list.GetEnumerator();
            var y = list2.GetEnumerator();
            while (true)
            {
                var x1 = x.MoveNext();
                var x2 = y.MoveNext();
                if (x1 != x2)
                    return false;
                if (!x1)
                    return true;
                if (!match(x.Current, y.Current))
                    return false;
            }
        }


    }


}
