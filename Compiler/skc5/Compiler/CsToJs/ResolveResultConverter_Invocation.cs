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
using SharpKit.JavaScript;

namespace SharpKit.Compiler.CsToJs
{
    class ResolveResultVisitor_Invcation
    {
        private IParameterizedMember Member;
        private IMethod Method;
        private IProperty Property;
        private JsMethodAttribute MethodAtt;
        private JsInvocationExpression Node2;
        private JsMemberExpression JsMember;
        private CSharpInvocationResolveResult Res;
        private List<ByReferenceResolveResult> ByRefs;
        private List<int> ByRefIndexes;
        private List<int> RefToRefs;
        private JsExpression JsTarget;
        private List<PrmBinding> PrmBindings;
        private List<GenericArg> GenericArgs;

        #region Invocation

        public JsNode VisitInvocationResolveResult(CSharpInvocationResolveResult res)
        {
            Res = res;

            ProcessMember();

            if (MethodAtt != null && MethodAtt.InlineCode != null)
                return Js.CodeExpression(MethodAtt.InlineCode);

            var conditional = ProcessConditional();
            if (conditional)
                return null;

            bool omittedCalls;
            var omittedCallsNode = ProcessOmitCalls(out omittedCalls);
            if (omittedCalls)
                return omittedCallsNode;

            JsMember = SkJs.EntityToMember(Member);
            ProcessTarget();

            ProcessPrmBindings();

            ProcessNativeParams();

            ProcessByRefs1();

            PrmBindings.ForEach(t => t.JsCallResult = VisitExpression(t.Binding.CallResult));
            Node2 = new JsInvocationExpression
            {
                Member = JsMember,
                Arguments = PrmBindings.Select(t => t.JsCallResult).ToList(),
            };

            ProcessByRefs2();

            ProcessExtensionImplementedInInstance();

            TransformIntoBaseMethodCallIfNeeded(Res, Node2);

            ProcessArgsCustomization();

            ProcessGenericMethodArgs();

            var inlineCodeExpression = ProcessInlineCodeExpression();
            if (inlineCodeExpression != null)
                return inlineCodeExpression;



            var omittedDotOperator = ProcessOmitDotOperator();
            if (omittedDotOperator != null)
                return omittedDotOperator;

            ProcessRemoveEmptyPreviousMemberName();

            var indexerAccess = ProcessIndexer();
            if (indexerAccess != null)
                return indexerAccess;

            ProcessByRefs3();

            return Node2;

        }

        bool ProcessConditional()
        {
            //TODO: move defines locally
            var condAtt = Method.GetMetadata<System.Diagnostics.ConditionalAttribute>();
            if (condAtt != null && Compiler != null && Compiler.Defines != null && !Compiler.Defines.Contains(condAtt.ConditionString))
                return true;
            return false;

        }

        JsNode ProcessOmitCalls(out bool exit)
        {
            if (MethodAtt != null && MethodAtt.OmitCalls)
            {
                exit = true;
                if (Method.IsStatic() && !Method.IsExtensionMethod)
                    return null;
                if (Method.IsExtensionMethod && !Res.IsExtensionMethodInvocation)
                    return null;
                if (Res.Arguments.IsEmpty() && Res.TargetResult != null)
                    return VisitExpression(Res.TargetResult);
                return Visit(Res.Arguments[0]);
            }
            exit = false;
            return null;
        }

        void ProcessPrmBindings()
        {
            var list = Res.GetArgumentsForCall2();
            PrmBindings = list.Select(t => new PrmBinding { Binding = t }).ToList();
            if (Sk.OmitOptionalParameters(Method))
            {
                PrmBindings.RemoveAll(t => t.Binding.ArgResult == null);
            }
        }

        void ProcessTarget()
        {
            if (Res.TargetResult != null && !Member.IsStatic() && Member.SymbolKind != SymbolKind.Constructor) //TargetResult==null when ctor
            {
                JsTarget = VisitExpression(Res.TargetResult);
                if (JsMember.PreviousMember != null)
                    throw new NotSupportedException();
                JsMember.PreviousMember = JsTarget;
            }
        }

        void ProcessMember()
        {
            Member = Res.Member;
            Method = Member as IMethod;
            if (Method == null)
            {
                Property = Member as IProperty;
                if (Property != null)
                {
                    Method = Property.Getter;
                    Member = Method;
                }
            }
            MethodAtt = Method != null ? Sk.GetJsMethodAttribute(Method) : null;
        }

        JsIndexerAccessExpression ProcessIndexer()
        {
            if (Res.Member.SymbolKind == SymbolKind.Indexer && Sk.UseNativeIndexer((IProperty)Res.Member))
            {
                var node3 = new JsIndexerAccessExpression
                {
                    Member = Node2.Member,
                    Arguments = Node2.Arguments,
                };
                return node3;
            }
            return null;

        }

        JsBinaryExpression ProcessOmitDotOperator()
        {
            if (MethodAtt != null && MethodAtt.OmitDotOperator)
            {
                if (Node2.Member.NodeType == JsNodeType.MemberExpression && Node2.Arguments.Count <= 1 && MethodAtt.OmitParanthesis)
                {
                    var meNode = (JsMemberExpression)Node2.Member;
                    if (Node2.Arguments.Count == 1)
                        return new JsBinaryExpression { Left = meNode.PreviousMember, Operator = meNode.Name, Right = Node2.Arguments[0] };
                    else
                        return new JsBinaryExpression { Left = meNode.PreviousMember, Operator = meNode.Name, Right = new JsCodeExpression() }; //FIX FOR ISSUE 320
                }
                else
                {
                    Importer.Log.Warn(Res.GetFirstNode(), "TODO:OmitDotOperator is not supported in this syntax.");
                }
            }
            return null;

        }

        void ProcessExtensionImplementedInInstance()
        {
            if (Method != null && Method.IsExtensionMethod && Res.IsExtensionMethodInvocation && Sk.ExtensionImplementedInInstance(Method))
            {
                var arg = Node2.Arguments[0];
                Node2.Arguments.RemoveAt(0);
                if (JsMember.PreviousMember != null)
                    throw new NotImplementedException();
                JsMember.PreviousMember = arg;
            }
        }

        JsCodeExpression ProcessInlineCodeExpression()
        {
            if (MethodAtt != null && MethodAtt.InlineCodeExpression != null)
            {
                var inliner = new JsCodeInliner
                {
                    InlineCodeExpression = MethodAtt.InlineCodeExpression,
                };
                if (JsTarget != null)
                    inliner.ThisCode = JsTarget.ToJs();

                foreach (var arg in GenericArgs)
                {
                    if (arg.TypeParam == null)
                        continue;
                    var prm = new CodeInlinerParameter { Name = arg.TypeParam.Name, Code = arg.JsExpression.ToJs() };
                    inliner.Params.Add(prm);
                }

                foreach (var b in PrmBindings)
                {
                    var exp = b.JsCallResult;
                    var prm = new CodeInlinerParameter { Name = b.Binding.Parameter.Name, Code = exp.ToJs() };
                    inliner.Params.Add(prm);
                }
                var inlinedCode = inliner.Process();
                return Js.CodeExpression(inlinedCode);
            }
            return null;


        }

        void ProcessRemoveEmptyPreviousMemberName()
        {
            if (Node2.Member is JsMemberExpression)
            {
                var x = (JsMemberExpression)Node2.Member;
                if (x.Name.IsNullOrEmpty() && JsMember.PreviousMember != null)
                    Node2.Member = x.PreviousMember;

            }
        }

        void ProcessGenericMethodArgs()
        {
            GenericArgs = new List<GenericArg>();
            if (Method != null && Method is SpecializedMethod && !Sk.IgnoreGenericMethodArguments(Method))
            {
                if (Method.IsConstructor)
                {
                    var ce = Method.DeclaringType as ParameterizedType;
                    if (ce != null)
                        GenericArgs.AddRange(ce.TypeArguments.Select(t => new GenericArg { JsExpression = SkJs.EntityTypeRefToMember(t, true), Arg = t }).ToList());
                }
                else
                {
                    var sme = (SpecializedMethod)Method;
                    var genericMethodArgs = sme.TypeArguments.Select(t => new GenericArg { JsExpression = SkJs.EntityTypeRefToMember(t, true), Arg = t }).ToList();
                    var i = 0;
                    foreach (var z in Method.TypeParameters)
                    {
                        if (i >= genericMethodArgs.Count)
                            continue;
                        genericMethodArgs[i].TypeParam = z;
                        i++;
                    }
                    GenericArgs.AddRange(genericMethodArgs);
                }
                var jsArgs = GenericArgs.Select(t => t.JsExpression).ToList();
                if (Node2.Arguments == null)
                    Node2.Arguments = new List<JsExpression>(jsArgs);
                else
                    Node2.Arguments.InsertRange(0, jsArgs);
            }
        }

        void ProcessArgsCustomization()
        {
            if (MethodAtt != null)
            {
                if (MethodAtt.OmitParanthesis)
                    Node2.OmitParanthesis = true;
                if (Node2.Arguments == null)
                    Node2.Arguments = new List<JsExpression>();
                if (MethodAtt.InsertArg2 != null)
                    Node2.Arguments.InsertOrAdd(2, new JsCodeExpression { Code = MethodAtt.InsertArg2.ToString() });
                if (MethodAtt.InsertArg1 != null)
                    Node2.Arguments.InsertOrAdd(1, new JsCodeExpression { Code = MethodAtt.InsertArg1.ToString() });
                if (MethodAtt.InsertArg0 != null)
                    Node2.Arguments.InsertOrAdd(0, new JsCodeExpression { Code = MethodAtt.InsertArg0.ToString() });
                Node2.OmitCommas = MethodAtt.OmitCommas;
                Node2.ArgumentsPrefix = MethodAtt.ArgumentsPrefix;
                Node2.ArgumentsSuffix = MethodAtt.ArgumentsSuffix;
                if (MethodAtt.InstanceImplementedAsExtension)
                {
                    var ext = (JsMemberExpression)Node2.Member;
                    Node2.Arguments.Insert(0, ext.PreviousMember);
                    ext.PreviousMember = null;
                }
            }
        }

        void ProcessNativeParams()
        {
            if (Sk.IsNativeParams(Method))
            {
                var binding = PrmBindings.Where(t => t.Binding.Parameter.IsParams).FirstOrDefault();
                if (binding != null)
                {
                    if (binding.Binding.CallResult is ArrayCreateResolveResult)
                    {
                        var arrayRes = (ArrayCreateResolveResult)binding.Binding.CallResult;
                        PrmBindings.Remove(binding);
                        if (arrayRes.InitializerElements.IsNotNullOrEmpty())
                        {
                            foreach (var init in arrayRes.InitializerElements)
                            {
                                var b = binding.Binding.Clone();
                                b.CallResult = init;
                                PrmBindings.Add(new PrmBinding { Binding = b, JsCallResult = binding.JsCallResult });
                            }
                        }
                    }
                    else
                    {
                        Importer.Log.Warn(Res.GetFirstNode(), "Invalid params parameter passed to method with NativeParams=true");
                    }

                }
            }
        }

        void TransformIntoBaseMethodCallIfNeeded(CSharpInvocationResolveResult res, JsInvocationExpression node2)
        {
            var target = res.TargetResult as ThisResolveResult;
            if (target != null && target.CausesNonVirtualInvocation) //base.
            {
                //var info = res.GetInfo();
                //var node = info.Nodes.FirstOrDefault();
                var ce = target.Type;// node.FindThisEntity();
                if (ce != null && Sk.IsExtJsType(ce.GetDefinitionOrArrayType()))
                {
                    node2.Member = Js.This().Member("callParent");
                    if (node2.Arguments.IsNotNullOrEmpty())
                        node2.Arguments = new List<JsExpression> { Js.NewJsonArray(node2.Arguments.ToArray()) };
                    //var me2 = (node2.Member as JsMemberExpression);
                    //me2.Name = "callParent";
                    return;

                }
                IMethod me2;
                var me = res.Member;
                if (me is IProperty)
                    me2 = ((IProperty)me).Getter;
                else if (me is IMethod)
                    me2 = (IMethod)res.Member;
                else
                    throw new Exception("Can't resolve method from member: " + res.Member);
                var member = SkJs.EntityMethodToJsFunctionRef(me2);
                member = member.Member("call");
                node2.Member = member;
                if (node2.Arguments == null)
                    node2.Arguments = new List<JsExpression>();
                node2.Arguments.Insert(0, Js.This());

            }
        }

        void ProcessByRefs1()
        {
            ByRefs = new List<ByReferenceResolveResult>();
            ByRefIndexes = new List<int>();
            RefToRefs = new List<int>();
            var c = 0;
            foreach (var bin in PrmBindings)
            {
                var binding = bin.Binding;
                var byRef = binding.CallResult as ByReferenceResolveResult;
                if (byRef == null)
                {
                    c++;
                    continue;
                }
                var x = byRef.ElementResult as LocalResolveResult;
                if (x != null && x.Variable != null && x.Variable.Type.Kind == TypeKind.ByReference)
                {
                    if (binding.Parameter.IsRef || binding.Parameter.IsOut)
                        RefToRefs.Add(c);
                    c++;
                    continue;
                }
                ByRefs.Add(byRef);
                ByRefIndexes.Add(c);
                c++;
            }
        }

        void ProcessByRefs2()
        {
            var c = 0;
            for (var i = 0; i < Node2.Arguments.Count; i++)
            {
                JsMemberExpression jsmex = Node2.Arguments[i] as JsMemberExpression;
                if (jsmex != null)
                {
                    if (RefToRefs.Contains(i))
                    {
                        Node2.Arguments[i] = jsmex.PreviousMember; //remove the .Value ref wrapper
                    }
                    else if (ByRefIndexes.Contains(i))
                    {
                        Node2.Arguments[i] = Js.Member(RefIndexToName(c));
                        c++;
                    }
                }
            }
        }

        void ProcessByRefs3()
        {
            if (ByRefs.IsNotNullOrEmpty())
            {
                var func = Js.Function();

                //It must assigned to a temporary variable, because typed arrays do not acceppt json.
                for (var i = 0; i < ByRefs.Count; i++)
                {
                    var byRef = ByRefs[i];
                    func.Add(Js.Var(RefIndexToName(i), Js.Json().Add("Value", VisitExpression(byRef))).Statement());
                }

                func.Add(Js.Var("$res", Node2).Statement());

                for (var i = 0; i < ByRefs.Count; i++)
                {
                    var byRef = ByRefs[i];
                    func.Add(Js.Assign(VisitExpression(byRef), Js.Member(Js.Member(RefIndexToName(i)), "Value")).Statement());
                }

                func.Add(Js.Return(Js.Member("$res")));
                Node2 = Importer.WrapFunctionAndInvoke(Res, func);
            }
        }

        static string RefIndexToName(int idx)
        {
            return "$" + (idx + 1).ToString();
        }

        #endregion

        #region Invocation as ctor

        public JsNode VisitInvocationResolveResultAsCtor(CSharpInvocationResolveResult res)
        {
            if (res.Type.Kind == TypeKind.Delegate)
                return Visit(res.Arguments.Single());
            var me = (IMethod)res.Member;
            var meAtt = Sk.GetJsMethodAttribute(me);
            var ce = me.GetDeclaringTypeDefinition();
            var att = ce == null ? null : ce.GetJsTypeAttribute();
            if (att != null && att.Mode == JsMode.Json && (meAtt == null || meAtt.Name == null))
            {
                var jtfn = ce == null ? null : Sk.GetJsonTypeFieldName(ce);
                var node2 = VisitInvocationResolveResult(res);
                var json = Importer.InitializersToJson(res.InitializerStatements, res.Type);
                if (jtfn != null)
                {
                    var x = json as JsJsonObjectExpression;
                    if (x != null)
                    {
                        x.Add(jtfn, SkJs.EntityTypeRefToMember(res.Type));
                    }
                }
                return json;
            }
            else
            {
                var invokeExp = (JsInvocationExpression)VisitInvocationResolveResult(res);
                var newExp = new JsNewObjectExpression { Invocation = invokeExp };
                JsExpression finalExp;
                if (meAtt != null && meAtt.OmitNewOperator)
                    finalExp = invokeExp;
                else
                    finalExp = newExp;

                if (meAtt != null && meAtt.JsonInitializers)
                {
                    var json = Importer.InitializersToJson(res.InitializerStatements, res.Type);
                    invokeExp.Arguments.Add(json);
                }
                else if (res.InitializerStatements.IsNotNullOrEmpty())
                {
                    var func = Js.Function();

                    var inits2 = res.InitializerStatements.Select(t => Visit(t)).ToList();
                    var init1 = res.InitializerStatements[0];

                    var target = AstNodeConverter.FindInitializedObjectResolveResult(res);// ((init1 as OperatorResolveResult).Operands[0] as MemberResolveResult).TargetResult as InitializedObjectResolveResult;
                    var varName = Importer.Initializers[target];
                    func.Add(Js.Var(varName, finalExp).Statement());

                    foreach (var init in inits2)
                    {
                        var exp = ((JsExpression)init);
                        func.Add(exp.Statement());
                    }
                    func.Add(Js.Return(Js.Member(varName)));
                    finalExp = Importer.WrapFunctionAndInvoke(res, func);
                }

                return finalExp;
            }

        }

        #endregion

        #region Helper Classes

        class GenericArg
        {
            public JsExpression JsExpression { get; set; }
            public IType Arg { get; set; }

            public ITypeParameter TypeParam { get; set; }
        }
        class PrmBinding
        {
            public ParameterBinding Binding { get; set; }

            public JsExpression JsCallResult { get; set; }
        }
        #endregion

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
