using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    class JsFormatter
    {
        public JsFormatter()
        {
            IndentSize = 4;
        }
        public int IndentSize { get; set; }
        public int Line { get; set; }
        public List<JsToken> Format(List<JsToken> tokens)
        {
            Tokens = new LinkedList<JsToken>(tokens);
            Events = new Stack<string>();
            CurrentNode = Tokens.First;
            Indent = 0;
            Line = 1;
            while (CurrentNode != null)
            {
                Process();
                MoveNext();
            }
            Line = 1;
            return Tokens.ToList();
        }

        private void VerifyNoEmptyLine()
        {
            if (!Current.IsNewLine)
                return;

            var x = CurrentNode.Previous;
            while (x != null)
            {
                if (x.Value.IsNewLine)
                {
                    var c = Tokens.RemoveRange(x, CurrentNode.Previous);
                    return;
                }
                if (!x.Value.IsWhitespace && !x.Value.IsNodeStartOrEnd)
                    return;
                x = x.Previous;
            }
        }

        public JsToken Current { get; private set; }
        
        public JsToken Next
        {
            get
            {
                if (CurrentNode == null || CurrentNode.Next == null)
                    return JsToken.Empty;
                return CurrentNode.Next.Value;
            }
        }
        public JsToken Prev
        {
            get
            {
                if (CurrentNode == null || CurrentNode.Previous == null)
                    return JsToken.Empty;
                return CurrentNode.Previous.Value;
            }
        }
        public LinkedListNode<JsToken> NextNode
        {
            get
            {
                if (CurrentNode == null)
                    return null;
                return CurrentNode.Next;
            }
        }
        public LinkedListNode<JsToken> PrevNode
        {
            get
            {
                if (CurrentNode == null)
                    return null;
                return CurrentNode.Previous;
            }
        }
        public bool MoveNext()
        {
            CurrentNode = CurrentNode.Next;
            if (CurrentNode == null)
            {
                Current = JsToken.Empty;
                return false;
            }
            Current = CurrentNode.Value;
            return true;
        }

        //StringWriter Log;
        Stack<string> Events;
        public string CurrentEvent { get { return Events.PeekOrDefault(); } }
        void On(Action ev)
        {
            if (ev == null)
                return;
            Events.Push(ev.Method.Name);
            //Log.WriteLine(CurrentEvent);
            ev();
            Events.Pop();
        }

        static string[] FunctionSwitchIfWhileForKeyword = new[] { "function", "switch", "if", "while", "for" };
        protected virtual void Process()
        {
            //Log.WriteLine(Token(CurrentNode.Value));
            if (Current.IsNodeStartOrEnd)
            {
                if (Current.IsNodeEnd && Current.Node.IsCodeStatement())
                    On(OnStatementSemicolon); //semicolon is inivisble because is in custom code
                if (Current.IsNodeEnd && Current.Node.Is(JsNodeType.CommentStatement))
                    On(OnCommentStatementEnd); //semicolon is inivisble because is in custom code

                if (Current.Node.IsSingleNestedStatement() && !Current.Node.IsElseIf())
                {
                    if (Current.IsNodeStart)
                        On(OnSingleNestedStatementStart);
                    else
                        On(OnSingleNestedStatementEnd);
                }
            }
            else if (Current.Value.IsNotNullOrEmpty())
            {
                if (Current.IsKeyword && FunctionSwitchIfWhileForKeyword.Contains(Current.Value))
                    On(OnFunctionSwitchIfWhileForKeyword);
                if (Current.IsKeyword && Current.Value == "function")
                    On(OnFunctionKeyword);
                if (Current.IsOperator && Current.Value == ")" && Current.Node.IsAny(JsNodeType.Function))
                    On(OnFunctionParametersClose);
                if (Current.Value == ")" && Current.IsControl && Current.Node.IsAny(JsNodeType.DoWhileStatement))
                    On(OnDoWhileStatementParanthesesClose);
                if (Current.IsOperator && Current.Node.IsAny(JsNodeType.BinaryExpression, JsNodeType.VariableDeclarator, JsNodeType.AssignmentExpression, JsNodeType.ConditionalExpression))
                    On(OnBinaryOperator);
                if (Current.Value == "," && Current.IsControl && Current.Node.Is(JsNodeType.JsonObjectExpression))
                    On(OnJsonObjectSeparator);
                if (Current.Value == "," && Current.IsControl && Current.Node.Is(JsNodeType.JsonArrayExpression))
                    On(OnJsonArraySeparator);
                if (Current.Value == "," && Current.IsControl && Current.Node.IsAny(JsNodeType.Function, JsNodeType.InvocationExpression))
                    On(OnFunctionOrInvocationParameterSeparator);
                //Add enter after '},', '};' or just '}' and decrease indent
                if (Current.IsControl && Current.Value == "}")
                    On(OnBracketClose);
                //{name: value} add space after :
                if (Current.Value == ":" && Current.Node != null && Current.IsControl && Current.Node.Is(JsNodeType.JsonNameValue))
                    On(OnJsonNameValueSeparator);
                if (Current.Value == ":" && Current.Node != null && Current.IsControl && Current.Node.Is(JsNodeType.SwitchLabel))
                    On(OnSwitchLabelSeparator);

                //Add indent spaces after new line
                var prev = CurrentNode.PreviousVisibleToken();//.Where(t => !t.Value.IsWhitespace && t.Value.Value != null).FirstOrDefault()
                if (prev != null && prev.Value.IsNewLine)
                {
                    On(OnFirstVisibleTokenAfterNewLine);
                    //&& !prev.Value.IsWhitespace && prevVisible.PreviousVisibleToken().Value.IsNewLine)
                }

                //Add enter and increase indent after '{'
                if (Current.IsControl && Current.Value == "{")
                    On(OnBracketOpen);
                //Add enter after any relevant statement that ends with ';'
                if (Current.Value == ";" && Current.IsControl && Current.Node.IsAny(
                    JsNodeType.ExpressionStatement,
                    JsNodeType.VariableDeclarationStatement,
                    JsNodeType.ReturnStatement,
                    JsNodeType.ThrowStatement,
                    JsNodeType.BreakStatement,
                    JsNodeType.ContinueStatement))
                    On(OnStatementSemicolon);
                if (Current.Value == ";" && Current.IsControl && Current.Node.IsAny(JsNodeType.ForStatement))
                    On(OnForStatementSemicolon);
                if (Current.Value == "," && Current.IsControl && Current.Node.IsAny(JsNodeType.ForStatement))
                    On(OnForStatementComma);
            }
        }


        protected virtual void OnCommentStatementEnd()
        {
        }

        protected virtual void OnDoWhileStatementParanthesesClose()
        {
        }

        protected virtual void OnSwitchLabelSeparator()
        {
        }

        protected virtual void OnForStatementSemicolon()
        {
        }
        protected virtual void OnForStatementComma()
        {
        }

        protected virtual void OnFunctionKeyword()
        {
        }
        protected virtual void OnFunctionSwitchIfWhileForKeyword()
        {
        }

        protected virtual void OnFunctionParametersClose()
        {
        }

        protected virtual void OnStatementSemicolon()
        {
        }

        protected virtual void OnBracketOpen()
        {
        }

        protected virtual void OnFirstVisibleTokenAfterNewLine()
        {
        }

        protected virtual void OnJsonNameValueSeparator()
        {
        }

        protected virtual void OnBracketClose()
        {
        }

        protected virtual void OnFunctionOrInvocationParameterSeparator()
        {
        }

        protected virtual void OnJsonObjectSeparator()
        {
        }

        protected virtual void OnJsonArraySeparator()
        {
        }

        protected virtual void OnBinaryOperator()
        {
        }

        protected virtual void OnSingleNestedStatementEnd()
        {
        }

        protected virtual void OnSingleNestedStatementStart()
        {
        }

        protected void AddAfterCurrent(JsToken token)
        {
            //LogOp("AddAfterCurrent", token);
            var node = Tokens.AddAfter(CurrentNode, token);
            OnNodeAdded(node);
        }

        protected virtual void AddBeforeCurrent(JsToken token)
        {
            //LogOp("AddBeforeCurrent", token);
            var node = Tokens.AddBefore(CurrentNode, token);
            OnNodeAdded(node);
        }

        protected virtual void OnNodeAdded(LinkedListNode<JsToken> node)
        {
            if (node.Value.IsNewLine)
            {
                var prev = node.PreviousVisibleToken();
                while (prev != null && (prev.Value.IsWhitespace || prev.Value.IsNewLine))
                {
                    var tmp = prev.PreviousVisibleToken();
                    RemoveToken(prev);
                    prev = tmp;
                }
            }
        }

        protected virtual void RemoveToken(LinkedListNode<JsToken> node)
        {
            if (node == CurrentNode)
                MoveNext();
            Tokens.Remove(node);
        }

        //private void LogOp(string name, JsToken token)
        //{
        //    //Log.WriteLine("{1}\t{0}\t{2}\tEvent={3}", name, Token(CurrentNode.Value), Token(token), CurrentEvent);
        //}
        //string Token(JsToken token)
        //{
        //    var type = token.Type;
        //    var value = token.Value;
        //    if (value != null && value.Length < 5)
        //    {
        //        value = value.Replace("\n", "").Replace("\r", "");
        //        return String.Format("{0} '{1}'", type, value);
        //    }
        //    return type.ToString();
        //}

        public LinkedListNode<JsToken> CurrentNode { get; private set; }

        public LinkedList<JsToken> Tokens { get; set; }

        public int Indent { get; set; }


        protected virtual void ReplaceCurrent(JsToken token)
        {
            var x = CurrentNode;
            var node = Tokens.AddAfter(x, token);
            CurrentNode = node;
            Current = token;
            Tokens.Remove(x);
        }

    }

}
