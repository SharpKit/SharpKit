using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.JavaScript.Ast
{
    struct JsToken
    {
        public static JsToken Empty = new JsToken();
        public JsToken(string value, JsTokenType type)
        {
            this.Type = type;
            this.Value = value;
            Node = null;
        }

        public override string ToString()
        {
            return String.Format("'{1}' {0} {2}", Type, Value, Node == null ? "null" : Node.NodeType.ToString());
        }
        public static JsToken NewLine()
        {
            return new JsToken(Environment.NewLine, JsTokenType.NewLine);
        }

        public bool IsNodeStartOrEnd { get { return Type == JsTokenType.NodeStart || Type==JsTokenType.NodeEnd; } }
        public bool IsNodeStart { get { return Type == JsTokenType.NodeStart; } }
        public bool IsNodeEnd { get { return Type == JsTokenType.NodeEnd; } }
        public bool IsValue { get { return Type == JsTokenType.Value; } }
        public bool IsLiteral { get { return Type == JsTokenType.Literal; } }
        public bool IsKeyword { get { return Type == JsTokenType.Keyword; } }
        public bool IsControl { get { return Type == JsTokenType.Control; } }
        public bool IsWhitespace { get { return Type == JsTokenType.Whitespace; } }
        public bool IsNewLine { get { return Type == JsTokenType.NewLine; } }
        public bool IsRaw { get { return Type == JsTokenType.Raw; } }
        public bool IsUnknown { get { return Type == JsTokenType.Unknown; } }
        public bool IsComment { get { return Type == JsTokenType.Comment; } }
        public bool IsOperator { get { return Type == JsTokenType.Operator; } }

        public JsTokenType Type;
        public string Value;
        public JsNode Node;

        public static JsToken Space(int size)
        {
            return new JsToken("".PadRight(size, ' '), JsTokenType.Whitespace);
        }
        public static JsToken Space()
        {
            return new JsToken(" ", JsTokenType.Whitespace);
        }

        public static JsToken Enter()
        {
            return new JsToken("\r\n", JsTokenType.NewLine);
        }
    }

    /// <summary>
    /// defines, how a token should be rendered, for example if a whitespace is requied or optional.
    /// </summary>
    enum JsTokenType
    {
        None,
        /// <summary>
        /// text, 5.1
        /// </summary>
        Value,

        /// <summary>
        /// name
        /// </summary>
        Literal,

        /// <summary>
        /// if
        /// </summary>
        Keyword,

        /// <summary>
        /// [(,
        /// </summary>
        Control,

        Unknown,
        Raw,
        Comment,
        Operator,
        NewLine,

        /// <summary>
        /// space, tab, newline
        /// </summary>
        Whitespace,
        NodeStart,
        NodeEnd,
    }

}
