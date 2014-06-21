using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;

namespace SharpKit.JavaScript
{
    class JsCodeInliner
    {
        public JsCodeInliner()
        {
            Params = new List<CodeInlinerParameter>();
        }
        public string ThisCode { get; set; }
        public List<CodeInlinerParameter> Params { get; set; }
        public string InlineCodeExpression { get; set; }

        public string Process()
        {
            CodeSettings = new CodeSettings
            {
                SourceMode = JavaScriptSourceMode.Program,
                MinifyCode = false,
            };
            var code = String.Format("function ___________({0}){{return {1};}};", Params.Select(t => t.Name).StringJoin(","), InlineCodeExpression);
            var block = ParseJavaScript(code);
            Func = ((FunctionObject)block[0]);
            var i = 0;
            foreach (var prm in Func.ParameterDeclarations)
            {
                var prm2 = prm as ParameterDeclaration;
                if (prm2 == null)
                    continue;
                var binding = prm2.Binding as BindingIdentifier;
                if (binding == null)
                    continue;
                var prm3 = Params[i];
                binding.VariableField.GetType().GetProperty("Name").SetValue(binding.VariableField, prm3.Code);
                i++;
            }
            if (ThisCode != null)
                Scan(Func.Body);
            var ret = Func.Body[0] as ReturnNode;
            if (ret == null)
                throw new Exception();
            var s = ToJavaScript(ret.Operand);
            return s;
        }



        private void Scan(AstNode node)
        {
            var replace = new List<AstNode>();
            foreach (var ch in node.Children)
            {
                if (ch is ThisLiteral)
                {
                    replace.Add(ch);
                }
                else
                {
                    Scan(ch);
                }
            }
            foreach (var x in replace)
            {
                node.ReplaceChild(x, new Lookup(x.Context) { Name = ThisCode });
            }
        }


        CodeSettings CodeSettings { get; set; }
        Block ParseJavaScript(string source)
        {
            this.ErrorList = new List<ContextError>();
            JSParser jSParser = new JSParser();
            jSParser.CompilerError += new EventHandler<ContextErrorEventArgs>(this.OnJavaScriptError);
            try
            {
                var block = jSParser.Parse(source, CodeSettings);
                return block;
            }
            catch (Exception ex)
            {
                this.ErrorList.Add(new ContextError
                {
                    Severity = 0,
                    //File = this.FileName,
                    Message = ex.Message
                });
                throw;
            }
        }

        string ToJavaScript(AstNode node)
        {
            var sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                OutputVisitor.Apply(sw, node, CodeSettings);
                return sw.GetStringBuilder().ToString();
            }
        }

        void OnJavaScriptError(object sender, ContextErrorEventArgs e)
        {
            //Console.WriteLine(e.Error);
           // throw new Exception(e.Error.ToString());
        }


        List<ContextError> ErrorList { get; set; }

        FunctionObject Func { get; set; }



    }

    class CodeInlinerParameter
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
