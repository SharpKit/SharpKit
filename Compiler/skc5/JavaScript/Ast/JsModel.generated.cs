using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace SharpKit.JavaScript.Ast
{
    public enum JsNodeType
    {
        AssignmentExpression,
        BinaryExpression,
        Block,
        BreakStatement,
        CatchClause,
        CodeExpression,
        CodeStatement,
        CommentStatement,
        ConditionalExpression,
        ContinueStatement,
        DoWhileStatement,
        Expression,
        ExpressionStatement,
        ExternalFileUnit,
        ForInStatement,
        ForStatement,
        Function,
        IfStatement,
        IndexerAccessExpression,
        InvocationExpression,
        JsonArrayExpression,
        JsonMember,
        JsonNameValue,
        JsonObjectExpression,
        MemberExpression,
        NewObjectExpression,
        NodeList,
        NullExpression,
        NumberExpression,
        ParenthesizedExpression,
        PostUnaryExpression,
        PreUnaryExpression,
        RegexExpression,
        ReturnStatement,
        Statement,
        StatementExpressionList,
        StringExpression,
        SwitchLabel,
        SwitchSection,
        SwitchStatement,
        This,
        ThrowStatement,
        TryStatement,
        Unit,
        VariableDeclarationExpression,
        VariableDeclarationStatement,
        VariableDeclarator,
        WhileStatement,
        UseStrictStatement,
    }
    partial class JsAssignmentExpression { [DebuggerStepThrough] public JsAssignmentExpression() { NodeType = JsNodeType.AssignmentExpression; } }
    partial class JsBinaryExpression { [DebuggerStepThrough] public JsBinaryExpression() { NodeType = JsNodeType.BinaryExpression; } }
    partial class JsBlock { [DebuggerStepThrough] public JsBlock() { NodeType = JsNodeType.Block; } }
    partial class JsBreakStatement { [DebuggerStepThrough] public JsBreakStatement() { NodeType = JsNodeType.BreakStatement; } }
    partial class JsCatchClause { [DebuggerStepThrough] public JsCatchClause() { NodeType = JsNodeType.CatchClause; } }
    partial class JsCodeExpression { [DebuggerStepThrough] public JsCodeExpression() { NodeType = JsNodeType.CodeExpression; } }
    partial class JsCommentStatement { [DebuggerStepThrough] public JsCommentStatement() { NodeType = JsNodeType.CommentStatement; } }
    partial class JsConditionalExpression { [DebuggerStepThrough] public JsConditionalExpression() { NodeType = JsNodeType.ConditionalExpression; } }
    partial class JsContinueStatement { [DebuggerStepThrough] public JsContinueStatement() { NodeType = JsNodeType.ContinueStatement; } }
    partial class JsDoWhileStatement { [DebuggerStepThrough] public JsDoWhileStatement() { NodeType = JsNodeType.DoWhileStatement; } }
    partial class JsExpression { [DebuggerStepThrough] public JsExpression() { NodeType = JsNodeType.Expression; } }
    partial class JsExpressionStatement { [DebuggerStepThrough] public JsExpressionStatement() { NodeType = JsNodeType.ExpressionStatement; } }
    partial class JsExternalFileUnit { [DebuggerStepThrough] public JsExternalFileUnit() { NodeType = JsNodeType.ExternalFileUnit; } }
    partial class JsForInStatement { [DebuggerStepThrough] public JsForInStatement() { NodeType = JsNodeType.ForInStatement; } }
    partial class JsForStatement { [DebuggerStepThrough] public JsForStatement() { NodeType = JsNodeType.ForStatement; } }
    partial class JsFunction { [DebuggerStepThrough] public JsFunction() { NodeType = JsNodeType.Function; } }
    partial class JsIfStatement { [DebuggerStepThrough] public JsIfStatement() { NodeType = JsNodeType.IfStatement; } }
    partial class JsIndexerAccessExpression { [DebuggerStepThrough] public JsIndexerAccessExpression() { NodeType = JsNodeType.IndexerAccessExpression; } }
    partial class JsInvocationExpression { [DebuggerStepThrough] public JsInvocationExpression() { NodeType = JsNodeType.InvocationExpression; } }
    partial class JsJsonArrayExpression { [DebuggerStepThrough] public JsJsonArrayExpression() { NodeType = JsNodeType.JsonArrayExpression; } }
    partial class JsJsonMember { [DebuggerStepThrough] public JsJsonMember() { NodeType = JsNodeType.JsonMember; } }
    partial class JsJsonNameValue { [DebuggerStepThrough] public JsJsonNameValue() { NodeType = JsNodeType.JsonNameValue; } }
    partial class JsJsonObjectExpression { [DebuggerStepThrough] public JsJsonObjectExpression() { NodeType = JsNodeType.JsonObjectExpression; } }
    partial class JsMemberExpression { [DebuggerStepThrough] public JsMemberExpression() { NodeType = JsNodeType.MemberExpression; } }
    partial class JsNewObjectExpression { [DebuggerStepThrough] public JsNewObjectExpression() { NodeType = JsNodeType.NewObjectExpression; } }
    partial class JsNodeList { [DebuggerStepThrough] public JsNodeList() { NodeType = JsNodeType.NodeList; } }
    partial class JsNullExpression { [DebuggerStepThrough] public JsNullExpression() { NodeType = JsNodeType.NullExpression; } }
    partial class JsNumberExpression { [DebuggerStepThrough] public JsNumberExpression() { NodeType = JsNodeType.NumberExpression; } }
    partial class JsParenthesizedExpression { [DebuggerStepThrough] public JsParenthesizedExpression() { NodeType = JsNodeType.ParenthesizedExpression; } }
    partial class JsPostUnaryExpression { [DebuggerStepThrough] public JsPostUnaryExpression() { NodeType = JsNodeType.PostUnaryExpression; } }
    partial class JsPreUnaryExpression { [DebuggerStepThrough] public JsPreUnaryExpression() { NodeType = JsNodeType.PreUnaryExpression; } }
    partial class JsRegexExpression { [DebuggerStepThrough] public JsRegexExpression() { NodeType = JsNodeType.RegexExpression; } }
    partial class JsReturnStatement { [DebuggerStepThrough] public JsReturnStatement() { NodeType = JsNodeType.ReturnStatement; } }
    partial class JsStatement { [DebuggerStepThrough] public JsStatement() { NodeType = JsNodeType.Statement; } }
    partial class JsStatementExpressionList { [DebuggerStepThrough] public JsStatementExpressionList() { NodeType = JsNodeType.StatementExpressionList; } }
    partial class JsStringExpression { [DebuggerStepThrough] public JsStringExpression() { NodeType = JsNodeType.StringExpression; } }
    partial class JsSwitchLabel { [DebuggerStepThrough] public JsSwitchLabel() { NodeType = JsNodeType.SwitchLabel; } }
    partial class JsSwitchSection { [DebuggerStepThrough] public JsSwitchSection() { NodeType = JsNodeType.SwitchSection; } }
    partial class JsSwitchStatement { [DebuggerStepThrough] public JsSwitchStatement() { NodeType = JsNodeType.SwitchStatement; } }
    partial class JsThis { [DebuggerStepThrough] public JsThis() { NodeType = JsNodeType.This; } }
    partial class JsThrowStatement { [DebuggerStepThrough] public JsThrowStatement() { NodeType = JsNodeType.ThrowStatement; } }
    partial class JsTryStatement { [DebuggerStepThrough] public JsTryStatement() { NodeType = JsNodeType.TryStatement; } }
    //partial class JsUnit { [DebuggerStepThrough] public JsUnit() { NodeType = JsNodeType.Unit; } }
    partial class JsVariableDeclarationExpression { [DebuggerStepThrough] public JsVariableDeclarationExpression() { NodeType = JsNodeType.VariableDeclarationExpression; } }
    partial class JsVariableDeclarationStatement { [DebuggerStepThrough] public JsVariableDeclarationStatement() { NodeType = JsNodeType.VariableDeclarationStatement; } }
    partial class JsVariableDeclarator { [DebuggerStepThrough] public JsVariableDeclarator() { NodeType = JsNodeType.VariableDeclarator; } }
    partial class JsWhileStatement { [DebuggerStepThrough] public JsWhileStatement() { NodeType = JsNodeType.WhileStatement; } }
    partial class JsUseStrictStatement { [DebuggerStepThrough] public JsUseStrictStatement() { NodeType = JsNodeType.UseStrictStatement; } }
}