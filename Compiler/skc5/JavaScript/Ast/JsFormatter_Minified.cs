using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    class JsFormatter_Minified : JsFormatter
    {
        int Col = 1;
        protected override void Process()
        {
            if (Current.IsComment || Current.Node.Is(JsNodeType.CommentStatement))
                return;
            if (Current.IsNewLine)
                Col = 1;
            else if (Current.Value != null)
            {
                var s = Current.Value;
                var index = s.LastIndexOf("\n");
                if (index >= 0)
                    Col = s.Length - index;
                else
                    Col += s.Length;
                if (Col > 7000)
                {
                    if (Current.IsWhitespace)
                    {
                        ReplaceCurrent(JsToken.Enter());
                        Col = 1;
                    }
                    else if (Current.IsKeyword || Current.IsControl || Current.IsLiteral || Current.IsValue)
                    {
                        AddAfterCurrent(JsToken.Enter());
                    }
                    else
                    {
                    }
                }
            }
            //base.Process();
        }


    }
}
