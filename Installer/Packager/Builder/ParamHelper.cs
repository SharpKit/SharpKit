using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpKit.Installer.Builder
{
    class ParamHelper
    {
        public static string Eval(string s, Dictionary<string, string> expressions)
        {
            var dic = new Dictionary<string, string>(expressions);
            var key = "~~~~";
            dic.Add(key, s);
            var x = EvalKeyInline(key, dic);
            return x;
        }
        public static string Eval(string s, object obj)
        {
            return Eval(s, ObjectToDictionary(obj));
        }
        public static Dictionary<string, string> Eval(Dictionary<string, string> expressions)
        {
            var dic = new Dictionary<string, string>(expressions);
            foreach (var pair in expressions)
            {
                EvalKeyInline(pair.Key, dic);
            }
            return dic;
        }
        public static void EvalInline(object obj)
        {
            var dic = ObjectToDictionary(obj);
            dic = Eval(dic);
            foreach (var pe in obj.EnumerateProperties())
            {
                var value = dic.TryGetValue(pe.Property.Name);
                pe.Setter(value);
            }
        }

        private static Dictionary<string, string> ObjectToDictionary(object obj)
        {
            var dic = new Dictionary<string, string>();
            foreach (var pe in obj.EnumerateProperties())
            {
                var value = pe.Getter();
                if (value != null)
                    dic.Add(pe.Property.Name, value.ToString());
            }
            return dic;
        }
        static string EvalKeyInline(string key, Dictionary<string, string> context)
        {
            var expression = context[key];
            var tokens = Parse(expression);
            var sb = new StringBuilder();
            foreach (var token in tokens)
            {
                if (token is ParamToken)
                {
                    var prmName = ((ParamToken)token).Name;
                    sb.Append(EvalKeyInline(prmName, context));
                }
                else if (token is StringToken)
                {
                    sb.Append(((StringToken)token).Value);
                }
                else
                {
                    throw new Exception();
                }
            }
            var s = sb.ToString();
            context[key] = s;
            return s;
        }
        static TokenList Parse(string expression)
        {
            var re = new Regex("\\{([a-zA-Z_]+[a-zA-Z0-9_]*)\\}");
            var matches = re.Matches(expression);
            var list = new TokenList();
            if (matches.Count > 0)
            {
                var i = 0;
                foreach (Match match in matches)
                {
                    if (match.Index > i)
                        list.Add(new StringToken(expression.SubstringBetween(i, match.Index)));
                    list.Add(new ParamToken(match.Groups[1].Captures[0].Value));
                    i = match.Index + match.Length;
                }
                if(i<expression.Length)
                    list.Add(new StringToken(expression.Substring(i)));
            }
            else
            {
                list.Add(new StringToken(expression));
            }
            return list;

        }
    }



    class TokenList : List<Token>
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var p in this)
                sb.Append(p.ToString());
            return sb.ToString();
        }
    }
    class Token
    {
    }
    class ParamToken : Token
    {
        public ParamToken(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public override string ToString()
        {
            return String.Format("{{{0}}}", Name);
        }
    }
    class StringToken : Token
    {
        public StringToken(string value)
        {
            Value = value;
        }
        public string Value { get; set; }
        public override string ToString()
        {
            return Value;
        }
    }
}
