using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.TypeScript
{
    abstract class TsEntity
    {
        public abstract void AcceptVisitor(ITsEntityVisitor visitor);
    }


    enum TsTypeKind
    {
        Class,
        Interface,
        Array,
    }

    class TsType : TsNamedEntity
    {
        public TsType()
        {
            Members = new List<TsMember>();
            TypeParameters = new List<TsTypeRef>();
        }
        public string ModuleName { get; set; }
        public bool IsModuleExport { get; set; }

        public TsTypeKind Kind { get; set; }
        public List<TsTypeRef> TypeParameters { get; set; }

        public List<TsMember> Members { get;set; }

        public override void AcceptVisitor(ITsEntityVisitor visitor)
        {
            visitor.VisitType(this);
        }

    }

    class TsFunction : TsMember
    {
        public TsFunction()
        {
            Parameters = new List<TsParameter>();
            TypeParameters = new List<TsTypeRef>();
        }
        public string Body { get; set; }

        public bool IsConstructor { get; internal set; }
        public List<TsParameter> Parameters { get; set; }
        public List<TsTypeRef> TypeParameters { get; set; }

        public override void AcceptVisitor(ITsEntityVisitor visitor)
        {
            visitor.VisitFunction(this);
        }

        public bool IsCallSignature { get; set; }
    }

    class TsParameter : TsNamedTypedEntity
    {
        public bool IsParams { get; set; }
        public bool IsOptional { get; set; }
        public override void AcceptVisitor(ITsEntityVisitor visitor)
        {
            visitor.VisitParameter(this);
        }
    }


    abstract class TsNamedEntity : TsEntity
    {
        public string Name { get; set; }

    }
    abstract class TsNamedTypedEntity : TsNamedEntity
    {
        public TsTypeRef Type { get; set; }

    }

    abstract class TsMember : TsNamedTypedEntity
    {
        public bool IsStatic { get; internal set; }

    }

    class TsTypeRef
    {
        public TsTypeRef()
        {
            GenericArguments = new List<TsTypeRef>();
        }
        public string Name { get; set; }
        public TsFunction AnonymousCallSignature { get; set; }
        public List<TsTypeRef> GenericArguments { get; set; }
    }

    class TsProperty : TsMember
    {
        public override void AcceptVisitor(ITsEntityVisitor visitor)
        {
            visitor.VisitProperty(this);
        }

    }


    interface ITsEntityVisitor
    {
        void VisitFunction(TsFunction func);
        void VisitType(TsType ce);
        void VisitProperty(TsProperty pe);
        void VisitParameter(TsParameter prm);

    }



    static class Extensions
    {
        public static bool IsNullOrVoid(this TsTypeRef tr)
        {
            return tr == null || tr.Name == "void";
        }
    }

}
