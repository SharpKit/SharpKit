using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Corex.IO.Tools;

namespace SharpKit.Compiler
{
    class CompilerToolArgs
    {
        public CompilerToolArgs()
        {
            Files = new List<string>();
            References = new List<string>();
            ContentFiles = new List<string>();
            NoneFiles = new List<string>();
            ResourceFiles = new List<string>();
        }
        static ToolArgsInfo<CompilerToolArgs> Info = new ToolArgsInfo<CompilerToolArgs> { Error = System.Console.WriteLine };
        public static CompilerToolArgs Parse(string[] args)
        {
            return Info.Parse(args);
        }
        public static void GenerateHelp(TextWriter writer)
        {
            Info.HelpGenerator.Generate(writer);

        }
        [ToolArgCommand]
        public List<string> Files { get; private set; }

        public string Service { get; set; }

        [ToolArgSwitch("?")]
        public bool Help { get; set; }

        /// <summary>
        /// designates the current directory that all paths are relative to
        /// </summary>
        [ToolArgSwitch("dir")]
        public string CurrentDirectory { get; set; }

        [ToolArgSwitch("target")]
        public string Target { get; set; }

        [ToolArgSwitch("out")]
        public string Output { get; set; }

        [ToolArgSwitch("reference")]
        public List<string> References { get; private set; }

        [ToolArgSwitch("plugin")]
        public List<string> Plugins { get; private set; }

        [ToolArgSwitch("contentfile")]
        public List<string> ContentFiles { get; private set; }

        [ToolArgSwitch("resource")]
        public List<string> ResourceFiles { get; private set; }

        [ToolArgSwitch("nonefile")]
        public List<string> NoneFiles { get; private set; }

        public bool why { get; set; }

        public bool? rebuild { get; set; }
        public bool? Enabled { get; set; }
        public bool? ExportToCSharp { get; set; }
        public bool? DebuggerBreak { get; set; }
        public bool? noconfig { get; set; }
        public bool? UseLineDirectives { get; set; }

        public string errorreport { get; set; }

        public int warn { get; set; }

        public string nowarn { get; set; }

        public string define { get; set; }

        [ToolArgSwitch("debug")]
        public string debugLevel { get; set; }

        public bool? debug { get; set; }

        public bool? optimize { get; set; }

        [ToolArgSwitch("filealign")]
        public int filealign { get; set; }

        private string _AssemblyName;
        public string AssemblyName
        {
            get
            {
                if (_AssemblyName == null)
                {
                    _AssemblyName = Path.GetFileNameWithoutExtension(Output);
                }
                return _AssemblyName;
            }
        }
        public string ManifestFile { get; set; }

        public string CodeAnalysisFile { get; set; }

        public string SecurityAnalysisFile { get; set; }

        public string OutputGeneratedJsFile { get; set; }

        public string OutputGeneratedFile { get; set; }

        public string OutputGeneratedDir { get; set; }

        public bool CheckForNewVersion { get; set; }

        /// <summary>
        /// /addbuildtarget:"pathToCsprojFile"
        /// /addbuildtarget:"pathToCsprojFile";nuget
        /// </summary>
        [ToolArgSwitch]
        public string AddBuildTarget { get; set; }

        public string TargetFrameworkVersion { get; set; }

        [ToolArgSwitch("ngen")]
        public bool CreateNativeImage { get; set; }

        public bool LastArgs { get; set; }

    }
}
