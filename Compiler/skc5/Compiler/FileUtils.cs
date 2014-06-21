using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpKit.Compiler
{
    class FileUtils
    {

        public static void CompareAndSaveFile(string file, string tmpFile)
        {
            var fi = new FileInfo(file);
            if (fi.Exists)// && fi.IsReadOnly)
            {
                if (!FileCompare(file, tmpFile))
                {
                    fi.IsReadOnly = false;
                    fi.Delete();
                    File.Move(tmpFile, file);
                }
                else
                {
                    File.Delete(tmpFile);
                }
            }
            else
            {
                if (fi.Exists)
                    fi.Delete();
                File.Move(tmpFile, file);
            }
        }

        //static bool FileCompare2(string file1, string file2)
        //{
        //    var x = new StreamReader(file1);
        //    var y = new StreamReader(file2);

        //    var bufSize = 4096;
        //    var b1 = new byte[bufSize];
        //    var b2 = new byte[bufSize];
        //    x.ReadBlock(buffer, index, bufSize);
        //    y.ReadBlock(buffer, index, bufSize);
        //}
        /// <summary>
        /// This method accepts two strings the represent two files to 
        /// compare. A return value of 0 indicates that the contents of the files
        /// are the same. A return value of any other value indicates that the 
        /// files are not the same.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns></returns>
        static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        //static void CreateVisitor()
        //{
        //    var NamesAndTypes = new List<VisitorNameAndType>();
        //    foreach (var t in Enum.GetNames(typeof(cs_node)))
        //    {
        //        var typeName = "cs" + t.Replace("_", "").Substring(1);
        //        var t2 = typeof(AstNode).Assembly.GetTypes().Where(x => x.Name.EqualsIgnoreCase(typeName)).FirstOrDefault();
        //        if (t2 != null)
        //            NamesAndTypes.Add(new VisitorNameAndType { Name = t, Type = t2 });

        //    }
        //    NamesAndTypes = NamesAndTypes.OrderBy(t => t.Type.Name).ToList();
        //    var writer = new StringWriter();
        //    writer.WriteLine("using ICSharpCode.NRefactory.TypeSystem;");
        //    writer.WriteLine("namespace SharpKit.Compiler");
        //    writer.WriteLine("{");
        //    writer.WriteLine("class ggg");
        //    writer.WriteLine("{");

        //    writer.WriteLine("#region _Visit");
        //    NamesAndTypes.ForEach(p =>
        //    {
        //        var t = p.Name;
        //        var t2 = p.Type;
        //        writer.WriteLine("public JsNode _Visit({0} node)\n{{\nreturn null;\n}}", t2.Name);

        //    });
        //    writer.WriteLine("#endregion");

        //    writer.WriteLine("public JsNode Visit(AstNode node)        {");
        //    writer.WriteLine("            JsNode node2 = null;\n            switch (node.e)            {");
        //    writer.WriteLine("#region switch case");
        //    NamesAndTypes.ForEach(p =>
        //    {
        //        var t = p.Name;
        //        var t2 = p.Type;
        //        writer.WriteLine("  case cs_node.{0}:", t);
        //        writer.WriteLine("  node2 = _Visit(({0})node); break;", t2.Name);

        //    });
        //    writer.WriteLine("#endregion");
        //    writer.WriteLine("}");
        //    writer.WriteLine("return node2;");

        //    writer.WriteLine("}");

        //    writer.WriteLine("}");
        //    writer.WriteLine("}");


        //    File.WriteAllText("ggg.cs", writer.GetStringBuilder().ToString());
        //}

        //public static void CreateEntityVisitor()
        //{
        //    var NamesAndTypes = new List<VisitorNameAndType>();
        //    foreach (var t in Enum.GetNames(typeof(cs_entity)))
        //    {
        //        var typeName = "Entity" + t.Replace("_", "").Substring(3);
        //        var t2 = typeof(AstNode).Assembly.GetTypes().Where(x => x.Name.EqualsIgnoreCase(typeName)).FirstOrDefault();
        //        if (t2 != null)
        //            NamesAndTypes.Add(new VisitorNameAndType { Name = t, Type = t2 });
        //        else
        //        {
        //        }


        //    }
        //    NamesAndTypes = NamesAndTypes.OrderBy(t => t.Type.Name).ToList();
        //    var writer = new StringWriter();
        //    writer.WriteLine("using ICSharpCode.NRefactory.TypeSystem;");
        //    writer.WriteLine("namespace SharpKit.Compiler");
        //    writer.WriteLine("{");
        //    writer.WriteLine("class ggg");
        //    writer.WriteLine("{");

        //    writer.WriteLine("#region _Visit");
        //    NamesAndTypes.ForEach(p =>
        //    {
        //        var t = p.Name;
        //        var t2 = p.Type;
        //        writer.WriteLine("public JsNode _Visit({0} node)\n{{\nreturn null;\n}}", t2.Name);

        //    });
        //    writer.WriteLine("#endregion");

        //    writer.WriteLine("public JsNode Visit(Entity me)        {");
        //    writer.WriteLine("            JsNode node2 = null;\n            switch (node.e)            {");
        //    writer.WriteLine("#region switch case");
        //    NamesAndTypes.ForEach(p =>
        //    {
        //        var t = p.Name;
        //        var t2 = p.Type;
        //        writer.WriteLine("  case cs_node.{0}:", t);
        //        writer.WriteLine("  node2 = _Visit(({0})node); break;", t2.Name);

        //    });
        //    writer.WriteLine("#endregion");
        //    writer.WriteLine("}");
        //    writer.WriteLine("return node2;");

        //    writer.WriteLine("}");

        //    writer.WriteLine("}");
        //    writer.WriteLine("}");


        //    File.WriteAllText("ggg.cs", writer.GetStringBuilder().ToString());
        //}

        public static void CssMinify(string file)
        {
            var tmpFile = file + ".minify";
            var css = CssCompressor.Compress(File.ReadAllText(file));
            File.WriteAllText(tmpFile, css);
            File.Delete(file);
            File.Move(tmpFile, file);
        }

    }

}
