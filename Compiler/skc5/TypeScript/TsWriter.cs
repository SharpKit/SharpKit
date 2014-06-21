using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.TypeScript
{
    class TsWriter : ITsEntityVisitor
    {
        public TsWriter()
        {
            _Writer = new VisitableWriter<object> { VisitAction = _Visit };
        }

        void _Visit(object t)
        {

            if (t is TsEntity)
                Visit((TsEntity)t);
            else if (t is TsTypeRef)
                Visit((TsTypeRef)t);
            else
                _Writer.Write(t.ToString());
        }

        void Visit(TsTypeRef tr)
        {
            if (tr.AnonymousCallSignature != null)
            {
                Visit(tr.AnonymousCallSignature);
                return;
            }
            _Writer
                .Write(((TsTypeRef)tr).Name)
                .VisitEachJoinIfNotNullOrEmpty(tr.GenericArguments, "<", ",", ">");

        }
        public TextWriter Writer
        {
            get
            {
                return _Writer.Writer;
            }
            set
            {
                _Writer.Writer = value;
            }
        }
        VisitableWriter<object> _Writer { get; set; }

        public void Visit(TsEntity me)
        {
            me.AcceptVisitor(this);
        }
        //public void Write(object obj)
        //{
        //    Writer.Write(obj);
        //}
        public void VisitFunction(TsFunction func)
        {
            if (func.IsCallSignature)
            {
                VisitAnonymousFunctionType(func);
                return;
            }
            _Writer
                .Write(func.Name)
                .VisitEachJoinIfNotNullOrEmpty(func.TypeParameters, "<", ",", ">")
                .Write("(").VisitEachJoin(func.Parameters, ",").Write(")")
                .PrefixVisitIf(func.Type!=null && !func.IsConstructor, ":", func.Type)
                .WriteIfElse(func.Body!=null, "{"+func.Body+"}", ";")
                .Write("\n");
        }

        public void VisitAnonymousFunctionType(TsFunction func)
        {
            _Writer
                .WriteIf(func.IsStatic, "static ")
                .Write("(").VisitEachJoin(func.Parameters, ",").Write(")")
                .PrefixVisit("=>", func.Type);
        }

        public void VisitParameter(TsParameter prm)
        {
            _Writer.Write(prm.Name).WriteIf(prm.IsOptional, "?").Write(":").Visit(prm.Type);
        }

        public void VisitProperty(TsProperty pe)
        {
            _Writer
                .WriteIf(pe.IsStatic, "static ")
                .Write(pe.Name)
                .Write(":")
                .Visit(pe.Type)
                .Write(";\n");
        }

        public void VisitType(TsType ce)
        {
            _Writer
                .WriteIf(ce.ModuleName != null, "module "+ce.ModuleName+"\n{\n")
                .WriteIf(ce.IsModuleExport, "export ")
                .Write(ce.Kind == TsTypeKind.Interface ? "interface " : "class ")
                .Write(ce.Name)
                .VisitEachJoinIfNotNullOrEmpty(ce.TypeParameters, "<", ",", ">")
                .Write("\n{\n").VisitEach(ce.Members).Write("}\n")
                .WriteIf(ce.ModuleName != null, "\n}\n")
                ;
        }
    }
}
