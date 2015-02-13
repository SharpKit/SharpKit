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
    class ResolveResultConverter : ICSharpResolveResultVisitor<JsNode>
    {
        public CompilerLogger Log { get; set; }
        public SkProject Project { get { return Compiler.Project; } }
        public CompilerTool Compiler { get; set; }
        public AstNodeConverter AstNodeConverter { get; set; }
        public event Action<ResolveResult> BeforeConvertCsToJsResolveResult;
        public event Action<ResolveResult, JsNode> AfterConvertCsToJsResolveResult;

        internal Dictionary<InitializedObjectResolveResult, string> Initializers = new Dictionary<InitializedObjectResolveResult, string>();

        #region Resolve Visitor

        public JsNode VisitInitializedObjectResolveResult(InitializedObjectResolveResult res)
        {
            var varName = Initializers.TryGetValue(res);
            if (varName == null)
            {
                varName = "$v" + VariableInitializerCounter++;
                Initializers[res] = varName;
            }
            return Js.Member(varName);
        }

        public JsNode VisitTypeOfResolveResult(TypeOfResolveResult res)
        {
            return Js.Member("Typeof").Invoke(SkJs.EntityTypeRefToMember(res.ReferencedType));
        }

        public JsNode VisitArrayAccessResolveResult(ArrayAccessResolveResult res)
        {
            var node2 = new JsIndexerAccessExpression { Member = VisitExpression(res.Array), Arguments = VisitExpressions(res.Indexes) };
            return node2;
        }

        public JsNode VisitArrayCreateResolveResult(ArrayCreateResolveResult res)
        {
            var jsArrayType = SkJs.GetJsArrayType((ArrayType)res.Type);
            JsExpression[] items = null;
            JsExpression size = null;
            if (res.InitializerElements.IsNotNullOrEmpty())
                items = VisitExpressions(res.InitializerElements).ToArray();
            else if (res.SizeArguments.IsNotNullOrEmpty())
                size = VisitExpression(res.SizeArguments.Single());

            return Js.NewArray(jsArrayType, size, items);
        }

        public JsNode VisitTypeIsResolveResult(TypeIsResolveResult res)
        {
            if (Sk.OmitCasts(res.TargetType))
            {
                return Js.True();
                //var node2 = Visit(res.Input);
                //return node2;
            }
            else if (Sk.NativeCasts(res.TargetType))
            {
                var typeFieldName = Sk.GetJsonTypeFieldName(res.TargetType.GetDefinitionOrArrayType());
                if (typeFieldName != null && Sk.IsJsonMode(res.TargetType.GetDefinitionOrArrayType()))
                {
                    return Js.Parentheses(VisitExpression(res.Input).Or(Js.Json())).Member(typeFieldName).Equal(SkJs.EntityTypeRefToMember(res.TargetType));
                }
                var node2 = VisitExpression(res.Input).InstanceOf(SkJs.EntityTypeRefToMember(res.TargetType));
                return node2;
            }
            else
            {
                var node2 = Js.Member("Is").Invoke(VisitExpression(res.Input), SkJs.EntityTypeRefToMember(res.TargetType));
                return node2;
            }
        }

        public JsNode VisitMethodGroupResolveResult(MethodGroupResolveResult res)
        {
            var info = res.GetInfo();
            IMethod me;
            if (info != null && info.Conversion != null && info.Conversion.Method != null)
            {
                me = info.Conversion.Method;
            }
            else //happens when invoking a method with overloads, and a parameter is dynamic
            {
                var list = res.Methods.ToList();
                if (list.Count == 0)
                    throw new Exception("Method group not resolved to any method");
                else if (list.Count == 1)
                    me = list[0];
                else
                    me = list[0];
                //TODO: verify all methods has the same js name
            }
            var isExtensionMethodStyle = me.IsExtensionMethod && !(res.TargetResult is TypeResolveResult) && info != null && info.Conversion != null && info.Conversion.DelegateCapturesFirstArgument;//TODO: IsExtensionMethodStyle(new CsInvocationExpression { entity = me, expression = node });
            JsExpression firstPrm = null;
            if (isExtensionMethodStyle)
            {
                firstPrm = (JsExpression)Visit(res.TargetResult);
            }
            var node2 = SkJs.EntityToMember(me);
            JsExpression node3;
            JsExpression instanceContext = null;
            if (me.IsStatic || res.TargetResult == null) //getting ThisResolveResult even on static methods, getting TargetResult=null when MethodGroupResolveResult when using delegates
            {
                node3 = node2;
            }
            else
            {
                instanceContext = VisitExpression(res.TargetResult);
                node3 = instanceContext.Member(node2);
            }
            if (info != null && (instanceContext != null || firstPrm != null))
            {
                var conv = info.Conversion;
                if (info.ConversionTargetType != null && !UseNativeFunctions(info.ConversionTargetType))//delegate type
                {
                    var parentMethod = info.Nodes.FirstOrDefault().GetCurrentMethod();
                    if (parentMethod == null || !Sk.ForceDelegatesAsNativeFunctions(parentMethod))
                    {
                        if (parentMethod == null)
                            Log.Warn(info.Nodes.FirstOrDefault(), "GetParentMethod() returned null");
                        var func = (JsExpression)node2;
                        if (instanceContext != null)
                            node3 = CreateJsDelegate(instanceContext, func);
                        else if (firstPrm != null)
                            node3 = CreateJsExtensionDelegate(firstPrm, func);
                    }
                }
            }
            return node3;


        }

        public JsNode VisitOperatorResolveResult(OperatorResolveResult res)
        {
            return new ResolveResultVisitor_Operator { Compiler = Compiler, Importer = this, Project = Project }.VisitOperatorResolveResult(res);
        }

        public JsNode VisitLambdaResolveResult(LambdaResolveResult res)
        {
            //var prmTypes = res.Parameters.Select(t => t.Type).ToArray();
            //var retType = res.GetInferredReturnType(prmTypes);
            //var conv = res.IsValid(prmTypes, retType, CSharpConversions.Get(Project.Compilation));
            //return Visit(conv);
            var func = new JsFunction { Parameters = res.Parameters.Select(t => JsIdentifier(t.Name)).ToList() };
            var body = res.Body;
            JsNode body2;
            var info = res.GetInfo();
            if (body.GetType() == typeof(ResolveResult) && body.Type.IsVoid())
            {
                var x = res.GetType().GetProperty("BodyExpression", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(res, null);
                var astNode = (AstNode)x;
                body2 = Visit(astNode);
            }
            else
            {
                body2 = Visit(res.Body);
            }
            var currentMember = res.GetCurrentMember();
            var delType = info.ConversionTargetType;
            if (body2 is JsExpression)
            {
                var delMethod = delType.GetDelegateInvokeMethod();
                JsStatement st = new JsReturnStatement { Expression = (JsExpression)body2 };
                if (delMethod != null && delMethod.ReturnType.IsVoid())
                    st = new JsExpressionStatement { Expression = (JsExpression)body2 };

                func.Block = new JsBlock { Statements = new List<JsStatement> { st } };
            }
            else if (body2 is JsBlock)
            {
                func.Block = (JsBlock)body2;
            }
            if (delType == null || currentMember == null)
            {
                Log.Warn(res.GetFirstNode(), "Cannot resolve delegate type / parent method");
                return func;
            }
            else
            {
                return CreateJsDelegateIfNeeded(func, currentMember, delType, true);
            }
        }

        public JsNode VisitTypeResolveResult(TypeResolveResult res)
        {
            return SkJs.EntityTypeRefToMember(res.Type);
            //throw new NotImplementedException();
        }

        public JsNode VisitResolveResult(ResolveResult res)
        {
            if (res.Type == SpecialType.NullType)
                return Js.Null();
            else if (res.Type.IsVoid())
            {
                Log.Warn("void");
                return Js.CodeExpression("void");
            }
            else if (res.Type.Kind == TypeKind.Dynamic)
            {
                var info = res.GetInfo();
                if (info == null || info.Nodes.Count != 1)
                    throw new NotImplementedException("Dynamics");
                var node2 = Visit(info.Nodes[0]);
                return node2;
            }
            throw new NotImplementedException();
        }

        public JsNode VisitLocalResolveResult(LocalResolveResult res)
        {
            var node2 = Js.Member(JsIdentifier(res.Variable.Name));
            if (res.Variable != null && res.Variable.Type.Kind == TypeKind.ByReference)
            {
                node2 = node2.Member("Value");
            }
            return node2;
        }

        public JsNode VisitConversionResolveResult(ConversionResolveResult res)
        {
            var input = res.Input;
            var conversion = res.Conversion;
            var conversionType = res.Type;

            var info = res.GetInfo();
            if (info == null)
            {
                info = new ResolveResultInfo { Conversion = conversion, ResolveResult = res, ConversionTargetType = res.Type };
                res.SetInfo(info);

            }
            var info2 = input.GetInfo();
            if (info2 == null)
            {
                input.SetInfo(new ResolveResultInfo { Nodes = info.Nodes.ToList(), Conversion = conversion, ConversionTargetType = res.Type, ResolveResult = input });
            }
            if (conversion.IsUserDefined && res.Type.FullName == "SharpKit.JavaScript.JsCode" && input is ConstantResolveResult)
            {
                var value = ((ConstantResolveResult)input).ConstantValue;
                var node3 = Js.CodeExpression(value == null ? "null" : value.ToString());
                return node3;
            }
            return VisitConversion(input, conversion, conversionType);
        }

        private JsNode VisitConversion(ResolveResult input, Conversion conversion, IType conversionType)
        {
            ////TODO: HACK: https://github.com/icsharpcode/NRefactory/issues/183
            //var isImplicit = res.Conversion.IsImplicit;
            //if (!isImplicit && res.Conversion.IsExplicit && res.Conversion.Method != null && res.Conversion.Method.Name != null && res.Conversion.Method.Name.Contains("Implicit"))
            //    isImplicit = true;
            if (conversion.IsMethodGroupConversion)
            {
            }
            else if (conversion.IsUserDefined)
            {
                ITypeDefinition typeDef;
                if (conversion.Method != null && conversion.Method.DeclaringType != null)
                {
                    typeDef = conversion.Method.DeclaringType.GetDefinitionOrArrayType();
                }
                else
                {
                    typeDef = conversionType.GetDefinitionOrArrayType();
                }
                var nativeOverloads = Sk.UseNativeOperatorOverloads(typeDef);
                if (nativeOverloads)
                    return Visit(input);
                ////TODO: Check if OmitCalls is found on conversion method, if so - return Visit(input);
                //if (Sk.IsOmitCalls(conversion.Method))
                //{
                //}
                var fake = conversion.Method.InvokeMethod(null, input);
                var node2 = Visit(fake);
                return node2;
            }
            else if (conversion.IsTryCast || conversion.IsExplicit)
            {
                if (ForceIntegers && conversion.IsNumericConversion && IsInteger(conversionType))
                {
                    return ForceInteger(Visit(input));
                }

                //Skip enum casts
                if ((conversionType.Kind == TypeKind.Enum && IsInteger(input.Type)) || (input.Type.Kind == TypeKind.Enum && IsInteger(conversionType)))
                    return Visit(input);

                var omitCasts = Sk.OmitCasts(conversionType);
                if (omitCasts)
                    return Visit(input);
                if (Sk.NativeCasts(conversionType))
                {
                    var exp2 = VisitExpression(input);
                    var type2 = SkJs.EntityTypeRefToMember(conversionType);
                    if (conversion.IsTryCast)
                    {
                        var node2 = exp2.InstanceOf(type2).Conditional(exp2, Js.Null());
                        return node2;
                    }
                    else
                    {
                        var node2 = Js.Conditional(exp2.InstanceOf(type2).Or(exp2.Equal(Js.Null())), exp2,
                            Js.Parentheses(Js.Function().Add(Js.ThrowNewError("InvalidCastException")).Invoke()));
                        //var node2 = Js.Parentheses(Js.Function().AddStatements(
                        //Js.If(exp2.InstanceOf(type2).Or(exp2.Equal(Js.Null())), Js.Return(exp2)),
                        //Js.ThrowNewError("InvalidCastException"))).Member("call").Invoke(Js.This());
                        return node2;
                    }
                }
                else
                {
                    var cast = conversion.IsTryCast ? "As" : "Cast";
                    var node2 = Js.Member(cast).Invoke(VisitExpression(input), SkJs.EntityTypeRefToMember(conversionType));
                    return node2;
                }
            }
            return Visit(input);
        }

        public JsNode VisitConstantResolveResult(ConstantResolveResult res)
        {
            var nodes = res.GetNodes();
            if (res.Type is DefaultTypeParameter)
            {
                return Js.Member("Default").Invoke(SkJs.EntityTypeRefToMember(res.Type));
            }
            if (res.Type != null && res.Type.Kind == TypeKind.Enum)
            {
                var enumMembers = res.Type.GetFields();
                var me = enumMembers.Where(t => (t.ConstantValue != null) && t.ConstantValue.Equals(res.ConstantValue)).FirstOrDefault();
                if (me != null)
                    return Visit(me.AccessSelf());//.Access().Member(c.CreateTypeRef(en), defaultEnumMember);

                //TODO:
                //return Visit(JsTypeImporter.GetValueTypeInitializer(res.Type, Project));
            }
            //var nodes = res.GetNodes();
            //if (nodes.IsNotNullOrEmpty())
            //{
            //    var node = nodes[0];
            //    if (node != null && node is PrimitiveExpression)
            //    {
            //        var node2 = Visit(node); //use literal value instead
            //        return node2;
            //    }
            //}
            return Js.Value(res.ConstantValue);
        }

        public JsNode VisitThisResolveResult(ThisResolveResult res)
        {
            return Js.This();
        }

        public JsNode VisitInvocationResolveResult(InvocationResolveResult res)
        {
            return Visit(res.ToCSharpInvocationResolveResult());
        }

        public JsNode VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult res)
        {
            if (res.Member.IsConstructor())
            {
                if (res.Type is AnonymousType)
                {
                    //TODO: check context class JsType.NativeJsons
                    var json = InitializersToJson(res.InitializerStatements, res.Type);
                    var parentType = res.GetParentType();
                    if (parentType != null && !Sk.UseNativeJsons(parentType))
                    {
                        return Js.Member("$CreateAnonymousObject").Invoke(json);
                    }
                    return json;
                }
                else
                {
                    return VisitInvocationResolveResultAsCtor(res);
                }
            }
            else
            {
                return VisitInvocationResolveResult(res);
            }
        }

        public JsNode VisitMemberResolveResult(MemberResolveResult res)
        {
            var me = res.Member;
            JsNode node2;
            bool enumValuesAsNames;

            if (me == null) //TODO: dynamics
            {
                throw new NotImplementedException();
                //var node3 = Js.Member(node.MemberName);
                //if (node.Target != null)
                //    node3.PreviousMember = VisitExpression(node.Target);
                //return node3;
            }
            else if (Sk.IsEntityFunctionProperty(res.Member, res))//(Entity)node.entity))
            {
                var pe = (IProperty)me;
                var xxx = new CSharpInvocationResolveResult(res.TargetResult, pe.Getter, null);
                node2 = Visit(xxx);
                return node2;
            }
            else if (me.IsEnumMember() && Sk.UseJsonEnums(me, out enumValuesAsNames))
            {
                var me2 = (IField)me;
                if (enumValuesAsNames)
                    return Js.String(SkJs.GetEntityJsName(me2));
                else
                    return Js.Value(me2.ConstantValue);
            }
            //TODO: Support a way to override this (JsField.ConstantInlining=false)
            else if (res.IsCompileTimeConstant && !me.IsEnumMember())
            {
                return Js.Value(res.ConstantValue);
            }
            else
            {
                var node3 = SkJs.EntityToMember(me);
                node2 = node3;
                if (res.TargetResult != null && !me.IsStatic())
                {
                    var instanceContext = VisitExpression(res.TargetResult);
                    if (node3.Name.IsNullOrEmpty()) //support Name=""
                        node2 = instanceContext;
                    else
                        node3.PreviousMember = instanceContext;
                }
            }
            return node2;
        }

        public JsNode VisitByReferenceResolveResult(ByReferenceResolveResult res)
        {
            return Visit(res.ElementResult);
        }

        public JsNode VisitNamedArgumentResolveResult(NamedArgumentResolveResult res)
        {
            throw new NotImplementedException("VisitNamedArgumentResolveResult");
        }

        public JsNode VisitDynamicInvocationResolveResult(DynamicInvocationResolveResult res)
        {
            var target2 = VisitExpression(res.Target);
            var args2 = VisitExpressions(res.Arguments);
            if (res.InvocationType == DynamicInvocationType.Invocation)
            {
                var node2 = target2.Invoke(args2.ToArray());
                return node2;
            }
            else if (res.InvocationType == DynamicInvocationType.ObjectCreation)
            {
                var node2 = Js.New(target2, args2.ToArray());
                return node2;
            }
            else if (res.InvocationType == DynamicInvocationType.Indexing)
            {
                var node2 = target2.IndexerAccess(args2.Single());
                return node2;
            }
            else
                throw new NotImplementedException("Dynamics: " + res.InvocationType);
        }

        public JsNode VisitDynamicMemberResolveResult(DynamicMemberResolveResult res)
        {
            var target2 = VisitExpression(res.Target);
            var node2 = target2.Member(res.Member);
            return node2;
        }

        #endregion

        #region ForceIntegers

        bool? _ForceIntegers;
        public bool ForceIntegers
        {
            get
            {
                if (_ForceIntegers == null)
                {
                    var att = Compiler.GetJsExportAttribute();
                    _ForceIntegers = att != null && att.ForceIntegers;
                }
                return _ForceIntegers.Value;
            }
        }
        internal JsNode ForceInteger(JsNode node2)
        {
            var exp = node2 as JsExpression;
            if (exp == null)
                return node2; //problem
            if (!exp.Is(JsNodeType.MemberExpression))
                exp = Js.Parentheses(exp);
            return Js.BitwiseOr(exp, Js.Value(0));
        }
        static string[] IntegerTypeNames = new string[]
        {
            "System.Int32",
            "System.UInt32",
            "System.UInt64",
            "System.Int16",
            "System.UInt16",
            //"System.Char",
            //"System.Byte",
            //"System.SByte",
        };
        internal bool IsInteger(IType type)
        {
            return IntegerTypeNames.Contains(type.FullName);
        }
        #endregion

        #region Utils

        private JsNode VisitInvocationResolveResult(CSharpInvocationResolveResult res)
        {
            return new ResolveResultVisitor_Invcation { Compiler = Compiler, Importer = this, Project = Project }.VisitInvocationResolveResult(res);
        }

        private JsNode VisitInvocationResolveResultAsCtor(CSharpInvocationResolveResult res)
        {
            return new ResolveResultVisitor_Invcation { Compiler = Compiler, Importer = this, Project = Project }.VisitInvocationResolveResultAsCtor(res);
        }

        internal JsExpression InitializersToJson(IList<ResolveResult> initializerStatements, IType type)
        {
            //if (Sk.IsNativeArrayEnumerator(type.GetDefinition()))
            //FIX for issue 325:
            if (Sk.IsNativeArrayEnumerator(type.GetDefinition()) && (initializerStatements.IsEmpty() || initializerStatements[0] is CSharpInvocationResolveResult))
            {
                var items = initializerStatements.Cast<CSharpInvocationResolveResult>().Select(t => t.Arguments[0]).ToList();
                var items2 = VisitExpressions(items);
                var arr = Js.NewJsonArray(items2.ToArray());
                return arr;
            }
            else
            {
                var json = Js.Json();
                foreach (var st in initializerStatements)
                {
                    if (st is OperatorResolveResult)
                    {
                        var op = (OperatorResolveResult)st;
                        var mrr = (MemberResolveResult)op.Operands[0];
                        var name = SkJs.GetEntityJsName(mrr.Member);
                        var value = VisitExpression(op.Operands[1]);
                        json.Add(name, value);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                //var inits2 = initializerStatements.Select(t => Visit(t)).ToList();
                //var namesValues = inits2.Cast<JsBinaryExpression>().Select(t => Js.JsonNameValue(((JsMemberExpression)t.Left).Name, t.Right)).ToList();
                //var json = Js.Json();
                //json.NamesValues = namesValues;
                return json;
            }
        }

        /// <summary>
        /// Wraps a setter invocation with a js function that returns the setter value back, if another assignment operation occurs
        /// var x = contact.Name = "Shooki";
        /// var x = contact.setName("Shooki"); //error
        /// var x = (function(arg){contact.setName(arg);return arg;}).call(this, "Shooki");
        /// </summary>
        /// <param name="res"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        internal JsNode WrapSetterToReturnValueIfNeeded(OperatorResolveResult res, JsNode node2)
        {
            var node3 = node2 as JsInvocationExpression;
            if (node3 == null)
                return node2;

            if (RequiresWrapSetterToReturnValue(res))
            {
                var lastArg = node3.Arguments.Last();
                var prmName = "$p" + ParameterNameCounter++;
                node3.Arguments[node3.Arguments.Count - 1] = Js.Member(prmName);

                var func = Js.Function(prmName).Add(((JsExpression)node2).Statement());
                func.Add(Js.Return(Js.Member(prmName)));
                node2 = WrapFunctionAndInvoke(res, func, lastArg);
            }
            return node2;
        }

        internal JsInvocationExpression WrapFunctionAndInvoke(ResolveResult instanceContext, JsFunction func, params JsExpression[] args)
        {
            JsExpression instanceContext2 = null;
            var me = instanceContext.GetCurrentMethod();
            if (me == null)
            {
                //TODO: WARN
                instanceContext2 = Js.This();
            }
            else if (IsNonStatic(me))
                instanceContext2 = Js.This();

            return func.Parentheses().InvokeWithContextIfNeeded(instanceContext2, args);
        }

        bool RequiresWrapSetterToReturnValue(OperatorResolveResult op)
        {
            var node = op.GetFirstNode();
            if (node == null)
                return false;

            var parentNode = node.Parent;
            if (parentNode == null)
                return false;

            if (parentNode is ReturnStatement)
                return true;


            var parentRes = parentNode.Resolve();
            if (parentRes is OperatorResolveResult)
            {
                var parentOp = (OperatorResolveResult)parentRes;

                if (parentOp.OperatorType == ExpressionType.Assign)
                    return true;
                if (parentOp.OperatorType == ExpressionType.Conditional && parentOp.Operands.IndexOf(op) > 0)
                    return true;
                return false;
            }
            if (parentRes is LocalResolveResult && parentRes.GetFirstNode() is VariableInitializer)
                return true;
            return false;

        }
        public static string JsIdentifier(string name)
        {
            return name.Replace("<", "$").Replace(">", "$");
        }
        bool IsInNonStaticMethodContext(ResolveResult res)
        {
            return IsNonStatic(res.GetCurrentMethod());
        }


        JsExpression CreateJsDelegate(JsExpression instanceContext, JsExpression func)
        {
            if (instanceContext == null)
                return func;
            return new JsInvocationExpression { Member = new JsMemberExpression { Name = "$CreateDelegate" }, Arguments = new List<JsExpression> { instanceContext.Clone(), func } };
        }
        JsExpression CreateAnonymousJsDelegate(JsExpression instanceContext, JsExpression func)
        {
            if (instanceContext == null)
                return func;
            return new JsInvocationExpression { Member = new JsMemberExpression { Name = "$CreateAnonymousDelegate" }, Arguments = new List<JsExpression> { instanceContext.Clone(), func } };
        }
        JsExpression CreateJsExtensionDelegate(JsExpression prm1, JsExpression func)
        {
            if (prm1 == null)
                return func;
            return new JsInvocationExpression { Member = new JsMemberExpression { Name = "$CreateExtensionDelegate" }, Arguments = new List<JsExpression> { prm1, func } };
        }
        JsNode CreateJsDelegateIfNeeded(JsFunction func, IMember currentMember, IType delType, bool isAnonymous)
        {
            if (currentMember != null && !currentMember.IsStatic && !UseNativeFunctions(delType) && !Sk.ForceDelegatesAsNativeFunctions(currentMember))
            {
                var instanceContext = new JsThis();
                JsExpression wrapper;
                if (isAnonymous)
                    wrapper = CreateAnonymousJsDelegate(instanceContext, func);
                else
                    wrapper = CreateJsDelegate(instanceContext, func);
                return wrapper;
            }
            else
            {
                return func;
            }
        }

        bool IsNonStatic(IEntity me)
        {
            if (!me.IsStatic())
                return true;
            if (me is IMethod && Sk.ExtensionImplementedInInstance((IMethod)me))
                return true;
            return false;
        }
        bool UseNativeFunctions(IType delegateType)
        {
            return UseNativeFunctions(delegateType.GetEntityType());
        }
        bool UseNativeFunctions(ITypeDefinition delegateType)
        {
            if (delegateType != null)
            {
                var att2 = delegateType.GetMetadata<JsDelegateAttribute>();
                if (att2 != null && att2.NativeFunction)
                    return true;
            }
            return false;
        }

        int VariableInitializerCounter = 1;
        int ParameterNameCounter = 1;

        private JsExpression ParenthesizeIfNeeded(ResolveResult res, JsExpression exp)
        {
            if (exp.NodeType == JsNodeType.ParenthesizedExpression)
                return exp;
            var nodes = res.GetNodes();
            if (nodes == null)
                return exp;
            var cspe = nodes.OfType<ParenthesizedExpression>().FirstOrDefault();
            if (cspe == null)
                return exp;
            return new JsParenthesizedExpression { Expression = exp };
        }
        #endregion

        #region Visit Utils

        [System.Diagnostics.DebuggerStepThrough]
        public JsExpression VisitExpression(ResolveResult res)
        {
            return (JsExpression)Visit(res);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public List<JsExpression> VisitExpressions(IList<ResolveResult> nodes)
        {
            return nodes.Select(VisitExpression).ToList();
        }
        [System.Diagnostics.DebuggerStepThrough]
        public JsNode Visit(ResolveResult res)
        {
            try
            {
                if (CompilerConfiguration.Current.EnableLogging)
                {
                    var node3 = res.GetFirstNode();
                    Log.WriteLine("JsCodeImporter: Visit ResolveResult: {0}, AstNode: {1}", res, AstNodeConverter.ToDebug(node3));
                }
                if (BeforeConvertCsToJsResolveResult != null)
                    BeforeConvertCsToJsResolveResult(res);
                var node2 = res.AcceptVisitor(this);
                if (node2 is JsExpression)
                    node2 = ParenthesizeIfNeeded(res, (JsExpression)node2);

                var astNode = res.GetFirstNode();
                if (astNode != null && node2 != null)
                {
                    if (node2.Annotation<AstNode>() == null)
                        node2.AddAnnotation(astNode);
                }
                if (AfterConvertCsToJsResolveResult != null)
                    AfterConvertCsToJsResolveResult(res, node2);
                return node2;
            }
            catch (CompilerException e)
            {
                if (e.AstNode == null)
                    e.AstNode = res.GetFirstNode();
                throw e;
            }
            catch (Exception e)
            {
                throw new CompilerException(res.GetFirstNode(), e);
            }
        }
        internal JsNode Visit(AstNode node)
        {
            return AstNodeConverter.Visit(node);
        }

		public JsNode VisitAwaitResolveResult(AwaitResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitAmbiguousTypeResolveResult(AmbiguousTypeResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitUnknownMethodResolveResult(UnknownMethodResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitSizeOfResolveResult(SizeOfResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitErrorResolveResult(ErrorResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitForEachResolveResult(ForEachResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitNamespaceResolveResult(NamespaceResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitUnknownMemberResolveResult(UnknownMemberResolveResult res)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitUnknownIdentifierResolveResult(UnknownIdentifierResolveResult res)
		{
			throw new NotImplementedException();
		}

		#endregion


	}


}
