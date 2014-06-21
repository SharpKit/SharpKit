using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.CSharp;

namespace SharpKit.Compiler
{
    public interface ICompilerPlugin
    {
        void Init(ICompiler compiler);
    }



}
