using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using SharpKit.Compiler;
using System.Diagnostics;
using ICSharpCode.NRefactory.TypeSystem;
using Mirrored.SharpKit.JavaScript;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler.CsToJs
{
	partial class AstNodeConverter : IAstVisitor<JsNode>
	{
		public AstNodeConverter()
		{
			ResolveResultConverter = new ResolveResultConverter { AstNodeConverter = this };
		}

		public void Init()
		{
			ResolveResultConverter.Compiler = Compiler;
			ResolveResultConverter.Log = Log;
		}

		public ResolveResultConverter ResolveResultConverter { get; set; }
		public CompilerTool Compiler { get; set; }
		public CompilerLogger Log { get; set; }
		public bool ExportComments { get; set; }
		public SkProject Project { get { return Compiler.Project; } }
		public event Action<AstNode> BeforeConvertCsToJsAstNode;
		public event Action<AstNode, JsNode> AfterConvertCsToJsAstNode;
		public bool SupportClrYield = false;

		#region Visit
		int VisitDepth;
		const int MaxVisitDepth = 100;
		[DebuggerStepThrough]
		public JsNode Visit(AstNode node)
		{
			if (node == null)
				return null;
			VisitDepth++;
			if (VisitDepth > MaxVisitDepth)
				throw new Exception("StackOverflow imminent, depth>" + MaxVisitDepth);
			try
			{
				var node2 = _Visit(node);
				return node2;
			}
			catch (CompilerException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new CompilerException(node, e);
			}
			finally
			{
				VisitDepth--;
			}
		}
		[DebuggerStepThrough]
		JsNode _Visit(AstNode node)
		{
			if (CompilerConfiguration.Current.EnableLogging)
			{
				var region = node.GetRegion();
				if (!region.IsEmpty)
					Log.Debug(String.Format("JsCodeImporter: Visit AstNode: {0}", ToDebug(node)));
			}
			if (BeforeConvertCsToJsAstNode != null)
				BeforeConvertCsToJsAstNode(node);
			var node2 = node.AcceptVisitor(this);
			if (node2 != null)
			{
				if (node2.Annotation<AstNode>() == null)
					node2.AddAnnotation(node);
			}
			if (AfterConvertCsToJsAstNode != null)
				AfterConvertCsToJsAstNode(node, node2);
			return node2;

		}
		internal string ToDebug(AstNode node)
		{
			if (node == null)
				return null;
			var region = node.GetRegion();
			if (!region.IsEmpty)
				return String.Format("{1}: [{2}, {3}] - {0}", node.GetType().Name, region.FileName, region.BeginLine, region.BeginColumn);
			return node.GetType().Name;
		}
		[DebuggerStepThrough]
		private List<JsExpression> VisitExpressions(IEnumerable<Expression> nodes)
		{
			return nodes.Select(VisitExpression).ToList();
		}
		[DebuggerStepThrough]
		public JsExpression VisitExpression(Expression node)
		{
			return (JsExpression)Visit(node);
		}
		[DebuggerStepThrough]
		private JsStatement VisitStatement(Statement node)
		{
			return (JsStatement)Visit(node);
		}

		public string _Visit(BinaryOperatorType op)
		{
			return BinaryOperatorExpression.GetOperatorRole(op).Token;
		}
		public string _Visit(UnaryOperatorType op)
		{
			return UnaryOperatorExpression.GetOperatorRole(op).Token;
		}
		private List<JsStatement> VisitStatements(AstNodeCollection<Statement> list)
		{
			return list.Select(VisitStatement).ToList();
		}

		#endregion

		#region Utils

		internal static InitializedObjectResolveResult FindInitializedObjectResolveResult(CSharpInvocationResolveResult res)
		{
			var init1 = res.InitializerStatements[0];
			while (init1 != null && !(init1 is InitializedObjectResolveResult))
			{
				if (init1 is OperatorResolveResult)
				{
					init1 = ((OperatorResolveResult)init1).Operands[0];
				}
				else if (init1 is CSharpInvocationResolveResult)
				{
					init1 = ((CSharpInvocationResolveResult)init1).TargetResult;
				}
				else if (init1 is MemberResolveResult)
				{
					init1 = ((MemberResolveResult)init1).TargetResult;
				}
				else
				{
					throw new NotImplementedException("FindInitializedObjectResolveResult");
				}
			}
			return ((InitializedObjectResolveResult)init1);

		}

		#endregion

		#region Visit Utils

		[System.Diagnostics.DebuggerStepThrough]
		public JsExpression VisitExpression(ResolveResult res)
		{
			return ResolveResultConverter.VisitExpression(res);
		}
		[System.Diagnostics.DebuggerStepThrough]
		public List<JsExpression> VisitExpressions(IList<ResolveResult> reses)
		{
			return ResolveResultConverter.VisitExpressions(reses);
		}
		[System.Diagnostics.DebuggerStepThrough]
		public JsNode Visit(ResolveResult res)
		{
			return ResolveResultConverter.Visit(res);
		}

		#endregion


		#region IAstVisitor<JsNode>

		#region IAstVisitor<JsNode> Members

		public JsNode VisitAnonymousMethodExpression(AnonymousMethodExpression node)
		{
			return Visit(node.Resolve());
			//var func = new JsFunction();
			//func.Parameters = node.Parameters.Select(t => t.Name).ToList();
			//var body = Visit(node.Body);
			//func.Block = (JsBlock)body;
			//return CreateJsDelegateIfNeeded(func, node, true);
		}


		public JsNode VisitArrayCreateExpression(ArrayCreateExpression node)
		{
			return Visit(node.Resolve());
		}


		public JsNode VisitAsExpression(AsExpression node)
		{
			return Visit(node.Resolve());
		}

		public JsNode VisitAssignmentExpression(AssignmentExpression node)
		{
			var res = node.Resolve();
			if (res.Type.Kind == TypeKind.Dynamic)
			{
				return VisitExpression(node.Left).Assign(VisitExpression(node.Right));
			}
			return Visit(res);
		}


		public JsNode VisitBinaryOperatorExpression(BinaryOperatorExpression node)
		{
			return Visit(node.Resolve());
		}

		public JsNode VisitCastExpression(CastExpression node)
		{
			var res = node.Resolve();
			return Visit(res);
		}


		public JsNode VisitConditionalExpression(ConditionalExpression node)
		{
			return Visit(node.Resolve());
		}

		public JsNode VisitDefaultValueExpression(DefaultValueExpression node)
		{
			return Visit(node.Resolve());
			//return Js.Member("Default").Invoke(SkJs.EntityTypeRefToMember(node.type.entity_typeref));
		}


		public JsNode VisitIdentifierExpression(IdentifierExpression node)
		{
			var res = node.Resolve();
			return Visit(res);
		}

		public JsNode VisitIndexerExpression(IndexerExpression node)
		{
			return Visit(node.Resolve());
		}

		public JsNode VisitInvocationExpression(InvocationExpression node)
		{
			var res = node.Resolve();
			//TEMP: danel
			//if (res.Type.Kind == TypeKind.Dynamic)
			//{
			//    var node2 = Js.Invoke(VisitExpression(node.Target), VisitExpressions(node.Arguments).ToArray());
			//    return node2;
			//}
			var node3 = Visit(res);
			return node3;
		}

		public JsNode VisitIsExpression(IsExpression node)
		{
			var res2 = node.Resolve();
			return Visit(res2);
		}

		public JsNode VisitLambdaExpression(LambdaExpression node)
		{
			return Visit(node.Resolve());
		}

		public JsNode VisitMemberReferenceExpression(MemberReferenceExpression node)
		{
			var res = node.Resolve();

			// The following is block uncommented because: See issue 352

			//if (res.Type.Kind == TypeKind.Dynamic)
			//{
			//    return VisitExpression(node.Target).Member(node.MemberName);
			//}

			return Visit(res);
		}


		public JsNode VisitNamedExpression(NamedExpression node)
		{
			var name = new JsJsonMember { Name = node.Name };
			if (name.Name.IsNullOrEmpty())
			{
				throw new NotImplementedException();
				//if (d.expression.e == cs_node.n_simple_name)
				//    name.Name = ((CsSimpleName)d.expression).identifier.identifier;
				//else if (d.expression.e == cs_node.n_primary_expression_member_access)
				//    name.Name = ((CsPrimaryExpressionMemberAccess)d.expression).identifier.identifier;
			}
			var value = VisitExpression(node.Expression);
			var ce = node.GetParentType();
			var nativeJson = Sk.UseNativeJsons(ce.GetDefinitionOrArrayType());

			if (!nativeJson)
			{
				name.Name = "get_" + name.Name;
				value = new JsFunction { Block = new JsBlock { Statements = new List<JsStatement> { new JsReturnStatement { Expression = value } } } };
			}
			return new JsJsonNameValue { Name = name, Value = value };
		}

		public JsNode VisitNullReferenceExpression(NullReferenceExpression node)
		{
			return Js.Null();
		}

		public JsNode VisitObjectCreateExpression(ObjectCreateExpression node)
		{
			var res = node.Resolve();
			return Visit(res);
		}

		public JsNode VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression node)
		{
			return Visit(node.Resolve());
		}

		public JsNode VisitParenthesizedExpression(ParenthesizedExpression node)
		{
			var res = node.Resolve();
			var node2 = Visit(res);
			return node2;
		}


		public JsNode VisitPrimitiveExpression(PrimitiveExpression node)
		{
			return Visit(node.Resolve());
		}


		public JsNode VisitThisReferenceExpression(ThisReferenceExpression node)
		{
			return Visit(node.Resolve());
		}

		public JsNode VisitTypeOfExpression(TypeOfExpression node)
		{
			return Visit(node.Resolve());
		}


		public JsNode VisitUnaryOperatorExpression(UnaryOperatorExpression node)
		{
			return Visit(node.Resolve());
			//return new JsPreUnaryExpression { Operator = _Visit(node.Operator), Right = VisitExpression(node.Expression) };
		}

		public JsNode VisitQueryExpression(QueryExpression node)
		{
			var res = node.Resolve();
			return Visit(res);
		}


		public JsNode VisitBlockStatement(BlockStatement node)
		{
			CommentsExporter cmt = null;
			if (ExportComments)
				cmt = new CommentsExporter { Nodes = node.Children.ToList() };
			var statements = new List<JsStatement>();
			foreach (var st in node.Statements)
			{
				var st2 = VisitStatement(st);
				if (cmt != null)
					st2.Comments = cmt.ExportCommentsUptoNode(st);
				statements.Add(st2);
			}
			var block = new JsBlock { Statements = new List<JsStatement>() };
			if (cmt != null)
				block.Comments = cmt.ExportAllLeftoverComments();
			foreach (var st in statements)
			{
				if (st is JsBlock)
				{
					var block2 = (JsBlock)st;
					if (block2.Statements != null)
						block.Statements.AddRange(block2.Statements);
				}
				else
				{
					block.Statements.Add(st);
				}
			}
			return block;
		}


		public JsNode VisitBreakStatement(BreakStatement node)
		{
			return new JsBreakStatement();
		}


		public JsNode VisitContinueStatement(ContinueStatement node)
		{
			return new JsContinueStatement();
		}

		public JsNode VisitDoWhileStatement(DoWhileStatement node)
		{
			var node2 = new JsDoWhileStatement { Statement = VisitStatement(node.EmbeddedStatement), Condition = VisitExpression(node.Condition) };
			return node2;
		}

		public JsNode VisitEmptyStatement(EmptyStatement node)
		{
			return new JsStatement();
		}

		public JsNode VisitExpressionStatement(ExpressionStatement node)
		{
			var exp2 = VisitExpression(node.Expression);
			if (exp2 == null)
				return new JsStatement();
			if (exp2.Is(JsNodeType.MemberExpression))
			{
				var me = (JsMemberExpression)exp2;
				if (me.Name.IsNullOrEmpty() && me.PreviousMember == null)
					return new JsStatement();
			}
			return new JsExpressionStatement { Expression = exp2 };
		}
		int VariableIteratorCounter = 1;


		public JsNode VisitForeachStatement(ForeachStatement node)
		{
			if (node.InExpression != null)
			{
				var expRes = node.InExpression.Resolve();
				var et = expRes.Type.GetDefinitionOrArrayType();
				//var et = node.expression.entity_typeref.GetEntityType();
				if (et != null)
				{
					var jta = Sk.GetJsTypeAttribute(et);
					if (jta != null && jta.NativeEnumerator)
					{
						var node2 = new JsForInStatement
						{
							Initializer = Js.Var(node.VariableName),
							Member = VisitExpression(node.InExpression),
							Statement = VisitStatement(node.EmbeddedStatement)
						};
						return node2;
					}
					else if (jta != null && jta.NativeArrayEnumerator)
					{
						VariableIteratorCounter++;
						var iteratorName = "$i" + VariableIteratorCounter;
						var lengthCacheName = "$l" + VariableIteratorCounter;
						var exp2 = VisitExpression(node.InExpression);
						var target = exp2;
						var targetCacheName = "$t" + VariableIteratorCounter;
						if (exp2.NodeType != JsNodeType.MemberExpression || ((JsMemberExpression)exp2).PreviousMember != null)//is not simple name
						{
							target = Js.Member(targetCacheName);

						}
						var itemAccess = target.IndexerAccess(Js.Member(iteratorName));
						var node2 = new JsForStatement();

						node2.Condition = Js.Member(iteratorName).LessThan(Js.Member(lengthCacheName));
						node2.Iterators = new List<JsStatement> { Js.Member(iteratorName).PlusPlus().Statement(), Js.Member(node.VariableName).Assign(itemAccess).Statement() };
						if (target != exp2)//use target caching
						{
							node2.Initializers = new List<JsStatement> { Js.Var(iteratorName, Js.Value(0)).AndVar(targetCacheName, exp2.Clone()).AndVar(lengthCacheName, target.Clone().Member("length")).AndVar(node.VariableName, itemAccess.Clone()).Statement() };
						}
						else
						{
							node2.Initializers = new List<JsStatement> { Js.Var(iteratorName, Js.Value(0)).AndVar(lengthCacheName, exp2.Clone().Member("length")).AndVar(node.VariableName, itemAccess.Clone()).Statement() };
						}
						node2.Statement = VisitStatement(node.EmbeddedStatement);
						return node2;
					}
				}
			}

			var iteratorName2 = "$it" + VariableIteratorCounter;
			VariableIteratorCounter++;
			var node3 = Js.Var(iteratorName2, VisitExpression(node.InExpression).Member("GetEnumerator").Invoke()).Statement();
			var whileNode = Js.While(Js.Member(iteratorName2).Member("MoveNext").Invoke());
			var getCurrentStatement = Js.Var(node.VariableName, Js.Member(iteratorName2).Member("get_Current").Invoke()).Statement();
			var jsStatement = VisitStatement(node.EmbeddedStatement);
			JsBlock block;
			if (jsStatement is JsBlock)
				block = (JsBlock)jsStatement;
			else
				block = Js.Block().Add(jsStatement);
			block.Statements.Insert(0, getCurrentStatement);
			whileNode.Statement = block;

			var block2 = Js.Block().Add(node3).Add(whileNode);
			return block2;
		}

		public JsNode VisitForStatement(ForStatement node)
		{
			var node2 = new JsForStatement
			{
				Condition = VisitExpression(node.Condition),
				Statement = VisitStatement(node.EmbeddedStatement),
			};

			if (node.Iterators != null)
				node2.Iterators = VisitStatements(node.Iterators);
			if (node.Initializers != null)
				node2.Initializers = VisitStatements(node.Initializers);
			return node2;
		}


		public JsNode VisitIfElseStatement(IfElseStatement node)
		{
			return new JsIfStatement { Condition = VisitExpression(node.Condition), IfStatement = VisitStatement(node.TrueStatement), ElseStatement = VisitStatement(node.FalseStatement) };
		}


		public JsNode VisitReturnStatement(ReturnStatement node)
		{
			return Js.Return(VisitExpression(node.Expression));
		}

		public JsNode VisitSwitchStatement(SwitchStatement node)
		{
			return new JsSwitchStatement
			{
				Expression = VisitExpression(node.Expression),
				Sections = node.SwitchSections.Select(t => (JsSwitchSection)Visit(t)).ToList(),
			};
		}

		public JsNode VisitSwitchSection(SwitchSection node)
		{
			return new JsSwitchSection
			{
				Labels = node.CaseLabels.Select(t => (JsSwitchLabel)Visit(t)).ToList(),
				Statements = node.Statements.Select(VisitStatement).ToList(),
			};
		}

		public JsNode VisitCaseLabel(CaseLabel node)
		{
			var node2 = new JsSwitchLabel
			{
				IsDefault = node.Expression.IsNull,	//the alternative doesn't work: node.Role == CaseLabel.DefaultKeywordRole,
				Expression = VisitExpression(node.Expression),
			};
			return node2;
		}

		public JsNode VisitThrowStatement(ThrowStatement node)
		{
			JsExpression node2;
			IType exceptionType;
			if (node.Expression == null || node.Expression.IsNull) //happens when performing "throw;"
			{
				var cc = node.GetParent<CatchClause>();
				if (cc != null)
				{
					node2 = Js.Member(cc.VariableName);
					var type = cc.Type;
					if (type == null || type.IsNull)
						exceptionType = Project.Compilation.FindType(KnownTypeCode.Exception);
					else
						exceptionType = cc.Type.Resolve().Type;
				}
				else
					throw new Exception("Rethrow not supported, catch clause not found");
			}
			else
			{
				node2 = VisitExpression(node.Expression);
				var res = node.Expression.Resolve();
				exceptionType = res.Type;
				if (res is ConversionResolveResult)
				{
					exceptionType = ((ConversionResolveResult)res).Input.Type;
				}
			}
			if (!Sk.IsNativeError(exceptionType.GetDefinitionOrArrayType()))
			{
				node2 = Js.Member("$CreateException").Invoke(node2, Js.New(Js.Member("Error")));
			}
			return new JsThrowStatement { Expression = node2 };
		}

		public JsNode VisitTryCatchStatement(TryCatchStatement node)
		{
			var node2 = new JsTryStatement { TryBlock = (JsBlock)Visit(node.TryBlock) };
			if (node.CatchClauses != null && node.CatchClauses.Count > 0)
			{
				if (node.CatchClauses.Count > 1)
					throw new CompilerException(node, "Client code may not have more than one catch clause, due to JavaScript limitation");
				node2.CatchClause = (JsCatchClause)Visit(node.CatchClauses.First());
			}
			if (node.FinallyBlock != null)
				node2.FinallyBlock = (JsBlock)Visit(node.FinallyBlock);
			return node2;
		}
		int VariableExceptionCounter = 1;

		public JsNode VisitCatchClause(CatchClause node)
		{
			var node2 = new JsCatchClause();

			if (node.VariableName.IsNullOrEmpty())
				node.VariableName = "$$e" + (VariableExceptionCounter++); //Generate a psuedo-unique variable name
			node2.IdentifierName = node.VariableName;

			node2.Block = (JsBlock)Visit(node.Body);
			if (node2.Block != null)
			{
				node2.Descendants<JsThrowStatement>().Where(t => t.Expression == null).ForEach(t => t.Expression = Js.Member(node2.IdentifierName));
			}
			return node2;
		}

		int VariableResourceCounter = 1;

		public JsNode VisitUsingStatement(UsingStatement node)
		{
			var st3 = Visit(node.ResourceAcquisition);
			JsVariableDeclarationStatement stVar;
			if (st3 is JsExpression)
			{
				stVar = Js.Var("$r" + VariableResourceCounter++, (JsExpression)st3).Statement();
			}
			else
			{
				stVar = (JsVariableDeclarationStatement)st3;
			}
			var trySt = VisitStatement(node.EmbeddedStatement);
			var st2 = new JsTryStatement { TryBlock = trySt.ToBlock(), FinallyBlock = Js.Block() };

			//var resource = node.ResourceAcquisition;
			//var decl = resource as VariableDeclarationStatement;
			//if (decl == null || decl.Variables.Count == 0)
			//    throw new Exception("using statement is supported only with the var keyword in javascript. Example: using(var g = new MyDisposable()){}");
			foreach (var dr in stVar.Declaration.Declarators)
			{
				st2.FinallyBlock.Add(Js.Member(dr.Name).Member("Dispose").Invoke().Statement());
			}
			return Js.Block().Add(stVar).Add(st2); //TODO: get rid of block
		}

		public JsNode VisitVariableDeclarationStatement(VariableDeclarationStatement node)
		{
			var vars = node.Variables.Select(Visit).Cast<JsVariableDeclarator>().ToList();

			return new JsVariableDeclarationStatement { Declaration = new JsVariableDeclarationExpression { Declarators = vars } };
		}

		public JsNode VisitWhileStatement(WhileStatement node)
		{
			return new JsWhileStatement { Condition = VisitExpression(node.Condition), Statement = (JsStatement)Visit(node.EmbeddedStatement) };
		}

		public JsNode VisitYieldBreakStatement(YieldBreakStatement node)
		{
			if (SupportClrYield)
			{
				return new JsYieldBreakStatement();
			}

			var me = node.GetCurrentMethod();
			var node2 = GenerateYieldReturnStatement(me);

			return node2;
		}

		public static JsReturnStatement GenerateYieldReturnStatement(IMethod me)
		{
			JsReturnStatement node2;
			if (me != null && me.ReturnType != null && me.ReturnType.FullName.StartsWith("System.Collections.Generic.IEnumerator"))
				node2 = Js.Return(Js.Member("$yield").Member("GetEnumerator").Invoke());
			else
				node2 = Js.Return(Js.Member("$yield"));
			return node2;
		}


		public JsNode VisitYieldReturnStatement(YieldReturnStatement node)
		{
			var exp2 = VisitExpression(node.Expression);
			if (SupportClrYield)
			{
				return new JsYieldReturnStatement { Expression = exp2 };
			}
			var node2 = Js.Member("$yield.push").Invoke(exp2).Statement();
			return node2;
		}


		public JsNode VisitVariableInitializer(VariableInitializer node)
		{
			return new JsVariableDeclarator { Name = node.Name, Initializer = VisitExpression(node.Initializer) };
		}

		public JsNode VisitUncheckedStatement(UncheckedStatement node)
		{
			return Visit(node.Body);
		}

		#endregion

		#region NotImplemented

		public JsNode VisitUnsafeStatement(UnsafeStatement node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitFixedFieldDeclaration(FixedFieldDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitFixedVariableInitializer(FixedVariableInitializer node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitSyntaxTree(SyntaxTree node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitSimpleType(SimpleType node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitMemberType(MemberType node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitComposedType(ComposedType node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitArraySpecifier(ArraySpecifier node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitPrimitiveType(PrimitiveType node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitComment(Comment node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitPreProcessorDirective(PreProcessorDirective node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitDocumentationReference(DocumentationReference node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitTypeParameterDeclaration(TypeParameterDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitConstraint(Constraint node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitCSharpTokenNode(CSharpTokenNode node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitIdentifier(Identifier node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitWhitespace(WhitespaceNode node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitText(TextNode node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitNewLine(NewLineNode node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitUndocumentedExpression(UndocumentedExpression node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitAccessor(Accessor node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitConstructorDeclaration(ConstructorDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitConstructorInitializer(ConstructorInitializer node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitDestructorDeclaration(DestructorDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitEnumMemberDeclaration(EnumMemberDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitEventDeclaration(EventDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitCustomEventDeclaration(CustomEventDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitFieldDeclaration(FieldDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitIndexerDeclaration(IndexerDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitMethodDeclaration(MethodDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitOperatorDeclaration(OperatorDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitParameterDeclaration(ParameterDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitPropertyDeclaration(PropertyDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitBaseReferenceExpression(BaseReferenceExpression node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitCheckedExpression(CheckedExpression node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitDirectionExpression(DirectionExpression node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitNamedArgumentExpression(NamedArgumentExpression node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitUncheckedExpression(UncheckedExpression node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitArrayInitializerExpression(ArrayInitializerExpression node)
		{
			return Visit(node.Resolve());
		}
		public JsNode VisitPointerReferenceExpression(PointerReferenceExpression node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitSizeOfExpression(SizeOfExpression node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitStackAllocExpression(StackAllocExpression node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitTypeReferenceExpression(TypeReferenceExpression node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitQueryContinuationClause(QueryContinuationClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQueryFromClause(QueryFromClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQueryLetClause(QueryLetClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQueryWhereClause(QueryWhereClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQueryJoinClause(QueryJoinClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQueryOrderClause(QueryOrderClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQueryOrdering(QueryOrdering node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQuerySelectClause(QuerySelectClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitQueryGroupClause(QueryGroupClause node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitAttribute(ICSharpCode.NRefactory.CSharp.Attribute node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitAttributeSection(AttributeSection node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitDelegateDeclaration(DelegateDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitNamespaceDeclaration(NamespaceDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitTypeDeclaration(TypeDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitUsingAliasDeclaration(UsingAliasDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitUsingDeclaration(UsingDeclaration node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitExternAliasDeclaration(ExternAliasDeclaration node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitCheckedStatement(CheckedStatement node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitFixedStatement(FixedStatement node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitGotoCaseStatement(GotoCaseStatement node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitGotoDefaultStatement(GotoDefaultStatement node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitGotoStatement(GotoStatement node)
		{
			throw new NotImplementedException();
		}
		public JsNode VisitLabelStatement(LabelStatement node)
		{
			throw new NotImplementedException();
		}

		public JsNode VisitLockStatement(LockStatement node)
		{
			return Visit(node.EmbeddedStatement);
		}

		public JsNode VisitNullNode(AstNode node)
		{
			if (node == Statement.Null)
				return Js.Null().Statement();
			return Js.Null();
		}

		public JsNode VisitErrorNode(AstNode node)
		{
			throw new NotImplementedException();
		}
		#endregion

		#endregion




	}
}
