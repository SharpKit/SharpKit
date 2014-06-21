using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using SharpKit.Compiler;
using SharpKit.Utils;
using System.CodeDom.Compiler;
using Mirrored.SharpKit.JavaScript;
using System.Diagnostics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.CSharp.Resolver;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Extensions;
using SharpKit.Compiler.CsToJs;

namespace SharpKit.Compiler.CsToJs
{
    class MemberConverter
    {

        public bool LongFunctionNames { get; set; }
        public AstNodeConverter AstNodeConverter { get; set; }
        public string AssemblyName { get; set; }
        public CompilerLogger Log { get; set; }
        public CompilerTool Compiler { get; set; }
        int VisitDepth;
        const int MaxVisitDepth = 100;

        #region _Visit
        protected virtual JsNode _Visit(ITypeDefinition ce)
        {
            if (CompilerConfiguration.Current.EnableLogging)
            {
                Log.Debug("JsTypeImporter: Visit Type: " + ce.ToString());
            }
            JsNode node;
            if (ce.Kind == TypeKind.Class)
            {
                node = _VisitClass(ce);
            }
            else if (ce.Kind == TypeKind.Interface)
            {
                node = _VisitInterface(ce);
            }
            else if (ce.Kind == TypeKind.Delegate)
            {
                node = _VisitDelegate(ce);
            }
            else if (ce.Kind == TypeKind.Struct)
            {
                node = _VisitStruct(ce);
            }
            else if (ce.Kind == TypeKind.Enum)
            {
                node = _VisitEnum(ce);
            }
            else
            {
                throw new NotImplementedException();
            }
            return node;
        }
        protected virtual JsNode _VisitStruct(ITypeDefinition ce)
        {
            throw new CompilerException(ce, "Member is not supported for export");
        }

        public virtual JsNode _VisitInterface(ITypeDefinition ce)
        {
            throw new CompilerException(ce, "Member is not supported for export");
        }
        public virtual JsNode _VisitClass(ITypeDefinition ce)
        {
            throw new CompilerException(ce, "Member is not supported for export");
        }
        public virtual JsNode _VisitDelegate(ITypeDefinition ce)
        {
            throw new CompilerException(ce, "Member is not supported for export");
        }
        public virtual JsNode _VisitEnum(ITypeDefinition ce)
        {
            throw new CompilerException(ce, "Member is not supported for export");
        }

        public virtual JsNode _Visit(IMethod me)
        {
            if (me.IsConstructor)
                return ExportConstructor(me);
            return ExportMethod(me);
        }
        public virtual JsNode _Visit(IField me)
        {
            return _VisitField(me);
        }
        public virtual JsNode _VisitField(IField me)
        {
            throw new CompilerException(me, "Member is not supported for export");
        }
        public virtual JsNode _Visit(IEvent me)
        {
            if (me.DeclaringType.Kind == TypeKind.Interface)
                return null;
            var list2 = new JsNodeList { Nodes = new List<JsNode>() };
            if (me.AddAccessor != null)
                list2.Nodes.Add(Visit(me.AddAccessor));
            if (me.RemoveAccessor != null)
                list2.Nodes.Add(Visit(me.RemoveAccessor));
            return list2;
        }
        public virtual JsNode _Visit(IProperty me)
        {
            throw new CompilerException(me, "Member is not supported for export");
        }


        /// <summary>
        /// Generates backing fields for automatic properties, and fake fields for properties who are defined as fields
        /// </summary>
        /// <param name="ce"></param>
        /// <returns></returns>
        protected IEnumerable<IField> GeneratePropertyFields(ITypeDefinition ce, bool isStatic)
        {
            //var list = new List<IField>();
            foreach (var pe in ce.GetProperties(t => t.IsStatic == isStatic, GetMemberOptions.IgnoreInheritedMembers))
            {
                if (!Sk.IsJsExported(pe))
                    continue;
                if (Sk.IsNativeField(pe))
                    yield return GenerateFakeField(pe);
                else if (pe.IsAutomaticProperty())
                    yield return GenerateBackingField(pe);
            }
            //return list;

        }

        protected IEnumerable<IMember> GetExportedDeclaredAndGeneratedFields(ITypeDefinition ce, bool isStatic)
        {
            foreach (var pe in ce.GetFields(t => t.IsStatic == isStatic, GetMemberOptions.IgnoreInheritedMembers))
            {
                if (!Sk.IsJsExported(pe))
                    continue;
                yield return pe;
            }

            foreach (var pe in ce.GetEvents(t => t.IsStatic == isStatic, GetMemberOptions.IgnoreInheritedMembers))
            {
                if (!Sk.IsJsExported(pe))
                    continue;
                yield return pe;
            }

            foreach (var fe in GeneratePropertyFields(ce, isStatic))
                yield return fe;
        }

        protected FakeField GenerateFakeField(IProperty pe)
        {
            return new FakeField
            {
                Name = pe.Name,
                DeclaringTypeDefinition = pe.DeclaringTypeDefinition,
                DeclaringType = pe.DeclaringType,
                BodyRegion = pe.BodyRegion,
                Region = pe.Region,
                Type = pe.ReturnType,
                IsStatic = pe.IsStatic,
            };
        }

        protected IField GenerateBackingField(IProperty pe)
        {
            var field = GenerateFakeField(pe);
            field.Name = "_" + SkJs.GetEntityJsName(pe);
            return field;
        }
        #endregion

        #region ExportMember

        public virtual JsNode ExportConstructor(IMethod ctor)
        {
            var ctorName = SkJs.GetEntityJsName(ctor);
            var func = new JsFunction { Parameters = new List<string>() };

            func.Block = ExportConstructorBody(ctor);
            ExportConstructorParameters(ctor, func);
            return func;
        }
        protected JsBlock ExportConstructorBody(IMethod ctor)
        {
            var ctorNode = (ConstructorDeclaration)ctor.GetDeclaration();
            BlockStatement ccc = null;
            if (ctorNode != null)
                ccc = ctorNode.Body;
            //var ccc = ctor.GetDefinition();//.decl as CsConstructor;
            //var ccc = ctor.GetDefinition();
            var block2 = (JsBlock)AstNodeConverter.Visit(ccc);
            if (block2 == null)
                block2 = new JsBlock { Statements = new List<JsStatement>() };
            var ce = ctor.GetDeclaringTypeDefinition();
            var isClr = Sk.IsClrType(ce);
            var isPrototype = Sk.IsNativeType(ce);
            var statements = new List<JsStatement>();
            //instance fields initializations
            if (!Sk.InlineFields(ce))
            {
                var isGlobal = Sk.IsGlobalType(ce);
                var fields = GetExportedDeclaredAndGeneratedFields(ce, ctor.IsStatic);
                //var fields = ctor.GetDeclaringTypeDefinition().GetFields(null, GetMemberOptions.IgnoreInheritedMembers).Where(t => t.IsStatic() == ctor.IsStatic).ToList();
                //fields = fields.Where(ShouldExportField).ToList();
                //fields.AddRange(GeneratePropertyFields(ctor.DeclaringTypeDefinition, ctor.IsStatic));

                //var props = ctor.GetDeclaringTypeDefinition().GetProperties(null, GetMemberOptions.IgnoreInheritedMembers).Where(t => t.IsStatic() == ctor.IsStatic).ToList();
                //props = props.Where(t=>Sk.IsNativeField(t)).ToList();
                //props = props.Where(t => Sk.IsJsExported(t)).ToList();
                //var fieldsAndProperties = fields.Cast<IEntity>().Concat(props.Cast<IEntity>());
                var initializers = fields.Select(fe => ExportInitializer(fe, null, isGlobal, false)).Cast<JsStatement>().ToList();
                if (initializers.Contains(null))
                    Log.Warn("Some field initializers were not exported");
                statements.AddRange(initializers.Where(t => t != null));
            }

            if (!ctor.IsStatic)
            {
                //base/this ctor invocation
                var invocation = GetConstructorBaseOrThisInvocation2(ctor);
                if (invocation != null)
                {
                    var baseThisCe = invocation.Member.DeclaringType;
                    var isBaseClr = Sk.IsClrType(baseThisCe.GetDefinition());
                    var isBasePrototype = Sk.IsNativeType(baseThisCe.GetDefinition()) && !Sk.IsJsonMode(baseThisCe.GetDefinition()) && !Sk.IsGlobalType(baseThisCe.GetDefinition());//happens when prototype inherits from json
                    if (isBaseClr == isClr && isBasePrototype == isPrototype) //base and derived are both prototype, or both are clr
                    {
                        var newObjExp2 = AstNodeConverter.VisitExpression(invocation);
                        JsInvocationExpression invocation2;
                        if (newObjExp2 is JsNewObjectExpression)
                        {
                            var newObjExp = (JsNewObjectExpression)newObjExp2;
                            invocation2 = newObjExp.Invocation;
                        }
                        else if (newObjExp2 is JsInvocationExpression)
                        {
                            invocation2 = (JsInvocationExpression)newObjExp2;
                        }
                        else
                        {
                            throw new Exception("Unexpected node: " + newObjExp2);
                        }
                        if (Sk.IsExtJsType(ce))
                        {
                            var invocation3 = Js.This().Member("callParent").Invoke();
                            if (invocation2.Arguments.IsNotNullOrEmpty())
                                invocation3.Arguments = new List<JsExpression> { Js.NewJsonArray(invocation2.Arguments.NotNull().ToArray()) };
                            statements.Add(invocation3.Statement());
                        }
                        else
                        {
                            JsRefactorer.ToCallWithContext(invocation2, new JsThis());
                            statements.Add(invocation2.Statement());
                        }
                    }
                }
            }
            if (block2.Statements == null)
                block2.Statements = new List<JsStatement>();
            block2.Statements.InsertRange(0, statements);

            return block2;
        }


        void ExportConstructorParameters(IMethod ctor, JsFunction func)
        {
            var ce = ctor.GetDeclaringTypeDefinition();
            var list = new List<string>();
            if (!Sk.IgnoreTypeArguments(ce))
            {
                //danel
                var gprms = ce.TypeParameters.ToList();//.GetGenericArguments().Where(ga => ga.isGenericParam()).ToList();
                if (gprms.IsNotNullOrEmpty())
                {
                    var i = 0;
                    foreach (var gprm in gprms)
                    {
                        func.Parameters.Add(gprm.Name);
                        if (!ctor.IsStatic && func.Block != null)
                        {
                            func.Block.Statements.Insert(i, Js.This().Member(gprm.Name).Assign(Js.Member(gprm.Name)).Statement());
                            i++;
                        }
                    }
                }
            }
            var prms = ctor.Parameters;
            if (prms != null)
            {
                func.Parameters.AddRange(prms.Select(t => t.Name));
            }
        }
        public virtual JsNode ExportMethod(IMethod me)
        {
            var jma = Sk.GetJsMethodAttribute(me);
            if (jma != null && jma.GlobalCode)
            {
                var block = ExportMethodBody(me);
                return new JsUnit { Statements = block.Statements };
            }
            var func = new JsFunction();
            func.Parameters = ExportMethodParameters(me);
            func.Name = SkJs.GetEntityJsName(me);
            func.Block = ExportMethodBody(me);
            func = ApplyYield(func);
            return func;
        }
        protected JsFunction ApplyYield(JsFunction func)
        {
            if (AstNodeConverter.SupportClrYield && func.Block.Descendants().OfType<JsYieldStatement>().FirstOrDefault() != null)
            {
                var yielder = new YieldRefactorer { BeforeFunction = func };
                yielder.Process();
                return yielder.AfterFunction;
            }
            return func;
        }
        protected JsBlock ExportMethodBody(IMethod me)
        {
            if (CompilerConfiguration.Current.EnableLogging)
            {
                Log.Debug("JsTypeImporter: Visit Method: " + me.ToString());
            }
            var nativeCode = Sk.GetNativeCode(me);
            if (nativeCode != null)
            {
                var block = Js.Block().Add(Js.CodeStatement(nativeCode)); //TODO: double semicolon?
                return block;
            }
            var def = me.GetDefinition();
            if (def == null || def.IsNull)
            {
                if (me.IsAutomaticEventAccessor())
                {
                    if (me.IsEventAddAccessor())
                    {
                        var node = GenerateAutomaticEventAccessor((IEvent)me.GetOwner(), false);
                        return node.Block;
                    }
                    else if (me.IsEventRemoveAccessor())
                    {
                        var node = GenerateAutomaticEventAccessor((IEvent)me.GetOwner(), true);
                        return node.Block;
                    }
                }
                else if (me.IsAutomaticPropertyAccessor())
                {
                    var bf = Js.Member("_" + SkJs.GetEntityJsName(me.AccessorOwner));
                    if (!me.IsStatic)
                        bf.PreviousMember = Js.This();
                    else if (!Sk.IsGlobalMethod(me))
                        bf.PreviousMember = SkJs.EntityToMember(me.DeclaringTypeDefinition);
                    if (me.IsGetter())
                        return Js.Block().Add(Js.Return(bf));
                    else
                        return Js.Block().Add(bf.Assign(Js.Member("value")).Statement());
                }
                return null;
            }
            var block2 = (JsBlock)AstNodeConverter.Visit(def);
            if (def.Descendants.OfType<YieldReturnStatement>().FirstOrDefault() != null)
            {
                if (!AstNodeConverter.SupportClrYield)
                {
                    if (block2.Statements == null)
                        block2.Statements = new List<JsStatement>();
                    block2.Statements.Insert(0, Js.Var("$yield", Js.NewJsonArray()).Statement());
                    block2.Statements.Add(AstNodeConverter.GenerateYieldReturnStatement(me));
                }
            }
            return block2;
        }
        protected List<string> ExportMethodParameters(IMethod me)
        {
            var list = new List<string>();
            if (!Sk.IgnoreGenericMethodArguments(me) && me.GetGenericArguments().Count() > 0)
            {
                list.AddRange(me.GetGenericArguments().Select(t => t.Name));
            }
            //if (me.Parameters.Where(t => t.IsOut || t.IsRef).FirstOrDefault() != null)
            //{
            //    throw new CompilerException(me, "Out and ref parameters are not supported");
            //}
            list.AddRange(me.Parameters.Select(t => t.Name));
            return list;
        }

        #endregion

        #region ShouldExport

        protected bool ShouldExportMember(IEntity entity)
        {
            switch (entity.SymbolKind)
            {
                case SymbolKind.Field: return ShouldExportField((IField)entity);
                //case EntityType.ent_constant: return ShouldExportConstant((IConst)entity);
                case SymbolKind.Event: return ShouldExportEvent((IEvent)entity);
                case SymbolKind.Property:
                case SymbolKind.Indexer:
                    var pe = (IProperty)entity;
                    return ShouldExportProperty(pe);

                case SymbolKind.Constructor: return ShouldExportConstructor((IMethod)entity);
                case SymbolKind.Method: return ShouldExportMethod((IMethod)entity);
                case SymbolKind.Operator: return ShouldExportMethod((IMethod)entity);

            }
            return false;
        }

        protected bool ShouldExportProperty(IProperty pe)
        {
            if (pe.IsIndexer)
                return !pe.IsExplicitInterfaceImplementation;
            if (pe.IsExplicitInterfaceImplementation)
                return false;
            var att = pe.GetMetadata<JsPropertyAttribute>();
            if (att != null && !att.Export)
                return false;
            if (Sk.IsNativeField(pe))
                return false;
            //{

            //    if (Sk.InlineFields(pe.GetDeclaringTypeDefinition()))
            //        return true;
            //    return false;
            //}
            return true;
        }

        protected bool ShouldExportEvent(IEvent ev)
        {
            return !ev.IsExplicitInterfaceImplementation && Sk.IsJsExported(ev);
        }

        protected bool ShouldExportField(IField fe)
        {
            var att = fe.GetMetadata<JsFieldAttribute>();
            if (att != null && att._Export != null)
                return att.Export;
            return true;
        }
        protected bool ShouldExportConstant(IField fe)
        {
            var att = fe.GetMetadata<JsFieldAttribute>();
            if (att != null && att._Export != null)
                return att.Export;
            return true;
        }

        protected bool ShouldExportMethod(IMethod mde)
        {
            var ret = !mde.IsExplicitInterfaceImplementation;
            if (Sk.NewInterfaceImplementation) ret = true;

            if (!Sk.NewInterfaceImplementation)
            {
                var body = mde.GetDeclarationBody();
                if (body == null || body.IsNull)
                    ret = false;
            }
            if (mde.IsAutomaticAccessor())
                ret = false;
            if (mde.GetOwner() != null)
                return false;
            var att = mde.GetMetadata<JsMethodAttribute>();
            if (att != null && !att.Export)
                return false;
            return ret;
        }

        protected bool ShouldExportConstructor(IMethod ctor)
        {
            var att = ctor.GetMetadata<JsMethodAttribute>();
            if (att != null && !att.Export)
                return false;
            if (ctor.IsGenerated(Compiler.Project) && Sk.OmitDefaultConstructor(ctor.GetDeclaringTypeDefinition()))
                return false;
            return true;
        }

        bool ShouldExportMethodBody(IMethod me)
        {
            return (Sk.IsJsExported(me) && me.GetMethodDeclaration() != null && !me.IsAnonymousMethod());
        }



        #endregion

        #region Field initializers


        public ResolveResult GetCreateInitializer(IMember member)
        {
            if (member is IField)
                return GetCreateFieldInitializer((IField)member);
            if (member is IEvent)
                return GetCreateEventInitializer((IEvent)member);
            throw new NotImplementedException("Member type not supported");

            //if (fieldOrProperty.EntityType == EntityType.Field)
            //    return GetCreateFieldInitializer((IField)fieldOrProperty);
            //else if (fieldOrProperty.EntityType == EntityType.Property)
            //{
            //    return GetDefaultValueExpression(((IProperty)fieldOrProperty).ReturnType);
            //}
            //else
            //    throw new NotSupportedException();
        }

        MemberResolveResult AccessSelfFieldOrProperty(IMember fieldOrProperty)
        {
            //if (fieldOrProperty.EntityType == EntityType.Field)
            return fieldOrProperty.AccessSelfForceNonConst();
            //else if (fieldOrProperty.EntityType == EntityType.Property)
            //    return ((IProperty)fieldOrProperty).AccessSelf();
            throw new NotSupportedException();
        }

        public JsNode ExportInitializer(IMember fieldOrProperty, BlockStatement ccBlock, bool isGlobal, bool isNative = false)
        {
            var initializer = GetCreateInitializer(fieldOrProperty);
            var init2 = AccessSelfFieldOrProperty(fieldOrProperty).Assign(initializer);

            var jsInit = AstNodeConverter.VisitExpression(init2);
            if (isGlobal)
            {
                //danel HACK
                var st = new JsPreUnaryExpression { Operator = "var ", Right = jsInit }.Statement();
                return st;
            }
            else if (isNative)
            {
                var st = jsInit.Statement();
                return st;
            }
            else //clr
            {
                var st = jsInit.Statement();
                return st;
            }
        }

        public ResolveResult GetDefaultValueExpression(IType typeRef)
        {
            if (typeRef.Kind == TypeKind.Struct || typeRef.Kind == TypeKind.Enum)
                return GetValueTypeInitializer(typeRef, Compiler.Project);
            return Cs.Null();
        }

        public ResolveResult GetCreateFieldInitializer(IField fe)
        {
            Expression initializer = null;
            //var decl = (FieldDeclaration)fe.GetDeclaration();
            //if (decl != null)
            //{
            //    if (decl.Variables.Count > 1)
            //        throw new CompilerException(fe, "Multiple field declarations is not supported: " + fe.FullName);
            //    var variable = decl.Variables.FirstOrDefault();

            //    initializer = variable.Initializer;
            //}
            //var df = fe as DefaultResolvedField;
            //if (df != null)
            //{
            //    var duf = (DefaultUnresolvedField)df.UnresolvedMember;
            //    initializer = ((VariableInitializer)duf.Initializer).Initializer;
            //}

            initializer = fe.GetInitializer();

            if (initializer == null || initializer.IsNull)
            {
                var res = GetDefaultValueExpression(fe.Type);
                if (res == null)
                {
                    Log.Warn(fe, "Can't initialize field, initializing to null: " + fe.FullName);
                    return Cs.Null();
                }
                return res;
            }
            else
            {
                var res = initializer.Resolve();
                return res;
            }
        }

        public ResolveResult GetCreateEventInitializer(IEvent fe)
        {
            return Cs.Null();
        }

        public static ResolveResult GetValueTypeInitializer(IType ce, NProject Project)
        {
            var fullName = SkJs.GetEntityJsName(ce);
            if (ce.FullName == "System.Nullable")
                return Cs.Null();
            if (ce is ITypeDefinition)
            {
                var def = (ITypeDefinition)ce;
                if (def.KnownTypeCode != KnownTypeCode.None)
                {
                    if (def.KnownTypeCode == KnownTypeCode.Boolean)
                    {
                        return Cs.Value(false, Project);
                    }
                    else if (def.KnownTypeCode == KnownTypeCode.Char)
                    {
                        return Cs.Value('\0', Project);
                    }
                    else if (def.KnownTypeCode == KnownTypeCode.SByte ||
                        def.KnownTypeCode == KnownTypeCode.Int16 ||
                        def.KnownTypeCode == KnownTypeCode.Int32 ||
                        def.KnownTypeCode == KnownTypeCode.Int64 ||
                        def.KnownTypeCode == KnownTypeCode.UInt16 ||
                        def.KnownTypeCode == KnownTypeCode.UInt32 ||
                        def.KnownTypeCode == KnownTypeCode.UInt64 ||
                        def.KnownTypeCode == KnownTypeCode.Byte ||
                        def.KnownTypeCode == KnownTypeCode.Decimal ||
                        def.KnownTypeCode == KnownTypeCode.Double ||
                        def.KnownTypeCode == KnownTypeCode.Single
                        )
                    {
                        return Cs.Value(0, Project);
                    }
                }
            }
            if (ce.Kind == TypeKind.Enum)
            {
                var en = ce;
                var enumMembers = en.GetFields();
                var defaultEnumMember = enumMembers.Where(t => (t.ConstantValue is int) && (int)t.ConstantValue == 0).FirstOrDefault() ?? enumMembers.FirstOrDefault();
                if (defaultEnumMember != null)
                    return defaultEnumMember.AccessSelf();//.Access().Member(c.CreateTypeRef(en), defaultEnumMember);
                else
                    return null;
            }
            else if (ce.GetEntityType().FullName == "System.DateTime")
            {
                var minDateFe = ce.GetFields(t => t.Name == "MinValue").First();
                return minDateFe.AccessSelf();// c.Member(c.Class(c.DateTimeType), minDateFe);
            }
            else
            {
                return Cs.New(ce);
            }
        }

        #endregion

        #region Utils

        protected JsExpression Serialize(object obj)
        {
            if (obj == null)
                return Js.Null();
            if (obj is JsExpression)
            {
                return (JsExpression)obj;
            }
            else if (obj is Dictionary<string, object>)
            {
                var obj2 = Js.Json();
                var dic = (Dictionary<string, object>)obj;
                dic.ForEach(pair => obj2.Add(pair.Key, Serialize(pair.Value)));
                return obj2;
            }
            else if (obj is IList)
            {
                var list = (IList)obj;
                var array = Js.NewJsonArray(list.Cast<object>().Select(Serialize).ToArray());
                return array;
            }
            else if (obj is Enum)
            {
                return Js.String(obj.ToString());
            }
            else if (obj is string || obj is bool || obj is int)
            {
                return Js.Value(obj);
            }
            else
            {
                var json = Js.Json();
                obj.GetType().GetProperties().ForEach(pe =>
                {
                    var value = pe.GetValue(obj, null);
                    if (value != null)
                        json.Add(pe.Name, Serialize(value));
                });
                return json;
            }
        }

        public List<IMember> GetMembersToExport(ITypeDefinition ce)
        {
            var members = ce.Members.Where(t => ShouldExportMember(t)).ToList();
            var fields = GeneratePropertyFields(ce, true).Concat(GeneratePropertyFields(ce, false));
            members = members.Concat(fields).ToList();

            var ctor = ce.Members.Where(t => t.SymbolKind == SymbolKind.Constructor && !t.IsStatic).FirstOrDefault();
            if (ctor == null && !Sk.OmitDefaultConstructor(ce))
            {
                ctor = GenerateDefaultConstructor(ce);
                if (ctor != null)
                    members.Add(ctor);
            }
            if (ctor != null && members.Contains(ctor))
            {
                var ctorIndex = 0;
                if (members.IndexOf(ctor) != ctorIndex)
                {
                    members.Remove(ctor);
                    members.Insert(ctorIndex, ctor);
                }
            }
            var inlineFields = Sk.InlineFields(ce);
            if (!inlineFields)
            {
                var vars = members.Where(t => t.SymbolKind == SymbolKind.Field).Cast<IField>();
                if (vars.Where(t => t.IsStatic()).FirstOrDefault() != null)
                {
                    var cctor = ce.GetConstructors(false, true).FirstOrDefault();
                    if (cctor == null)
                    {
                        cctor = CreateStaticCtor(ce);
                        members.Insert(1, cctor);
                    }
                }
                members.RemoveAll(t => t.SymbolKind == SymbolKind.Field);
            }

            return members;
        }

        public static IMethod CreateStaticCtor(ITypeDefinition ce)
        {
            var cctor = new FakeMethod(SymbolKind.Constructor)
            {
                Name = ".cctor",
                IsStatic = true,
                DeclaringTypeDefinition = ce,
                Region = ce.Region,
                BodyRegion = ce.BodyRegion,
                DeclaringType = ce,
            };
            //var cctor = (IMethod)new DefaultUnresolvedMethod
            //{
            //    Name = ".cctor",
            //    IsStatic = true,
            //    EntityType = EntityType.Constructor,
            //}.CreateResolved(ce.ParentAssembly.Compilation.TypeResolveContext.WithCurrentTypeDefinition(ce));
            return cctor;
        }

        /// <summary>
        /// Returns base type of a type, only if base type is Clr or Prototype
        /// </summary>
        /// <param name="ce"></param>
        /// <returns></returns>
        protected virtual IType GetBaseClassIfValid(ITypeDefinition ce, bool recursive)
        {
            var baseClass = ce.GetBaseType();
            while (baseClass != null)
            {
                if (Sk.IsClrType(baseClass.GetDefinition()) || (Sk.IsNativeType(baseClass.GetDefinition()) && !Sk.IsGlobalType(baseClass.GetDefinition())) || !recursive)
                    return baseClass;
                baseClass = baseClass.GetBaseType();
            }
            return null;
        }

        //InvocationExpression GetConstructorBaseOrThisInvocation(IMethod ctor)
        //{
        //    var ctorNode = (ConstructorDeclaration)ctor.GetDeclaration();
        //    InvocationExpression node = null;
        //    if (ctorNode != null && ctorNode.Initializer != null && !ctorNode.Initializer.IsNull)
        //    {
        //        var xxx = (CSharpInvocationResolveResult)ctorNode.Initializer.Resolve();
        //        //throw new NotImplementedException();
        //        //danel
        //        var baseCtor = xxx.Member;
        //        var id = new IdentifierExpression(baseCtor.Name);
        //        id.SetResolveResult(new MemberResolveResult(null, baseCtor));
        //        node = new InvocationExpression(id);
        //        node.SetResolveResult(xxx);
        //        //{ entity = ctorNode.invoked_method, Target = ctorNode.invoked_method.Access() };
        //        //node.SetResolveResult(
        //        // node.Arguments.AddRange(ctorNode.Initializer.Arguments);
        //    }
        //    else
        //    {
        //        var ce = ctor.GetDeclaringTypeDefinition();
        //        if (Sk.OmitInheritance(ce))
        //            return null;
        //        var baseType = GetBaseClassIfValid(ce, true);
        //        if (baseType != null)
        //        {
        //            var baseCtor = baseType.GetConstructor();
        //            if (baseCtor != null)
        //            {
        //                //danel
        //                //throw new NotImplementedException();
        //                var id = new IdentifierExpression(baseCtor.Name);
        //                id.SetResolveResult(new MemberResolveResult(null, baseCtor));
        //                node = new InvocationExpression(id);// { entity = baseCtor, expression = baseCtor.Access() };
        //                node.SetResolveResult(new CSharpInvocationResolveResult(null, baseCtor, null));

        //            }
        //        }
        //    }
        //    return node;
        //}

        InvocationResolveResult GetConstructorBaseOrThisInvocation2(IMethod ctor)
        {
            var ctorNode = (ConstructorDeclaration)ctor.GetDeclaration();
            InvocationResolveResult node = null;
            if (ctorNode != null && ctorNode.Initializer != null && !ctorNode.Initializer.IsNull)
            {
                var xxx = (CSharpInvocationResolveResult)ctorNode.Initializer.Resolve();
                return xxx;
            }
            else
            {
                var ce = ctor.GetDeclaringTypeDefinition();
                if (Sk.OmitInheritance(ce))
                    return null;
                var baseType = GetBaseClassIfValid(ce, true);
                if (baseType != null)
                {
                    var baseCtor = baseType.GetConstructors(t => t.Parameters.Count == 0, GetMemberOptions.IgnoreInheritedMembers).Where(t => !t.IsStatic).FirstOrDefault();
                    if (baseCtor != null)
                    {
                        return baseCtor.AccessSelf().Invoke();

                    }
                }
            }
            return node;
        }


        protected List<IMethod> GetAccessorsToExport(IProperty pe)
        {
            var list = new List<IMethod>();
            if (pe.IsAutomaticProperty() && !Sk.IsNativeField(pe))
            {
                list.Add(pe.Getter);
                list.Add(pe.Setter);
            }
            else
            {
                var exportGetter = (pe.Getter != null && !pe.Getter.GetDeclarationBody().IsNull);
                var exportSetter = (pe.Setter != null && !pe.Setter.GetDeclarationBody().IsNull);
                if (exportGetter)
                    list.Add(pe.Getter);
                if (exportSetter)
                    list.Add(pe.Setter);
            }
            list.RemoveAll(t => !Sk.IsJsExported(t));
            return list;
        }
        protected IMethod GenerateDefaultConstructor(ITypeDefinition ce)
        {
            var compilerGeneratedCtor = ce.GetConstructors(true, false).FirstOrDefault(t => t.GetDeclarationBody() == null); //TODO: where is this phantom ctor coming from??? (dan-el)
            if (compilerGeneratedCtor == null)
            {
                var cePart = ce.Parts.First();
                var me2 = new DefaultUnresolvedMethod(cePart, ".ctor");
                me2.SymbolKind = SymbolKind.Constructor;
                me2.IsSynthetic = true;
                me2.UnresolvedFile = cePart.UnresolvedFile;
                var x = new DefaultResolvedMethod(me2, Compiler.Project.Compilation.TypeResolveContext.WithCurrentTypeDefinition(ce));
                compilerGeneratedCtor = x;
            }
            return compilerGeneratedCtor;
        }
        protected string GetJsTypeName(ITypeDefinition ce)
        {
            return GetJsTypeName(ce.GetEntityTypeRef());
        }

        protected string GetJsTypeName(IType etr)
        {
            return SkJs.GetEntityJsName(etr);//.FixClassName(etr, true);
        }

        #endregion

        #region Visit
        JsMode GetJsMode(ITypeDefinition ce)
        {
            var isGlobal = Sk.IsGlobalType(ce);
            if (isGlobal)
                return JsMode.Global;
            var isNative = Sk.IsNativeType(ce);
            if (isNative)
                return JsMode.Prototype;
            return JsMode.Clr;
        }

        [DebuggerStepThrough]
        public JsNode Visit(IEntity node)
        {
            if (node == null)
                return null;
            VisitDepth++;
            if (VisitDepth > MaxVisitDepth)
                throw new Exception("StackOverflow imminent, depth>" + MaxVisitDepth);
            try
            {
                var node2 = UnsafeVisit(node);
                return node2;
            }
            catch (CompilerException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new CompilerException(node, "Error while processing node", e);
            }
            finally
            {
                VisitDepth--;
            }
        }

        public event Action<IEntity> BeforeVisitEntity;
        public event Action<IEntity, JsNode> AfterVisitEntity;
        public JsNode UnsafeVisit(IEntity me)
        {
            if (CompilerConfiguration.Current.EnableLogging)
            {
                Log.Debug("JsTypeImporter: Visit Entity: " + me.ToString());
            }
            if (BeforeVisitEntity != null)
                BeforeVisitEntity(me);
            JsNode node2 = null;
            switch (me.SymbolKind)
            {
                #region switch case
                //case EntityType.ent_anonymous_method:
                //    node2 = _Visit((CsEntityAnonymousMethod)me); break;
                //case EntityType.ent_block:
                //    node2 = _Visit((CsEntityBlock)me); break;
                //case EntityType.ent_block_variable:
                //    node2 = _Visit((CsEntityBlockVariable)me); break;
                case SymbolKind.TypeDefinition:
                    node2 = _Visit((ITypeDefinition)me); break;
                //case EntityType.ent_constant:
                //    node2 = _Visit((IConst)me); break;
                //case EntityType.ent_delegate:
                //    node2 = _Visit((IDelegate)me); break;
                //case EntityType.ent_enum:
                //    node2 = _Visit((CsEntityEnum)me); break;
                case SymbolKind.Event:
                    node2 = _Visit((IEvent)me); break;
                //case EntityType.ent_formal_parameter:
                //    node2 = _Visit((CsEntityFormalParameter)me); break;
                //case EntityType.ent_generic_param:
                //    node2 = _Visit((CsEntityGenericParam)me); break;
                //case EntityType.Interface:
                //    node2 = _Visit((IInterface)me); break;
                //case EntityType.ent_local_constant:
                //    node2 = _Visit((CsEntityLocalConstant)me); break;
                //case EntityType.ent_local_variable:
                //    node2 = _Visit((CsEntityLocalVariable)me); break;
                case SymbolKind.Method:
                case SymbolKind.Constructor:
                case SymbolKind.Operator:
                case SymbolKind.Accessor:
                    node2 = _Visit((IMethod)me); break;
                //case EntityType.ent_namespace:
                //    node2 = _Visit((CsEntityNamespace)me); break;
                case SymbolKind.Property:
                case SymbolKind.Indexer:
                    node2 = _Visit((IProperty)me); break;
                //case EntityType.ent_struct:
                //    node2 = _Visit((IStruct)me); break;
                case SymbolKind.Field:
                    node2 = _Visit((IField)me); break;
                #endregion
            }
            if (AfterVisitEntity != null)
                AfterVisitEntity(me, node2);
            return node2;
        }

        protected JsUnit VisitToUnit(List<IMember> list)
        {
            var unit = new JsUnit { Statements = new List<JsStatement>() };
            VisitToUnit(unit, list);
            return unit;
        }
        protected void VisitToUnit(JsUnit unit, List<IMember> list)
        {
            var nodes = list.Select(Visit).ToList();
            ImportToUnit(unit, nodes);
        }
        protected void ImportToUnit(JsUnit unit, List<JsNode> list)
        {
            foreach (var node in list)
            {
                if (node == null)
                    continue;
                JsStatement st = null;
                if (node is JsFunction)
                    st = ((JsFunction)node).Statement();
                else if (node is JsNodeList)
                    ImportToUnit(unit, ((JsNodeList)node).Nodes);
                else if (node is JsUnit)
                    unit.Statements.AddRange(((JsUnit)node).Statements);
                else
                    st = (JsStatement)node;
                if (st != null)
                    unit.Statements.Add(st);
            }
        }

        #endregion

        protected JsJsonObjectExpression VisitEnumToJson(ITypeDefinition ce)
        {
            bool valuesAsNames;
            Sk.UseJsonEnums(ce, out valuesAsNames);
            //var valuesAsNames = att != null && att.ValuesAsNames;
            var constants = ce.GetConstants().ToList();
            if (!valuesAsNames && constants.Where(t => t.ConstantValue == null).FirstOrDefault() != null)
            {
                var value = 0L;
                foreach (var c in constants)
                {
                    if (c.ConstantValue == null)
                        c.SetConstantValue(value);
                    else
                        value = Convert.ToInt64(c.ConstantValue);
                    value++;
                }
            }
            constants.RemoveAll(t => !Sk.IsJsExported(t));
            var json = new JsJsonObjectExpression { NamesValues = new List<JsJsonNameValue>() };
            json.NamesValues.AddRange(constants.Select(t => VisitEnumField(t, valuesAsNames)));
            return json;
        }

        JsJsonNameValue VisitEnumField(IField pe, bool valuesAsNames)
        {
            if (valuesAsNames)
            {
                return Js.JsonNameValue(pe.Name, Js.String(pe.Name));
            }
            else
            {
                return Js.JsonNameValue(pe.Name, Js.Value(pe.ConstantValue));
            }
        }

        protected virtual JsFunction GenerateAutomaticEventAccessor(IEvent ee, bool isRemove)
        {
            if (isRemove)
            {
                var remover = Js.Function("value").Add(Js.This().Member(ee.Name).Assign(Js.Member("$RemoveDelegate").Invoke(Js.This().Member(ee.Name), Js.Member("value"))).Statement());
                return remover;
            }
            var adder = Js.Function("value").Add(Js.This().Member(ee.Name).Assign(Js.Member("$CombineDelegates").Invoke(Js.This().Member(ee.Name), Js.Member("value"))).Statement());
            return adder;
        }

    }



}
