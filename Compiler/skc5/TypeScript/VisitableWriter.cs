using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.TypeScript
{
    class VisitableWriter<T> where T : class
    {
        
        public TextWriter Writer { get; set; }
        public Action<T> VisitAction { get; set; }
        public VisitableWriter<T> Write(string s)
        {
            Writer.Write(s);
            return this;
        }
        public VisitableWriter<T> Visit(T node)
        {
            VisitAction(node);
            return this;
        }
        public VisitableWriter<T> VisitEach(IEnumerable<T> list)
        {
            list.ForEach(VisitAction);
            return this;
        }
        public VisitableWriter<T> VisitEachJoin(IEnumerable<T> list, string separator)
        {
            list.ForEachJoin(VisitAction, () => Write(separator));
            return this;
        }
        public VisitableWriter<T> VisitEachJoinIfNotNullOrEmpty(IEnumerable<T> list, string left, string separator, string right)
        {
            if (list == null)
                return this;
            var first = true;
            foreach (var item in list)
            {
                if (first)
                {
                    Write(left);
                    first = false;
                }
                else
                {
                    Write(separator);
                }
                Visit(item);
            }
            if (!first)
                Write(right);
            return this;
        }

        internal VisitableWriter<T> PrefixVisitIf(bool condition, string prefix, T node)
        {
            if (!condition)
                return this;
            PrefixVisit(prefix, node);
            return this;
        }

        public VisitableWriter<T> PrefixVisitIfNotNull(string prefix, T node)
        {
            if (node == null)
                return this;
            PrefixVisit(prefix, node);
            return this;
        }

        public VisitableWriter<T> PrefixVisit(string prefix, T node)
        {
            Write(prefix);
            Visit(node);
            return this;
        }

        public VisitableWriter<T> WriteIf(bool condition, string s)
        {
            if (condition)
                Write(s);
            return this;
        }
        public VisitableWriter<T> WriteIfElse(bool condition, string s, string elseWrite)
        {
            if (condition)
                Write(s);
            else
                Write(elseWrite);
            return this;
        }
    }
}
