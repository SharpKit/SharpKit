using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using System.CodeDom.Compiler;
using ICSharpCode.NRefactory.CSharp;

namespace SharpKit.Compiler
{
    class CommentsExporter
    {
        int CurrentTokenIndex = -1;

        public List<string> ExportAllLeftoverComments()
        {
            return ExportCommentsUptoNode(null);//MaxLine.GetValueOrDefault(int.MaxValue));
        }
        public List<AstNode> Nodes { get; set; }
        public List<string> ExportCommentsUptoNode(AstNode stopNode)
        {
            if (CurrentTokenIndex == -1)
                CurrentTokenIndex = 0;
            var sb = new StringBuilder();
            var started = false;
            var lines = new List<string>();
            while (CurrentTokenIndex < Nodes.Count)
            {
                var token = Nodes[CurrentTokenIndex];
                if (token == stopNode)
                    break;
                if (token is Comment)
                {
                    var cmt = (Comment)token;
                    started = true;
                    if (cmt.CommentType == CommentType.SingleLine)
                    {
                        sb.AppendFormat("//{0}", cmt.Content);
                    }
                    else if (cmt.CommentType == CommentType.MultiLine)
                    {
                        sb.AppendFormat("/*{0}*/", cmt.Content);
                    }
                    lines.Add(sb.ToString());
                    sb.Clear();
                }
                CurrentTokenIndex++;
            }
            if (started && sb.Length > 0)
            {
                lines.Add(sb.ToString());
            }
            return lines;
        }

    }
}
