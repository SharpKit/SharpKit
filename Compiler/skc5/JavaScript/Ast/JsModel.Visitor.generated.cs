using System.Collections.Generic;
namespace SharpKit.JavaScript.Ast
{
    partial class JsNode
    {
        public virtual void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public virtual R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    //partial class JsYieldStatement
    //{
    //    public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
    //    public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    //}
    //partial class JsYieldReturnStatement
    //{
    //    public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
    //    public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    //}
    //partial class JsYieldBreakStatement
    //{
    //    public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
    //    public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    //}
    partial class JsUnit
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsExternalFileUnit
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsNodeList
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsCodeStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsSwitchStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsSwitchSection
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsSwitchLabel
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsWhileStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsDoWhileStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsIfStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsUseStrictStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsForStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsForInStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsContinueStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsBlock
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsThrowStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsTryStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsBreakStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsExpressionStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsReturnStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsVariableDeclarationStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsCommentStatement
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsConditionalExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsAssignmentExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsParenthesizedExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsBinaryExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsPostUnaryExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsPreUnaryExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsJsonObjectExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsStringExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsNumberExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsRegexExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsNullExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsVariableDeclarationExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsVariableDeclarator
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsNewObjectExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsFunction
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsInvocationExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsIndexerAccessExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsMemberExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsThis
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsJsonArrayExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsStatementExpressionList
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsCatchClause
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsJsonMember
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsCodeExpression
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    partial class JsJsonNameValue
    {
        public override void Visit(IJsNodeVisitor visitor) { visitor.Visit(this); }
        public override R Visit<R>(IJsNodeVisitor<R> visitor) { return visitor.Visit(this); }
    }
    public partial interface IJsNodeVisitor
    {
        void Visit(JsNode node);
        void Visit(JsStatement node);
        //void Visit(JsYieldStatement node);
        //void Visit(JsYieldReturnStatement node);
        //void Visit(JsYieldBreakStatement node);
        void Visit(JsUnit node);
        void Visit(JsExternalFileUnit node);
        void Visit(JsExpression node);
        void Visit(JsNodeList node);
        void Visit(JsCodeStatement node);
        void Visit(JsSwitchStatement node);
        void Visit(JsSwitchSection node);
        void Visit(JsSwitchLabel node);
        void Visit(JsWhileStatement node);
        void Visit(JsDoWhileStatement node);
        void Visit(JsIfStatement node);
        void Visit(JsUseStrictStatement node);
        void Visit(JsForStatement node);
        void Visit(JsForInStatement node);
        void Visit(JsContinueStatement node);
        void Visit(JsBlock node);
        void Visit(JsThrowStatement node);
        void Visit(JsTryStatement node);
        void Visit(JsBreakStatement node);
        void Visit(JsExpressionStatement node);
        void Visit(JsReturnStatement node);
        void Visit(JsVariableDeclarationStatement node);
        void Visit(JsCommentStatement node);
        void Visit(JsConditionalExpression node);
        void Visit(JsAssignmentExpression node);
        void Visit(JsParenthesizedExpression node);
        void Visit(JsBinaryExpression node);
        void Visit(JsPostUnaryExpression node);
        void Visit(JsPreUnaryExpression node);
        void Visit(JsJsonObjectExpression node);
        void Visit(JsStringExpression node);
        void Visit(JsNumberExpression node);
        void Visit(JsRegexExpression node);
        void Visit(JsNullExpression node);
        void Visit(JsVariableDeclarationExpression node);
        void Visit(JsVariableDeclarator node);
        void Visit(JsNewObjectExpression node);
        void Visit(JsFunction node);
        void Visit(JsInvocationExpression node);
        void Visit(JsIndexerAccessExpression node);
        void Visit(JsMemberExpression node);
        void Visit(JsThis node);
        void Visit(JsJsonArrayExpression node);
        void Visit(JsStatementExpressionList node);
        void Visit(JsCatchClause node);
        void Visit(JsJsonMember node);
        void Visit(JsCodeExpression node);
        void Visit(JsJsonNameValue node);
    }
    public partial interface IJsNodeVisitor<R>
    {
        R Visit(JsNode node);
        R Visit(JsStatement node);
        //R Visit(JsYieldStatement node);
        //R Visit(JsYieldReturnStatement node);
        //R Visit(JsYieldBreakStatement node);
        R Visit(JsUnit node);
        R Visit(JsExternalFileUnit node);
        R Visit(JsExpression node);
        R Visit(JsNodeList node);
        R Visit(JsCodeStatement node);
        R Visit(JsSwitchStatement node);
        R Visit(JsSwitchSection node);
        R Visit(JsSwitchLabel node);
        R Visit(JsWhileStatement node);
        R Visit(JsDoWhileStatement node);
        R Visit(JsIfStatement node);
        R Visit(JsUseStrictStatement node);
        R Visit(JsForStatement node);
        R Visit(JsForInStatement node);
        R Visit(JsContinueStatement node);
        R Visit(JsBlock node);
        R Visit(JsThrowStatement node);
        R Visit(JsTryStatement node);
        R Visit(JsBreakStatement node);
        R Visit(JsExpressionStatement node);
        R Visit(JsReturnStatement node);
        R Visit(JsVariableDeclarationStatement node);
        R Visit(JsCommentStatement node);
        R Visit(JsConditionalExpression node);
        R Visit(JsAssignmentExpression node);
        R Visit(JsParenthesizedExpression node);
        R Visit(JsBinaryExpression node);
        R Visit(JsPostUnaryExpression node);
        R Visit(JsPreUnaryExpression node);
        R Visit(JsJsonObjectExpression node);
        R Visit(JsStringExpression node);
        R Visit(JsNumberExpression node);
        R Visit(JsRegexExpression node);
        R Visit(JsNullExpression node);
        R Visit(JsVariableDeclarationExpression node);
        R Visit(JsVariableDeclarator node);
        R Visit(JsNewObjectExpression node);
        R Visit(JsFunction node);
        R Visit(JsInvocationExpression node);
        R Visit(JsIndexerAccessExpression node);
        R Visit(JsMemberExpression node);
        R Visit(JsThis node);
        R Visit(JsJsonArrayExpression node);
        R Visit(JsStatementExpressionList node);
        R Visit(JsCatchClause node);
        R Visit(JsJsonMember node);
        R Visit(JsCodeExpression node);
        R Visit(JsJsonNameValue node);
    }
}
