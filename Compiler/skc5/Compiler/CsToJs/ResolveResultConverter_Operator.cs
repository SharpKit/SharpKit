using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using SharpKit.Compiler;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Mirrored.SharpKit.JavaScript;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.Extensions;
using System.Linq.Expressions;

namespace SharpKit.Compiler.CsToJs
{
    class ResolveResultVisitor_Operator
    {

        /// <summary>
        /// https://github.com/icsharpcode/NRefactory/issues/501
        /// </summary>
        OperatorResolveResult WorkaroundIssue501(OperatorResolveResult res)
        {
            if (res.UserDefinedOperatorMethod == null)
                return res;
            if ((res.OperatorType == ExpressionType.AndAlso && res.UserDefinedOperatorMethod.Name == "op_BitwiseAnd") ||
                (res.OperatorType == ExpressionType.OrElse && res.UserDefinedOperatorMethod.Name == "op_BitwiseOr"))
            {
                var fake = new OperatorResolveResult(res.Type, res.OperatorType, null, res.IsLiftedOperator, res.Operands);
                return fake;

            }
            return res;
        }

        public JsNode VisitOperatorResolveResult(OperatorResolveResult res)
        {
            if (res.Operands.Count == 1)
                return Unary(res);
            else if (res.Operands.Count == 2)
            {
                res = WorkaroundIssue501(res);
                var node2 = Binary(res);
                if (Importer.ForceIntegers)
                {
                    if (Importer.IsInteger(res.Type) && res.OperatorType == ExpressionType.Divide)
                        node2 = Importer.ForceInteger(node2);
                }
                return node2;
            }
            else if (res.Operands.Count == 3)
                return Trinary(res);
            else
                throw new NotImplementedException();
        }

        private JsNode Trinary(OperatorResolveResult res)
        {
            if (res.OperatorType == ExpressionType.Conditional)
            {
                var node5 = new JsConditionalExpression { Condition = VisitExpression(res.Operands[0]), TrueExpression = VisitExpression(res.Operands[1]), FalseExpression = VisitExpression(res.Operands[2]) };
                return node5;

            }
            else
                throw new NotImplementedException();
        }

        private JsNode Binary(OperatorResolveResult res)
        {
            if (res.UserDefinedOperatorMethod != null && !Sk.UseNativeOperatorOverloads(res.UserDefinedOperatorMethod.DeclaringTypeDefinition))
            {
                var op2 = res.OperatorType.ExtractCompoundAssignment();
                if (op2 != null)
                {
                    var fakeRight = new OperatorResolveResult(res.Type, op2.Value, res.UserDefinedOperatorMethod, res.IsLiftedOperator, res.Operands);
                    var fakeAssign = Cs.Assign(res.Operands[0], fakeRight);
                    return Visit(fakeAssign);
                }

                var fake = Cs.InvokeMethod(res.UserDefinedOperatorMethod, null, res.Operands[0], res.Operands[1]);
                return Visit(fake);
            }

            if (res.OperatorType == ExpressionType.Coalesce)
            {
                var fake = Cs.Conditional(res.Operands[0].NotEqual(Cs.Null(), Project), res.Operands[0], res.Operands[1], res.Type);
                var fake2 = Visit(fake);
                fake2 = new JsParenthesizedExpression { Expression = (JsExpression)fake2 };
                return fake2;
            }
            var mrr = res.Operands[0] as MemberResolveResult;
            if (mrr != null && mrr.Member.SymbolKind == SymbolKind.Event)
            {
                var pe = (IEvent)mrr.Member;
                if (res.OperatorType.IsAny(ExpressionType.AddAssign, ExpressionType.SubtractAssign))
                {
                    var accessor = res.OperatorType == ExpressionType.AddAssign ? pe.AddAccessor : pe.RemoveAccessor;
                    var fake = new CSharpInvocationResolveResult(mrr.TargetResult, accessor, new List<ResolveResult>
                            {
                                res.Operands[1]
                            });
                    var node6 = Visit(fake);
                    return node6;
                }
            }
            if (mrr != null && IsEntityFunctionProperty(mrr.Member, res))
            {
                var simpleOp = res.OperatorType.ExtractCompoundAssignment();

                var pe = (IProperty)mrr.Member;
                if (simpleOp != null)
                {
                    // x.Name += "Hello"    ->  x.Name = x.Name + "Hello"
                    // x.Dic["Hello"] += 7  ->  x.Dic["Hello"] = x.Dic["Hello"] + 7;
                    var fake = res.Operands[0].Assign(res.Operands[0].Binary(simpleOp.Value, res.Operands[1], res.Type));
                    var node6 = Visit(fake);
                    return node6;
                }
                else if (res.OperatorType == ExpressionType.Assign)
                {
                    var args = new List<ResolveResult>();
                    if (pe.IsIndexer)
                    {
                        var irrOp0 = (CSharpInvocationResolveResult)res.Operands[0];
                        args.AddRange(irrOp0.Arguments);
                    }
                    args.Add(res.Operands[1]);
                    var fake = new CSharpInvocationResolveResult(mrr.TargetResult, pe.Setter, args).AssociateWithOriginal(res);
                    var node6 = Visit(fake);
                    node6 = WrapSetterToReturnValueIfNeeded(res, node6);
                    return node6;
                }
            }
            if (res.Operands[0] is ConversionResolveResult && res.Operands[1] is ConstantResolveResult)
            {
                var leftConv = (ConversionResolveResult)res.Operands[0];
                var rightConst = (ConstantResolveResult)res.Operands[1];
                if (leftConv.Conversion.IsNumericConversion && leftConv.Input.Type == Cs.CharType(Project))
                {
                    var value = ((char)(int)rightConst.ConstantValue).ToString();
                    var fake = Cs.Binary(leftConv.Input, res.OperatorType, Cs.Value(value, Project), leftConv.Input.Type);
                    return Visit(fake);
                }
            }
            if (res.Operands[0].Type.Kind == TypeKind.Delegate && res.Operands[1].Type.Kind == TypeKind.Delegate)
            {
                if (res.OperatorType.IsAny(ExpressionType.AddAssign, ExpressionType.SubtractAssign))
                {
                    var op = res.OperatorType == ExpressionType.AddAssign ? ExpressionType.Add : ExpressionType.Subtract;
                    var fake = Cs.Assign(res.Operands[0], Cs.Binary(res.Operands[0], op, res.Operands[1], res.Type));
                    var node6 = Visit(fake);
                    return node6;
                }
                else if (res.OperatorType.IsAny(ExpressionType.Add, ExpressionType.Subtract))
                {
                    var combineMethod = Project.Compilation.FindType(KnownTypeCode.Delegate).GetMethods(t => t.Name == "Combine").FirstOrDefault();
                    var removeMethod = Project.Compilation.FindType(KnownTypeCode.Delegate).GetMethods(t => t.Name == "Remove").FirstOrDefault();

                    var meOp = res.OperatorType == ExpressionType.Add ? combineMethod : removeMethod;

                    var fake = Cs.Member(null, meOp).Invoke(res.Operands[0], res.Operands[1]);
                    var node6 = Visit(fake);
                    return node6;

                }
            }

            var node5 = new JsBinaryExpression { Operator = Visit(res.OperatorType), Left = VisitExpression(res.Operands[0]), Right = VisitExpression(res.Operands[1]) };
            if (res.OperatorType == ExpressionType.Equal && node5.Operator == "==")
            {
                var att = Compiler.GetJsExportAttribute();
                if (att != null && att.UseExactEquals)
                    node5.Operator = "===";
            }
            if (res.OperatorType == ExpressionType.NotEqual && node5.Operator == "!=")
            {
                var att = Compiler.GetJsExportAttribute();
                if (att != null && att.UseExactEquals)
                    node5.Operator = "!==";
            }

            return node5;
        }

        private JsNode Unary(OperatorResolveResult res)
        {
            if (res.UserDefinedOperatorMethod != null && !Sk.UseNativeOperatorOverloads(res.UserDefinedOperatorMethod.DeclaringTypeDefinition))
            {
                var fake = Cs.InvokeMethod(res.UserDefinedOperatorMethod, null, res.Operands[0]);
                return Visit(fake);
            }

            var isProperty = false;
            var meRes = res.Operands[0] as MemberResolveResult;
            if (meRes != null && meRes.Member != null && IsEntityFunctionProperty(meRes.Member, res))
                isProperty = true;

            JsExpression node2;
            if (res.OperatorType.IsAny(ExpressionType.Negate, ExpressionType.PreDecrementAssign, ExpressionType.PreIncrementAssign, ExpressionType.Not, ExpressionType.OnesComplement))
            {
                var simpler = res.OperatorType.ExtractCompoundAssignment();
                if (isProperty && simpler != null)
                {
                    var fakeCs = meRes.ShallowClone().Binary(simpler.Value, Cs.Value(1, Project), meRes.Type);
                    node2 = VisitExpression(fakeCs);
                }
                else
                {
                    node2 = new JsPreUnaryExpression { Operator = Visit(res.OperatorType), Right = VisitExpression(res.Operands[0]) };
                }
            }
            else if (res.OperatorType.IsAny(ExpressionType.PostIncrementAssign, ExpressionType.PostDecrementAssign, ExpressionType.PreIncrementAssign, ExpressionType.PreDecrementAssign))
            {
                if (isProperty)
                {
                    var simpler = res.OperatorType.ExtractCompoundAssignment();
                    var fakeCs = meRes.ShallowClone().Binary(simpler.Value, Cs.Value(1, Project), meRes.Type);
                    node2 = VisitExpression(fakeCs);

                }
                else
                {
                    node2 = new JsPostUnaryExpression { Operator = Visit(res.OperatorType), Left = VisitExpression(res.Operands[0]) };
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return node2;
        }

        #region Shortcuts

        public ResolveResultConverter Importer { get; set; }
        public SkProject Project { get; set; }
        public CompilerTool Compiler { get; set; }

        JsNode Visit(ResolveResult res)
        {
            return Importer.Visit(res);
        }

        JsExpression VisitExpression(ResolveResult res)
        {
            return Importer.VisitExpression(res);
        }

        string Visit(ExpressionType op)
        {
            return op.ToJs();
        }
        bool IsEntityFunctionProperty(IEntity me, ResolveResult scope)
        {
            return Sk.IsEntityFunctionProperty(me, scope);
        }
        private JsNode WrapSetterToReturnValueIfNeeded(OperatorResolveResult res, JsNode node2)
        {
            return Importer.WrapSetterToReturnValueIfNeeded(res, node2);
        }

        #endregion

    }
}
