using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpKit.Compiler.SourceMapping
{
    static class SourceMappingHelper
    {
        public static string GetJsMappingCode(string srcUrl)
        {
            return String.Format("//@ sourceMappingURL={0}", srcUrl);
        }

        public static List<int> DecodeAll(string s)
        {
            var list = new List<int>();
            var enumerator = s.GetEnumerator();
            while (true)
            {
                var x = Base64VLQ.Decode(enumerator);
                if (x == null)
                    break;
                list.Add(x.Value);
            }
            return list;

        }
        public static string EncodeAll(List<int> numbers)
        {
            var sb = new StringBuilder();
            foreach (var num in numbers)
                Base64VLQ.Encode(sb, num);
            return sb.ToString();
        }

    }
}
