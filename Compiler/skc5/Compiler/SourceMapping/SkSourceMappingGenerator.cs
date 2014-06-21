using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpKit.JavaScript.Ast;
using System.IO;
using System.Runtime.Serialization;
using ICSharpCode.NRefactory.CSharp;
namespace SharpKit.Compiler.SourceMapping
{
    class SkSourceMappingGenerator
    {
        public CompilerTool Compiler { get; set; }

        public void AddMappingDirective(string generatedFilename, string mappingFilename)
        {
            var mapCode = SourceMappingHelper.GetJsMappingCode(Path.GetFileName(mappingFilename));
            File.AppendAllText(generatedFilename, mapCode);
            //file.JsFile.Units.Add(new JsUnit { Statements = new List<JsStatement> { Js.Code(mapCode).Statement() } });
        }
        public void TryGenerateAndAddMappingDirective(SkJsFile file)
        {
            if (!CanGenerate(file))
                return;
            if (Generate(file))
            {
                var generatedFilename = file.TempFilename;
                var mappingFilename = GetSourceMapFilename(file.JsFile.Filename);

                AddMappingDirective(generatedFilename, mappingFilename);
            }
        }
        public bool CanGenerate(SkJsFile skFile)
        {
            //if (skFile.Minify)
            //    return false;
            if (skFile.JsFile == null)
                return false;
            var file = skFile.JsFile;
            if (file.Units.IsNullOrEmpty())
                return false;
            return true;
        }
        public string GetSourceMapFilename(string jsFilename)
        {
            return Path.ChangeExtension(jsFilename, ".map.js");
        }
        public bool Generate(SkJsFile skFile)
        {
            var file = skFile.JsFile;
            var doc = new SourceMappingDocument { Mappings = new List<SourceMappingItem>() };
            var generatedFilename = file.Filename;
            var mappingFilename = GetSourceMapFilename(generatedFilename);
            Compiler.Log.WriteLine("      {0}", mappingFilename);
            foreach (var unit in file.Units)
            {
                foreach (var node in unit.Descendants())
                {
                    if (node is JsStatement)
                    {
                        if (node.StartLocation.IsEmpty)
                            continue;
                        var astNode = node.Annotation<AstNode>();
                        if (astNode == null || astNode.StartLocation.IsEmpty)
                            continue;
                        var region = astNode.GetRegion();
                        if (region.FileName.IsNullOrEmpty())
                            continue;
                        string sourceName = null;
                        if (node.NodeType == JsNodeType.MemberExpression)
                            sourceName = ((JsMemberExpression)node).Name;
                        doc.Mappings.Add(new SourceMappingItem
                        {
                            GeneratedLocation = new FileLocation(file.Filename, node.StartLocation.Line, node.StartLocation.Column),
                            SourceLocation = new FileLocation(region.FileName, region.BeginLine, region.BeginColumn),
                            SourceName = sourceName,
                        });
                        if (node is JsBlock)
                        {
                            var genCol = node.EndLocation.Column;
                            if (node.EndLocation.Line > node.StartLocation.Line)
                            {
                                //GenCol=1 otherwise chrome doesn't move debugger to close brace
                                genCol = 1;
                            }
                            doc.Mappings.Add(new SourceMappingItem
                            {
                                GeneratedLocation = new FileLocation(file.Filename, node.EndLocation.Line, genCol),
                                SourceLocation = new FileLocation(region.FileName, region.EndLine, region.EndColumn),
                            });
                        }
                    }
                }
            }
            var v3Doc = doc.GenerateV3MappingDocs().SingleOrDefault();
            if (v3Doc == null)
            {
                Compiler.Log.WriteLine("Cannot generate source mapping file: " + mappingFilename + " - document not found");
                return false;
            }
            v3Doc.sourceRoot = "SourceMaps.ashx/";
            var tmpFilename = mappingFilename + ".tmp";
            v3Doc.SaveAs(tmpFilename);
            FileUtils.CompareAndSaveFile(mappingFilename, tmpFilename);
            if (CompilerConfiguration.Current.GenerateSourceMapsDebugFiles)
            {
                var lines = doc.Mappings.OrderBy(t => t.GeneratedLocation.Line).ThenBy(t => t.GeneratedLocation.Column).Select(t => String.Format("{0} -> {1} = {2}", t.GeneratedLocation, t.SourceLocation, t.SourceName)).ToArray();
                var xmlDebugFile = mappingFilename + ".txt";
                File.WriteAllLines(xmlDebugFile, lines);
                v3Doc.SaveAsText(mappingFilename + ".v3.txt");
            }
            return true;
        }
    }
}
