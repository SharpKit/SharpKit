using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    partial class JsNode
    {
        public virtual JsNode New()
        {
            return new JsNode();
        }
        public virtual void Clone(JsNode node)
        {
            node.StartLocation = StartLocation;
            node.EndLocation = EndLocation;
            Annotations.ForEach(t => node.AddAnnotation(t));
        }
    }
}
