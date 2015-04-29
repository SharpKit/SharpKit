using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpKit.Utils;

namespace SharpKit.JavaScript.Ast
{
    static class Js
    {
        public static JsInvocationExpression AddArgument(this JsInvocationExpression exp, JsExpression arg)
        {
            if (exp.Arguments == null)
                exp.Arguments = new List<JsExpression> { arg };
            else
                exp.Arguments.Add(arg);
            return exp;
        }
        public static JsInvocationExpression InsertArgument(this JsInvocationExpression exp, int index, JsExpression arg)
        {
            if (exp.Arguments == null)
                exp.Arguments = new List<JsExpression> { arg };
            else
                exp.Arguments.Insert(index, arg);
            return exp;
        }

        public static JsVariableDeclarationExpression AndVar(this JsVariableDeclarationExpression decl, string variableName, JsExpression initializer = null)
        {
            if (decl.Declarators == null)
                decl.Declarators = new List<JsVariableDeclarator>();
            decl.Declarators.Add(new JsVariableDeclarator { Name = variableName, Initializer = initializer });
            return decl;
        }
        public static JsVariableDeclarationExpression Var(string variableName, JsExpression initializer = null)
        {
            return new JsVariableDeclarationExpression { Declarators = new List<JsVariableDeclarator> { new JsVariableDeclarator { Name = variableName, Initializer = initializer } } };
        }

        public static JsJsonObjectExpression Json()
        {
            return new JsJsonObjectExpression();
        }
        public static JsJsonObjectExpression Add(this JsJsonObjectExpression exp, string name, JsExpression value)
        {
            exp.Add(JsonNameValue(name, value));
            return exp;
        }
        public static JsJsonObjectExpression Add(this JsJsonObjectExpression exp, JsJsonNameValue nameValue)
        {
            if (exp.NamesValues == null)
                exp.NamesValues = new List<JsJsonNameValue>();
            exp.NamesValues.Add(nameValue);
            return exp;
        }
        public static JsJsonNameValue JsonNameValue(string name, JsExpression value)
        {
            return new JsJsonNameValue { Name = new JsJsonMember { Name = name }, Value = value };
        }
        public static JsExpression Value(object value)
        {
            var s = JavaScriptHelper.ToJavaScriptValue(value);
            //if (value is char)
            //    return Code(JavaScriptHelper.ToJavaScriptChar((char)value));
            //else if(value is string)
            //    return Code(JavaScriptHelper.ToJavaScriptString((string)value));
            //var s = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(value);
            return CodeExpression(s);//JavaScriptHelper.ToJavaScriptValue(value));
        }

        public static JsSwitchStatement Switch(JsExpression exp)
        {
            return new JsSwitchStatement { Expression = exp, Sections = new List<JsSwitchSection>() };
        }

        public static JsSwitchStatement Case(this JsSwitchStatement node, JsExpression value, List<JsStatement> statements)
        {
            if (node.Sections == null)
                node.Sections = new List<JsSwitchSection>();
            node.Sections.Add(new JsSwitchSection { Labels = new List<JsSwitchLabel> { new JsSwitchLabel { Expression = value } }, Statements = statements });
            return node;
        }

        public static JsUnit Unit()
        {
            return new JsUnit { Statements = new List<JsStatement>() };
        }
        public static JsThis This()
        {
            return new JsThis();
        }
        public static JsFunction Function(params string[] prms)
        {
            return new JsFunction { Parameters = prms == null ? null : prms.ToList() };
        }
        public static JsBlock Block(this JsFunction func)
        {
            if (func.Block == null)
                func.Block = new JsBlock();
            return func.Block;
        }
        /// <summary>
        /// If statement is a block, cast and return it, otherwise, create a new block with statement inside.
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public static JsBlock ToBlock(this JsStatement st)
        {
            if (st is JsBlock)
                return (JsBlock)st;
            return new JsBlock { Statements = new List<JsStatement> { st } };
        }
        public static JsBlock Block()
        {
            return new JsBlock();
        }
        public static JsFunction Add(this JsFunction func, JsStatement st)
        {
            if (func.Block == null)
                func.Block = new JsBlock();
            if (func.Block.Statements == null)
                func.Block.Statements = new List<JsStatement>();
            func.Block.Statements.Add(st);
            return func;
        }
        public static JsFunction AddStatements(this JsFunction func, params JsStatement[] sts)
        {
            if (sts.IsNullOrEmpty())
                return func;
            if (func.Block == null)
                func.Block = new JsBlock();
            if (func.Block.Statements == null)
                func.Block.Statements = new List<JsStatement>();
            func.Block.Statements.AddRange(sts);
            return func;
        }
        public static JsBlock Add(this JsBlock block, JsStatement st)
        {
            if (block.Statements == null)
                block.Statements = new List<JsStatement>();
            block.Statements.Add(st);
            return block;
        }
        public static JsNullExpression Null()
        {
            return new JsNullExpression();
        }
        public static JsCodeExpression CodeExpression(string code)
        {
            return new JsCodeExpression { Code = code };
        }
        public static JsCodeStatement CodeStatement(string code)
        {
            return new JsCodeStatement { Code = code };
        }
        public static JsReturnStatement Return(JsExpression exp = null)
        {
            return new JsReturnStatement { Expression = exp };
        }
        public static JsStatement Statement(this JsExpression exp)
        {
            if (exp != null && exp.NodeType == JsNodeType.VariableDeclarationExpression)
                return ((JsVariableDeclarationExpression)exp).Statement();//new JsVariableDeclarationStatement { Declaration =  };
            return new JsExpressionStatement { Expression = exp };
        }
        public static JsVariableDeclarationStatement Statement(this JsVariableDeclarationExpression exp)
        {
            return new JsVariableDeclarationStatement { Declaration = exp };
        }
        public static JsIfStatement If(JsExpression condition, JsStatement ifStatement = null, JsStatement elseStatement = null)
        {
            return new JsIfStatement { Condition = condition, IfStatement = ifStatement, ElseStatement = elseStatement };
        }
        public static JsIfStatement Then(this JsIfStatement ifStatement, JsStatement thenStatement)
        {
            ifStatement.IfStatement = thenStatement;
            return ifStatement;
        }
        public static JsIfStatement Else(this JsIfStatement ifStatement, JsStatement elseStatement)
        {
            ifStatement.ElseStatement = elseStatement;
            return ifStatement;
        }
        public static JsInvocationExpression Typeof(JsMemberExpression me)
        {
            return new JsInvocationExpression { Member = new JsMemberExpression { Name = "typeof" }, Arguments = new List<JsExpression> { me } };
        }
        public static JsInvocationExpression Invoke(this JsExpression me, params JsExpression[] prms)
        {
            return new JsInvocationExpression { Member = me, Arguments = prms == null ? null : prms.ToList() };
        }
        public static JsInvocationExpression InvokeWithContextIfNeeded(this JsExpression me, JsExpression context, params JsExpression[] prms)
        {
            if (context == null)
                return me.Invoke(prms);
            var prms2 = prms.ToList();
            prms2.Insert(0, context);
            return me.Member("call").Invoke(prms2.ToArray());
        }
        public static JsIndexerAccessExpression IndexerAccess(this JsExpression me, JsExpression prm)
        {
            return new JsIndexerAccessExpression { Member = me, Arguments = new List<JsExpression> { prm } };
        }
        public static JsNewObjectExpression New(JsExpression ctor, params JsExpression[] prms)
        {
            return new JsNewObjectExpression { Invocation = ctor.Invoke(prms) };
        }
        public static JsJsonArrayExpression NewJsonArray(params JsExpression[] items)
        {
            return new JsJsonArrayExpression { Items = items == null ? null : items.ToList() };
        }
        public static JsExpression NewArray(string type, JsExpression size, JsExpression[] items)
        {
            if (type.IsNullOrEmpty())
                type = "Array";

            if (type == "Array")
            {
                if (items != null)
                    return NewJsonArray(items);
                else if (size == null)
                    return NewJsonArray();
            }

            //Typed arrays
            if (items != null)
                return Js.New(Member(type), NewJsonArray(items));
            if (size == null)
                return Js.New(Member(type));

            return Js.New(Member(type), size);
        }
        public static JsNewObjectExpression NewArray(int size)
        {
            return New(Member("Array"), Value(size));
        }
        public static JsMemberExpression Member(string name)
        {
            return new JsMemberExpression { Name = name };
        }
        public static JsMemberExpression MemberOrSelf(this JsMemberExpression member, string name)
        {
            if (name.IsNullOrEmpty())
                return member;
            return member.Member(name);
        }
        public static JsMemberExpression Member(this JsExpression member, string name)
        {
            return new JsMemberExpression { Name = name, PreviousMember = member };
        }
        public static JsMemberExpression Member(this JsExpression member, JsMemberExpression exp)
        {
            exp.PreviousMember = member;
            return exp;
        }
        public static JsBinaryExpression InstanceOf(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = "instanceof", Right = exp2 };
        }
        public static JsBinaryExpression Equal(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = "==", Right = exp2 };
        }
        public static JsBinaryExpression NotEqual(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = "!=", Right = exp2 };
        }
        public static JsBinaryExpression Or(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = "||", Right = exp2 };
        }
        public static JsBinaryExpression BitwiseOr(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = "|", Right = exp2 };
        }
        public static JsBinaryExpression LessThan(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = "<", Right = exp2 };
        }
        public static JsBinaryExpression GreaterThan(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = ">", Right = exp2 };
        }
        public static JsBinaryExpression GreaterThanOrEqualTo(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = ">=", Right = exp2 };
        }
        public static JsBinaryExpression LessThanOrEqualTo(this JsExpression exp, JsExpression exp2)
        {
            return new JsBinaryExpression { Left = exp, Operator = "<=", Right = exp2 };
        }
        public static JsPostUnaryExpression PlusPlus(this JsExpression exp)
        {
            return new JsPostUnaryExpression { Left = exp, Operator = "++" };
        }
        public static JsPostUnaryExpression MinusMinus(this JsExpression exp)
        {
            return new JsPostUnaryExpression { Left = exp, Operator = "--" };
        }
        public static JsParenthesizedExpression Parentheses(this JsExpression exp)
        {
            return new JsParenthesizedExpression { Expression = exp };
        }
        public static JsAssignmentExpression Assign(this JsExpression me, JsExpression exp2)
        {
            var node = new JsAssignmentExpression { Left = me, Right = exp2 };
            return node;
        }
        public static JsWhileStatement While(JsExpression condition, JsStatement statement = null)
        {
            return new JsWhileStatement { Condition = condition, Statement = statement };
        }
        public static JsStringExpression String(string s)
        {
            return new JsStringExpression { Value = s };
        }
        public static JsMemberExpression Members(string members)
        {
            JsMemberExpression node = null;
            foreach (var token in members.Split('.'))
            {
                node = new JsMemberExpression { PreviousMember = node };
                node.Name = token;
            }
            return node;
        }

        public static JsExpression True()
        {
            return Js.Value(true);
        }
        public static JsExpression Conditional(this JsExpression condition, JsExpression trueExp, JsExpression falseExp)
        {
            return new JsConditionalExpression { Condition = condition, TrueExpression = trueExp, FalseExpression = falseExp };
        }
        public static JsThrowStatement ThrowNewError(string msg)
        {
            return new JsThrowStatement { Expression = Js.New(Js.Member("Error"), Js.String(msg)) };
        }
    }

    static class JsRefactorer
    {
        /// <summary>
        /// MyFunction(prm1, prm2) -> MyFunction.call(context, prm1, prm2)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static void ToCallWithContext(JsInvocationExpression node, JsExpression context)
        {
            node.Member = node.Member.Member("call");
            if (node.Arguments == null)
                node.Arguments = new List<JsExpression>();
            node.Arguments.Insert(0, Js.This());
        }
    }


}
