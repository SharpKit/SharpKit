using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    class JsFormatter_Default : JsFormatter
    {
        protected override void OnStatementSemicolon()
        {
            AddAfterCurrent(JsToken.Enter());
        }

        protected override void OnDoWhileStatementParanthesesClose()
        {
            AddAfterCurrent(JsToken.Enter());
        }
        protected override void OnBracketOpen()
        {
            AddAfterCurrent(JsToken.Enter());
            Indent++;
        }
        protected override void OnForStatementSemicolon()
        {
            var next = CurrentNode.NextVisibleToken();
            if (next != null && next.Value.Value == ")")
                return;
            AddAfterCurrent(JsToken.Space());
        }
        protected override void OnForStatementComma()
        {
            AddAfterCurrent(JsToken.Space());
        }

        protected override void OnFirstVisibleTokenAfterNewLine()
        {
            AddBeforeCurrent(JsToken.Space(Indent * IndentSize));
            if (Current.Node.IsSwitchSectionStatement())
                AddBeforeCurrent(JsToken.Space(1 * IndentSize));
        }

        protected override void OnJsonNameValueSeparator()
        {
            AddAfterCurrent(JsToken.Space());
        }

        protected override void OnBracketClose()
        {
            if (Current.Node.Is(JsNodeType.JsonObjectExpression))
            {
                //if (Prev.Node.Is(JsNodeType.JsonNameValue))
                //{
                    AddBeforeCurrent(JsToken.Enter());
                //}
            }
            var next = CurrentNode.NextVisibleToken();
            if (next != null && next.Value.IsControl && ";,)".Contains(next.Value.Value))
            {
                //tokens2.AddAfter(current.NextVisibleToken(), JsToken.Enter());
            }
            else
            {

                AddAfterCurrent(JsToken.Enter());
            }
            Indent--;
        }

        protected override void OnFunctionOrInvocationParameterSeparator()
        {
            AddAfterCurrent(JsToken.Space());
        }

        protected override void OnJsonObjectSeparator()
        {
            AddAfterCurrent(JsToken.Enter());
        }
        protected override void OnJsonArraySeparator()
        {
            AddAfterCurrent(JsToken.Space());
        }

        protected override void OnBinaryOperator()
        {
            AddBeforeCurrent(JsToken.Space());
            AddAfterCurrent(JsToken.Space());
        }

        protected override void OnSingleNestedStatementEnd()
        {
            Indent--;
        }

        protected override void OnSingleNestedStatementStart()
        {
            AddAfterCurrent(JsToken.Enter());
            Indent++;
        }


        protected override void OnSwitchLabelSeparator()
        {
            AddAfterCurrent(JsToken.Enter());
            //Indent++;
        }

        protected override void OnCommentStatementEnd()
        {
            base.OnCommentStatementEnd();
            AddAfterCurrent(JsToken.Enter());
        }


        protected override void OnFunctionSwitchIfWhileForKeyword()
        {
            var func = Current.Node as JsFunction;
            if (func != null && func.Name.IsNotNullOrEmpty())
                return;
            AddAfterCurrent(JsToken.Space());
        }


    }
}
