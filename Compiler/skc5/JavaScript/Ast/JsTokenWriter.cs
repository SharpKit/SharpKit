using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    class JsTokenWriter
    {
        public JsTokenWriter()
        {
            CurrentColumn = 1;
            CurrentLine = 1;
        }
        public int CurrentLine { get; set; }
        public int CurrentColumn { get; set; }

        public TextWriter InnerWriter { get; set; }
        public void Write(List<JsToken> tokens)
        {
            foreach (var token in tokens)
            {
                if (token.IsNodeStart)
                {
                    token.Node.StartLocation = new TextLocation(CurrentLine, CurrentColumn);
                }
                else if (token.IsNodeEnd && token.Node.EndLocation.IsEmpty)
                {
                    token.Node.EndLocation = new TextLocation(CurrentLine, CurrentColumn);
                }
                else
                {
                    var s = token.Value;
                    if (s.IsNullOrEmpty())
                        continue;
                    InnerWriter.Write(s);
                    foreach (var ch in s)
                    {
                        if (ch == '\n')
                        {
                            CurrentLine++;
                            CurrentColumn = 1;
                        }
                        else
                        {
                            CurrentColumn++;
                        }
                    }
                }
            }
        }

    }
}
