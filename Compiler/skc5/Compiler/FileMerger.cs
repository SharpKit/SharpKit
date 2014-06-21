using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Mirrored.SharpKit.JavaScript;
using System.IO;
using System.Collections;
using System.Diagnostics;
using ICSharpCode.NRefactory.CSharp;
using SharpKit.JavaScript.Ast;

namespace SharpKit.Compiler
{
    class FileMerger
    {
        public FileMerger()
        {
            ExternalJsFiles = new List<SkJsFile>();
        }
        public CompilerTool Compiler { get; set; }
        public SkProject Project { get; set; }
        /// <summary>
        /// This collection may be updated to include new files
        /// </summary>
        public List<SkJsFile> JsFiles { get; set; }
        public List<SkJsFile> ExternalJsFiles { get; set; }
        public CompilerLogger Log { get; set; }
        public void MergeFiles()
        {
            var atts = GetJsMergedFileAttributes(Project.Compilation.MainAssembly);//.getAssemblyEntity());
            var includedFiles = new HashSet<string>();
            foreach (var att in atts)
            {
                MergeFiles(Compiler.PathMerger.ConvertRelativePath(att.Filename), att.Sources, att.Minify);
            }
        }

        JsFile CreateExternalJsFile(string filename)
        {
            var unit = new JsUnit { Statements = new List<JsStatement>() };
            var st = new JsCodeStatement
            {
                Code = File.ReadAllText(filename)
            };
            var file = new JsFile { Filename = filename, Units = new List<JsUnit> { unit } };
            unit.Statements.Add(st);
            return file;
        }
        bool FileEquals(string file1, string file2)
        {
            return Path.GetFullPath(file1).EqualsIgnoreCase(Path.GetFullPath(file2));
        }

        public SkJsFile GetJsFile(string filename, bool isExternal)
        {
            filename = filename.Replace("/", Sk.DirectorySeparator);
            var file = JsFiles.Where(t => FileEquals(t.JsFile.Filename, filename)).FirstOrDefault();
            if (file == null)
                file = ExternalJsFiles.Where(t => FileEquals(t.JsFile.Filename, filename)).FirstOrDefault();
            if (file == null)
            {
                file = new SkJsFile { JsFile = new JsFile { Filename = filename, Units = new List<JsUnit>() }, Compiler = Compiler };
                if (isExternal)
                {
                    file.JsFile.Units.Add(new JsExternalFileUnit { Filename = filename });
                    ExternalJsFiles.Add(file);
                }
                else
                {
                    JsFiles.Add(file);
                }
            }
            return file;
        }
        void MergeFiles(string target, string[] sources, bool minify)
        {
            var target2 = GetJsFile(target, false);
            if (minify)
                target2.Minify = minify;
            var sources2 = sources.Select(t => GetJsFile(t, true)).ToList();
            MergeFiles(target2, sources2);
        }
        
        public void MergeFiles(SkJsFile target, List<SkJsFile> sources)
        {
            foreach (var source2 in sources)
            {
                target.JsFile.Units.AddRange(source2.JsFile.Units);
            }
        }

        JsMergedFileAttribute[] GetJsMergedFileAttributes(IAssembly asm)
        {
            var list = new List<JsMergedFileAttribute>();
            if (asm != null && asm.AssemblyAttributes != null)
            {
                var list2 = asm.GetMetadatas<JsMergedFileAttribute>();
                list.AddRange(list2);
            }
            return list.ToArray();
        }

    }

    //FIX FOR ISSUE 306. Only the casing of the first path will be used, to make is compatible to windows.
    class PathMerger
    {

        private static Dictionary<string, string> exportedPaths;
        public string ConvertRelativePath(string path)
        {
            if (exportedPaths == null)
                exportedPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            exportedPaths.TryAdd(path, path);
            return exportedPaths[path];
        }

        public void Reset()
        {
            exportedPaths = null;
        }

    }

}
