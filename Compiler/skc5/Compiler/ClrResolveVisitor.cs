using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;

namespace SharpKit.Compiler
{
    class ClrResolveVisitor : ICSharpResolveResultVisitor<object>
    {
        public object Visit(ResolveResult res)
        {
            return res.AcceptVisitor(this);
        }

        #region IResolveResultVisitor<object> Members

        public object VisitResolveResult(ResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitInvocationResolveResult(InvocationResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitMemberResolveResult(MemberResolveResult res)
        {
            var type = res.Type.GetMirroredClrType();
            if (type.IsInstanceOfType(res.ConstantValue))
                return res.ConstantValue;
            var value = Enum.ToObject(type, res.ConstantValue);
            return value;
        }

        public object VisitThisResolveResult(ThisResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitConstantResolveResult(ConstantResolveResult res)
        {
            if (res.Type.Kind == TypeKind.Enum)
            {
                var value = Enum.ToObject(res.Type.GetMirroredClrType(), res.ConstantValue);
                return value;
            }
            return res.ConstantValue;
        }

        public object VisitConversionResolveResult(ConversionResolveResult res)
        {
            var value = res.Input.AcceptVisitor(this);
            return value;
        }

        public object VisitLocalResolveResult(LocalResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitTypeResolveResult(TypeResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitTypeOfResolveResult(TypeOfResolveResult res)
        {
            return res.ReferencedType;
        }

        public object VisitOperatorResolveResult(OperatorResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitTypeIsResolveResult(TypeIsResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitArrayCreateResolveResult(ArrayCreateResolveResult res)
        {
            if (res.InitializerElements.IsNotNullOrEmpty())
            {
                var array = res.InitializerElements.Select(Visit).ToArray();
                var arrayType = res.Type.GetMirroredClrType();
                //HACK: bug in parser
                if (arrayType.GetElementType().IsArray)
                    arrayType = arrayType.GetElementType();
                var array2 = Array.CreateInstance(arrayType.GetElementType(), array.Length);
                array.CopyTo(array2, 0);
                return array2;
            }
            return null;
        }

        public object VisitArrayAccessResolveResult(ArrayAccessResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitInitializedObjectResolveResult(InitializedObjectResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitByReferenceResolveResult(ByReferenceResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitNamedArgumentResolveResult(NamedArgumentResolveResult res)
        {
            throw new NotImplementedException("VisitNamedArgumentResolveResult");
        }

        #endregion

        #region ICSharpResolveResultVisitor<object> Members

        public object VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitLambdaResolveResult(LambdaResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitMethodGroupResolveResult(MethodGroupResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitDynamicInvocationResolveResult(DynamicInvocationResolveResult res)
        {
            throw new NotImplementedException();
        }

        public object VisitDynamicMemberResolveResult(DynamicMemberResolveResult res)
        {
            throw new NotImplementedException();
        }

		public object VisitAwaitResolveResult(AwaitResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitAmbiguousTypeResolveResult(AmbiguousTypeResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitUnknownMethodResolveResult(UnknownMethodResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitSizeOfResolveResult(SizeOfResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitErrorResolveResult(ErrorResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitForEachResolveResult(ForEachResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitNamespaceResolveResult(NamespaceResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitUnknownMemberResolveResult(UnknownMemberResolveResult res)
		{
			throw new NotImplementedException();
		}

		public object VisitUnknownIdentifierResolveResult(UnknownIdentifierResolveResult res)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
