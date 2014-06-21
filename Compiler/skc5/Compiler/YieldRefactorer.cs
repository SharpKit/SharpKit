using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpKit.JavaScript.Ast;

namespace SharpKit.Compiler
{
    class YieldRefactorer
    {
        public JsFunction BeforeFunction { get; set; }
        public JsFunction AfterFunction { get; set; }

        Dictionary<JsNode, JsNode> Parents = new Dictionary<JsNode, JsNode>();
        void SetParents(JsNode node)
        {
            foreach (var ch in node.Children())
            {
                Parents[ch] = node;
                SetParents(ch);
            }
        }
        JsNode GetParent(JsNode node)
        {
            return Parents.TryGetValue(node);
        }
        public void Process()
        {
            AfterFunction = BeforeFunction;
            SetParents(BeforeFunction.Block);
            foreach (var me in BeforeFunction.Block.Descendants<JsMemberExpression>().ToList())
            {
                if (me.PreviousMember == null && me.NodeType == JsNodeType.MemberExpression)
                    me.PreviousMember = Js.This();
            }

            BeginNewStep();
            ProcessStatement(BeforeFunction.Block);
            BeforeFunction.Block.Statements.Clear();

            var func = new JsFunction { Block = new JsBlock { Statements = new List<JsStatement>() } };
            var i = 0;
            func.Block.Statements.Add(Js.Var("result").Statement());
            var stSwitch = Js.Switch(_state());
            var lastStep = Js.Block().Add(_state().Assign(Js.Value(Steps.Count)).Statement()).Add(new JsBreakStatement());
            Steps.Add(new YieldStep { Statements = { lastStep } });
            foreach (var step in Steps)
            {
                stSwitch.Case(Js.Value(i), step.Statements);
                i++;
            }
            func.Block.Statements.Add(stSwitch);
            func.Block.Statements.Add(Js.Member("result").Assign(Js.Value(false)).Statement());
            func.Block.Statements.Add(Js.Return(Js.Member("result")));

            BeforeFunction.Block.Statements.Add(Js.Return(Js.New(Js.Member("CustomEnumerable"), func)));
            return;
        }

        void BeginNewStep()
        {
            Steps.Add(new YieldStep());
            AddToCurrentStep(_state().Assign(Js.Value(-1)).Statement());
        }

        void AddToCurrentStep(JsStatement st)
        {
            Steps.Last().Statements.Add(st);
        }

        void YieldReturnInCurrentStep(JsExpression item)
        {
            var result = Js.Value(true);
            var stepIndex = Steps.Count - 1;
            AddToCurrentStep(Js.This().Member("_current").Assign(item).Statement());
            AddToCurrentStep(_state().Assign(Js.Value(stepIndex + 1)).Statement());
            AddToCurrentStep(Js.Member("result").Assign(result).Statement());
            AddToCurrentStep(Js.Return(result));
        }


        object ProcessStatement(JsStatement node)
        {
            if (node is JsYieldReturnStatement)
            {
                var st2 = (JsYieldReturnStatement)node;
                YieldReturnInCurrentStep(st2.Expression);
                BeginNewStep();
                return null;
            }
            else if (node is JsVariableDeclarationStatement)
            {
                var node2 = (JsVariableDeclarationStatement)node;
                var decl = node2.Declaration.Declarators.Single();
                var node3 = Js.This().Member(decl.Name).Assign(decl.Initializer).Statement();
                AddToCurrentStep(node3);
                return null;
            }

            if (node is JsBlock)
            {
                //BeginNewStep();
                var block = (JsBlock)node;
                foreach (var st in block.Statements)
                {
                    ProcessStatement(st);
                }
            }
            else if (node is JsWhileStatement)
            {
                BeginNewStep();
                var st = (JsWhileStatement)node;
                ProcessStatement(st.Statement);
                var step = Steps.Last();
                st.Statement = new JsBlock { Statements = step.Statements.ToList() };
                step.Statements.Clear();
                step.Statements.Add(st);
                BeginNewStep();
            }
            else
            {
                AddToCurrentStep(node);
            }
            return node;
        }

        private static JsMemberExpression _state()
        {
            return Js.This().Member("_state");
        }

        List<YieldStep> Steps = new List<YieldStep>();

        private void ReplaceNode(JsNode node, JsNode node2)
        {
            var parent = GetParent(node);
            if (parent is JsBlock)
            {
                var block = (JsBlock)parent;
                var index = block.Statements.IndexOf((JsStatement)node);
                if (index < 0)
                    throw new Exception("ReplaceNode Failed");
                block.Statements[index] = (JsStatement)node2;
                return;
            }
            foreach (var pe in parent.GetType().GetProperties())
            {
                var obj = pe.GetValue(parent, null);
                if (obj == node)
                {
                    pe.SetValue(parent, node2, null);
                    return;
                }
            }
            throw new Exception("ReplaceNode failed");
        }
    }

    class JsYieldStatement : JsStatement
    {
    }
    class JsYieldReturnStatement : JsYieldStatement
    {
        public JsExpression Expression { get; set; }
        public override IEnumerable<JsNode> Children()
        {
            if (Expression != null)
                yield return Expression;
        }
    }

    class JsYieldBreakStatement : JsYieldStatement
    {
    }
    class YieldStep
    {
        public YieldStep()
        {
            Statements = new List<JsStatement>();
        }
        public List<JsStatement> Statements { get; set; }
    }
}
