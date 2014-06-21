using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace SharpKit.Compiler
{
    class NotifyNavigator : IResolveVisitorNavigator
    {
        #region IResolveVisitorNavigator Members

        ResolveVisitorNavigationMode IResolveVisitorNavigator.Scan(AstNode node)
        {
            return ResolveVisitorNavigationMode.Resolve;
        }

        public Action<AstNode, ResolveResult> Resolved { get; set; }
        void IResolveVisitorNavigator.Resolved(AstNode node, ResolveResult result)
        {
            if (Resolved != null)
                Resolved(node, result);
        }

        public Action<Expression, ResolveResult, Conversion, IType> ProcessConversion { get; set; }
        void IResolveVisitorNavigator.ProcessConversion(Expression expression, ResolveResult result, Conversion conversion, IType targetType)
        {
            if (ProcessConversion != null)
                ProcessConversion(expression, result, conversion, targetType);
        }

        #endregion
    }
}
