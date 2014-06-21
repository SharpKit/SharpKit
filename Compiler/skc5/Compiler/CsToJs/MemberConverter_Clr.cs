using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using ICSharpCode.NRefactory.TypeSystem;
using Mirrored.SharpKit.JavaScript;
using SharpKit.Utils;
using System.IO;
using System.Collections;
using System.Diagnostics;
using ICSharpCode.NRefactory.Semantics;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.Extensions;
using ICSharpCode.NRefactory.CSharp;

namespace SharpKit.Compiler.CsToJs
{
    class MemberConverter_Clr : MemberConverter
    {
        public MemberConverter_Clr()
        {
            VerifyJsTypesArrayStatement = Js.If(Js.Typeof(Js.Member("JsTypes")).Equal(Js.String("undefined"))).Then(Js.Var("JsTypes", Js.NewJsonArray()).Statement());
            VerifyJsTypesArrayStatement.AddAnnotation(new VerifyJsTypesArrayStatementAnnotation());
        }

        #region _Visit

        protected override JsNode _VisitStruct(ITypeDefinition ce)
        {
            return ExportClassStructInterface(ce);
        }
        public override JsNode _VisitInterface(ITypeDefinition ce)
        {
            return ExportClassStructInterface(ce);
        }
        public override JsNode _VisitClass(ITypeDefinition ce)
        {
            return ExportClassStructInterface(ce);
        }
        public override JsNode _VisitDelegate(ITypeDefinition ce)
        {
            var CurrentType = new JsClrType { Kind = JsClrTypeKind.Delegate };
            CurrentType.fullname = GetJsTypeName(ce);

            //Generate constructor
            var genericParams = new List<ITypeParameter>(ce.GetGenericArguments());
            var func = new JsFunction();
            func.Parameters = genericParams.Select(t => t.Name).ToList();
            func.Parameters.Add("obj");
            func.Parameters.Add("func");
            func.Block = Js.Block();
            foreach (var ga in genericParams)
            {
                func.Block.Add(Js.This().Member(ga.Name).Assign(Js.Member(ga.Name)).Statement());
            }
            func.Block.Add(Js.Members("System.MulticastDelegate.ctor.call").Invoke(Js.This(), Js.Member("obj"), Js.Member("func")).Statement());
            CurrentType.GetDefinition(false)["ctor"] = func;
            return OnAfterExportType(ce, CurrentType);
            //return func;

            //FullName',{ ShortName:function(T1,T2,T3,...,obj,func){this.T1=T1;....;this.construct(obj,func);},   })");
        }
        public override JsNode _VisitEnum(ITypeDefinition ce)
        {
            var CurrentType = new JsClrType { Kind = JsClrTypeKind.Enum };
            CurrentType.fullname = GetJsTypeName(ce);
            var json = VisitEnumToJson(ce);
            foreach (var item in json.NamesValues)
            {
                ////TODO: baseType (long, byte, etc...)
                //foreach (var pe in ce.GetConstants())
                //{
                //var name = pe.Name;
                CurrentType.GetDefinition(true)[item.Name.Name] = item.Value;
                //  }
            }
            return OnAfterExportType(ce, CurrentType);
        }

        public override JsNode _Visit(IProperty pe)
        {
            if (Sk.IsNativeField(pe))
            {
                if (Sk.InlineFields(pe.GetDeclaringTypeDefinition()))
                {
                    var fe = GenerateFakeField(pe);
                    var value = AstNodeConverter.Visit(GetCreateInitializer(fe));
                    return Js.JsonNameValue(pe.Name, (JsExpression)value);
                }
                throw new Exception();
            }
            else
            {
                var list2 = new JsNodeList { Nodes = new List<JsNode>() };
                var node2 = ExportPropertyInfo(pe);
                if (node2 != null)
                    list2.Nodes.Add(node2);
                var list = GetAccessorsToExport(pe);
                if (list.Count > 0)
                {
                    foreach (var accessor in list)
                    {
                        var pair = (JsJsonNameValue)ExportMethod(accessor);
                        list2.Nodes.Add(pair);
                    }
                }
                //else if (pe.IsAutomaticProperty())
                //{
                //    throw new NotImplementedException();
                //    //var def = CurrentType.GetDefinition(pe.IsStatic);
                //    var getter = Js.Code(String.Format("function(){{return this._{0};}}", pe.Name)); ;
                //    var setter = Js.Code(String.Format("function(value){{this._{0} = value;}}", pe.Name));
                //    list2.Nodes.Add(new JsJsonNameValue { Name = new JsJsonMember { Name = SkJs.GetEntityJsName(pe.Getter) }, Value = getter });
                //    list2.Nodes.Add(new JsJsonNameValue { Name = new JsJsonMember { Name = SkJs.GetEntityJsName(pe.Setter) }, Value = setter });
                //}
                return list2;
            }
        }

        public override JsNode _Visit(IEvent ee)
        {
            if (ee.DeclaringType.Kind == TypeKind.Interface)
                return null;
            var list2 = new JsNodeList { Nodes = new List<JsNode>() };
            if (ee.IsAutomatic(Compiler.Project))
            {
                var adder = GenerateAutomaticEventAccessor(ee, false);
                var remover = GenerateAutomaticEventAccessor(ee, true);
                //TODO:
                //var adder = Js.Code(String.Format("function(value){{this.{0}=CombineDelegates(this.{0}, value);}}", ee.Name));
                //var remover = Js.Code(String.Format("function(value){{this.{0}=RemoveDelegate(this.{0}, value);}}", ee.Name));
                list2.Nodes.Add(Js.JsonNameValue("add_" + ee.Name, adder));
                list2.Nodes.Add(Js.JsonNameValue("remove_" + ee.Name, remover));
            }
            else
            {
                var list = new List<IMethod>();
                if (ee.AddAccessor != null)
                {
                    if (ee.AddAccessor.Name == null)
                    {
                        throw new NotImplementedException();
                        //ee.AddAccessor.Name = "add_" + ee.Name;
                    }
                    list.Add(ee.AddAccessor);
                }
                if (ee.RemoveAccessor != null)
                {
                    if (ee.RemoveAccessor.Name == null)
                        throw new NotImplementedException();
                    //ee.RemoveAccessor.name = "remove_" + ee.name;
                    list.Add(ee.RemoveAccessor);
                }
                foreach (var node in list)
                {
                    var node2 = Visit(node);
                    //if (node2 == null)
                    //    throw new CompilerException(ee, "Export of event accessor returned null");
                    list2.Nodes.Add(node2);
                }

            }
            return list2;
        }


        public override JsNode _VisitField(IField fld)
        {
            var initializer = GetCreateFieldInitializer(fld);
            if (initializer == null)
                initializer = Cs.Null();
            var node = AstNodeConverter.Visit(initializer);
            return Js.JsonNameValue(fld.Name, (JsExpression)node);
        }


        #endregion

        #region Export
        public JsStatement VerifyJsTypesArrayStatement { get; set; }
        protected virtual JsUnit OnAfterExportType(ITypeDefinition ce, JsClrType jsType)
        {
            //HACK: backward
            if (jsType.Kind == JsClrTypeKind.Interface && jsType.baseTypeName.IsNullOrEmpty())
                jsType.baseTypeName = "System.Object";
            var unit = new JsUnit { Statements = new List<JsStatement>() };
            unit.Statements.Add(VerifyJsTypesArrayStatement.Clone());

            var json = (JsJsonObjectExpression)Serialize(jsType);
            var moveToEnd = json.NamesValues.Where(t => t.Name.Name.Contains("definition")).ToList();
            moveToEnd.ForEach(t => json.NamesValues.Remove(t));
            json.NamesValues.AddRange(moveToEnd);

            var ceVarName = GetJsTypeName(ce).Replace(".", "$");
            unit.Statements.Add(Js.Var(ceVarName, json).Statement());
            unit.Statements.Add(Js.Member("JsTypes").Member("push").Invoke(Js.Member(ceVarName)).Statement());
            return unit;
        }

        JsClrTypeKind GetJsTypeKind(ITypeDefinition ce)
        {
            if (ce.IsEnum())
                return JsClrTypeKind.Enum;
            else if (ce.IsInterface())
                return JsClrTypeKind.Interface;
            else if (ce.Kind == TypeKind.Struct)
                return JsClrTypeKind.Struct;
            return JsClrTypeKind.Class;
        }
        private JsNode ExportClassStructInterface(ITypeDefinition ce)
        {
            var jsType = new JsClrType();
            jsType.Kind = GetJsTypeKind(ce);
            jsType.fullname = GetJsTypeName(ce);
            var baseClass = GetBaseClassIfValid(ce, true);
            if (baseClass != null)
            {
                var baseClassName = GetJsTypeName(baseClass);
                if (baseClassName != jsType.fullname)   //avoid object:object inheritance recursion
                    jsType.baseTypeName = baseClassName;
            }
            var globalCode = new List<JsUnit>();
            if (!ce.IsInterface() || Sk.NewInterfaceImplementation)
            {
                var members = GetMembersToExport(ce);
                var inlineFields = Sk.InlineFields(ce);
                var cctor = ce.GetConstructors(false, true).FirstOrDefault();
                if (cctor == null && !inlineFields && ce.GetFields().Where(t => t.IsStatic).FirstOrDefault() != null)
                {

                    cctor = CreateStaticCtor(ce);
                    if (ShouldExportConstructor(cctor))
                        members.Insert(0, cctor);
                }

                var mappings = members.Select(t => new EntityToJsNode { JsNode = Visit(t), Entity = t }).ToList();

                //Remove GlobalCode=True members from mappings
                for (var i = mappings.Count - 1; i >= 0; i--)
                {
                    if (mappings[i].JsNode is JsUnit)
                    { //Maybe more clean: Another way could be to check the IEntity for GlobalCode=True
                        globalCode.Add((JsUnit)mappings[i].JsNode);
                        mappings.RemoveAt(i);
                    }
                }

                ImportToType(jsType, mappings);
            }
            ExportImplementedInterfaces(ce, jsType);
            jsType.assemblyName = AssemblyName;
            var atts = ExportCustomAttributes(ce);
            if (atts.IsNotNullOrEmpty())
            {
                if (jsType.customAttributes == null)
                    jsType.customAttributes = new JsArray<JsClrAttribute>();
                jsType.customAttributes.AddRange(atts);
            }
            var unit = OnAfterExportType(ce, jsType);
            globalCode.ForEach(t => unit.Statements.AddRange(t.Statements));
            return unit;
        }

        private void ImportToType(JsClrType ce, List<EntityToJsNode> mappings)
        {
            foreach (var map in mappings)
            {
                if (map.JsNode == null)
                    continue;
                var isStatic = map.Entity.IsStatic();
                var def = ce.GetDefinition(isStatic);
                List<JsJsonNameValue> pairs;
                if (map.JsNode is JsNodeList)
                    pairs = ((JsNodeList)map.JsNode).Nodes.Cast<JsJsonNameValue>().ToList();
                else
                    pairs = new List<JsJsonNameValue> { (JsJsonNameValue)map.JsNode };
                foreach (var pair in pairs)
                {
                    if (pair == null)
                        throw new CompilerException(map.Entity, "Export of entity is null");
                    var name = pair.Name.Name;
                    //if (def[name] != pair.Value)
                    //    throw new Exception();
                    def[name] = pair.Value;
                }
            }
        }

        private void ExportImplementedInterfaces(ITypeDefinition ce, JsClrType jsType)
        {
            var list = ce.DirectBaseTypes.Where(t => t.Kind == TypeKind.Interface).ToList();
            if (list.Count == 0)
                return;
            if (jsType.interfaceNames == null)
                jsType.interfaceNames = new JsArray<string>();
            jsType.interfaceNames.AddRange(list.Select(GetJsTypeName));
        }

        public override JsNode ExportMethod(IMethod me)
        {
            var node = base.ExportMethod(me);
            if (node == null)
                return node;
            if (!node.Is(JsNodeType.Function))
                return node;
            var func = (JsFunction)node;
            var funcName = func.Name;
            func.Name = null;
            if (LongFunctionNames)
                func.Name = SkJs.GetLongFunctionName(me);
            return Js.JsonNameValue(funcName, func);
        }

        public override JsNode ExportConstructor(IMethod ctor)
        {
            var ctorName = SkJs.GetEntityJsName(ctor);
            var func = base.ExportConstructor(ctor);
            //CurrentType.definition[ctorName] = func;
            return Js.JsonNameValue(ctorName, (JsExpression)func);
            //return func;
        }

        protected virtual JsNode ExportPropertyInfo(IProperty pe)
        {
            var name = SkJs.GetEntityJsName(pe) + "$$";
            var value = Js.String(pe.GetPropertyTypeRef().GetFullyQualifiedCLRName());
            //CurrentType.GetDefinition(pe.IsStatic)[name] = value;
            return Js.JsonNameValue(name, value);
        }

        JsStatement ExportDescription(string Description)
        {
            if (String.IsNullOrEmpty(Description))
                return null;
            string[] lines = Description.Split('\n');
            if (lines.Length > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("/// <summary>");
                foreach (string l in lines)
                {
                    sb.AppendLine("/// " + l.Replace("\r", ""));
                }
                sb.AppendLine("/// </summary>");
                return Js.CodeStatement(sb.ToString());
            }
            return null;
        }

        #endregion


        #region Export Attributes

        private List<JsClrAttribute> ExportCustomAttributes(ITypeDefinition ce)
        {
            var list = new List<JsClrAttribute>();
            list.AddRange(ExportAttributes(ce, ce.Attributes));
            foreach (var me in ce.GetMethods())
            {
                list.AddRange(ExportAttributes(me, me.Attributes));
            }
            foreach (var pe in ce.GetProperties())
            {
                list.AddRange(ExportAttributes(pe, pe.Attributes));
            }
            return list;
        }

        private List<JsClrAttribute> ExportAttributes(IEntity parent, IList<IAttribute> attributes)
        {
            var list = new List<JsClrAttribute>();
            if (attributes == null || attributes.Count == 0)
                return list;
            var list2 = attributes.Where(t =>
            {
                {
                    var attCtor = t.Constructor;
                    if (attCtor == null)
                    {
                        Log.Warn(t.GetParent(), "Cannot resolve attribute constructor");
                        return false;
                    }
                    var attType = attCtor.GetDeclaringTypeDefinition();
                    if (!Sk.IsJsExported(attType))
                        return false;
                }
                return true;
            }).ToList();
            if (list2.Count > 0)
                list.AddRange(list2.Select(t => ExportAttribute(parent, t)));
            return list;
        }

        private JsClrAttribute ExportAttribute(IEntity parent, IAttribute att)
        {
            var attCtor = att.Constructor;
            var attType = att.AttributeType;
            var target = GetAttributeTarget(parent, att);
            var jsAtt = new JsClrAttribute
            {
                targetType = target.Key,
                targetMemberName = target.Value,
                typeName = SkJs.GetEntityJsName(attType),
                ctorName = SkJs.GetEntityJsName(attCtor),
            };
            if (att.PositionalArguments.IsNotNullOrEmpty())
            {
                jsAtt.positionalArguments = new JsArray<object>();
                var args = att.PositionalArguments.Select(exp => ImportAttributeValue(exp)).ToList();
                jsAtt.positionalArguments.AddRange(args);
            }
            if (att.NamedArguments.IsNotNullOrEmpty())
            {
                jsAtt.namedArguments = new JsObject();
                foreach (var arg in att.NamedArguments)
                    jsAtt.namedArguments[arg.Key.Name] = ImportAttributeValue(arg.Value);
            }
            return jsAtt;

        }

        private JsNode ImportAttributeValue(ResolveResult res)
        {
            var node = AstNodeConverter.Visit(res);
            if (node.NodeType == JsNodeType.InvocationExpression)
            {
                var exp = (JsInvocationExpression)node;
                var me = exp.Member as JsMemberExpression;
                if (me != null && me.Name == "Typeof")
                    node = Js.Function().AddStatements(Js.Return(exp));
            }
            return node;
        }

        private KeyValuePair<string, string> GetAttributeTarget(IEntity parent, IAttribute att)
        {
            if (parent is ITypeDefinition)
            {
                return new KeyValuePair<string, string>("type", null);
            }
            else if (parent.SymbolKind == SymbolKind.Property)
            {
                return new KeyValuePair<string, string>("property", SkJs.GetEntityJsName(parent));
            }
            else if (parent.SymbolKind == SymbolKind.Method)
            {
                var method = (IMethod)parent;
                if (method.IsConstructor)
                {
                    return new KeyValuePair<string, string>("constructor", SkJs.GetEntityJsName(method));
                }
                else
                {
                    return new KeyValuePair<string, string>("method", SkJs.GetEntityJsName(method));
                }
            }
            throw new NotImplementedException("GetAttributeTarget no implemented for member: " + parent);
        }

        #endregion


    }



}
