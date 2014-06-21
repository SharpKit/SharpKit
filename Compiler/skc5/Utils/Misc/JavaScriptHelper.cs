using System;
using System.Collections.Generic;
using System.Text;
using SharpKit.Utils;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom;

namespace SharpKit.Utils
{

    internal static class JavaScriptHelper
    {
        static JavaScriptHelper()
        {
            JsToClrTypeMapping = new Dictionary<string, Type>();
            JsToClrTypeMapping["string"] = typeof(string);
            JsToClrTypeMapping["date"] = typeof(DateTime);
            JsToClrTypeMapping["boolean"] = typeof(bool);
            //TODO: JsToClrTypeMapping["number"] = typeof(JSNumber);
        }
        public static string ToJavaScriptChar(char ch)
        {
            return ToJavaScriptString(ch.ToString());
        }

        static Dictionary<string, Type> JsToClrTypeMapping;
        public static string ToJavaScriptString(string value)
        {
            if (value == null)
                return "null";
            var writer = new StringWriter();
            var x = SharpKit.Utils.Misc.CSharpHelper.QuoteSnippetStringCStyle(value);
            return x;
            //var provider = new CSharpCodeProvider();
            //provider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, null);
            //return writer.GetStringBuilder().ToString();
            ////return String.Format("\"{0}\"", HttpContext.Current.Server.HtmlEncode(value));
            //StringBuilder sb = new StringBuilder();
            //sb.Append('\"');

            //foreach (char ch in value)
            //{
            //    if (ch == '\"')
            //        sb.Append("\\\"");
            //    else if (ch == '\\')
            //        sb.Append(@"\\");
            //    else if (ch == '\n')
            //        sb.Append(@"\n");
            //    else if (ch == '\r')
            //        sb.Append(@"\r");
            //    else if (ch == '\0')
            //        sb.Append(@"\0");
            //    else
            //        sb.Append(ch);
            //}
            //sb.Append('\"');
            //return sb.ToString();
            ////value = value.Replace("\"", "\\\"");
            ////value = value.Replace("\\", @"\\");
            ////value = value.Replace("\n", @"\n");
            ////value = value.Replace("\r", @"\r");
            ////return String.Format("\"{0}\"", value); ;
        }

        public static object ToClrValue(string value, string jsTypeName)
        {
            Type clrType = JsToClrTypeMapping.TryGetValue(jsTypeName);
            if (clrType != null)
                return ToClrValue(value, clrType);
            Trace.TraceData(TraceEventType.Error, 0, "JavaScriptHelper", "JsTypeName", "Couldn't identify jsTypeName " + jsTypeName, "", "");
            return null;
        }

        public static TraceSource Trace = new TraceSource("skc");

        public static object ToClrValue(string value, Type clrType)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(clrType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    object clrValue = converter.ConvertFrom(value);
                    return clrValue;
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("value {0} is not a valid {1}", value, clrType), e);
                }
            }
            else
            {
                throw new Exception(String.Format("{0} doesn't support convertion from string.", clrType));
            }

            //return Convert.ChangeType(value, clrType);
        }

        public static string ToJavaScriptObject(HashSet<string> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool first = true;
            foreach (string value in list)
            {
                if (first)
                    first = false;
                else
                    sb.AppendLine(",");
                sb.AppendFormat("{0} : null", ToJavaScriptString(value));
            }
            sb.Append("};");
            return sb.ToString();
        }

        public static string ToJavaScriptObject(object obj)
        {
            StringBuilder sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append("{");
            bool first = true;
            foreach (var pe in type.GetProperties())
            {
                if (first)
                    first = false;
                else
                    sb.AppendLine(",");
                var value = pe.GetValue(obj, null);
                sb.AppendFormat("{0} : {1}", ToJavaScriptString(pe.Name), ToJavaScriptValue(value));
            }
            sb.Append("}");
            return sb.ToString();
        }


        public static string ToJavaScriptValue(object value)
        {
            if (value == null)
                return "null";
            string jsValue;
            if (value is string)
                jsValue = ToJavaScriptString((string)value);
            else if (value is char)
                jsValue = ToJavaScriptChar((char)value);
            else if (value is bool)
            {
                jsValue = ((bool)value == true) ? "true" : "false";
            }
            else if (value is DateTime)
            {
                DateTime date = (DateTime)value;
                jsValue = String.Format("new Date({0},{1},{2},{3},{4},{5},{6})", date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
                //jsValue = String.Format("new Date({0})", date.Ticks);
            }
            else if (value is decimal || value is float)
            {
                jsValue = value.ToString();
            }
            else if (value is Enum || !value.GetType().IsPrimitive)
                jsValue = String.Format("\"{0}\"", value);
            else
            {
                jsValue = value.ToString();
            }
            return jsValue;
        }
        public static string ToJavaScriptArray(IEnumerable objects)
        {
            if (objects == null)
                return "null";
            var sb = new StringBuilder("[");
            var first = true;
            foreach (var obj in objects)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");
                sb.Append(ToJavaScriptValue(obj));
            }
            sb.Append("]");
            return sb.ToString();
        }



        public static bool IsPrimitive(object obj)
        {
            return obj == null || obj is string || obj is Enum || obj.GetType().IsPrimitive;
        }
    }


    class JsonSerializer
    {
        public JsonSerializer()
        {
        }

        public void Serialize(object obj, TextWriter writer)
        {
            if (JavaScriptHelper.IsPrimitive(obj))
            {
                writer.Write(JavaScriptHelper.ToJavaScriptValue(obj));
            }
            else
            {
                writer.Write("{");
                var first = true;
                foreach (var prop in obj.GetType().GetProperties())
                {
                    if (first)
                        first = false;
                    else
                        writer.Write(",");
                    writer.Write("{0} : ", prop.Name);
                    Serialize(prop.GetValue(obj, null), writer);
                }
                writer.Write("}");
            }
        }
    }
}
