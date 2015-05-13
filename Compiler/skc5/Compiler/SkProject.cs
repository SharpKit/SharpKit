using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using SharpKit.Compiler;
using System.Collections.Concurrent;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler
{
    class SkProject : NProject
    {
        static SkProject()
        {
            Cache = new NAssemblyCache
            {
                //IdleTimeToClear = TimeSpan.FromMinutes(1),
            };
        }

        internal ConcurrentDictionary<IAttribute, object> AttributeCache = new ConcurrentDictionary<IAttribute, object>();

        static NAssemblyCache Cache;
        public SkProject()
        {
            Parallel = CollectionExtensions.Parallel;
            AssemblyCache = Cache;
        }
        public CompilerTool Compiler { get; set; }

        protected override void ParseCsFiles()
        {
            base.ParseCsFiles();
            var errors = NFiles.SelectMany(t => t.SyntaxTree.Errors).ToList();
            if (errors.Count==0)
                return;
            foreach (var error in errors)
            {
                var item = new CompilerLogItem
                {
                    ProjectRelativeFilename = error.Region.FileName,
                    Line = error.Region.BeginLine,
                    Column = error.Region.BeginColumn,
                    Text = error.Message,
                    Type = CompilerLogItemType.Error,
                };
                if (error.ErrorType == ErrorType.Warning)
                    item.Type = CompilerLogItemType.Warning;
                Compiler.Log.Log(item);

            }
        }



        protected override void WriteLine(object obj)
        {
            Compiler.Log.WriteLine("{0:HH:mm:ss.fff}: {1}", DateTime.Now, obj);
        }
        protected override void FormatLine(string format, params object[] args)
        {
            WriteLine(String.Format(format, args));
        }



        public CompilerLogger Log { get; set; }
    }

}
