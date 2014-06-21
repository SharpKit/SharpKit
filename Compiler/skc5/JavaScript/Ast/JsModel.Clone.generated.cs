using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpKit.JavaScript.Ast
{
    partial class JsStatement
    {
        public override JsNode New()
        {
            return new JsStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsStatement)node;
            if (Comments != null)
            {
                if (node2.Comments == null)
                    node2.Comments = new List<String>();
                node2.Comments.AddRange(Comments);
            }
        }
    }
    partial class JsUnit
    {
        public override JsNode New()
        {
            return new JsUnit();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsUnit)node;
            if (Statements != null)
            {
                if (node2.Statements == null)
                    node2.Statements = new List<JsStatement>();
                node2.Statements.AddRange(Statements.Select(t => t.Clone()));
            }
        }
    }
    partial class JsExternalFileUnit
    {
        public override JsNode New()
        {
            return new JsExternalFileUnit();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsExternalFileUnit)node;
            node2.Filename = Filename;
        }
    }
    partial class JsExpression
    {
        public override JsNode New()
        {
            return new JsExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsExpression)node;
        }
    }
    partial class JsNodeList
    {
        public override JsNode New()
        {
            return new JsNodeList();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsNodeList)node;
            if (Nodes != null)
            {
                if (node2.Nodes == null)
                    node2.Nodes = new List<JsNode>();
                node2.Nodes.AddRange(Nodes.Select(t => t.Clone()));
            }
        }
    }
    partial class JsSwitchStatement
    {
        public override JsNode New()
        {
            return new JsSwitchStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsSwitchStatement)node;
            node2.Expression = Expression.Clone();
            if (Sections != null)
            {
                if (node2.Sections == null)
                    node2.Sections = new List<JsSwitchSection>();
                node2.Sections.AddRange(Sections.Select(t => t.Clone()));
            }
        }
    }
    partial class JsSwitchSection
    {
        public override JsNode New()
        {
            return new JsSwitchSection();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsSwitchSection)node;
            if (Labels != null)
            {
                if (node2.Labels == null)
                    node2.Labels = new List<JsSwitchLabel>();
                node2.Labels.AddRange(Labels.Select(t => t.Clone()));
            }
            if (Statements != null)
            {
                if (node2.Statements == null)
                    node2.Statements = new List<JsStatement>();
                node2.Statements.AddRange(Statements.Select(t => t.Clone()));
            }
        }
    }
    partial class JsSwitchLabel
    {
        public override JsNode New()
        {
            return new JsSwitchLabel();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsSwitchLabel)node;
            node2.IsDefault = IsDefault;
            node2.Expression = Expression.Clone();
        }
    }
    partial class JsWhileStatement
    {
        public override JsNode New()
        {
            return new JsWhileStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsWhileStatement)node;
            node2.Condition = Condition.Clone();
            node2.Statement = Statement.Clone();
        }
    }
    partial class JsDoWhileStatement
    {
        public override JsNode New()
        {
            return new JsDoWhileStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsDoWhileStatement)node;
            node2.Condition = Condition.Clone();
            node2.Statement = Statement.Clone();
        }
    }
    partial class JsIfStatement
    {
        public override JsNode New()
        {
            return new JsIfStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsIfStatement)node;
            node2.Condition = Condition.Clone();
            node2.IfStatement = IfStatement.Clone();
            node2.ElseStatement = ElseStatement.Clone();
        }
    }
    partial class JsUseStrictStatement
    {
        public override JsNode New()
        {
            return new JsUseStrictStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsUseStrictStatement)node;
        }
    }
    partial class JsForStatement
    {
        public override JsNode New()
        {
            return new JsForStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsForStatement)node;
            if (Initializers != null)
            {
                if (node2.Initializers == null)
                    node2.Initializers = new List<JsStatement>();
                node2.Initializers.AddRange(Initializers.Select(t => t.Clone()));
            }
            node2.Condition = Condition.Clone();
            if (Iterators != null)
            {
                if (node2.Iterators == null)
                    node2.Iterators = new List<JsStatement>();
                node2.Iterators.AddRange(Iterators.Select(t => t.Clone()));
            }
            node2.Statement = Statement.Clone();
        }
    }
    partial class JsForInStatement
    {
        public override JsNode New()
        {
            return new JsForInStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsForInStatement)node;
            node2.Initializer = Initializer.Clone();
            node2.Member = Member.Clone();
            node2.Statement = Statement.Clone();
        }
    }
    partial class JsContinueStatement
    {
        public override JsNode New()
        {
            return new JsContinueStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsContinueStatement)node;
        }
    }
    partial class JsBlock
    {
        public override JsNode New()
        {
            return new JsBlock();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsBlock)node;
            if (Statements != null)
            {
                if (node2.Statements == null)
                    node2.Statements = new List<JsStatement>();
                node2.Statements.AddRange(Statements.Select(t => t.Clone()));
            }
        }
    }
    partial class JsThrowStatement
    {
        public override JsNode New()
        {
            return new JsThrowStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsThrowStatement)node;
            node2.Expression = Expression.Clone();
        }
    }
    partial class JsTryStatement
    {
        public override JsNode New()
        {
            return new JsTryStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsTryStatement)node;
            node2.TryBlock = TryBlock.Clone();
            node2.CatchClause = CatchClause.Clone();
            node2.FinallyBlock = FinallyBlock.Clone();
        }
    }
    partial class JsBreakStatement
    {
        public override JsNode New()
        {
            return new JsBreakStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsBreakStatement)node;
        }
    }
    partial class JsExpressionStatement
    {
        public override JsNode New()
        {
            return new JsExpressionStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsExpressionStatement)node;
            node2.Expression = Expression.Clone();
        }
    }
    partial class JsReturnStatement
    {
        public override JsNode New()
        {
            return new JsReturnStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsReturnStatement)node;
            node2.Expression = Expression.Clone();
        }
    }
    partial class JsVariableDeclarationStatement
    {
        public override JsNode New()
        {
            return new JsVariableDeclarationStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsVariableDeclarationStatement)node;
            node2.Declaration = Declaration.Clone();
        }
    }
    partial class JsCommentStatement
    {
        public override JsNode New()
        {
            return new JsCommentStatement();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsCommentStatement)node;
            node2.Text = Text;
        }
    }
    partial class JsConditionalExpression
    {
        public override JsNode New()
        {
            return new JsConditionalExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsConditionalExpression)node;
            node2.Condition = Condition.Clone();
            node2.TrueExpression = TrueExpression.Clone();
            node2.FalseExpression = FalseExpression.Clone();
        }
    }
    partial class JsAssignmentExpression
    {
        public override JsNode New()
        {
            return new JsAssignmentExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsAssignmentExpression)node;
            node2.Operator = Operator;
            node2.Left = Left.Clone();
            node2.Right = Right.Clone();
        }
    }
    partial class JsParenthesizedExpression
    {
        public override JsNode New()
        {
            return new JsParenthesizedExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsParenthesizedExpression)node;
            node2.Expression = Expression.Clone();
        }
    }
    partial class JsBinaryExpression
    {
        public override JsNode New()
        {
            return new JsBinaryExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsBinaryExpression)node;
            node2.Operator = Operator;
            node2.Left = Left.Clone();
            node2.Right = Right.Clone();
        }
    }
    partial class JsPostUnaryExpression
    {
        public override JsNode New()
        {
            return new JsPostUnaryExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsPostUnaryExpression)node;
            node2.Operator = Operator;
            node2.Left = Left.Clone();
        }
    }
    partial class JsPreUnaryExpression
    {
        public override JsNode New()
        {
            return new JsPreUnaryExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsPreUnaryExpression)node;
            node2.Operator = Operator;
            node2.Right = Right.Clone();
        }
    }
    partial class JsJsonObjectExpression
    {
        public override JsNode New()
        {
            return new JsJsonObjectExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsJsonObjectExpression)node;
            if (NamesValues != null)
            {
                if (node2.NamesValues == null)
                    node2.NamesValues = new List<JsJsonNameValue>();
                node2.NamesValues.AddRange(NamesValues.Select(t => t.Clone()));
            }
        }
    }
    partial class JsStringExpression
    {
        public override JsNode New()
        {
            return new JsStringExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsStringExpression)node;
            node2.Value = Value;
        }
    }
    partial class JsNumberExpression
    {
        public override JsNode New()
        {
            return new JsNumberExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsNumberExpression)node;
            node2.Value = Value;
        }
    }
    partial class JsRegexExpression
    {
        public override JsNode New()
        {
            return new JsRegexExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsRegexExpression)node;
            node2.Code = Code;
        }
    }
    partial class JsNullExpression
    {
        public override JsNode New()
        {
            return new JsNullExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsNullExpression)node;
        }
    }
    partial class JsVariableDeclarationExpression
    {
        public override JsNode New()
        {
            return new JsVariableDeclarationExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsVariableDeclarationExpression)node;
            if (Declarators != null)
            {
                if (node2.Declarators == null)
                    node2.Declarators = new List<JsVariableDeclarator>();
                node2.Declarators.AddRange(Declarators.Select(t => t.Clone()));
            }
        }
    }
    partial class JsVariableDeclarator
    {
        public override JsNode New()
        {
            return new JsVariableDeclarator();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsVariableDeclarator)node;
            node2.Name = Name;
            node2.Initializer = Initializer.Clone();
        }
    }
    partial class JsNewObjectExpression
    {
        public override JsNode New()
        {
            return new JsNewObjectExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsNewObjectExpression)node;
            node2.Invocation = Invocation.Clone();
        }
    }
    partial class JsFunction
    {
        public override JsNode New()
        {
            return new JsFunction();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsFunction)node;
            node2.Name = Name;
            if (Parameters != null)
            {
                if (node2.Parameters == null)
                    node2.Parameters = new List<String>();
                node2.Parameters.AddRange(Parameters);
            }
            node2.Block = Block.Clone();
        }
    }
    partial class JsInvocationExpression
    {
        public override JsNode New()
        {
            return new JsInvocationExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsInvocationExpression)node;
            node2.Member = Member.Clone();
            if (Arguments != null)
            {
                if (node2.Arguments == null)
                    node2.Arguments = new List<JsExpression>();
                node2.Arguments.AddRange(Arguments.Select(t => t.Clone()));
            }
            node2.ArgumentsPrefix = ArgumentsPrefix;
            node2.ArgumentsSuffix = ArgumentsSuffix;
            node2.OmitParanthesis = OmitParanthesis;
            node2.OmitCommas = OmitCommas;
        }
    }
    partial class JsIndexerAccessExpression
    {
        public override JsNode New()
        {
            return new JsIndexerAccessExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsIndexerAccessExpression)node;
            node2.Member = Member.Clone();
            if (Arguments != null)
            {
                if (node2.Arguments == null)
                    node2.Arguments = new List<JsExpression>();
                node2.Arguments.AddRange(Arguments.Select(t => t.Clone()));
            }
        }
    }
    partial class JsMemberExpression
    {
        public override JsNode New()
        {
            return new JsMemberExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsMemberExpression)node;
            node2.Name = Name;
            node2.PreviousMember = PreviousMember.Clone();
        }
    }
    partial class JsThis
    {
        public override JsNode New()
        {
            return new JsThis();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsThis)node;
        }
    }
    partial class JsJsonArrayExpression
    {
        public override JsNode New()
        {
            return new JsJsonArrayExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsJsonArrayExpression)node;
            if (Items != null)
            {
                if (node2.Items == null)
                    node2.Items = new List<JsExpression>();
                node2.Items.AddRange(Items.Select(t => t.Clone()));
            }
        }
    }
    partial class JsStatementExpressionList
    {
        public override JsNode New()
        {
            return new JsStatementExpressionList();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsStatementExpressionList)node;
            if (Expressions != null)
            {
                if (node2.Expressions == null)
                    node2.Expressions = new List<JsExpression>();
                node2.Expressions.AddRange(Expressions.Select(t => t.Clone()));
            }
        }
    }
    partial class JsCatchClause
    {
        public override JsNode New()
        {
            return new JsCatchClause();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsCatchClause)node;
            node2.IdentifierName = IdentifierName;
            node2.Block = Block.Clone();
        }
    }
    partial class JsJsonMember
    {
        public override JsNode New()
        {
            return new JsJsonMember();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsJsonMember)node;
            node2.IsStringLiteral = IsStringLiteral;
            node2.Name = Name;
        }
    }
    partial class JsCodeExpression
    {
        public override JsNode New()
        {
            return new JsCodeExpression();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsCodeExpression)node;
            node2.Code = Code;
        }
    }
    partial class JsJsonNameValue
    {
        public override JsNode New()
        {
            return new JsJsonNameValue();
        }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var node2 = (JsJsonNameValue)node;
            node2.Name = Name.Clone();
            node2.Value = Value.Clone();
        }
    }
}
