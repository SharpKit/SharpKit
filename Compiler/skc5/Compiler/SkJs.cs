using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirrored.SharpKit.JavaScript;
using ICSharpCode.NRefactory.TypeSystem;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.Extensions;
using SharpKit.Compiler.CsToJs;

namespace SharpKit.Compiler
{
    /// <summary>
    /// Provides utilities for proper cs to js naming
    /// </summary>
    public static class SkJs
    {
        public static string GetJsArrayType(ArrayType arrayType)
        {
            var elementType = arrayType.ElementType;
            switch (elementType.Name)
            {
                case "SByte": return "Int8Array";  //-128 to 127, other numbers will be converted "silent" (without exception/console log) with overflow
                case "Byte": return "Uint8Array"; //0 to 255
                case "Int16": return "Int16Array";
                case "UInt16": return "Uint16Array";
                case "Int32": return "Int32Array";
                case "UInt32": return "Uint32Array";
                case "Single": return "Float32Array";
                case "Double": return "Float64Array";
            }
            return "Array";
        }

        public static JsExpression EntityTypeRefToMember(IType typeRef)
        {
            return EntityTypeRefToMember(typeRef, false);
        }
        public static JsExpression EntityTypeRefToMember(IType typeRef, bool isGenericArgument)
        {
            if (typeRef == null)
                return null;
            if (typeRef.Kind == TypeKind.Anonymous)
                return Js.Null();
            if (isGenericArgument)
            {
                var code = Sk.GetGenericArugmentJsCode(typeRef.GetEntityType());
                if (code != null)
                    return Js.CodeExpression(code);
            }
            var name = GetEntityJsName(typeRef);
            if (Sk.IsJsonMode(typeRef.GetEntityType()))
                return Js.String(name);
            var member = Js.Members(name);
            if (typeRef.IsGenericTypeParameter())
            {
                if (member.PreviousMember == null)
                    member.PreviousMember = Js.This();
                else
                    throw new Exception();
            }
            var def = typeRef.GetDefinitionOrArrayType();
            if (def != null && Sk.IsClrType(def))
            {
                member = member.Member("ctor");
            }
            return member;
        }
        public static JsMemberExpression EntityToMember(IEntity me)
        {
            if (me == null)
                return null;
            if (me.DeclaringType != null && (me.DeclaringType.IsGenericMethodArgument() || me.DeclaringType.IsGenericTypeParameter()))
            {
                var x = (JsMemberExpression)EntityTypeRefToMember(me.DeclaringType);
                var s = x.ToJs();
                return x;
            }
            var name = GetEntityJsName(me);
            if (me is ITypeDefinition)
            {
                return Js.Member(name);
            }
            else if (me.IsStatic())
            {
                var member = Js.Member(name);
                if (Sk.IsGlobalMember(me))
                    return member;
                if (me is IMethod && Sk.ExtensionImplementedInInstance((IMethod)me))
                    return member;
                member.PreviousMember = EntityToMember(me.GetDeclaringTypeDefinition());
                return member;
            }
            else if (me.SymbolKind == SymbolKind.Constructor)
            {
                var att = Sk.GetJsMethodAttribute((IMethod)me);
                if (att != null && att.Name != null) //TODO: hack
                    return Js.Member(att.Name);
                var ce = me.GetDeclaringTypeDefinition();
                var member = EntityToMember(ce);
                var att2 = Sk.GetJsTypeAttribute(ce);
                if (att2 != null && att2.NativeConstructors)
                    return member;
                member = member.Member(name);
                return member;
            }
            return Js.Member(name);
        }

        public static JsMemberExpression EntityMethodToJsFunctionRef(IMethod me)
        {
            var ownerType = me.GetDeclaringTypeDefinition();
            if (Sk.IsGlobalMethod(me))
            {
                var member = Js.Member(SkJs.GetEntityJsName(me));
                return member;
            }
            else
            {
                var member = SkJs.EntityToMember(ownerType);
                if (!me.IsStatic)
                {
                    if (Sk.IsNativeType(ownerType))
                        member = member.Member("prototype");
                    else
                        member = member.Member("commonPrototype");
                }
                member = member.Member(SkJs.GetEntityJsName(me));
                return member;
            }
        }


        #region GetEntityJsName
        public static string GetEntityJsName(IEntity me)
        {
            if (me is IMethod)//.EntityType == EntityType.Method
                return GetEntityJsName((IMethod)me);
            else if (me is ITypeDefinition)
                return GetEntityJsName((ITypeDefinition)me);
            else if (me is IProperty)//.EntityType == EntityType.Property)
                return GetEntityJsName((IProperty)me);
            else if (me.SymbolKind == SymbolKind.Field)
                return GetEntityJsName((IField)me);
            //danel: else if (me.EntityType == EntityType.ent_constant)
            //return GetEntityJsName((IConst)me);
            return me.Name;
        }
        //public static string GetEntityJsName(IType ce)
        //{
        //    var name = GetEntityJsName(ce.GetDefinition());
        //    return name;
        //}
        public static string GetEntityJsName(ITypeDefinition ce)
        {
            var name = GetEntityJsName(ce, false);
            return name;
        }
        public static string GetEntityJsName(IField fe)
        {
            var name = fe.Name;
            var att = fe.GetMetadata<JsFieldAttribute>();
            if (att != null && att.Name != null)
                name = att.Name;
            return name;

        }

        public static string GetEntityJsName(IMethod me2)
        {
            IMethod me;
            if (me2.IsConstructor && me2.DeclaringType.Kind == TypeKind.TypeParameter)//happens when invoking new T();
                me = me2;
            else
                me = (IMethod)me2.MemberDefinition;
            var name = me.Name;
            if (name != null)
            {
                if (name == ".ctor")
                    name = "ctor";
                else if (name == ".cctor")
                    name = "cctor";
            }
            var att = me.GetMetadata<JsMethodAttribute>(true);
            if (att != null && att.Name != null)
            {
                name = att.Name;
            }
            else if (me.DeclaringType.Kind == TypeKind.Delegate && me.Name == "Invoke")
            {
                return "";
            }
            else if (me.DeclaringType.IsGenericMethodArgument()) //happens when invoking new T() in method MyMethod<T>();
            {
                name = GetEntityJsName(me.DeclaringType);
            }
            //else if (me.DeclaringType.IsGenericTypeParameter()) //happens when invoking new T() in class List<T>;
            //{
            //    name = EntityTypeRefToMember(me.DeclaringType);
            //}
            else
            {
                var owner = me.GetOwner();
                if (owner != null && owner is IProperty)
                {
                    var pe = (IProperty)owner;
                    if (pe.SymbolKind == SymbolKind.Indexer && Sk.UseNativeIndexer(pe))
                        return "";
                    name = GetEntityJsName(pe);
                    if (me.IsGetter())
                        name = "get_" + name;
                    else
                        name = "set_" + name;
                }
            }

            if (Sk.NewInterfaceImplementation)
                name = SkJs.GetMethodPrefix(me) + name;

            if (!Sk.UseNativeOverloads(me))
            {
                if (me.TypeParameters.IsNotNullOrEmpty())
                    name += "$" + me.TypeParameters.Count;
                name += SkJs.GetOverloadedMethodSuffix(me);
            }
            return name;

        }
        public static string GetEntityJsName(IProperty pe)
        {
            var name = pe.Name;
            var att = pe.GetMetadata<JsPropertyAttribute>();
            if (att != null)
            {
                if (att.Name != null)
                    name = att.Name;
            }
            return ResolveResultConverter.JsIdentifier(name);
        }
        public static string GetEntityJsName(IType ceref)
        {
            if (ceref is ITypeDefinition)
                return GetEntityJsName((ITypeDefinition)ceref);
            var includeGenericArgs = !Sk.IgnoreTypeArguments(ceref.GetEntityType());
            return ceref.GetEntityJsName(false, includeGenericArgs);
        }

        static string GetEntityJsName(this IType ceref, bool shortName)
        {
            if (ceref is ITypeDefinition)
                return GetEntityJsName((ITypeDefinition)ceref, shortName);

            var ce = ceref.GetEntityType();
            var includeGenericArgs = false;
            if (ce != null)
                includeGenericArgs = !Sk.IgnoreTypeArguments(ceref.GetEntityType());
            return ceref.GetEntityJsName(shortName, includeGenericArgs);
        }
        static string GetEntityJsName(this IType ceref, bool shortName, bool includeGenericArgs)
        {
            if (ceref.Kind == TypeKind.TypeParameter)
            {
                return ((ITypeParameter)ceref).GetEntityJsName(shortName);
            }
            else if (ceref.Kind == TypeKind.Array)
            {
                return ((ArrayType)ceref).GetEntityJsName(shortName);
            }
            else if (ceref.Kind == TypeKind.ByReference)
            {
                return ((ByReferenceType)ceref).ElementType.GetEntityJsName(shortName);
            }
            else if (ceref.Kind == TypeKind.Dynamic) //TODO: Bug in NRefactory?
            {
                return "Object";
                //var objType = CompilerTool.Current.Project.Compilation.FindType(KnownTypeCode.Object);
                //return GetEntityJsName(objType, shortName, includeGenericArgs);
            }
            //var ce = ceref.GetDefinition();
            //var s = ce.GetEntityJsNameV2(shortName);
            var s = ceref.GetEntityJsNameV2(shortName);
            return s;
        }
        static string GetEntityJsName(this ArrayType tr, bool shortName)
        {
            if (shortName)//TOOD: HACK: backward compatibility for parameter naming
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(tr.ElementType.GetEntityJsName(shortName));
                sb.Append("[");
                for (int i = 1; i < tr.Dimensions; i++)
                    sb.Append(",");
                sb.Append("]");
                return sb.ToString();
            }
            else
            {
                return GetJsArrayType(tr);
                //return "Array";
            }
        }
        //static string GetEntityJsName(this CsEntityInstanceSpecifier tr, bool shortName)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(tr.type.GetEntityJsName(shortName));
        //    return sb.ToString();
        //}
        static string GetEntityJsName(this ITypeParameter tr, bool shortName)
        {
            return tr.Name;
        }

        static bool UseFullParameterizedName(ParameterizedTypeReference pr, IType t)
        {
            if (!Sk.NewInterfaceImplementation)
                return pr.TypeArguments.All((p) => p is ICSharpCode.NRefactory.TypeSystem.Implementation.GetClassTypeReference);

            return pr.TypeArguments.All((p) => p is ICSharpCode.NRefactory.TypeSystem.Implementation.GetClassTypeReference) && t.Kind != TypeKind.Interface;
        }

        static string GetEntityJsNameV2(this IType type, bool shortName)
        {
            //if (type.Kind == TypeKind.Anonymous)
            //    return "Object";
            var ce = type.GetDefinition();

            var r = type.ToTypeReference();
            var pr = r as ParameterizedTypeReference;

            var jta = ce.GetJsTypeAttribute();
            if (shortName)
            {
                if (jta != null && jta.Name != null)
                {
                    if (jta.Name.Contains('.'))
                        return jta.Name.Substring(jta.Name.LastIndexOf('.') + 1);
                    else
                        return jta.Name;
                }
                var name = ce.Name;
                if (ce.TypeParameters.IsNotNullOrEmpty() && !Sk.IgnoreTypeArguments(ce))
                    if (pr == null || !UseFullParameterizedName(pr, type))
                        name += "$" + ce.TypeParameterCount;
                    else
                    {
                        name += pr.TypeArguments.StringConcat((p) =>
                        {
                            var p2 = p as ICSharpCode.NRefactory.TypeSystem.Implementation.GetClassTypeReference;
                            return p2.FullTypeName.Name;
                        }, "$" + ce.TypeParameterCount + "$", "$", "");
                    }
                return name;
            }
            else
            {
                if (jta != null && jta.Name != null)
                    return jta.Name;
                var name = ce.FullName;
                if (ce.ParentAssembly != null)
                {
                    var atts = ce.ParentAssembly.GetMetadatas<JsNamespaceAttribute>().Where(t => t.Namespace.IsNotNullOrEmpty() && t.JsNamespace != null).OrderByDescending(t => t.Namespace.Length).ToList();
                    if (atts.Count > 0)
                    {
                        foreach (var att in atts)
                        {
                            if (name.StartsWith(att.Namespace))
                            {
                                if (att.JsNamespace.IsNullOrEmpty())
                                    name = name.ReplaceFirst(att.Namespace + ".", "");
                                else
                                    name = name.ReplaceFirst(att.Namespace, att.JsNamespace);
                                break;
                            }
                        }
                    }
                }
                if (ce.TypeParameters.IsNotNullOrEmpty() && !Sk.IgnoreTypeArguments(ce))
                    name += "$" + ce.TypeParameterCount;
                return name;
            }

        }

        static string GetEntityJsName(this ITypeDefinition ce, bool shortName)
        {
            var jta = ce.GetJsTypeAttribute();
            if (shortName)
            {
                if (jta != null && jta.Name != null)
                {
                    if (jta.Name.Contains('.'))
                        return jta.Name.Substring(jta.Name.LastIndexOf('.') + 1);
                    else
                        return jta.Name;
                }
                var name = ce.Name;
                if (ce.TypeParameters.IsNotNullOrEmpty() && !Sk.IgnoreTypeArguments(ce))
                    name += "$" + ce.TypeParameterCount;
                return name;
            }
            else
            {
                if (jta != null && jta.Name != null)
                    return jta.Name;
                var name = ce.FullName;
                if (ce.ParentAssembly != null)
                {
                    var atts = ce.ParentAssembly.GetMetadatas<JsNamespaceAttribute>().Where(t => t.Namespace.IsNotNullOrEmpty() && t.JsNamespace != null).OrderByDescending(t => t.Namespace.Length).ToList();
                    if (atts.Count > 0)
                    {
                        foreach (var att in atts)
                        {
                            if (name.StartsWith(att.Namespace))
                            {
                                if (att.JsNamespace.IsNullOrEmpty())
                                    name = name.ReplaceFirst(att.Namespace + ".", "");
                                else
                                    name = name.ReplaceFirst(att.Namespace, att.JsNamespace);
                                break;
                            }
                        }
                    }
                }
                if (ce.TypeParameters.IsNotNullOrEmpty() && !Sk.IgnoreTypeArguments(ce))
                    name += "$" + ce.TypeParameterCount;
                return name;
            }
        }

        static string GetAliasTypeName(IType type, bool shortName)
        {
            throw new NotImplementedException();
            //switch (type)
            //{
            //    case cs_entity_type.et_void:
            //        return shortName ? "Void" : "System.Void";
            //    case cs_entity_type.et_string:
            //        return shortName ? "String" : "System.String";
            //    case cs_entity_type.et_object:
            //        return shortName ? "Object" : "System.Object";
            //    case cs_entity_type.et_char:
            //        return shortName ? "Char" : "System.Char";
            //    case cs_entity_type.et_boolean:
            //        return shortName ? "Boolean" : "System.Boolean";
            //    case cs_entity_type.et_decimal:
            //        return shortName ? "Decimal" : "System.Decimal";
            //    case cs_entity_type.et_float32:
            //        return shortName ? "Float" : "System.Float";
            //    case cs_entity_type.et_float64:
            //        return shortName ? "Double" : "System.Double";
            //    case cs_entity_type.et_int16:
            //        return shortName ? "Short" : "System.Short";
            //    case cs_entity_type.et_int32:
            //        return shortName ? "Int32" : "System.Int32";
            //    case cs_entity_type.et_int64:
            //        return shortName ? "Int64" : "System.Int64";
            //    case cs_entity_type.et_literal_null:
            //        return "null";
            //    case cs_entity_type.et_uint16:
            //        return shortName ? "UInt16" : "System.UInt16";
            //    case cs_entity_type.et_uint32:
            //        return shortName ? "UInt32" : "System.UInt32";
            //    case cs_entity_type.et_uint64:
            //        return shortName ? "UInt64" : "System.UInt64";
            //    case cs_entity_type.et_uint8:
            //        return shortName ? "Byte" : "System.Byte";
            //}
            //return null;
        }

        #endregion

        #region Old

        internal static string GetLongFunctionName(IMethod me)
        {
            var x = me.GetDeclaringTypeDefinition().GetEntityJsName(false).Replace('.', '$');
            return String.Join("$", x, GetEntityJsName(me));
        }

        static string GetMethodPrefix(IMethod me)
        {
            if (me.IsExplicitInterfaceImplementation)
                return SkJs.GetEntityJsName(me.ImplementedInterfaceMembers.First().DeclaringType, true) + "$$"; //TODO: impl. of ns1.IName and ns2.IName (same name) will not work. Full path should added.
            if (me.DeclaringType.Kind == TypeKind.Interface)
                return SkJs.GetEntityJsName(me.DeclaringType, true) + "$$";
            return "";
        }

        static string GetOverloadedMethodSuffix(IMethod me)
        {
            if (me == null) return "";

            var needOverloadSuffix = NeedsJsOverloadSuffix(me);

            ////debugger hint: ((IMethod)((IMethod)me.ImplementedInterfaceMembers.First()).MemberDefinition).Parameters.First().Type.GetEntityJsName(true)
            if (Sk.NewInterfaceImplementation)
                if (me != null && me.ImplementedInterfaceMembers != null && me.ImplementedInterfaceMembers.Count != 0)
                {
                    var interfaceMember = me.ImplementedInterfaceMembers.First().MemberDefinition;
                    if (interfaceMember is IMethod)
                    {
                        needOverloadSuffix = true;
                        me = (IMethod)interfaceMember;
                    }
                }

            if (me == null || !needOverloadSuffix)
                return "";

            var prms = me.Parameters.ToList();
            if (me.IsIndexerSetter())
                prms.RemoveAt(prms.Count - 1);
            string ret = prms.StringConcat(p => p.Type.GetEntityJsName(true), "", "$$", "").Replace("[]", "$Array");
            if (ret.IsNotNullOrEmpty())
                return "$$" + ret;
            else
                return "";
        }
        static bool NeedsJsOverloadSuffix(IMethod me)
        {
            if (me.IsIndexerAccessor()) //TODO: HACK: bug with metaspec - base_type is null in overridden indexer methods
            {
                //var x = me.HasFlag(entity_flags.f_override);
                //var y = me.HasFlag(entity_flags.f_method_Virtual);
                return true;
            }
            if (me.Parameters.IsNullOrEmpty())
                return false;
            var ce = me.GetDeclaringTypeDefinition();
            if (me.IsConstructor)
            {
                //var ctors = ce.GetConstructors().Where(t => !t.IsGenerated()).ToList(); //isGenerated - metaspec bug? parser generates empty ctor when a ctor with parameters is written
                var ctors = ce.GetConstructors().ToList(); //removed "IsGenerated" removed, because of default constructor of structs
                if (ctors.Count > 1)
                {
                    if (ctors.Any((m) => m.IsInternal))
                    {
                        if (!me.IsInternal && ctors.Count((m) => !m.IsInternal) == 1) return false;
                    }
                    return true;
                }
                return false;
            }
            var bm = me.GetBaseMethod();
            if (bm != null)
                return NeedsJsOverloadSuffix(bm);
            if (ce != null)
            {
                var methods = ce.GetAllMethods(me.Name).Where(t => !t.IsExplicitInterfaceImplementation).ToList();
                if (methods.Count == 1)
                    return false;
            }
            return true;
        }


        #endregion
    }
}
