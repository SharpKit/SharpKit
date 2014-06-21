using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace SharpKit.Compiler
{
    static class Cs2
    {
        public static IfElseStatement If(Expression condition, Statement trueStatement, Statement falseStatement = null)
        {
            return new IfElseStatement(condition, trueStatement, falseStatement);
        }
        public static ThisReferenceExpression This()
        {
            return new ThisReferenceExpression();
        }
        public static IdentifierExpression Access(this ParameterDeclaration prm)
        {
            return Identifier(prm.Name);
        }
        public static IdentifierExpression Identifier(string name)
        {
            return new IdentifierExpression(name);
        }
        public static PrimitiveExpression Value(object value)
        {
            return new PrimitiveExpression(value);
        }
        public static AnonymousTypeCreateExpression New()
        {
            return new AnonymousTypeCreateExpression();
        }
        public static AssignmentExpression Assign(this Expression left, Expression right)
        {
            return new AssignmentExpression(left, right);
        }
        public static AnonymousTypeCreateExpression Set(this AnonymousTypeCreateExpression exp, string name, Expression value)
        {
            exp.Initializers.Add(Cs2.Identifier(name).Assign(value));
            return exp;
        }
        public static AnonymousTypeCreateExpression Set(this AnonymousTypeCreateExpression exp, Expression memberAndValue)
        {
            exp.Initializers.Add(memberAndValue);
            return exp;
        }
        public static ExpressionStatement Statement(this Expression exp)
        {
            return new ExpressionStatement(exp);
        }
    }


    class ResolveResultExpression : Expression
    {
        private ResolveResult ResolveResult;

        public ResolveResultExpression(ResolveResult res)
        {
            ResolveResult = res;
        }
        public override void AcceptVisitor(IAstVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }

        public override S AcceptVisitor<T, S>(IAstVisitor<T, S> visitor, T data)
        {
            throw new NotImplementedException();
        }

        protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            throw new NotImplementedException();
        }
    }
}
