using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpKit.JavaScript.Ast
{
    static class JsModelExtensions
    {
        public static IEnumerable<JsNode> DescendantsAndSelf(this JsNode node)
        {
            return GetDescendants(node, true);
        }
        public static IEnumerable<JsNode> Descendants(this JsNode node)
        {
            return GetDescendants(node, false);
        }
        public static IEnumerable<T> Descendants<T>(this JsNode node) where T : JsNode
        {
            return node.Descendants().OfType<T>();
        }

        public static T Clone<T>(this T node) where T:JsNode
        {
            if (node == null)
                return null;
            var node2 = (T)node.New();
            node.Clone(node2);
            return node2;
        }

        // System.Xml.Linq.XContainer
        static IEnumerable<JsNode> GetDescendants(JsNode node, bool self)
        {
            if (self)
                yield return node;
            var list = node.Children().ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var child = list[i];
                yield return child;
                var children = child.Children();
                if (children.Contains(null))
                {
                    var x = node.ToParentedNode().Descendants().ToList();
                    var xx = x.Where(t => t.Node == null).FirstOrDefault();
                }
                list.AddRange(children);
            }
        }
        static JsParentedNode ToParentedNode(this JsNode node)
        {
            return new JsParentedNode(node, null);
        }

        static IEnumerable<JsParentedNode> ParentedChildren(this JsNode node)
        {
            return node.ToParentedNode().Children();
        }
        static IEnumerable<JsParentedNode> Children(this JsParentedNode node)
        {
            return node.Node.Children().Select(t => new JsParentedNode(t, node));
        }


        static IEnumerable<JsParentedNode> Descendants(this JsParentedNode node)
        {
            var list = node.Children().ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var child = list[i];
                yield return child;
                if (child.Node != null)
                {
                    var children = child.Children();
                    list.AddRange(children);
                }
            }
        }

        public static LinkedListNode<JsToken> PreviousVisibleToken(this LinkedListNode<JsToken> node)
        {
            var x = node.Previous;
            while (x != null)
            {
                if (x.Value.Value.IsNotNullOrEmpty())
                    return x;
                x = x.Previous;
            }
            return null;
        }

        public static LinkedListNode<JsToken> NextVisibleToken(this LinkedListNode<JsToken> node)
        {
            var x = node.Next;
            while (x!=null)
            {
                if (x.Value.Value.IsNotNullOrEmpty())
                    return x;
                x = x.Next;
            }
            return null;

        }


        public static IEnumerable<LinkedListNode<T>> NextNodes<T>(this LinkedListNode<T> node)
        {
            var x = node.Next;
            while (x != null)
            {
                yield return x;
                x = x.Next;
            }
        }
        public static IEnumerable<LinkedListNode<T>> PreviousNodes<T>(this LinkedListNode<T> node)
        {
            var x = node.Previous;
            while (x != null)
            {
                yield return x;
                x = x.Previous;
            }
        }

        public static IEnumerable<T> NextValues<T>(this LinkedListNode<T> node)
        {
            return node.NextNodes().Select(t => t.Value);
        }
        public static IEnumerable<T> PreviousValues<T>(this LinkedListNode<T> node)
        {
            return node.PreviousNodes().Select(t => t.Value);
        }
        public static bool IsSingleNestedStatement(this JsNode node)
        {
            if (!(node is JsStatement))
                return false;
            if (node.Is(JsNodeType.Block))
                return false;
            var parent = node.Parent;
            if (parent == null)
                return false;
            return parent.IsAny(JsNodeType.IfStatement, JsNodeType.ForStatement, JsNodeType.ForInStatement, JsNodeType.WhileStatement, JsNodeType.DoWhileStatement);
        }
        public static bool IsElseIf(this JsNode node)
        {
            return node.Is(JsNodeType.IfStatement) && node.Parent.Is(JsNodeType.IfStatement);
        }

        public static int RemoveRange<T>(this LinkedList<T> list, LinkedListNode<T> from, LinkedListNode<T> to)
        {
            var count = 0;
            var x = from;
            while (x != null)
            {
                var y = x.Next;
                list.Remove(x);
                count++;
                if (x == to)
                    break;
                x = y;
            }
            return count;
        }

    }

    public class JsParentedNode
    {
        public JsParentedNode(JsNode node, JsParentedNode parent)
        {
            Node = node;
            Parent = parent;
        }
        public JsNode Node { get; private set; }
        public JsParentedNode Parent { get; private set; }
        public override bool Equals(object obj)
        {
            var x = obj as JsParentedNode;
            if (x != null && x.Node == Node && x.Parent == Parent)
                return true;
            if (obj == this)
                return true;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Node == null)
                return 0;
            return Node.GetHashCode();
        }
    }
}
