using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpKit.JavaScript.Ast;
using System.IO;
using Corex.Helpers;

namespace SharpKit.Compiler
{
    class Skc5CacheData
    {
        public string VersionKey { get; set; }
        public int? NGenRetries { get; set; }
        public bool? CreatedNativeImage { get; set; }
        static Dictionary<string, string> ReadIniFile(string filename)
        {
            var dic = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filename))
            {
                if (line.StartsWith("#"))
                    continue;
                var index = line.IndexOf("=");
                if (index <= 0)
                    continue;
                var name = line.Substring(0, index);
                var value = line.Substring(index + 1);
                dic[name] = value;
            }
            return dic;
        }
        public void Load(string filename)
        {
            var dic = ReadIniFile(filename);
            VersionKey = dic.TryGetValue("VersionKey");
            NGenRetries = ParseHelper.TryInt(dic.TryGetValue("NGenRetries"));
            CreatedNativeImage = ParseHelper.TryBoolean(dic.TryGetValue("CreatedNativeImage"));
        }
        public void Save(string filename)
        {
            var dir = Path.GetDirectoryName(filename);
            if (dir.IsNotNullOrEmpty() && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllLines(filename, new[]
            {
                "VersionKey="+VersionKey,
                "NGenRetries="+NGenRetries,
                "CreatedNativeImage="+CreatedNativeImage,
            });
        }
    }
    static class ListExtensions
    {
        public static void RemoveDoubles<T>(this List<T> list, T item) where T : class
        {
            var i = 0;
            var count = 0;
            while (i < list.Count)
            {
                var item2 = list[i];
                if (item2 == item)
                {
                    count++;
                    if (count > 1)
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                }
                i++;
            }
        }
        public static void RemoveDoubles<T>(this List<T> list, Func<T, bool> selector) where T : class
        {
            var i = 0;
            var count = 0;
            while (i < list.Count)
            {
                var item2 = list[i];
                if (selector(item2))
                {
                    count++;
                    if (count > 1)
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                }
                i++;
            }
        }
        public static void RemoveDoublesByKey<K, T>(this List<T> list, Func<T, K> keySelector)
            where T : class
            where K : class
        {
            var set = new HashSet<K>();
            var i = 0;
            while (i < list.Count)
            {
                var item = list[i];
                var key = keySelector(item);
                if (key != null)
                {
                    if (set.Contains(key))
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        set.Add(key);
                    }
                }
                i++;
            }
        }

    }


    class CodeInjection
    {
        public CodeInjection()
        {
            Dependencies = new List<CodeInjection>();
        }
        public List<CodeInjection> SelfAndDependencies()
        {
            var list = new List<CodeInjection> { this };
            if (Dependencies.IsNotNullOrEmpty())
            {
                foreach (var dep in Dependencies)
                {
                    list.AddRange(dep.SelfAndDependencies());
                }
            }
            return list;
        }
        public string JsCode { get; set; }
        public string FunctionName { get; set; }
        public JsStatement JsStatement { get; set; }
        public List<CodeInjection> Dependencies { get; set; }
    }

    class CompilerEvent
    {
        public CompilerEvent(Action before, Action action, Action after)
        {
            Before = before;
            Action = action;
            After = after;
        }
        public CompilerEvent(Action action)
        {
            Action = action;
        }
        public Action Before { get; set; }
        public Action Action { get; set; }
        public Action After { get; set; }
    }
}
