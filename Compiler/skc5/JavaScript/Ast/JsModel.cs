using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Globalization;

namespace SharpKit.JavaScript.Ast
{
    /// <summary>
    /// A line/column position.
    /// Text editor lines/columns are counted started from one.
    /// </summary>
    /// <remarks>
    /// The document provides the methods <see cref="Editor.IDocument.GetLocation"/> and
    /// <see cref="Editor.IDocument.GetOffset(TextLocation)"/> to convert between offsets and TextLocations.
    /// </remarks>
    [Serializable]
    public struct TextLocation : IComparable<TextLocation>, IEquatable<TextLocation>
    {
        /// <summary>
        /// Represents no text location (0, 0).
        /// </summary>
        public static readonly TextLocation Empty = new TextLocation(0, 0);

        /// <summary>
        /// Constant of the minimum line.
        /// </summary>
        public const int MinLine = 1;

        /// <summary>
        /// Constant of the minimum column.
        /// </summary>
        public const int MinColumn = 1;

        /// <summary>
        /// Creates a TextLocation instance.
        /// </summary>
        public TextLocation(int line, int column)
        {
            this.line = line;
            this.column = column;
        }

        int column, line;

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int Line
        {
            get { return line; }
        }

        /// <summary>
        /// Gets the column number.
        /// </summary>
        public int Column
        {
            get { return column; }
        }

        /// <summary>
        /// Gets whether the TextLocation instance is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return column < MinLine && line < MinColumn;
            }
        }

        /// <summary>
        /// Gets a string representation for debugging purposes.
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "(Line {1}, Col {0})", this.column, this.line);
        }

        /// <summary>
        /// Gets a hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return unchecked(191 * column.GetHashCode() ^ line.GetHashCode());
        }

        /// <summary>
        /// Equality test.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is TextLocation)) return false;
            return (TextLocation)obj == this;
        }

        /// <summary>
        /// Equality test.
        /// </summary>
        public bool Equals(TextLocation other)
        {
            return this == other;
        }

        /// <summary>
        /// Equality test.
        /// </summary>
        public static bool operator ==(TextLocation left, TextLocation right)
        {
            return left.column == right.column && left.line == right.line;
        }

        /// <summary>
        /// Inequality test.
        /// </summary>
        public static bool operator !=(TextLocation left, TextLocation right)
        {
            return left.column != right.column || left.line != right.line;
        }

        /// <summary>
        /// Compares two text locations.
        /// </summary>
        public static bool operator <(TextLocation left, TextLocation right)
        {
            if (left.line < right.line)
                return true;
            else if (left.line == right.line)
                return left.column < right.column;
            else
                return false;
        }

        /// <summary>
        /// Compares two text locations.
        /// </summary>
        public static bool operator >(TextLocation left, TextLocation right)
        {
            if (left.line > right.line)
                return true;
            else if (left.line == right.line)
                return left.column > right.column;
            else
                return false;
        }

        /// <summary>
        /// Compares two text locations.
        /// </summary>
        public static bool operator <=(TextLocation left, TextLocation right)
        {
            return !(left > right);
        }

        /// <summary>
        /// Compares two text locations.
        /// </summary>
        public static bool operator >=(TextLocation left, TextLocation right)
        {
            return !(left < right);
        }

        /// <summary>
        /// Compares two text locations.
        /// </summary>
        public int CompareTo(TextLocation other)
        {
            if (this == other)
                return 0;
            if (this < other)
                return -1;
            else
                return 1;
        }
    }

    public partial class JsNode : AbstractAnnotatable
    {
        public JsNodeType NodeType { get; protected set; }
        public string ToJs()
        {
            using (var writer = JsWriter.CreateInMemory())
            {
                writer.VisitFormatAndWrite(this);
                return writer.GetStringBuilder().ToString();
            }
        }
        public JsNode Parent { get; set; }
        //public object Metadata { get; set; }
        public TextLocation StartLocation { get; set; }
        public TextLocation EndLocation { get; set; }
    }
    public partial class JsExpression : JsNode
    {
    }

    public partial class JsNodeList : JsNode
    {
        public List<JsNode> Nodes { get; set; }
    }
    public partial class JsUnit : JsNode
    {
        public JsUnit()
        {
            NodeType = JsNodeType.Unit;
            TokensByFormat = new Dictionary<string, List<JsToken>>();
        }
        public List<JsStatement> Statements { get; set; }
        internal List<JsToken> Tokens { get; set; }
        internal Dictionary<string, List<JsToken>> TokensByFormat { get; set; }
    }

    public partial class JsStatement : JsNode
    {
        public List<string> Comments { get; set; }
    }
    public partial class JsCodeStatement : JsStatement
    {
        public JsCodeStatement()
        {
            NodeType = JsNodeType.CodeStatement;
        }
        public string Code { get; set; }
        public override void Clone(JsNode node)
        {
            base.Clone(node);
            var st = (JsCodeStatement)node;
            st.Code = Code;
        }
        public override JsNode New()
        {
            return new JsCodeStatement();
        }
    }

    #region Statements
    public partial class JsSwitchStatement : JsStatement
    {
        public JsExpression Expression { get; set; }
        public List<JsSwitchSection> Sections { get; set; }
    }
    public partial class JsSwitchSection : JsNode
    {
        public List<JsSwitchLabel> Labels { get; set; }
        public List<JsStatement> Statements { get; set; }
    }
    public partial class JsSwitchLabel : JsNode
    {
        public bool IsDefault { get; set; }
        public JsExpression Expression { get; set; }
    }

    public partial class JsWhileStatement : JsStatement
    {
        public JsExpression Condition { get; set; }
        public JsStatement Statement { get; set; }
    }
    public partial class JsDoWhileStatement : JsStatement
    {
        public JsExpression Condition { get; set; }
        public JsStatement Statement { get; set; }
    }
    public partial class JsIfStatement : JsStatement
    {
        public JsExpression Condition { get; set; }
        public JsStatement IfStatement { get; set; }
        public JsStatement ElseStatement { get; set; }
    }

    public partial class JsUseStrictStatement : JsStatement
    {
    }

    public partial class JsForStatement : JsStatement
    {
        public List<JsStatement> Initializers { get; set; }
        public JsExpression Condition { get; set; }
        public List<JsStatement> Iterators { get; set; }
        public JsStatement Statement { get; set; }
    }
    public partial class JsForInStatement : JsStatement
    {
        public JsVariableDeclarationExpression Initializer { get; set; }
        public JsExpression Member { get; set; }
        public JsStatement Statement { get; set; }
    }
    public partial class JsContinueStatement : JsStatement
    {
    }
    public partial class JsBlock : JsStatement
    {
        public List<JsStatement> Statements { get; set; }
    }

    public partial class JsThrowStatement : JsStatement
    {
        public JsExpression Expression { get; set; }
    }
    public partial class JsTryStatement : JsStatement
    {
        public JsBlock TryBlock { get; set; }
        public JsCatchClause CatchClause { get; set; }
        public JsBlock FinallyBlock { get; set; }
    }

    public partial class JsBreakStatement : JsStatement
    {
    }

    public partial class JsExpressionStatement : JsStatement
    {
        public JsExpression Expression { get; set; }
        internal bool SkipSemicolon()
        {
            if (Expression is JsFunction)
                return true;
            if (Expression is JsAssignmentExpression && ((JsAssignmentExpression)Expression).Right is JsFunction)//TODO: check parent is unit
                return true;
            if (Expression is JsCodeExpression)
                return true;
            return false;
        }
    }
    public partial class JsReturnStatement : JsStatement
    {
        public JsExpression Expression { get; set; }
    }
    public partial class JsVariableDeclarationStatement : JsStatement
    {
        public JsVariableDeclarationExpression Declaration { get; set; }
    }
    public partial class JsCommentStatement : JsStatement
    {
        public string Text { get; set; }
    }

    #endregion



    #region Expressions
    public partial class JsConditionalExpression : JsExpression
    {
        public JsExpression Condition { get; set; }
        public JsExpression TrueExpression { get; set; }
        public JsExpression FalseExpression { get; set; }
    }
    public partial class JsAssignmentExpression : JsExpression
    {
        public string Operator { get; set; }
        public JsExpression Left { get; set; }
        public JsExpression Right { get; set; }
    }
    public partial class JsParenthesizedExpression : JsExpression
    {
        public JsExpression Expression { get; set; }
    }
    public partial class JsBinaryExpression : JsExpression
    {
        public string Operator { get; set; }
        public JsExpression Left { get; set; }
        public JsExpression Right { get; set; }
    }
    public partial class JsPostUnaryExpression : JsExpression
    {
        public string Operator { get; set; }
        public JsExpression Left { get; set; }
    }
    public partial class JsPreUnaryExpression : JsExpression
    {
        public string Operator { get; set; }
        public JsExpression Right { get; set; }
    }
    public partial class JsJsonObjectExpression : JsExpression
    {
        public List<JsJsonNameValue> NamesValues { get; set; }
    }
    public partial class JsStringExpression : JsExpression
    {
        public string Value { get; set; }
    }
    public partial class JsNumberExpression : JsExpression
    {
        public double Value { get; set; }
    }
    public partial class JsRegexExpression : JsExpression
    {
        public string Code { get; set; }
    }

    public partial class JsNullExpression : JsExpression
    {
    }

    public partial class JsVariableDeclarationExpression : JsExpression
    {
        public List<JsVariableDeclarator> Declarators { get; set; }
    }
    public partial class JsVariableDeclarator : JsNode
    {
        public string Name { get; set; }
        public JsExpression Initializer { get; set; }
    }
    public partial class JsNewObjectExpression : JsExpression
    {
        public JsInvocationExpression Invocation { get; set; }
    }
    public partial class JsFunction : JsExpression
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public JsBlock Block { get; set; }
    }
    public partial class JsInvocationExpression : JsExpression
    {
        public JsExpression Member { get; set; }
        public List<JsExpression> Arguments { get; set; }
        public string ArgumentsPrefix { get; set; }
        public string ArgumentsSuffix { get; set; }
        public bool OmitParanthesis { get; set; }

        public bool OmitCommas { get; set; }
    }
    public partial class JsIndexerAccessExpression : JsExpression
    {
        public JsExpression Member { get; set; }
        public List<JsExpression> Arguments { get; set; }
    }
    public partial class JsMemberExpression : JsExpression
    {
        public string Name { get; set; }
        public JsExpression PreviousMember { get; set; }
    }
    public partial class JsThis : JsMemberExpression
    {
    }
    public partial class JsJsonArrayExpression : JsExpression
    {
        public List<JsExpression> Items { get; set; }
    }
    public partial class JsStatementExpressionList : JsExpression
    {
        public List<JsExpression> Expressions { get; set; }
    }
    #endregion

    public partial class JsCatchClause : JsNode
    {
        public string IdentifierName { get; set; }
        public JsBlock Block { get; set; }
    }

    public partial class JsJsonMember : JsNode
    {
        public bool IsStringLiteral { get; set; }
        public string Name { get; set; }
    }
    public partial class JsCodeExpression : JsExpression
    {
        public string Code { get; set; }
    }

    public partial class JsJsonNameValue : JsNode
    {
        public JsJsonMember Name { get; set; }
        public JsExpression Value { get; set; }
    }

}
