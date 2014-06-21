using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpKit.Compiler;

namespace SharpKit.JavaScript.Ast
{
    public class JsFile
    {
        public string Filename { get; set; }
        public List<JsUnit> Units { get; set; }

        public void CompareAndSave()
        {
            var tmpFile = Filename + ".tmp";
            SaveAs(tmpFile);
            FileUtils.CompareAndSaveFile(Filename, tmpFile);


        }

        public void SaveAs(string filename)
        {
            SaveAs(filename, null, null);
        }
        internal void SaveAs(string filename, string format, CompilerTool compiler)
        {
            var tmpFile = filename;
            using (var writer = JsWriter.Create(tmpFile, false))
            {
                var att = compiler.GetJsExportAttribute();
                if (format == null)
                    format = "JavaScript";
                writer.Format = format;
                try
                {
                    if (CompilerConfiguration.Current.EnableLogging && compiler != null)
                    {
                        writer.Visiting += node => compiler.Log.Debug(String.Format("JsWriter: Visit JsNode: {0}", filename));
                    }
                    foreach (var unit in Units)
                    {
                        if (unit.Tokens == null)
                            unit.Tokens = writer.GetTokens(unit);
                        else
                        {
                        }
                        var formatted = unit.TokensByFormat.TryGetValue(format);
                        if (formatted == null)
                        {
                            formatted = writer.FormatTokens(unit.Tokens);
                            unit.TokensByFormat[format] = formatted;
                        }
                        else
                        {
                        }
                        writer.WriteTokens(formatted);
                        writer.Flush();
                    }
                }
                catch (Exception e)
                {
                    if (compiler != null)
                        compiler.Log.Log(new CompilerLogItem { Type = CompilerLogItemType.Error, ProjectRelativeFilename = tmpFile, Text = e.Message });
                    throw e;
                }
            }
        }


    }

    public partial class JsExternalFileUnit : JsUnit
    {
        public string Filename { get; set; }
    }
}
