using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.Extensions;
using ICSharpCode.NRefactory.TypeSystem;
using SharpKit.TypeScript;

namespace SharpKit.Compiler.CsToJs
{
    class TsMemberConverter
    {
        public TypeConverter TypeConverter { get; set; }

        public List<TsType> Visit(List<ITypeDefinition> list)
        {
            //var byNamespace = list.GroupBy(t => t.Namespace).ToList();

            var list2 =  list.Select(Visit).ToList();
            list2 = list2.Where(t => t.Name != "Array").ToList();
            return list2;
        }
        public TsType Visit(ITypeDefinition ce)
        {
            var name = SkJs.GetEntityJsName(ce);
            var ce2 = new TsType
            {
                Name = name, Kind=(ce.IsInterface() || ce.IsDelegate()) ? TsTypeKind.Interface :  TsTypeKind.Class,
                TypeParameters = ce.TypeParameters.Select(Visit).ToList()
            };
            if (name.Contains("."))
            {
                var pair = name.SplitAt(name.LastIndexOf("."), true);
                ce2.Name = pair[1];
                ce2.ModuleName = pair[0];
                ce2.IsModuleExport = true;
            }
            if (ce.IsDelegate())
            {
                var func = Visit(ce.GetDelegateInvokeMethod());
                //func.IsCallSignature = true;
                func.Name = null;
                ce2.Members.Add(func);
            }
            else
            {
                var members = TypeConverter.ClrConverter.GetMembersToExport(ce);
                var members2 = members.Select(Visit).Where(t => t != null).ToList();
                ce2.Members.AddRange(members2);
                if (ce2.Kind == TsTypeKind.Class)
                {
                    ce2.Members.OfType<TsFunction>().Where(t => !t.IsConstructor || !t.Type.IsNullOrVoid()).ForEach(t => t.Body = "return null;");
                    ce2.Members.OfType<TsFunction>().Where(t => t.IsConstructor).ForEach(t => t.Body = "");
                }
            }
            return ce2;
        }

        public TsParameter Visit(IParameter prm)
        {
            return new TsParameter { Name = prm.Name, Type = Visit(prm.Type) };
        }

        private TsTypeRef Visit(IType type)
        {
            if(type==null)
                return new TsTypeRef { Name = "void" };
            var typeDef = type.GetDefinition();
            if (typeDef!=null)
            {
                var ktc = typeDef.KnownTypeCode;
                if(ktc != KnownTypeCode.None)
                {
                    if (ktc==KnownTypeCode.Void)
                        return new TsTypeRef { Name = "void" };
                    else if (ktc== KnownTypeCode.Object)
                        return new TsTypeRef { Name = "any" };
                    else if (ktc == KnownTypeCode.String)
                        return new TsTypeRef { Name = "string" };
                    else if (ktc == KnownTypeCode.Boolean)
                        return new TsTypeRef { Name = "boolean" };
                    else if (ktc == KnownTypeCode.Int32
                        || ktc == KnownTypeCode.Int32
                        || ktc == KnownTypeCode.Int16
                        || ktc == KnownTypeCode.Int64
                        || ktc == KnownTypeCode.Double
                        || ktc == KnownTypeCode.Single
                        || ktc == KnownTypeCode.SByte
                        || ktc == KnownTypeCode.Byte
                        || ktc == KnownTypeCode.UInt16
                        || ktc == KnownTypeCode.UInt32
                        || ktc == KnownTypeCode.UInt64
                        || ktc == KnownTypeCode.Decimal
                        )
                        return new TsTypeRef { Name = "number" };
                }
                if (Sk.IsJsonMode(typeDef))
                    return new TsTypeRef { Name = "Object" };
                if (type.Kind == TypeKind.Delegate && !Sk.IsJsExported(type.GetDefinition()))
                {
                    var func = Visit(type.GetDelegateInvokeMethod());
                    func.Name = null;
                    func.IsCallSignature = true;
                    return new TsTypeRef { AnonymousCallSignature = func};
                }
            }
            var tr2 = new TsTypeRef { Name = SkJs.GetEntityJsName(type) };
            tr2.GenericArguments = type.TypeArguments.Select(Visit).ToList();
            if (tr2.Name == "Array" && tr2.GenericArguments.Count == 0)
            {
                tr2.GenericArguments.Add(new TsTypeRef { Name = "any" });
            }
            return tr2;
        }

        public TsMember Visit(IMember me)
        {
            if(me is IMethod)
            {
                return Visit((IMethod)me);
            }
            return new TsProperty
            {
                Name = me.Name,
                Type = Visit(me.ReturnType),
                IsStatic = me.IsStatic,
            };
        }


        public TsFunction Visit(IMethod me)
        {
            if (me is FakeMethod)
                return null;
            TsFunction me2 = new TsFunction
            {
                Name = SkJs.GetEntityJsName(me),
                IsStatic = me.IsStatic,
                Parameters = me.Parameters.Select(Visit).ToList()
            };
            if (me.SymbolKind == SymbolKind.Constructor)
            {
                me2.Name = SkJs.GetEntityJsName(me);// "constructor";
                //if (me2.Name.StartsWith("."))
                //me2.IsConstructor = true;
            }
            else
            {
                me2.Type = Visit(me.ReturnType);
                me2.TypeParameters = me.TypeParameters.Select(Visit).ToList();
            }
            return me2;

        }
    }
}
