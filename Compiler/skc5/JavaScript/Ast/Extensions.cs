using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    static class JsNodeExtensions
    {
        public static bool Is(this JsNode node, JsNodeType type)
        {
            return node != null && node.NodeType == type;
        }

        public static JsStatement GetStatement(this JsNode node)
        {
            var x = node;
            while (x != null)
            {
                if (x is JsStatement)
                    return (JsStatement)x;
                x = x.Parent;
            }
            return null;
        }
        public static bool IsSwitchSectionStatement(this JsNode node)
        {
            var st = node.GetStatement();
            if (st != null && st.Parent != null && st.Parent.Is(JsNodeType.SwitchSection))
            {
                var section = ((JsSwitchSection)st.Parent);
                var x = section.Statements.Contains(st);
                return x;
            }
            return false;
        }


        public static bool IsCodeStatement(this JsNode node)
        {
            return node.Is(JsNodeType.CodeStatement);
        }
        public static bool IsCodeExpression(this JsNode node)
        {
            return node != null && node.NodeType == JsNodeType.CodeExpression;
        }
        public static bool IsNot(this JsNode node, JsNodeType type)
        {
            return node == null || node.NodeType != type;
        }
        public static bool IsAny(this JsNode node, List<JsNodeType> list)
        {
            if (node == null)
                return false;
            foreach (var type in list)
            {
                if (node.NodeType == type)
                    return true;
            }
            return false;
        }
        public static bool IsAny(this JsNode node, params JsNodeType[] list)
        {
            if (node == null)
                return false;
            foreach (var type in list)
            {
                if (node.NodeType == type)
                    return true;
            }
            return false;
        }
    }
}
