using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpKit.JavaScript.Ast;
using SharpKit.Compiler.SourceMapping;

namespace SharpKit.Compiler
{
    public class SkJsFile
    {
        public override string ToString()
        {
            if (JsFile != null && JsFile.Filename.IsNotNullOrEmpty())
                return JsFile.Filename;
            return base.ToString();
        }
        public bool GenerateSourceMap { get; set; }
        public bool Minify { get; set; }
        public string Format { get; set; }

        public JsFile JsFile { get; set; }
        public string TempFilename { get; set; }

        internal CompilerTool Compiler { get; set; }
        public void Save()
        {
            var jsFile = JsFile;
            Compiler.Log.WriteLine("    {0}", jsFile.Filename);
            var ext = Path.GetExtension(jsFile.Filename).ToLower();
            if (TempFilename.IsNullOrEmpty())
                TempFilename = jsFile.Filename + ".tmp";
            var dir = Path.GetDirectoryName(TempFilename);
            if (dir.IsNotNullOrEmpty() && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            jsFile.SaveAs(TempFilename, Minify ? "Minified" : Format, Compiler);
            if (Minify)
            {
                if (ext == ".js")
                {
                    //FileUtils.JsMinify(TempFilename);
                }
                else if (ext == ".css")
                    FileUtils.CssMinify(TempFilename);
                else
                    Compiler.Log.Warn("Cannot minify file:" + jsFile.Filename + " unknown extension");
            }
            if (GenerateSourceMap)
            {
                var smg = new SkSourceMappingGenerator { Compiler = Compiler };
                smg.TryGenerateAndAddMappingDirective(this);
            }
            FileUtils.CompareAndSaveFile(jsFile.Filename, TempFilename);
        }
    }
}
