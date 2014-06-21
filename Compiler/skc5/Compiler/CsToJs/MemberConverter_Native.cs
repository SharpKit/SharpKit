using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using SharpKit.Compiler;
using SharpKit.Utils;
using System.CodeDom.Compiler;
using System.IO;
using Mirrored.SharpKit.JavaScript;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler.CsToJs
{
    class MemberConverter_Native : MemberConverter
    {

        public override JsNode _VisitClass(ITypeDefinition ce)
        {
            var unit = new JsUnit { Statements = new List<JsStatement>() };
            ExportTypeNamespace(unit, ce);
            var members = GetMembersToExport(ce);
            VisitToUnit(unit, members);
            var baseCe = ce.GetBaseTypeDefinition();
            if (baseCe != null && Sk.IsNativeType(baseCe) && !Sk.IsGlobalType(baseCe) && !Sk.OmitInheritance(ce))
            {
                unit.Statements.Add(Js.Member("$Inherit").Invoke(SkJs.EntityToMember(ce), SkJs.EntityToMember(baseCe)).Statement());
            }
            return unit;
        }

        public override JsNode _VisitEnum(ITypeDefinition ce)
        {
            var unit = new JsUnit { Statements = new List<JsStatement>() };
            ExportTypeNamespace(unit, ce);
            var json = VisitEnumToJson(ce);
            var typeName = GetJsTypeName(ce);
            var st = Js.Members(typeName).Assign(json).Statement();

            unit.Statements.Add(st);
            return unit;
        }

        public override JsNode _VisitField(IField fld)
        {
            var init2 = GetCreateFieldInitializer(fld);
            var initializer = AstNodeConverter.VisitExpression(init2);
            if (initializer != null)
            {
                var member = ExportTypePrefix(fld.GetDeclaringTypeDefinition(), fld.IsStatic());
                member = member.Member(fld.Name);
                var st2 = member.Assign(initializer).Statement();
                return st2;
            }
            return null;
        }

        MemberConverter_Global CreateGlobalMemberConverter()
        {
            return new MemberConverter_Global
            {
                Compiler = Compiler,
                AssemblyName = AssemblyName,
                AstNodeConverter = AstNodeConverter,
                Log = Log,
                LongFunctionNames = LongFunctionNames
            };
        }

        public override JsNode ExportConstructor(IMethod ctor)
        {
            if (ctor.IsStatic)
            {
                var globaImporter = CreateGlobalMemberConverter();
                var node2 = globaImporter.ExportConstructor(ctor);
                return node2;
            }
            else
            {
                var func = (JsFunction)base.ExportConstructor(ctor);
                var fullname = GetJsTypeName(ctor.GetDeclaringTypeDefinition());
                if (fullname.Contains("."))
                {
                    var st = Js.Members(fullname).Assign(func).Statement();
                    return st;
                }
                else
                {
                    return Js.Var(fullname, func).Statement();
                }
            }
        }

        public override JsNode ExportMethod(IMethod me)
        {
            if (Sk.IsGlobalMethod(me))
            {
                return CreateGlobalMemberConverter().ExportMethod(me);
            }
            var node = base.ExportMethod(me);
            if (node == null)
                return node;
            if (!node.Is(JsNodeType.Function))
                return node;

            var func = (JsFunction)node;
            func.Name = null;
            var ce = me.GetDeclaringTypeDefinition();
            var member = ExportTypePrefix(ce, me.IsStatic);
            member = member.Member(SkJs.GetEntityJsName(me));
            if (LongFunctionNames)
                func.Name = SkJs.GetLongFunctionName(me);
            var st = member.Assign(func).Statement();
            return st;
        }

        public override JsNode _Visit(IProperty pe)
        {
            var list = GetAccessorsToExport(pe);
            if (Sk.IsNativeProperty(pe))
            {
                var statements = new List<JsStatement>();

                statements.AddRange(list.Select(ExportMethod).Cast<JsStatement>());

                var json = new JsJsonObjectExpression();
                foreach (var accessor in list)
                {
                    if (accessor == pe.Getter)
                        json.Add("get", ExportTypePrefix(pe.Getter.GetDeclaringTypeDefinition(), pe.IsStatic).Member("get_" + pe.Name));
                    if (accessor == pe.Setter)
                        json.Add("set", ExportTypePrefix(pe.Setter.GetDeclaringTypeDefinition(), pe.IsStatic).Member("set_" + pe.Name));
                }

                if (Sk.IsNativePropertyEnumerable(pe))
                    json.Add("enumerable", Js.True());

                var defineStatement = Js.Member("Object").Member("defineProperty").Invoke(
                    ExportTypePrefix(pe.GetDeclaringTypeDefinition(), pe.IsStatic),
                    Js.String(pe.Name),
                    json).Statement();

                statements.Add(defineStatement);

                return new JsUnit() { Statements = statements };
            }
            else
            {
                var list2 = list.Select(ExportMethod).Cast<JsStatement>().ToList();
                return new JsUnit { Statements = list2 };
            }
        }

        #region Utils
        void ExportNamespace(JsUnit unit, string ns)
        {
            var Writer = new StringWriter();
            if (ns.IsNotNullOrEmpty())
            {
                var tokens = ns.Split('.');
                for (var i = 0; i < tokens.Length; i++)
                {
                    var ns2 = tokens.Take(i + 1).StringJoin(".");
                    JsStatement st;
                    if (i == 0)
                        st = Js.Var(ns2, Js.Json()).Statement();
                    else
                        st = Js.Member(ns2).Assign(Js.Json()).Statement();
                    var st2 = Js.If(Js.Typeof(Js.Member(ns2)).Equal(Js.String("undefined"))).Then(st);
                    unit.Statements.Add(st2);
                    st2.AddAnnotation(new NamespaceVerificationAnnotation { Namespace = ns2 });//.Ex(true).NamespaceVerification = ns2;
                }
            }
        }
        void ExportTypeNamespace(JsUnit unit, ITypeDefinition ce)
        {
            var name = GetJsTypeName(ce);
            if (name.IsNotNullOrEmpty() && name.Contains("."))
            {
                var ns = name.Split('.');
                ns = ns.Take(ns.Length - 1).ToArray();
                ExportNamespace(unit, ns.StringConcat("."));
            }
        }
        JsMemberExpression ExportTypePrefix(ITypeDefinition ce, bool isStatic)
        {
            var me = Js.Members(GetJsTypeName(ce));
            if (!isStatic)
                me = me.MemberOrSelf(Sk.GetPrototypeName(ce));
            return me;
        }

        #endregion

    }

}
