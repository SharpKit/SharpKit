using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    class JsFormatter_CSharp : JsFormatter_Default
    {

        protected override void OnBracketOpen()
        {
            //if (Current.Node.Is(JsNodeType.JsonObjectExpression))
            //{
            //}
            AddBeforeCurrent(JsToken.Enter());
            OnFirstVisibleTokenAfterNewLine();
            base.OnBracketOpen();
        }

    }
}
