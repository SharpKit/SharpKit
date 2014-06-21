using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using SharpKit.Compiler;
using SharpKit.Utils;
using System.CodeDom.Compiler;
using Mirrored.SharpKit.JavaScript;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler.CsToJs
{
    class MemberConverter_Global : MemberConverter
    {

        public override JsNode ExportConstructor(IMethod ctor)
        {
            var func = (JsFunction)base.ExportConstructor(ctor);
            if (ctor.IsStatic)
                return new JsUnit { Statements = func.Block.Statements };
            return func;
        }

        public override JsNode _VisitField(IField fld)
        {
            var st = (JsStatement)ExportInitializer(fld, null, true);
            return st;
        }

        public override JsNode _VisitClass(ITypeDefinition ce)
        {
            var members = GetMembersToExport(ce);
            var list = new List<IMember>();
            foreach (var member in members)
            {
                if (!member.IsStatic)
                {
                    if (!member.IsCompilerGenerated())
                        Log.Error(member, "Global objects cannot have instance members");
                }
                else if (member.SymbolKind == SymbolKind.TypeDefinition)
                    Log.Error(member, "Global objects cannot have inner types");
                else
                    list.Add(member);
            }
            return VisitToUnit(list);
        }

        public override JsNode _Visit(IProperty pe)
        {
            var list = GetAccessorsToExport(pe);
            var list2 = list.Select(ExportMethod).ToList();
            return new JsUnit { Statements = list2.Cast<JsStatement>().ToList() };
        }

        public override JsNode ExportMethod(IMethod me)
        {
            var node = base.ExportMethod(me);
            if (node == null)
                return node;
            if (!node.Is(JsNodeType.Function))
                return node;
            var func = (JsFunction)node;
            var st = func.Statement();
            return st;
        }

    }

}
