using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;

namespace SharpKit.Compiler.SourceMapping
{
    ///<summary>
    /// Writes out the source map in the following format (line numbers are for
    /// reference only and are not part of the format):
    ///
    /// 1.  {
    /// 2.    version: 3,
    /// 3.    file: "out.js",
    /// 4.    lineCount: 2,
    /// 5.    sourceRoot: "",
    /// 6.    sources: ["foo.js", "bar.js"],
    /// 7.    names: ["src", "maps", "are", "fun"],
    /// 8.    mappings: "a;;abcde,abcd,a;"
    /// 9.  }
    ///
    /// Line 1: The entire file is a single JSON object
    /// Line 2: File revision (always the first entry in the object)
    /// Line 3: The name of the file that this source map is associated with.
    /// Line 4: The number of lines represented in the sourcemap.
    /// Line 5: An optional source root, useful for relocating source files on a
    ///     server or removing repeated prefix values in the "sources" entry.
    /// Line 6: A list of sources used by the "mappings" entry relative to the
    ///     sourceRoot.
    /// Line 7: A list of symbol names used by the "mapping" entry.  This list
    ///     may be incomplete.
    /// Line 8: The mappings field.
    ///</summary>
    class SourceMappingV3Document
    {
        public SourceMappingV3Document()
        {
            version = 3;
        }
        public void SaveAs(string filename)
        {
            if (mappings == null)
                GenerateMappings();
            var s = new JavaScriptSerializer().Serialize(this);
            File.WriteAllText(filename, s);
        }
        public int version { get; set; }
        public string file { get; set; }
        public int lineCount { get; set; }
        public string sourceRoot { get; set; }
        public List<string> sources { get; set; }
        public List<string> names { get; set; }
        public string mappings { get; set; }
        [ScriptIgnore]
        public List<List<SourceMappingV3Item>> ParsedMappings { get; set; }

        public static SourceMappingV3Document Load(string filename)
        {
            var ser = new JavaScriptSerializer();
            var doc = ser.Deserialize<SourceMappingV3Document>(File.ReadAllText(filename));
            doc.ParseMappings();
            return doc;
        }

        public void SaveAsText(string filename)
        {
            var lines = new List<string>();
            var index = 0;
            foreach (var line in ParsedMappings)
            {
                lines.Add(index.ToString());
                foreach (var map in line)
                {
                    lines.Add("    " + map.ToString());
                }
                index++;
            }
            File.WriteAllLines(filename, lines.ToArray());
        }
        public void ParseMappings()
        {
            ParsedMappings = new List<List<SourceMappingV3Item>>();
            foreach (var line in mappings.Split(';'))
            {
                var line2 = new List<SourceMappingV3Item>();
                foreach (var token in line.Split(','))
                {
                    //Console.WriteLine(token);
                    var data = SourceMappingHelper.DecodeAll(token);
                    var numbers = data.Cast<int?>().ToList();
                    while (numbers.Count < 5)
                        numbers.Add(null);
                    var map = new SourceMappingV3Item
                    {
                        GeneratedColumn = numbers[0],
                        SourceFilenameIndex = numbers[1],
                        SourceLine = numbers[2],
                        SourceColumn = numbers[3],
                        SourceNameIndex = numbers[4],
                        OriginalData = data,
                    };
                    line2.Add(map);
                }
                ParsedMappings.Add(line2);
            }
        }
        public void GenerateMappings()
        {
            var sb = new StringBuilder();
            foreach (var line in ParsedMappings)
            {
                var firstMap = true;
                foreach (var map in line)
                {
                    if (firstMap)
                        firstMap = false;
                    else
                        sb.Append(",");

                    var numbers = new List<int?> { map.GeneratedColumn, map.SourceFilenameIndex, map.SourceLine, map.SourceColumn, map.SourceNameIndex };
                    while (numbers.Count > 0 && numbers.Last() == null)
                        numbers.RemoveAt(numbers.Count - 1);
                    var nums = numbers.Select(t => t.GetValueOrDefault()).ToList();
                    var x = SourceMappingHelper.EncodeAll(nums);
                    sb.Append(x);
                }
                sb.Append(";");
            }
            mappings = sb.ToString();
            lineCount = ParsedMappings.Count;
            if (sourceRoot == null)
                sourceRoot = "";
            if (names == null)
                names = new List<string>();
        }
    }

    ///<summary>
    ///Each entry represent a block of text in the original source, and
    ///     consists four fields:
    ///     The source file name
    ///     The line in the source file the text begins
    ///     The column in the line that the text begins
    ///     An optional name (from the original source) that this entry represents.
    ///     This can either be an string or index into the "names" field. 
    ///</summary>
    class SourceMappingV3Item
    {
        public List<int> OriginalData { get; set; }
        public int? GeneratedColumn { get; set; }
        public int? SourceFilenameIndex { get; set; }
        public int? SourceLine { get; set; }
        public int? SourceColumn { get; set; }
        public int? SourceNameIndex { get; set; }

        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4}", GeneratedColumn, SourceFilenameIndex, SourceLine, SourceColumn, SourceNameIndex);
        }
    }
}
