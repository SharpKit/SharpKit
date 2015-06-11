using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.Runtime.Serialization;

namespace SharpKit.Compiler.SourceMapping
{
    [DataContract]
    class SourceMappingDocument
    {
        public List<SourceMappingV3Document> GenerateV3MappingDocs(string sourceRoot)
        {
            var list = new List<SourceMappingV3Document>();
            var byGenFile = Mappings.GroupBy(t => t.GeneratedLocation.Filename).ToList();
            foreach (var genFile in byGenFile)
            {
                var doc = new SourceMappingV3Document { ParsedMappings = new List<List<SourceMappingV3Item>>() };
                doc.names = genFile.Select(t => t.SourceName).Where(t => t != null).Distinct().ToList();
                var mappingsByGenFile = genFile.ToList();
                doc.file = genFile.Key.Replace("\\", "/");
                var bySourceFile = mappingsByGenFile.GroupBy(t => t.SourceLocation.Filename).ToList();
                var sources = bySourceFile.Select(t => t.Key).ToList();
                doc.sources = new List<string>();
                foreach (var src in sources)
                    doc.sources.Add(sourceRoot + Path.GetFullPath(src).Replace("\\", "/"));
                var byGenLine = mappingsByGenFile.GroupBy(t => t.GeneratedLocation.Line).OrderBy(t => t.Key).ToList();
                var prevSrcLine = 0;
                var prevSrcColumn = 0;
                var prevSrcFilenameIndex = 0;
                foreach (var mappingsByLine in byGenLine)
                {
                    var prevGenColumn = 0;
                    var lineIndex = mappingsByLine.Key - 1;
                    while (doc.ParsedMappings.Count <= lineIndex)
                        doc.ParsedMappings.Add(new List<SourceMappingV3Item>());
                    foreach (var mappingInLine in mappingsByLine.OrderBy(t => t.GeneratedLocation.Column))
                    {
                        var line = doc.ParsedMappings[lineIndex];
                        var genColumn = mappingInLine.GeneratedLocation.Column - 1;
                        var srcColumn = mappingInLine.SourceLocation.Column - 1;
                        var srcLine = mappingInLine.SourceLocation.Line - 1;
                        var srcFilenameIndex = sources.IndexOf(mappingInLine.SourceLocation.Filename);
                        var map2 = new SourceMappingV3Item
                        {
                            GeneratedColumn = genColumn - prevGenColumn,
                            SourceColumn = srcColumn - prevSrcColumn,
                            SourceLine = srcLine - prevSrcLine,
                            SourceFilenameIndex = srcFilenameIndex - prevSrcFilenameIndex,
                        };
                        prevGenColumn = genColumn;
                        prevSrcColumn = srcColumn;
                        prevSrcLine = srcLine;
                        prevSrcFilenameIndex = srcFilenameIndex;
                        if (mappingInLine != null)
                        {
                            map2.SourceNameIndex = (int?)doc.names.IndexOf(mappingInLine.SourceName);
                            if (map2.SourceNameIndex < 0)
                                map2.SourceNameIndex = null;
                        }
                        line.Add(map2);
                    }
                }
                list.Add(doc);
                foreach (var line in doc.ParsedMappings)
                {
                    if (line.Count <= 1)
                        continue;
                    foreach (var map in line.Skip(1).ToList())
                    {
                        if (map.GeneratedColumn == 0 &&
                            map.SourceColumn == 0 &&
                            map.SourceFilenameIndex == 0 &&
                            map.SourceLine == 0 &&
                            (map.SourceNameIndex == 0 || map.SourceNameIndex == null))
                            line.Remove(map);
                    }
                }
            }
            return list;
        }
        [DataMember]
        public List<SourceMappingItem> Mappings { get; set; }
    }
    [DataContract]
    class SourceMappingItem
    {
        [DataMember]
        public FileLocation GeneratedLocation { get; set; }
        [DataMember]
        public FileLocation SourceLocation { get; set; }
        [DataMember]
        public string SourceName { get; set; }
    }
    [DataContract]
    class FileLocation
    {
        public FileLocation()
        {
        }
        public FileLocation(string filename, int line, int col)
        {
            Filename = filename;
            Line = line;
            Column = col;
        }
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public int Line { get; set; }
        [DataMember]
        public int Column { get; set; }
        public override string ToString()
        {
            return String.Format("{0} [{1},{2}]", Filename, Line, Column);
        }
    }
}
