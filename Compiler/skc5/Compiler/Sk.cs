using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirrored.SharpKit.JavaScript;
using System.IO;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler
{
    static class Sk
    {

        public static bool NewInterfaceImplementation = false; //New interface implementation

        public static string DirectorySeparator = Path.DirectorySeparatorChar.ToString();
        public static string MirrorTypePrefix = "Mirrored.";

        public static string GetExportPath(ITypeDefinition ce)
        {
            var att = ce.GetJsTypeAttribute();
            string path;
            if (att != null && att.Filename.IsNotNullOrEmpty())
            {
                path = att.Filename.Replace("/", Sk.DirectorySeparator);
                if (path.StartsWith(@"~\") || path.StartsWith(@"~/"))
                    path = path.Substring(2);
                else
                    path = Path.Combine(Path.GetDirectoryName(ce.GetFileOrigin()), path);
                var asm = ce.ParentAssembly;
                var att2 = asm.GetMetadata<JsExportAttribute>();
                if (att2 != null && att2.FilenameFormat.IsNotNullOrEmpty())
                    path = String.Format(att2.FilenameFormat, path);
            }
            else
            {
                path = GetDefaultJsFilename(ce);
            }
            return path;
        }
        private static string GetDefaultJsFilename(ITypeDefinition ce)
        {
            var asm = ce.ParentAssembly;
            var s = "res" + Sk.DirectorySeparator + asm.AssemblyName + ".js";
            var att = asm.GetMetadata<JsExportAttribute>();
            if (att != null)
            {
                if (att.DefaultFilename.IsNotNullOrEmpty())
                {
                    s = att.DefaultFilename;
                }
                else if (att.DefaultFilenameAsCsFilename)
                {
                    var filename = ce.GetFileOrigin();
                    filename = Path.ChangeExtension(filename, ".js");
                    if (att.FilenameFormat.IsNotNullOrEmpty())
                        filename = String.Format(att.FilenameFormat, filename);
                    s = filename;
                }
            }
            return s.Replace("/", Sk.DirectorySeparator);
        }


        #region JsMethodAttribute
        public static JsMethodAttribute GetJsMethodAttribute(IMethod me)
        {
            if (me == null)
                return null;
            return me.GetMetadata<JsMethodAttribute>(true);
        }
        public static bool UseNativeOverloads(IMethod me)
        {
            if (me.IsPropertyAccessor())
                return true;
            if (me.IsEventAccessor())
                return true;
            JsMethodAttribute jma = me.GetMetadata<JsMethodAttribute>(true);
            if (jma != null && jma._NativeOverloads != null)
                return jma._NativeOverloads.GetValueOrDefault();

            var t = me.GetDeclaringTypeDefinition();
            if (t != null)
            {
                return UseNativeOverloads(t);
            }
            else
            {
                return false; //Not declared on method, not declared on type
            }

        }
        public static string GetNativeCode(IMethod me)
        {
            JsMethodAttribute jma = me.GetMetadata<JsMethodAttribute>();
            return (jma == null) ? null : jma.Code;
        }
        public static bool ExtensionImplementedInInstance(IMethod me)
        {
            JsMethodAttribute jma = me.GetMetadata<JsMethodAttribute>();
            return (jma == null) ? false : jma.ExtensionImplementedInInstance;
        }
        public static bool IgnoreGenericMethodArguments(IMethod me)
        {
            if (me == null)
                return false;
            return MD_JsMethodOrJsType(me, t => t._IgnoreGenericArguments, t => t._IgnoreGenericMethodArguments).GetValueOrDefault();
        }
        public static bool IsGlobalMethod(IMethod me)
        {
            var att = me.GetMetadata<JsMethodAttribute>(true);
            if (att != null && att._Global != null)
                return att._Global.Value;
            var owner = me.GetOwner();
            if (owner != null && owner is IProperty)
            {
                return IsGlobalProperty((IProperty)owner);
            }
            return IsGlobalType(me.GetDeclaringTypeDefinition());

        }

        #endregion

        #region JsEventAttribute
        public static JsEventAttribute GetJsEventAttribute(IEntity me) //TODO: implement
        {
            return me.GetMetadata<JsEventAttribute>();
        }
        #endregion

        #region JsPropertyAttribute
        public static JsPropertyAttribute GetJsPropertyAttribute(IProperty pe)
        {
            return pe.GetMetadata<JsPropertyAttribute>();
        }
        public static bool IsNativeField(IProperty pe)
        {
            var jpa = pe.GetMetadata<JsPropertyAttribute>();
            if (jpa != null)
                return jpa.NativeField;
            var att = GetJsTypeAttribute(pe.GetDeclaringTypeDefinition());//.GetMetadata<JsTypeAttribute>(true);
            if (att != null)
            {
                if (att.PropertiesAsFields)
                    return true;
                else if (att.AutomaticPropertiesAsFields && pe.IsAutomaticProperty())
                    return true;
                else
                    return false;
            }
            return false;
        }
        public static bool UseNativeIndexer(IProperty pe)
        {
            return pe.MD<JsPropertyAttribute, bool>(t => t.NativeIndexer);
        }

        public static bool IsNativeProperty(IProperty pe)
        {
            if (IsNativeField(pe))
                return false;
            var x = MD_JsPropertyOrJsType(pe, t => t._NativeProperty, t => t._NativeProperties);
            return x.GetValueOrDefault();
            //var attr = GetJsPropertyAttribute(pe);
            //return attr != null && attr.NativeProperty;
        }

        public static bool IsNativePropertyEnumerable(IProperty pe)
        {
            var x = MD_JsPropertyOrJsType(pe, t => t._NativePropertyEnumerable, t => t._NativePropertiesEnumerable);
            return x.GetValueOrDefault(); ;
        }

        #endregion

        #region JsExport

        public static bool IsJsExported(IEntity me)
        {
            var ext = me.GetExtension(true);
            if (ext.IsJsExported == null)
            {
                ext.IsJsExported = IsJsExported_Internal(me).GetValueOrDefault();
                //if (ext.IsJsExported == null)
                //{
                //    var decType = me.GetDeclaringTypeDefinition();
                //    if(decType!=null)
                //        ext.IsJsExported = IsJsExported(decType);
                //}
            }
            return ext.IsJsExported.Value;
        }

        private static bool? IsJsExported_Internal(IEntity me)
        {
            if (me is ITypeDefinition)
            {
                var ce = (ITypeDefinition)me;
                return ce.MD_JsType(t => t.Export);
            }
            if (me.SymbolKind == SymbolKind.Method || me.SymbolKind == SymbolKind.Accessor)
            {
                var me2 = (IMethod)me;
                return me2.MD_JsMethodOrJsType(t => t._Export, t => t.Export);
            }
            if (me.SymbolKind == SymbolKind.Property)
            {
                var pe = (IProperty)me;
                return pe.MD_JsPropertyOrJsType(t => t._Export, t => t.Export);
            }
            if (me.SymbolKind == SymbolKind.Field)//danel: || const
            {
                var pe = (IField)me;
                return pe.MD_JsFieldOrJsType(t => t._Export, t => t.Export);
            }
            if (me.SymbolKind == SymbolKind.Event)//danel: || const
            {
                var pe = (IEvent)me;
                return pe.MD_JsEventOrJsType(t => t._Export, t => t.Export);
            }
            //other entity types
            var decType = me.GetDeclaringTypeDefinition();
            if (decType != null)
                return IsJsExported(decType);
            return null;
        }


        #endregion

        #region JsType
        public static JsMode? GetJsMode(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._Mode);
        }
        public static string GetJsonTypeFieldName(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t.JsonTypeFieldName);
        }

        public static bool UseNativeJsons(ITypeDefinition type)
        {
            var att = type.GetJsTypeAttribute();
            if (att != null && att.NativeJsons)
                return true;
            return false;
        }

        public static JsTypeAttribute GetJsTypeAttribute(this ITypeDefinition ce)
        {
            if (ce == null)
                return null;
            var att = ce.GetMetadata<JsTypeAttribute>();
            if (att == null && ce.ParentAssembly != null)
                att = GetDefaultJsTypeAttribute(ce);
            return att;
        }

        private static JsTypeAttribute GetDefaultJsTypeAttribute(ITypeDefinition ce)
        {
            if (ce == null)
                return null;
            return ce.ParentAssembly.GetMetadatas<JsTypeAttribute>().Where(t => t.TargetType == null && t.TargetTypeName.IsNullOrEmpty()).FirstOrDefault();
        }
        private static JsEnumAttribute GetDefaultJsEnumAttribute(ITypeDefinition ce)
        {
            if (ce == null)
                return null;
            return ce.ParentAssembly.GetMetadatas<JsEnumAttribute>()/*TODO:.Where(t => t.TargetType == null)*/.FirstOrDefault();
        }
        public static bool UseNativeOperatorOverloads(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._NativeOperatorOverloads).GetValueOrDefault();
        }
        public static bool IsNativeArrayEnumerator(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._NativeArrayEnumerator).GetValueOrDefault();
        }
        public static bool UseNativeOverloads(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._NativeOverloads).GetValueOrDefault();
        }
        public static bool IgnoreTypeArguments(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._IgnoreGenericTypeArguments).GetValueOrDefault();
        }
        public static bool IsGlobalType(ITypeDefinition ce)
        {
            if (ce == null)
                return false;
            var ext = ce.GetExtension(true);
            if (ext.IsGlobalType == null)
                ext.IsGlobalType = IsGlobalType_Internal(ce);
            return ext.IsGlobalType.Value;
        }

        public static bool IsGlobalType_Internal(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._GlobalObject).GetValueOrDefault();
        }

        public static bool IsClrType(ITypeDefinition ce)
        {
            if (ce == null)
                return false;
            return !IsNativeType(ce) && !IsGlobalType(ce);
        }

        public static bool IsNativeType(ITypeDefinition ce)
        {
            if (ce == null)
                return false;
            var ext = ce.GetExtension(true);
            if (ext.IsNativeType == null)
                ext.IsNativeType = IsNativeType_Internal(ce);
            return ext.IsNativeType.Value;
        }
        public static bool IsExtJsType(ITypeDefinition ce)
        {
            var mode = ce.MD_JsType(t => t._Mode);
            return mode != null && mode.Value == JsMode.ExtJs;
        }

        public static bool IsNativeType_Internal(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._Native).GetValueOrDefault();
        }

        public static bool OmitDefaultConstructor(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._OmitDefaultConstructor).GetValueOrDefault();
        }

        #endregion

        #region JsDelegate

        public static JsDelegateAttribute GetJsDelegateAttribute(ITypeDefinition et)
        {
            if (et == null || !et.IsDelegate())
                return null;

            var data = et.GetMetadata<JsDelegateAttribute>();
            return data;
        }

        #endregion

        #region Entity

        public static bool IsGlobalMember(IEntity me)
        {
            if (me is IMethod)
                return IsGlobalMethod((IMethod)me);
            if (me is ITypeDefinition)
                return IsGlobalType((ITypeDefinition)me);
            if (me is IProperty)
                return IsGlobalProperty((IProperty)me);
            return IsGlobalType(me.GetDeclaringTypeDefinition());

        }

        private static bool IsGlobalProperty(IProperty me)
        {
            var att = me.GetMetadata<JsPropertyAttribute>(true);
            if (att != null && att._Global != null)
                return att._Global.Value;
            return IsGlobalType(me.GetDeclaringTypeDefinition());
        }

        #endregion

        public static ITypeDefinition GetBaseJsClrType(ITypeDefinition ce)
        {
            var baseCe = ce.GetBaseTypeDefinition();
            while (baseCe != null && !IsClrType(baseCe))
                baseCe = baseCe.GetBaseTypeDefinition();
            return baseCe;
        }

        public static bool IsJsonMode(ITypeDefinition ce)
        {
            return GetJsMode(ce) == JsMode.Json;
        }

        //public static bool ForceDelegatesAsNativeFunctions(IEntity me)
        //{
        //    if(me is IMethod)
        //        return ForceDelegatesAsNativeFunctions((IMethod)me);
        //    else if (me is ITypeDefinition)
        //        return ForceDelegatesAsNativeFunctions(((ITypeDefinition)me));
        //    else if (me is IType)
        //        return ForceDelegatesAsNativeFunctions(((IType)me).GetDefinitionOrArrayType());
        //    return ForceDelegatesAsNativeFunctions(me.DeclaringTypeDefinition);
        //}
        public static bool ForceDelegatesAsNativeFunctions(IMethod me)
        {
            return me.MD_JsMethodOrJsType(t => t._ForceDelegatesAsNativeFunctions, t => t._ForceDelegatesAsNativeFunctions).GetValueOrDefault();
        }
        public static bool ForceDelegatesAsNativeFunctions(IMember me)
        {
            if (me is IMethod)
                return ForceDelegatesAsNativeFunctions((IMethod)me);
            ITypeDefinition ce;
            if (me is ITypeDefinition)
                ce = (ITypeDefinition)me;
            else
                ce = me.DeclaringTypeDefinition;

            return ce.MD_JsType(t => t._ForceDelegatesAsNativeFunctions).GetValueOrDefault();
        }
        //public static bool ForceDelegatesAsNativeFunctions(ITypeDefinition ce)
        //{
        //    return ce.MD_JsType(t => t._ForceDelegatesAsNativeFunctions).GetValueOrDefault();
        //}

        public static bool InlineFields(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._InlineFields).GetValueOrDefault();
        }
        public static bool OmitInheritance(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._OmitInheritance).GetValueOrDefault();
        }

        public static bool NativeCasts(IType ce)
        {

            if (ce.IsGenericTypeParameter())
                return false;
            return NativeCasts(ce.GetDefinitionOrArrayType());
        }

        public static bool OmitCasts(IType ce)
        {
            if (ce.IsGenericTypeParameter() || ce.IsGenericMethodArgument())
            {
                var tp = ce as ITypeParameter;
                var owner = tp.Owner;
                var me = owner as IMethod;
                if (me != null)
                    return IgnoreGenericMethodArguments(me);
                var ce2 = owner as IType;
                if (ce2 != null)
                {
                    var ce3 = ce2.GetDefinitionOrArrayType();
                    if (ce3 != null)
                        return IgnoreTypeArguments(ce3);
                }
                //this shouldn't happen
                return false;
            }
            return OmitCasts(ce.GetDefinitionOrArrayType());
        }

        public static bool OmitCasts(ITypeDefinition ce)
        {
            if (ce == null)
                return true; //FIX FOR ISSUE 321, ce will be null, when the type is generic
            var att = GetJsExportAttribute(ce.ParentAssembly.Compilation);
            if (att != null && att.ForceOmitCasts)
                return true;
            var value = ce.MD_JsType(t => t._OmitCasts);
            if (value == null)
            {
                var typeFieldName = GetJsonTypeFieldName(ce);
                if (typeFieldName == null && IsJsonMode(ce))
                    return true;
            }
            return value.GetValueOrDefault();
        }

        public static JsExportAttribute GetJsExportAttribute(ICompilation compilation)
        {
            return compilation.MainAssembly.GetMetadata<JsExportAttribute>();
        }

        public static bool OmitOptionalParameters(IMethod me)
        {
            return me.MD_JsMethodOrJsType(t => t._OmitOptionalParameters, t => t._OmitOptionalParameters).GetValueOrDefault();
        }

        //public static bool IsStructAsClass(IStruct ce)
        //{
        //    var att = GetJsStructAttribute(ce);
        //    if (att != null)
        //        return att.IsClass;
        //    return false;
        //}

        //private static JsStructAttribute GetJsStructAttribute(IStruct ce)
        //{
        //    return ce.GetMetadata<JsStructAttribute>();
        //}

        #region Utils

        static R MD<T, R>(this IEntity me, Func<T, R> selector) where T : Attribute
        {
            var att = me.GetMetadata<T>(true);
            if (att != null)
                return selector(att);
            return default(R);
        }
        static R MD_JsMethod<R>(this IMethod me, Func<JsMethodAttribute, R> func)
        {
            return me.MD(func);
        }
        static R MD_JsProperty<R>(this IProperty me, Func<JsPropertyAttribute, R> func)
        {
            return me.MD(func);
        }
        static R MD_JsField<R>(this IField me, Func<JsFieldAttribute, R> func)
        {
            return me.MD(func);
        }
        static R MD_JsEvent<R>(this IEvent me, Func<JsEventAttribute, R> func)
        {
            return me.MD(func);
        }
        static R MD_JsType<R>(this ITypeDefinition ce, Func<JsTypeAttribute, R> func2)
        {
            var att = ce.GetMetadata<JsTypeAttribute>();
            if (att != null)
            {
                var x = func2(att);
                if (((object)x) != null)
                    return x;
            }
            att = GetDefaultJsTypeAttribute(ce);
            if (att != null)
                return func2(att);
            return default(R);
        }
        static R MD_JsMethodOrJsType<R>(this IMethod me, Func<JsMethodAttribute, R> func, Func<JsTypeAttribute, R> func2)
        {
            var x = me.MD_JsMethod(func);
            if (((object)x) != null)
                return x;
            var ce = me.GetDeclaringTypeDefinition();
            if (ce != null)
                x = ce.MD_JsType(func2);
            return x;
        }
        static R MD_JsPropertyOrJsType<R>(this IProperty me, Func<JsPropertyAttribute, R> func, Func<JsTypeAttribute, R> func2)
        {
            var x = me.MD_JsProperty(func);
            if (((object)x) != null)
                return x;
            var ce = me.GetDeclaringTypeDefinition();
            if (ce != null)
                x = ce.MD_JsType(func2);
            return x;
        }
        static R MD_JsFieldOrJsType<R>(this IField me, Func<JsFieldAttribute, R> func, Func<JsTypeAttribute, R> func2)
        {
            var x = me.MD_JsField(func);
            if (((object)x) != null)
                return x;
            var ce = me.GetDeclaringTypeDefinition();
            if (ce != null)
                x = ce.MD_JsType(func2);
            return x;
        }
        static R MD_JsEventOrJsType<R>(this IEvent me, Func<JsEventAttribute, R> func, Func<JsTypeAttribute, R> func2)
        {
            var x = me.MD_JsEvent(func);
            if (((object)x) != null)
                return x;
            var ce = me.GetDeclaringTypeDefinition();
            if (ce != null)
                x = ce.MD_JsType(func2);
            return x;
        }

        static R MD_JsEnum<R>(this ITypeDefinition ce, Func<JsEnumAttribute, R> func2)
        {
            var att = ce.GetMetadata<JsEnumAttribute>();
            if (att != null)
            {
                var x = func2(att);
                if (((object)x) != null)
                    return x;
            }
            att = GetDefaultJsEnumAttribute(ce);
            if (att != null)
                return func2(att);
            return default(R);
        }
        #endregion

        public static bool IsNativeParams(IMethod me)
        {
            var x = me.MD_JsMethodOrJsType(t => t._NativeParams, t => t._NativeParams);
            if (x == null)
                return true;
            return x.Value;
        }

        public static string GetPrototypeName(ITypeDefinition ce)
        {
            var att = GetJsTypeAttribute(ce);
            if (att != null && att.PrototypeName != null)
                return att.PrototypeName;
            return "prototype";
        }

        public static bool IsNativeError(ITypeDefinition ce)
        {
            return ce.MD_JsType(t => t._NativeError).GetValueOrDefault();
        }

        public static bool NativeCasts(ITypeDefinition ce)
        {
            var value = ce.MD_JsType(t => t._NativeCasts);
            if (value == null && IsJsonMode(ce) && GetJsonTypeFieldName(ce) != null)
                return true;
            return value.GetValueOrDefault();
        }

        public static string GetGenericArugmentJsCode(ITypeDefinition ce)
        {
            return MD_JsType(ce, t => t._GenericArgumentJsCode);
        }


        /// <summary>
        /// Gets a member or a class, identifies if it's an enum member or type, 
        /// and returns whether this enum has no JsType attribute or has JsType(JsMode.Json)
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static bool UseJsonEnums(IEntity me, out bool valuesAsNames)
        {
            ITypeDefinition ce;
            if (me.IsEnumMember())
                ce = me.GetDeclaringTypeDefinition();
            else if (me.IsEnum())
                ce = (ITypeDefinition)me;
            else
            {
                valuesAsNames = false;
                return false;
            }

            valuesAsNames = UseEnumValuesAsNames(ce);
            return UseJsonEnums(ce);
        }
        public static bool UseJsonEnums(ITypeDefinition ce)
        {
            if (!ce.IsEnum())
                return false;
            var mode = ce.MD_JsType(t => t._Mode);
            var use = true;
            if (mode != null)
                use = mode.Value == JsMode.Json;
            return use;
        }
        public static bool UseEnumValuesAsNames(ITypeDefinition ce)
        {
            return ce.MD_JsEnum(t => t._ValuesAsNames).GetValueOrDefault();
        }

        public static bool ExportTsHeaders(ICompilation compilation)
        {
            var att = GetJsExportAttribute(compilation);
            if (att == null)
                return false;
            return att.ExportTsHeaders;
        }


        /// <summary>
        /// Indicates that object is IProperty that uses getter setter functions, and not native fields
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static bool IsEntityFunctionProperty(IEntity entity, ICSharpCode.NRefactory.Semantics.ResolveResult scope)
        {
            var pe = entity as IProperty;
            if (pe != null)
            {
                var ce = pe.DeclaringType;
                if (ce != null && ce.Kind == TypeKind.Anonymous)
                {
                    var ce2 = scope.GetParentType();
                    if (ce2 != null && Sk.UseNativeJsons(ce2))
                        return false;
                }
                return !Sk.IsNativeField(pe) && !Sk.UseNativeIndexer(pe); // && !Sk.IsNativeProperty(pe);
            }
            return false;
        }
    }

}
