using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Corex.IO.Tools;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Extensions;
using ICSharpCode.NRefactory.TypeSystem;

namespace SharpKit.Compiler
{
    class CompilerService
    {
        public void PreLoad()
        {
            new SkProject();
            new NFile();
            new CSharpParser();
            AssemblyLoader.Create();

        }
        public CompileResponse Compile(CompileRequest req)
        {
            var skc = new CompilerTool
            {
                Args = req.Args,
                Log = new CompilerLogger { Console = { AutoFlush = false } },
            };
            if (req.CommandLineArgs.IsNotNullOrEmpty())
            {
                skc.CommandLineArguments = new ToolArgsTokenizer().Tokenize(req.CommandLineArgs);
            }
            skc.Init();
            var x = skc.Run();
            var xx = new CompileResponse { Output = skc.Log.Console.Items.ToList(), ExitCode = x };
            return xx;

        }

        public void Test()
        {

        }
    }

    [DataContract]
    class CompileRequest
    {
        [DataMember]
        public string CommandLineArgs { get; set; }
        [DataMember]
        public CompilerToolArgs Args { get; set; }
    }
    [DataContract]
    class CompileResponse
    {
        [DataMember]
        public List<string> Output { get; set; }
        [DataMember]
        public int ExitCode { get; set; }
    }

}
