using System.Collections.Generic;

namespace SharpKit.JavaScript.Ast
{
    partial class JsNode
    {
        public virtual IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsNodeList
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Nodes != null) foreach (var x in Nodes) yield return x;
        }
    }
    partial class JsUnit
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Statements != null) foreach (var x in Statements) yield return x;
        }
    }
    partial class JsStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsSwitchStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Expression != null) yield return Expression;
            if (Sections != null) foreach (var x in Sections) yield return x;
        }
    }
    partial class JsSwitchSection
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Labels != null) foreach (var x in Labels) yield return x;
            if (Statements != null) foreach (var x in Statements) yield return x;
        }
    }
    partial class JsSwitchLabel
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Expression != null) yield return Expression;
        }
    }
    partial class JsWhileStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Condition != null) yield return Condition;
            if (Statement != null) yield return Statement;
        }
    }
    partial class JsDoWhileStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Condition != null) yield return Condition;
            if (Statement != null) yield return Statement;
        }
    }
    partial class JsIfStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Condition != null) yield return Condition;
            if (IfStatement != null) yield return IfStatement;
            if (ElseStatement != null) yield return ElseStatement;
        }
    }
    partial class JsForStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Initializers != null)
                foreach (var x in Initializers)
                    yield return x;
            if (Condition != null) yield return Condition;
            if (Iterators != null)
                foreach (var x in Iterators)
                    yield return x;
            if (Statement != null) yield return Statement;
        }
    }
    partial class JsForInStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Initializer != null) yield return Initializer;
            if (Member != null) yield return Member;
            if (Statement != null) yield return Statement;
        }
    }
    partial class JsContinueStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsBlock
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Statements != null) foreach (var x in Statements) yield return x;
        }
    }
    partial class JsThrowStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Expression != null) yield return Expression;
        }
    }
    partial class JsTryStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (TryBlock != null) yield return TryBlock;
            if (CatchClause != null) yield return CatchClause;
            if (FinallyBlock != null) yield return FinallyBlock;
        }
    }
    partial class JsBreakStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsExpressionStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Expression != null) yield return Expression;
        }
    }
    partial class JsReturnStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Expression != null) yield return Expression;
        }
    }
    partial class JsVariableDeclarationStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Declaration != null) yield return Declaration;
        }
    }
    partial class JsCommentStatement
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsConditionalExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Condition != null) yield return Condition;
            if (TrueExpression != null) yield return TrueExpression;
            if (FalseExpression != null) yield return FalseExpression;
        }
    }
    partial class JsAssignmentExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Left != null) yield return Left;
            if (Right != null) yield return Right;
        }
    }
    partial class JsParenthesizedExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Expression != null) yield return Expression;
        }
    }
    partial class JsBinaryExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Left != null) yield return Left;
            if (Right != null) yield return Right;
        }
    }
    partial class JsPostUnaryExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Left != null) yield return Left;
        }
    }
    partial class JsPreUnaryExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Right != null) yield return Right;
        }
    }
    partial class JsJsonObjectExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (NamesValues != null) foreach (var x in NamesValues) yield return x;
        }
    }
    partial class JsStringExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsNumberExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsRegexExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsNullExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsVariableDeclarationExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Declarators != null) foreach (var x in Declarators) yield return x;
        }
    }
    partial class JsVariableDeclarator
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Initializer != null) yield return Initializer;
        }
    }
    partial class JsNewObjectExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Invocation != null) yield return Invocation;
        }
    }
    partial class JsFunction
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Block != null) yield return Block;
        }
    }
    partial class JsInvocationExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Member != null) yield return Member;
            if (Arguments != null) foreach (var x in Arguments) yield return x;
        }
    }
    partial class JsIndexerAccessExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Member != null) yield return Member;
            if (Arguments != null) foreach (var x in Arguments) yield return x;
        }
    }
    partial class JsMemberExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (PreviousMember != null) yield return PreviousMember;
        }
    }
    partial class JsThis
    {
        public override IEnumerable<JsNode> Children()
        {
            if (PreviousMember != null) yield return PreviousMember;
        }
    }
    partial class JsJsonArrayExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Items != null) foreach (var x in Items) yield return x;
        }
    }
    partial class JsStatementExpressionList
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Expressions != null) foreach (var x in Expressions) yield return x;
        }
    }
    partial class JsCatchClause
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Block != null) yield return Block;
        }
    }
    partial class JsJsonMember
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsCodeExpression
    {
        public override IEnumerable<JsNode> Children()
        {
            yield break;
        }
    }
    partial class JsJsonNameValue
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Name != null) yield return Name;
            if (Value != null) yield return Value;
        }
    }
    partial class JsExternalFileUnit
    {
        public override IEnumerable<JsNode> Children()
        {
            if (Statements != null) foreach (var x in Statements) yield return x;
        }
    }
}
