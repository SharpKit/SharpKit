using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Corex.Text;

namespace SharpKit.JavaScript.Ast
{
    class JsWriter : IDisposable
    {
        public List<JsToken> GetTokens(JsNode node)
        {
            if (Tokens.Count > 0)
                throw new Exception("Unused tokens left in writer");
            Visit(node);
            var list = Tokens;
            Tokens = new List<JsToken>();
            return list;
        }

        public void VisitFormatAndWrite(JsNode node)
        {
            var tokens = GetTokens(node);
            var formatted = FormatTokens(tokens);
            WriteTokens(formatted);
        }

        public void WriteTokens(List<JsToken> tokens)
        {
            if (TokenWriter == null)
                TokenWriter = new JsTokenWriter { InnerWriter = InnerWriter };
            TokenWriter.Write(tokens);
        }

        private List<JsToken> GetFormattedTokens(JsNode node)
        {
            var tokens = GetTokens(node);
            var formatted = FormatTokens(tokens);
            return formatted;
        }

        public List<JsToken> FormatTokens(List<JsToken> tokens)
        {
            var formatted = CreateFormatter().Format(tokens);
            var token = tokens.FirstOrDefault();
            return formatted;
        }

        public JsTokenWriter TokenWriter { get; set; }
        public string Format { get; set; }
        JsFormatter CreateFormatter()
        {
            JsFormatter x;
            if (Format.IsNullOrEmpty() || Format.EqualsIgnoreCase("JavaScript"))
                x = new JsFormatter_Default();
            else if (Format.EqualsIgnoreCase("C#") || Format.EqualsIgnoreCase("CSharp"))
                x = new JsFormatter_CSharp();
            else if (Format.EqualsIgnoreCase("Minified"))
                x = new JsFormatter_Minified();
            else
                throw new Exception("No format named :" + Format + ", options are JavaScript, C# or Minified");
            return x;
        }

        public Action<string> Warn { get; set; }

        public static JsWriter CreateInMemory()
        {
            var innerWriter = new StringWriter();
            var writer = new JsWriter { InnerWriter = innerWriter };
            return writer;
        }
        public static JsWriter Create(string filename, bool append)
        {
            var innerWriter = new StreamWriter(filename, append);
            var writer = new JsWriter { InnerWriter = innerWriter };
            return writer;
        }

        Stack<JsNode> Parents = new Stack<JsNode>();
        private JsNode GetNodeParent(JsNode node)
        {
            return node.Parent;
        }

        #region _Visit

        void _Visit(JsAssignmentExpression node)
        {
            Visit(node.Left);
            Operator(node.Operator ?? "=");
            Visit(node.Right);
        }
        void _Visit(JsBinaryExpression node)
        {
            Visit(node.Left);
            if (char.IsLetter(node.Operator[0]))
            {
                Space();
                Keyword(node.Operator);
                Space();
            }
            else
                Operator(node.Operator);
            Visit(node.Right);
        }
        void _Visit(JsBlock node)
        {
            BeginBlock();
            VisitEach(node.Statements);
            if (node.Comments.IsNotNullOrEmpty())
                WriteComments(node.Comments);
            var parent = GetNodeParent(node);
            EndBlock();
        }

        void _Visit(JsBreakStatement node)
        {
            Keyword("break");
            Semicolon();
        }
        void _Visit(JsCatchClause node)
        {
            Keyword("catch");
            Control("(");
            Literal(node.IdentifierName);
            Control(")");
            Visit(node.Block);
        }
        void _Visit(JsCodeExpression node)
        {
            Write(node.Code, JsTokenType.Raw);
        }
        void _Visit(JsCodeStatement node)
        {
            Write(node.Code, JsTokenType.Raw);
        }
        void _Visit(JsCommentStatement node)
        {
            Write(string.Format("/*{0}*/", node.Text), JsTokenType.Comment);
        }
        void _Visit(JsConditionalExpression node)
        {
            Visit(node.Condition);
            Operator("?");
            Visit(node.TrueExpression);
            Operator(":");
            Visit(node.FalseExpression);
        }
        void _Visit(JsContinueStatement node)
        {
            Keyword("continue");
            Semicolon();
        }
        void _Visit(JsDoWhileStatement node)
        {
            Keyword("do");
            Visit(node.Statement);
            Keyword("while");
            Control("(");
            Visit(node.Condition);
            Control(")");
        }
        void _Visit(JsExpression node)
        {
        }
        void _Visit(JsExpressionStatement node)
        {
            Visit(node.Expression);
            Semicolon();
        }
        void _Visit(JsExternalFileUnit node)
        {
            _Visit((JsUnit)node);
            Write(File.ReadAllText(node.Filename), JsTokenType.Raw);
            NewLine();
        }
        void _Visit(JsForInStatement node)
        {
            Keyword("for");
            Control("(");
            Visit(node.Initializer);
            Space();
            Keyword("in");
            Space();
            Visit(node.Member);
            Control(")");
            Visit(node.Statement);
        }

        void _Visit(JsForStatement node)
        {
            Keyword("for");
            Control("(");
            if (node.Initializers != null)
            {
                VisitEachJoin(ExtractExpressions(node.Initializers), Comma);
            }
            Control(";");
            if (node.Condition != null)
                Visit(node.Condition);
            Control(";");
            if (node.Iterators != null)
            {
                var exps = ExtractExpressions(node.Iterators);
                VisitEachJoin(exps, Comma);
            }
            Control(")");
            Visit(node.Statement);
        }

        private static List<JsExpression> ExtractExpressions(List<JsStatement> statements)
        {
            var list = new List<JsExpression>();
            foreach (var st in statements)
            {
                if (st is JsExpressionStatement)
                    list.Add(((JsExpressionStatement)st).Expression);
                else if (st is JsVariableDeclarationStatement)
                    list.Add(((JsVariableDeclarationStatement)st).Declaration);
                else
                    throw new Exception("Error extracting expressions from statements");
            }
            return list;
        }
        void _Visit(JsFunction node)
        {
            Keyword("function");
            if (node.Name.IsNotNullOrEmpty())
            {
                Write(" ", JsTokenType.Whitespace);
                Literal(node.Name);
            }
            Control("(");

            bool first = true;
            foreach (var param in node.Parameters.NotNull())
            {
                if (first) first = false;
                else Comma();

                Literal(param);
            }

            Control(")");
            if (node.Block == null)
            {
                BeginBlock();
                EndBlock();
            }
            else
            {
                Visit(node.Block);
            }
        }
        void _Visit(JsIfStatement node)
        {
            Keyword("if");
            Control("(");
            Visit(node.Condition);
            Control(")");
            Visit(node.IfStatement);
            if (node.ElseStatement != null)
            {
                Keyword("else");
                Space();
                if (node.ElseStatement is JsIfStatement)
                {
                    Visit(node.ElseStatement);
                }
                else
                {
                    Visit(node.ElseStatement);
                }
            }
        }
        void _Visit(JsIndexerAccessExpression node)
        {
            Visit(node.Member);
            Control("[");
            VisitEachJoin(node.Arguments, Comma);
            Control("]");
        }
        void _Visit(JsInvocationExpression node)
        {
            Visit(node.Member);
            if (!node.OmitParanthesis)
                Control("(");
            else if (node.Arguments.IsNotNullOrEmpty() && node.ArgumentsPrefix == null)
                Space();
            Write(node.ArgumentsPrefix, JsTokenType.Unknown);
            if (node.Arguments != null)
            {
                if (node.OmitCommas)
                    VisitEach(node.Arguments);
                else
                    VisitEachJoin(node.Arguments, Comma);
            }
            Write(node.ArgumentsSuffix, JsTokenType.Unknown);
            if (!node.OmitParanthesis)
                Control(")");
        }
        void _Visit(JsJsonArrayExpression node)
        {
            Control("[");
            VisitEachJoin(node.Items, Comma);
            Control("]");
        }
        void _Visit(JsJsonMember node)
        {
            if (node.IsStringLiteral)
                Value(string.Format("\"{0}\"", node.Name));
            else
                Literal(node.Name);
        }
        void _Visit(JsJsonNameValue node)
        {
            Visit(node.Name);
            Control(":");
            Visit(node.Value);
        }
        void _Visit(JsJsonObjectExpression node)
        {
            if (node.NamesValues.IsNullOrEmpty())
            {
                Control("{}");
            }
            else
            {
                BeginBlock();
                VisitEachJoin(node.NamesValues, (first) =>
                {
                }, (prev, next) =>
                {
                    Comma();
                }, (last) =>
                {
                });
                EndBlock();
            }
        }
        void _Visit(JsMemberExpression node)
        {
            if (node.PreviousMember != null)
            {
                Visit(node.PreviousMember);
                Control(".");
            }
            Literal(node.Name);
        }
        void _Visit(JsNewObjectExpression node)
        {
            Keyword("new");
            Space();
            Visit(node.Invocation);
        }
        void _Visit(JsNodeList node)
        {
            VisitEachJoin(node.Nodes, Comma);
        }
        void _Visit(JsNullExpression node)
        {
            Keyword("null");
        }
        void _Visit(JsNumberExpression node)
        {
            Keyword(node.Value.ToString());
        }
        void _Visit(JsParenthesizedExpression node)
        {
            Control("(");
            Visit(node.Expression);
            Control(")");
        }
        void _Visit(JsPostUnaryExpression node)
        {
            Visit(node.Left);
            Control(node.Operator);
        }
        void _Visit(JsPreUnaryExpression node)
        {
            Control(node.Operator);
            Visit(node.Right);
        }
        void _Visit(JsRegexExpression node)
        {
            Control(node.Code);
        }
        void _Visit(JsReturnStatement node)
        {
            Keyword("return");
            if (node.Expression != null)
            {
                Space();
                Visit(node.Expression);
            }
            Semicolon();
        }

        void _Visit(JsStatement node)
        {
            var parent = GetNodeParent(node);
            if (parent != null && parent.Is(JsNodeType.Block))
                return;
            Semicolon();
        }
        void _Visit(JsStatementExpressionList node)
        {
            VisitEachJoin(node.Expressions, Comma);
        }
        void _Visit(JsStringExpression node)
        {
            Write("\"" + node.Value.ToString() + "\"", JsTokenType.Value);
        }
        void _Visit(JsSwitchLabel node)
        {
            if (node.IsDefault)
            {
                Keyword("default");
            }
            else
            {
                Keyword("case");
                Space();
                Visit(node.Expression);
            }
            Control(":");
        }
        void _Visit(JsSwitchSection node)
        {
            VisitEach(node.Labels);
            VisitEach(node.Statements);
        }
        void _Visit(JsSwitchStatement node)
        {
            Keyword("switch");
            Control("(");
            Visit(node.Expression);
            Control(")");
            BeginBlock();
            VisitEach(node.Sections);
            EndBlock();
        }
        void _Visit(JsThis node)
        {
            Literal("this");
        }
        void _Visit(JsThrowStatement node)
        {
            Keyword("throw");
            Space();
            Visit(node.Expression);
            Semicolon();
        }
        void _Visit(JsTryStatement node)
        {
            Keyword("try");

            Visit(node.TryBlock);
            if (node.CatchClause != null)
                Visit(node.CatchClause);
            if (node.FinallyBlock != null)
            {
                Keyword("finally");

                Visit(node.FinallyBlock);
            }
        }
        void _Visit(JsUnit node)
        {
            if (node.Statements == null)
                return;
            VisitEach(node.Statements);
            NewLine();
        }
        void _Visit(JsUseStrictStatement node)
        {
            Write("\"use strict\";", JsTokenType.Raw);
            NewLine();
        }
        void _Visit(JsVariableDeclarationExpression node)
        {
            Keyword("var");
            Space();
            VisitEachJoin(node.Declarators, Comma);
        }
        void _Visit(JsVariableDeclarationStatement node)
        {
            Visit(node.Declaration);
            Semicolon();
        }
        void _Visit(JsVariableDeclarator node)
        {
            Literal(node.Name);
            if (node.Initializer != null)
            {
                Operator("=");
                Visit(node.Initializer);
            }
        }
        void _Visit(JsWhileStatement node)
        {
            Keyword("while");
            Control("(");
            Visit(node.Condition);
            Control(")");
            Visit(node.Statement);
        }
        #endregion

        #region Visit
        public Action<JsNode> Visiting { get; set; }
        [DebuggerStepThrough]
        public void Visit(JsNode node)
        {
            if (Visiting != null)
                Visiting(node);
            var parent = Parents.Count > 0 ? Parents.Peek() : null;
            Parents.Push(node);
            JsNode prevParent = null;
            if (node.Parent != null && node.Parent != parent)
            {
                prevParent = node.Parent;
                Console.WriteLine("Warning: multiple parents for node {0}", node); //TODO:
            }
            node.Parent = parent;
            Tokens.Add(new JsToken { Node = node, Type = JsTokenType.NodeStart });
            var st = node as JsStatement;
            if (st != null && st.Comments.IsNotNullOrEmpty() && !(st is JsBlock))
                WriteComments(st.Comments);

            try
            {
                switch (node.NodeType)
                {
                    case JsNodeType.CodeStatement: _Visit((JsCodeStatement)node); break;
                    case JsNodeType.AssignmentExpression: _Visit((JsAssignmentExpression)node); break;
                    case JsNodeType.BinaryExpression: _Visit((JsBinaryExpression)node); break;
                    case JsNodeType.Block: _Visit((JsBlock)node); break;
                    case JsNodeType.BreakStatement: _Visit((JsBreakStatement)node); break;
                    case JsNodeType.CatchClause: _Visit((JsCatchClause)node); break;
                    case JsNodeType.CodeExpression: _Visit((JsCodeExpression)node); break;
                    case JsNodeType.CommentStatement: _Visit((JsCommentStatement)node); break;
                    case JsNodeType.ConditionalExpression: _Visit((JsConditionalExpression)node); break;
                    case JsNodeType.ContinueStatement: _Visit((JsContinueStatement)node); break;
                    case JsNodeType.DoWhileStatement: _Visit((JsDoWhileStatement)node); break;
                    case JsNodeType.Expression: _Visit((JsExpression)node); break;
                    case JsNodeType.ExpressionStatement: _Visit((JsExpressionStatement)node); break;
                    case JsNodeType.ExternalFileUnit: _Visit((JsExternalFileUnit)node); break;
                    case JsNodeType.ForInStatement: _Visit((JsForInStatement)node); break;
                    case JsNodeType.ForStatement: _Visit((JsForStatement)node); break;
                    case JsNodeType.Function: _Visit((JsFunction)node); break;
                    case JsNodeType.IfStatement: _Visit((JsIfStatement)node); break;
                    case JsNodeType.IndexerAccessExpression: _Visit((JsIndexerAccessExpression)node); break;
                    case JsNodeType.InvocationExpression: _Visit((JsInvocationExpression)node); break;
                    case JsNodeType.JsonArrayExpression: _Visit((JsJsonArrayExpression)node); break;
                    case JsNodeType.JsonMember: _Visit((JsJsonMember)node); break;
                    case JsNodeType.JsonNameValue: _Visit((JsJsonNameValue)node); break;
                    case JsNodeType.JsonObjectExpression: _Visit((JsJsonObjectExpression)node); break;
                    case JsNodeType.MemberExpression: _Visit((JsMemberExpression)node); break;
                    case JsNodeType.NewObjectExpression: _Visit((JsNewObjectExpression)node); break;
                    case JsNodeType.NodeList: _Visit((JsNodeList)node); break;
                    case JsNodeType.NullExpression: _Visit((JsNullExpression)node); break;
                    case JsNodeType.NumberExpression: _Visit((JsNumberExpression)node); break;
                    case JsNodeType.ParenthesizedExpression: _Visit((JsParenthesizedExpression)node); break;
                    case JsNodeType.PostUnaryExpression: _Visit((JsPostUnaryExpression)node); break;
                    case JsNodeType.PreUnaryExpression: _Visit((JsPreUnaryExpression)node); break;
                    case JsNodeType.RegexExpression: _Visit((JsRegexExpression)node); break;
                    case JsNodeType.ReturnStatement: _Visit((JsReturnStatement)node); break;
                    case JsNodeType.Statement: _Visit((JsStatement)node); break;
                    case JsNodeType.StatementExpressionList: _Visit((JsStatementExpressionList)node); break;
                    case JsNodeType.StringExpression: _Visit((JsStringExpression)node); break;
                    case JsNodeType.SwitchLabel: _Visit((JsSwitchLabel)node); break;
                    case JsNodeType.SwitchSection: _Visit((JsSwitchSection)node); break;
                    case JsNodeType.SwitchStatement: _Visit((JsSwitchStatement)node); break;
                    case JsNodeType.This: _Visit((JsThis)node); break;
                    case JsNodeType.ThrowStatement: _Visit((JsThrowStatement)node); break;
                    case JsNodeType.TryStatement: _Visit((JsTryStatement)node); break;
                    case JsNodeType.Unit: _Visit((JsUnit)node); break;
                    case JsNodeType.VariableDeclarationExpression: _Visit((JsVariableDeclarationExpression)node); break;
                    case JsNodeType.VariableDeclarationStatement: _Visit((JsVariableDeclarationStatement)node); break;
                    case JsNodeType.VariableDeclarator: _Visit((JsVariableDeclarator)node); break;
                    case JsNodeType.WhileStatement: _Visit((JsWhileStatement)node); break;
                    case JsNodeType.UseStrictStatement: _Visit((JsUseStrictStatement)node); break;
                }

            }
            finally
            {
                Tokens.Add(new JsToken { Node = node, Type = JsTokenType.NodeEnd });
                if (prevParent != null)
                    node.Parent = prevParent;
                Parents.Pop();
            }
        }
        [DebuggerStepThrough]
        public void VisitEach<T>(List<T> list) where T : JsNode
        {
            if (list.IsNullOrEmpty())
                return;
            list.ForEach(Visit);
        }

        [DebuggerStepThrough]
        public void VisitEachJoin<T>(List<T> list, Action actionBetweenItems) where T : JsNode
        {
            if (list.IsNullOrEmpty())
                return;
            list.ForEachJoin(Visit, actionBetweenItems);
        }

        [DebuggerStepThrough]
        public void VisitEachJoin<T>(List<T> list, Action<T> first, Action<T, T> actionBetweenItems, Action<T> last) where T : JsNode
        {
            if (list.IsNullOrEmpty())
                return;
            list.ForEachJoin(Visit, first, actionBetweenItems, last);
        }

        #endregion


        #region Utils
        void WriteComments(List<string> list)
        {
            foreach (var cmt in list)
            {
                if (cmt == null)
                    continue;
                if (cmt.Contains('\n'))
                {
                    var reader = new StringReader(cmt);
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;
                        line = line.TrimStart();
                        Write(line, JsTokenType.Comment);
                        NewLine();
                    }
                }
                else
                {
                    Write(cmt, JsTokenType.Comment);
                    NewLine();
                }
            }
        }
        void Comma() { Control(","); }
        void Dot() { Control("."); }
        void Semicolon() { Control(";"); }
        void Space() { Write(" ", JsTokenType.Whitespace); }
        void BeginBlock() { Control("{"); }
        void EndBlock() { Control("}"); }
        void NewLine() { Write(JsToken.NewLine()); }

        void Keyword(string s) { Write(s, JsTokenType.Keyword); }
        void Control(string s) { Write(s, JsTokenType.Control); }
        void Operator(string s) { Write(s, JsTokenType.Operator); }
        void Literal(string s) { Write(s, JsTokenType.Literal); }
        void Value(string s) { Write(s, JsTokenType.Value); }

        public void Dispose()
        {
            if (InnerWriter != null)
                InnerWriter.Dispose();
        }

        #endregion

        #region Writer

        public StringBuilder GetStringBuilder()
        {
            var sw = InnerWriter as StringWriter;
            if (sw != null)
            {
                InnerWriter.Flush();
                return sw.GetStringBuilder();
            }
            return null;
        }

        public TextWriter InnerWriter { get; set; }

        public void Close()
        {
            InnerWriter.Close();
        }

        public void Write(JsToken token)
        {
            if (token.Node == null)
                token.Node = CurrentNode;
            Tokens.Add(token);
        }

        public void Write(string token, JsTokenType type)
        {
            Write(new JsToken(token, type));
        }

        List<JsToken> Tokens = new List<JsToken>();

        #endregion


        int Indent { get; set; }

        public void Flush()
        {
            InnerWriter.Flush();
        }

        public JsNode CurrentNode
        {
            get
            {
                return Parents.PeekOrDefault();
            }
        }

    }


}
