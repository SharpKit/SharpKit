using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SharpKit.Utils.Misc
{
    class CSharpHelper
    {
        public static string QuoteSnippetStringCStyle(string value)
        {
            StringBuilder stringBuilder = new StringBuilder(value.Length + 5);
            //Indentation indentation = new Indentation((IndentedTextWriter)this.Output, this.Indent + 1);
            stringBuilder.Append("\"");
            int i = 0;
            while (i < value.Length)
            {
                char c = value[i];
                if (c <= '"')
                {
                    if (c != '\0')
                    {
                        switch (c)
                        {
                            case '\t':
                                stringBuilder.Append("\\t");
                                break;
                            case '\n':
                                stringBuilder.Append("\\n");
                                break;
                            case '\v':
                            case '\f':
                                goto IL_107;
                            case '\r':
                                stringBuilder.Append("\\r");
                                break;
                            default:
                                if (c != '"')
                                {
                                    goto IL_107;
                                }
                                stringBuilder.Append("\\\"");
                                break;
                        }
                    }
                    else
                    {
                        stringBuilder.Append("\\0");
                    }
                }
                else
                {
                    if (c != '\'')
                    {
                        if (c != '\\')
                        {
                            switch (c)
                            {
                                case '\u2028':
                                case '\u2029':
                                    AppendEscapedChar(stringBuilder, value[i]);
                                    break;
                                default:
                                    goto IL_107;
                            }
                        }
                        else
                        {
                            stringBuilder.Append("\\\\");
                        }
                    }
                    else
                    {
                        stringBuilder.Append("\\'");
                    }
                }
            IL_115:
                if (i > 0 && i % 80 == 0)
                {
                    if (char.IsHighSurrogate(value[i]) && i < value.Length - 1 && char.IsLowSurrogate(value[i + 1]))
                    {
                        stringBuilder.Append(value[++i]);
                    }
                    //stringBuilder.Append("\" +");
                    //stringBuilder.Append(Environment.NewLine);
                    //stringBuilder.Append(indentation.IndentationString);
                    //stringBuilder.Append('"');
                }
                i++;
                continue;
            IL_107:
                stringBuilder.Append(value[i]);
                goto IL_115;
            }
            stringBuilder.Append("\"");
            return stringBuilder.ToString();
        }

        // Microsoft.CSharp.CSharpCodeGenerator
        private static void AppendEscapedChar(StringBuilder b, char value)
        {
            b.Append("\\u");
            int num2 = (int)value;
            b.Append(num2.ToString("X4", CultureInfo.InvariantCulture));
        }

    }
}
