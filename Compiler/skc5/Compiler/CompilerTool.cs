using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mirrored.SharpKit.JavaScript;
using ICSharpCode.NRefactory.TypeSystem;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Globalization;
using SharpKit.JavaScript.Ast;
using SharpKit.Compiler.SourceMapping;
using Mono.Cecil;
using System.Collections;
using System.Xml.Linq;
using ICSharpCode.NRefactory.CSharp;
using SharpKit.Utils.Http;
using Corex.Helpers;
using System.ServiceProcess;
using Corex.IO.Tools;
using SharpKit.Compiler.CsToJs;

namespace SharpKit.Compiler
{
    class CompilerTool : ICompiler
    {
        public CompilerTool()
        {
            EmbeddedResourceFiles = new List<string>();
            CompilerConfiguration.LoadCurrent();
            NativeAnonymousDelegateSupportStatement = Js.CodeStatement(NativeAnonymousDelegateSupportCode);
            NativeDelegateSupportStatement = Js.CodeStatement(NativeDelegateSupportCode);
            NativeInheritanceSupportStatement = Js.CodeStatement(NativeInheritanceSupportCode);
            NativeExtensionDelegateSupportStatement = Js.CodeStatement(NativeExtensionDelegateSupportCode);
            CombineDelegatesSupportStatement = Js.CodeStatement(CombineDelegatesSupportCode);
            RemoveDelegateSupportStatement = Js.CodeStatement(RemoveDelegateSupportCode);
            CreateMulticastDelegateFunctionSupportStatement = Js.CodeStatement(CreateMulticastDelegateFunctionSupportCode);
            CreateExceptionSupportStatement = Js.CodeStatement(CreateExceptionSupportCode);
            CreateAnonymousObjectSupportStatement = Js.CodeStatement(CreateAnonymousObjectSupportCode);

            CodeInjections = new List<CodeInjection>
            {
                new CodeInjection
                {
                    JsCode = NativeAnonymousDelegateSupportCode,
                    JsStatement = NativeAnonymousDelegateSupportStatement,
                    FunctionName = "$CreateAnonymousDelegate",
                },
                new CodeInjection
                {
                    JsCode = NativeDelegateSupportCode,
                    JsStatement = NativeDelegateSupportStatement,
                    FunctionName = "$CreateDelegate",
                },
                new CodeInjection
                {
                    JsCode = NativeInheritanceSupportCode,
                    JsStatement = NativeInheritanceSupportStatement,
                    FunctionName = "$Inherit",
                },
                new CodeInjection
                {
                    JsCode = NativeExtensionDelegateSupportCode,
                    JsStatement = NativeExtensionDelegateSupportStatement,
                    FunctionName = "$CreateExtensionDelegate",
                },
                new CodeInjection
                {
                    JsCode = CreateExceptionSupportCode,
                    JsStatement = CreateExceptionSupportStatement,
                    FunctionName = "$CreateException",
                },
                new CodeInjection
                {
                    JsCode = CreateMulticastDelegateFunctionSupportCode,
                    JsStatement = CreateMulticastDelegateFunctionSupportStatement,
                    FunctionName = "$CreateMulticastDelegateFunction",
                },
                new CodeInjection
                {
                    JsCode = CombineDelegatesSupportCode,
                    JsStatement = CombineDelegatesSupportStatement,
                    FunctionName = "$CombineDelegates",
                },
                new CodeInjection
                {
                    JsCode = RemoveDelegateSupportCode,
                    JsStatement = RemoveDelegateSupportStatement,
                    FunctionName = "$RemoveDelegate",
                },
                new CodeInjection
                {
                    JsCode = CreateAnonymousObjectSupportCode,
                    JsStatement = CreateAnonymousObjectSupportStatement,
                    FunctionName = "$CreateAnonymousObject",
                },
          };
            var dep1 = CodeInjections.Where(t => t.FunctionName == "$CreateMulticastDelegateFunction").FirstOrDefault();

            AddCodeInjectionDependency("$CombineDelegates", "$CreateMulticastDelegateFunction");
            AddCodeInjectionDependency("$RemoveDelegate", "$CreateMulticastDelegateFunction");


            foreach (var ta in TypedArrays)
            {
                var define = Js.If(Js.Typeof(Js.Member(ta)).Equal(Js.Value("undefined")), Js.Var(ta, Js.Member("Array")).Statement());
                CodeInjections.Add(new CodeInjection
                {
                    FunctionName = ta,
                    JsStatement = define,
                });
            }
        }


        #region Properties
        //public static CompilerTool Current { get; set; }
        public CompilerToolArgs Args { get; set; }
        public List<SkJsFile> SkJsFiles { get; set; }
        public CompilerLogger Log { get; set; }
        public SkProject Project { get; set; }
        public List<string> Defines { get; set; }
        public CsExternalMetadata CsExternalMetadata { get; set; }
        public string SkcVersion { get; set; }
        public string[] CommandLineArguments { get; set; }
        public bool Debug { get; set; }


        public JsStatement CombineDelegatesSupportStatement { get; set; }
        public JsStatement RemoveDelegateSupportStatement { get; set; }
        public JsStatement CreateMulticastDelegateFunctionSupportStatement { get; set; }
        public SkSourceMappingGenerator SourceMapsGenerator { get; set; }
        public SkJsFile CodeInjectionFile { get; set; }
        public List<string> EmbeddedResourceFiles { get; set; }
        public string VersionKey { get; set; }
        public FileMerger FileMerger { get; set; }
        public PathMerger PathMerger { get; set; }

        #endregion

        #region Fields

        TypeConverter TypeConverter;
        JsStatement NativeInheritanceSupportStatement;
        JsStatement NativeExtensionDelegateSupportStatement;
        JsStatement CreateExceptionSupportStatement;
        JsStatement NativeDelegateSupportStatement;
        JsStatement NativeAnonymousDelegateSupportStatement;

        public static string[] TypedArrays = new[]
            {
                "Int8Array",
                "Uint8Array",
                "Int16Array",
                "Uint16Array",
                "Int32Array",
                "Uint32Array",
                "Float32Array",
                "Float64Array",
            };
        List<CodeInjection> CodeInjections;

        //Skc5CacheData Skc5CacheData;
        //string Skc5CacheDataFilename;

        #endregion

        public void Init()
        {
            if (Log == null)
            {
                Log = new CompilerLogger();
                Log.Init();
            }
        }
        public int Run()
        {
            var x = InternalRun();
            if (BeforeExit != null)
                BeforeExit();
            return x;
        }
        int InternalRun()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                SkcVersion = typeof(CompilerTool).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
                VersionKey = this.SkcVersion + "||" + File.GetLastWriteTime(Process.GetCurrentProcess().MainModule.FileName).ToBinary();


                Time(SaveLastInputArgs);
                WriteArgs();
                Time(ParseArgs);
                if (Args.LastArgs)
                {
                    var tokenizer = new ToolArgsTokenizer();
                    var args = tokenizer.Tokenize(LastArgs);
                    CommandLineArguments = args;
                    Time(SaveLastInputArgs);
                    WriteArgs();
                    Time(ParseArgs);
                }
                if (Args.CheckForNewVersion)
                {
                    Time(CheckForNewVersion);
                    if (Args.Files.IsNullOrEmpty())
                        return 0;
                }

                if (!Args.AddBuildTarget.IsNullOrEmpty())
                {
                    Time(AddBuildTarget);
                    return 0;
                }

                if (Help())
                    return 0;
                if (Args.Service != null)
                {
                    var action = Args.Service.ToLower();
                    if (action == "start")
                    {
                        StartWindowsService();
                    }
                    else if (action == "stop")
                    {
                        StopWindowsService();
                    }
                    else if (action == "restart")
                    {
                        Try(StopWindowsService);
                        StartWindowsService();
                    }
                    else if (action == "install")
                    {
                        InstallWindowsService();
                    }
                    else if (action == "uninstall")
                    {
                        UninstallWindowsService();
                    }
                    else if (action == "reinstall")
                    {
                        Log.WriteLine("Reinstalling Service");
                        Try(UninstallWindowsService);
                        InstallWindowsService();
                    }
                    else if (action == "console")
                    {
                        Log.WriteLine("Starting Console Service");
                        RunInServiceConsoleMode();
                    }
                    else if (action == "windows")
                    {
                        Log.WriteLine("Starting Windows Service");
                        RunInWindowsServiceMode();
                    }
                    return 0;
                }

                Time(CalculateMissingArgs);
                if (Time2(CheckManifest))
                    return 0;
                Time(VerifyNativeImage);
                Time(LoadPlugins);

                Time(ParseCs);
                Time(ApplyExternalMetadata);
                Time(ConvertCsToJs);
                Time(MergeJsFiles);
                //Time(ValidateUnits));
                Time(InjectJsCode);
                Time(OptimizeJsFiles);
                Time(SaveJsFiles);
                Time(EmbedResources);
                //Time(GenerateSourceMappings);
                if (Log.Items.Where(t => t.Type == CompilerLogItemType.Error).FirstOrDefault() != null)
                    return -1;
                Time(SaveNewManifest);
                return 0;
            }
            catch (Exception e)
            {
                Log.Log(e);
                Log.Console.WriteLine(e);
                return -1;
            }
        }

        private void StopWindowsService()
        {
            Log.WriteLine("Stopping Service");
            WindowsServiceHelper.StopService(WindowsServiceName);
        }

        private void StartWindowsService()
        {
            Log.WriteLine("Starting Service");
            WindowsServiceHelper.StartService(WindowsServiceName);
        }

        string WindowsServiceName = "SharpKit";

        private void UninstallWindowsService()
        {
            Try(StopWindowsService);
            WindowsServiceHelper.DeleteService(WindowsServiceName);
        }

        bool Try(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                return false;
            }
        }
        private void InstallWindowsService()
        {
            Log.WriteLine("Installing Service");
            WindowsServiceHelper.CreateService(WindowsServiceName, ProcessHelper.CurrentProcessFile.FullName + " /service:windows", "auto");
            StartWindowsService();
        }

        bool Help()
        {
            if (Args.Help)
            {
                CompilerToolArgs.GenerateHelp(System.Console.Out);
                return true;
            }
            return false;
        }
        public void RunInServiceConsoleMode()
        {
            var server = CreateServer();
            server.Run();
        }

        private JsonServer CreateServer()
        {
            var compilerService = new CompilerService();
            compilerService.PreLoad();
            var server = new JsonServer { Service = compilerService, Url = CompilerConfiguration.Current.SharpKitServiceUrl };
            //server.DeserializeFromQueryStringOverride += e =>
            //    {
            //        if (e.Type == typeof(CompileRequest))
            //        {
            //            var req = new CompileRequest();
            //            var args = server.DeserializeFromQueryString(typeof(CompilerToolArgs), e.QueryString) as CompilerToolArgs;
            //            req.Args = args;
            //            e.Handled = true;
            //            e.Result = req;
            //        }
            //    };
            return server;
        }


        public void RunInWindowsServiceMode()
        {
            var server = CreateServer();
            var ws = new WindowsService();
            ws.StartAction = () => server.Start();
            ws.StopAction = () => server.Stop();
            ws.Run();
        }


        void CheckForNewVersion()
        {
            Process.Start("http://sharpkit.net/CheckForNewVersion.aspx?v=" + SkcVersion);
        }

        void AddBuildTarget()
        {
            bool nuget = false;
            var file = Args.AddBuildTarget;
            if (file.EndsWith(";nuget"))
            {
                nuget = true;
                file = file.Replace(";nuget", "");
            }
            var doc = XDocument.Parse(File.ReadAllText(file, Encoding.UTF8));

            if (((XElement)doc.LastNode).Nodes().Count((n) => n is XElement ? ((XElement)n).Name.LocalName == "Import" && ((XElement)n).LastAttribute.Value.Contains("SharpKit") : false) > 0)
            {
                Log.WriteLine("Already registered");
                return;
            }

            var importNode = (XElement)((XElement)doc.LastNode).Nodes().Last(
                (n) =>
                {
                    return n is XElement ? ((XElement)n).Name.LocalName == "Import" : false;
                });

            var path = Path.Combine("$(MSBuildBinPath)", "SharpKit", "5", "SharpKit.Build.targets");
            if (nuget)
                path = Path.Combine("$(SolutionDir)", "packages", "SharpKit.5.0.0", "tools", "SharpKit.Build.targets");
            importNode.AddAfterSelf(new XElement(XName.Get("Import", importNode.Name.NamespaceName), new XAttribute("Project", path)));

            doc.Save(file);
        }

        void LoadPlugins()
        {
            if (Args.Plugins.IsNullOrEmpty())
                return;
            foreach (var plugin in Args.Plugins)
            {
                try
                {
                    Log.WriteLine("Loading plugin: " + plugin);
                    var type = Type.GetType(plugin);
                    Log.WriteLine("Found plugin: " + plugin);
                    var obj = Activator.CreateInstance(type, true);
                    Log.WriteLine("Created plugin: " + plugin);
                    Log.WriteLine("Started: Initialize plugin" + plugin);
                    var plugin2 = (ICompilerPlugin)obj;
                    plugin2.Init(this);
                    Log.WriteLine("Finished: Initialize plugin " + plugin);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to load plugin: " + plugin, e);
                }
            }
        }

        #region Manifest

        void SaveNewManifest()
        {

            TriggerEvent(BeforeSaveNewManifest);
            CreateManifest(FileMerger.ExternalJsFiles.Select(t => t.JsFile.Filename).Concat(EmbeddedResourceFiles).ToList()).SaveToFile(Args.ManifestFile);

            TriggerEvent(AfterSaveNewManifest);
        }

        Manifest CreateManifest(List<string> externalFiles)
        {
            return new ManifestHelper { Args = Args, Log = Log, SkcVersion = SkcVersion, SkcFile = typeof(CompilerTool).Assembly.Location, ExternalFiles = externalFiles }.CreateManifest();
        }

        #endregion

        #region ParseArgs

        bool CheckManifest()
        {
            if (Args.rebuild.GetValueOrDefault())
                return false;
            if (!File.Exists(Args.ManifestFile))
                return false;
            var prev = Manifest.LoadFromFile(Args.ManifestFile);
            var current = CreateManifest(prev.ExternalFiles.Select(t => t.Filename).ToList());
            Trace.TraceInformation("[{0}] Comparing manifests", DateTime.Now);
            var diff = current.GetManifestDiff(prev);
            if (diff.AreManifestsEqual)
            {
                Log.WriteLine("Code was unmodified - build skipped");
                return true;
            }
            else
            {
                File.Delete(Args.ManifestFile);
                Log.WriteLine("Reasons for rebuild:\n" + diff.ToString());
            }
            return false;
        }

        void ParseArgs()
        {
            Args = CompilerToolArgs.Parse(CommandLineArguments);
        }

        void WriteArgs()
        {
            Log.WriteLine(Process.GetCurrentProcess().MainModule.FileName + " " + ArgsToString());
        }

        string LastArgs;
        void SaveLastInputArgs()
        {
            var file = Process.GetCurrentProcess().MainModule.FileName.ToFileInfo().Directory.GetFile("prms.txt");
            if (file.Exists)
                LastArgs = file.ReadAllText();
            var s = ArgsToString();
            try
            {
                file.WriteAllText(s);
            }
            catch
            {
            }
        }

        string ArgsToString()
        {
            var sb = new StringBuilder();
            CommandLineArguments.ForEachJoin(arg =>
            {
                if (arg.StartsWith("@"))
                    sb.Append(File.ReadAllText(arg.Substring(1)));
                else
                    sb.Append(arg);
            }, () => sb.Append(" "));
            var s = sb.ToString();
            if (!s.Contains("/dir"))
                s = String.Format("/dir:\"{0}\" ", Directory.GetCurrentDirectory()) + s;
            return s;
        }

        void CalculateMissingArgs()
        {
            if (Args.CurrentDirectory.IsNotNullOrEmpty())
                Directory.SetCurrentDirectory(Args.CurrentDirectory);
            if (Args.Output == null)
                Args.Output = "output.js";
            if (Args.ManifestFile == null)
                Args.ManifestFile = Path.Combine(Path.GetDirectoryName(Args.Output), Args.AssemblyName + ".skccache");
            if (Args.CodeAnalysisFile == null)
                Args.CodeAnalysisFile = Path.Combine(Path.GetDirectoryName(Args.Output), Args.AssemblyName + ".CodeAnalysis");
            if (Args.SecurityAnalysisFile == null)
                Args.SecurityAnalysisFile = Path.Combine(Path.GetDirectoryName(Args.Output), Args.AssemblyName + ".securityanalysis");

            Defines = Args.define != null ? Args.define.Split(';').ToList() : new List<string>();

        }

        #endregion

        void ParseCs()
        {
            TriggerEvent(BeforeParseCs);
            _CustomAttributeProvider = new CustomAttributeProvider();
            Project = new SkProject
            {
                SourceFiles = Args.Files,
                Defines = Defines,
                References = Args.References,
                Log = Log,
                TargetFrameworkVersion = Args.TargetFrameworkVersion,
                Compiler = this,
                AssemblyName = Args.AssemblyName,
            };
            Project.Parse();
            var asm = Project.Compilation.MainAssembly;
            if (asm != null && asm.AssemblyName == null)
            {
                throw new NotImplementedException();
                //asm.AssemblyName = Args.AssemblyName;
            }

            TriggerEvent(AfterParseCs);
        }

        void ApplyExternalMetadata()
        {

            TriggerEvent(BeforeApplyExternalMetadata);
            CsExternalMetadata = new CsExternalMetadata { Project = Project, Log = Log };
            CsExternalMetadata.Process();

            TriggerEvent(AfterApplyExternalMetadata);
        }

        void ConvertCsToJs()
        {
            TriggerEvent(BeforeConvertCsToJs);
            PathMerger = new PathMerger();
            TypeConverter = new TypeConverter
            {
                Compiler = this,
                Log = Log,
                ExternalMetadata = CsExternalMetadata,
                AssemblyName = Args.AssemblyName,
                BeforeExportTypes = byFile =>
                    {
                        var list = new List<ITypeDefinition>();
                        foreach (var list2 in byFile.Values)
                        {
                            list.AddRange(list2);
                        }
                        var skFiles = Project.GetNFiles(list);
                        Project.ApplyNavigator(skFiles);
                    }
            };
            TypeConverter.ConfigureMemberConverter += new Action<MemberConverter>(JsModelImporter_ConfigureJsTypeImporter);
            var att = GetJsExportAttribute();
            if (att != null)
            {
                TypeConverter.ExportComments = att.ExportComments;
                //LongFunctionNames = att.LongFunctionNames;
                //Minify = att.Minify;
                //EnableProfiler = att.EnableProfiler;
            }


            TypeConverter.Process();
            SkJsFiles = TypeConverter.JsFiles.Select(ToSkJsFile).ToList();

            TriggerEvent(AfterConvertCsToJs);
        }

        private SkJsFile ToSkJsFile(JsFile t)
        {
            return new SkJsFile { JsFile = t, Compiler = this };
        }

        void JsModelImporter_ConfigureJsTypeImporter(MemberConverter obj)
        {
            obj.BeforeVisitEntity += me =>
            {
                if (BeforeConvertCsToJsEntity != null)
                    BeforeConvertCsToJsEntity(me);
            };
            obj.AfterVisitEntity += (me, node) =>
            {
                if (AfterConvertCsToJsEntity != null)
                    AfterConvertCsToJsEntity(me, node);
            };
            obj.AstNodeConverter.BeforeConvertCsToJsAstNode += node =>
            {
                if (BeforeConvertCsToJsAstNode != null)
                    BeforeConvertCsToJsAstNode(node);
            };
            obj.AstNodeConverter.AfterConvertCsToJsAstNode += (node, node2) =>
            {
                if (AfterConvertCsToJsAstNode != null)
                    AfterConvertCsToJsAstNode(node, node2);
            };
            obj.AstNodeConverter.ResolveResultConverter.BeforeConvertCsToJsResolveResult += res =>
            {
                if (BeforeConvertCsToJsResolveResult != null)
                    BeforeConvertCsToJsResolveResult(res);
            };
            obj.AstNodeConverter.ResolveResultConverter.AfterConvertCsToJsResolveResult += (res, node) =>
            {
                if (AfterConvertCsToJsResolveResult != null)
                    AfterConvertCsToJsResolveResult(res, node);
            };
        }



        void MergeJsFiles()
        {

            TriggerEvent(BeforeMergeJsFiles);
            FileMerger = new FileMerger { Project = Project, JsFiles = SkJsFiles, Log = Log, Compiler = this };
            Time(GenerateCodeInjectionFile);

            FileMerger.MergeFiles();
            if (Args.OutputGeneratedJsFile.IsNotNullOrEmpty())
            {
                var file = FileMerger.GetJsFile(Args.OutputGeneratedJsFile, false);
                FileMerger.MergeFiles(file, SkJsFiles.Where(t => t != file).ToList());
                SkJsFiles.Clear();
                SkJsFiles.Add(file);
            }

            ApplyJsMinifyAndSourceMap();


            TriggerEvent(AfterMergeJsFiles);
        }

        void ApplyJsMinifyAndSourceMap()
        {
            var att = Project.Compilation.MainAssembly.GetMetadata<JsExportAttribute>();
            if (att != null)
            {
                if (att.Minify)
                    SkJsFiles.ForEach(t => t.Minify = true);
                if (att.GenerateSourceMaps)
                    SkJsFiles.ForEach(t => t.GenerateSourceMap = true);
            }
        }

        #region Code Injection
        void AddCodeInjectionDependency(string funcName, string depFuncName)
        {
            var func = CodeInjections.Where(t => t.FunctionName == funcName).FirstOrDefault();
            var dep = CodeInjections.Where(t => t.FunctionName == depFuncName).FirstOrDefault();
            if (func != null && dep != null)
                func.Dependencies.Add(dep);
        }

        JsUnit CreateSharpKitHeaderUnit()
        {
            var unit = new JsUnit { Statements = new List<JsStatement>() };
            var att = GetJsExportAttribute();
            if (att == null || !att.OmitSharpKitHeaderComment)
            {
                var txt = " Generated by SharpKit 5 " + SkcVersion + " ";
                if (att != null && att.AddTimeStampInSharpKitHeaderComment)
                    txt += "on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ";
                unit.Statements.Add(new JsCommentStatement { Text = txt });
            }
            if (att != null && att.UseStrict)
            {
                unit.Statements.Add(new JsUseStrictStatement());
            }

            return unit;
        }

        HashSet<string> CheckHelperMethodsUsage(SkJsFile file, string[] methods)
        {
            var usage = new HashSet<string>();
            if (methods.Length == 0)
                return usage;
            var list = new HashSet<string>(methods);
            foreach (var unit in file.JsFile.Units)
            {
                foreach (var node in unit.Descendants<JsInvocationExpression>())
                {
                    var me = node.Member as JsMemberExpression;
                    if (me != null && list.Remove(me.Name))
                    {
                        usage.Add(me.Name);
                        if (list.Count == 0)
                            break;
                    }
                }
            }
            return usage;
        }

        void GenerateCodeInjectionFile()
        {
            var att = Project.Compilation.MainAssembly.GetMetadata<JsExportAttribute>();
            if (att != null && att.CodeInjectionFilename.IsNotNullOrEmpty())
            {
                CodeInjectionFile = GetCreateSkJsFile(att.CodeInjectionFilename);
                var headerUnit = CreateSharpKitHeaderUnit();
                foreach (var ci in CodeInjections)
                {
                    headerUnit.Statements.Add(ci.JsStatement);
                }
                CodeInjectionFile.JsFile.Units.Insert(0, headerUnit);
            }
        }

        SkJsFile GetCreateSkJsFile(string filename)
        {
            return FileMerger.GetJsFile(filename, false);
            //return SkJsFiles.Where(t => t.JsFile.Filename.EqualsIgnoreCase(filename)).FirstOrDefault();
        }

        void InjectJsCode()
        {

            TriggerEvent(BeforeInjectJsCode);
            if (SkJsFiles.IsNotNullOrEmpty())
            {
                var helperMethods = CodeInjections.Select(t => t.FunctionName).ToArray();
                foreach (var file in SkJsFiles)
                {
                    if (file.JsFile == null || file.JsFile.Units.IsNullOrEmpty())
                        continue;
                    if (file == CodeInjectionFile)
                        continue;
                    var jsFile = file.JsFile;
                    var ext = Path.GetExtension(jsFile.Filename).ToLower();
                    if (ext == ".js")
                    {
                        var headerUnit = CreateSharpKitHeaderUnit();
                        if (CodeInjectionFile == null)
                        {
                            var usage = CheckHelperMethodsUsage(file, helperMethods);
                            var handled = new HashSet<CodeInjection>();
                            foreach (var funcName in usage)
                            {
                                var ci = CodeInjections.Where(t => t.FunctionName == funcName).FirstOrDefault();
                                foreach (var ci2 in ci.SelfAndDependencies())
                                {
                                    if (handled.Add(ci2))
                                        headerUnit.Statements.Add(ci2.JsStatement.Clone());
                                }
                            }
                        }
                        jsFile.Units.Insert(0, headerUnit);
                    }

                }
            }

            TriggerEvent(AfterInjectJsCode);
        }

        #endregion

        #region ValidateUnits
        void ValidateUnits()
        {
            SkJsFiles.Select(t => t.JsFile).ToList().ForEach(ValidateUnits);
        }
        void ValidateUnits(JsFile file)
        {
            file.Units.ForEach(ValidateUnit);
        }
        void ValidateUnit(JsNode node)
        {
            if (node == null)
                throw new NotImplementedException();
            var children = node.Children().ToList();
            children.ForEach(ValidateUnit);
        }

        #endregion

        #region Optimize

        void OptimizeClrJsTypesArrayVerification()
        {
            if (TypeConverter == null || TypeConverter.ClrConverter == null)
                return;
            var st = TypeConverter.ClrConverter.VerifyJsTypesArrayStatement;
            if (st == null)
                return;
            if (SkJsFiles.IsNullOrEmpty())
                return;
            foreach (var file in SkJsFiles)
            {
                if (file.JsFile == null || file.JsFile.Units == null)
                    continue;
                foreach (var unit in file.JsFile.Units)
                {
                    if (unit.Statements == null)
                        continue;
                    unit.Statements.RemoveDoubles(t => t.Annotation<VerifyJsTypesArrayStatementAnnotation>() != null);
                }
            }
        }

        void OptimizeJsFiles()
        {

            TriggerEvent(BeforeOptimizeJsFiles);
            OptimizeClrJsTypesArrayVerification();
            OptimizeNamespaceVerification();

            TriggerEvent(AfterOptimizeJsFiles);
        }

        void OptimizeNamespaceVerification()
        {
            if (SkJsFiles.IsNullOrEmpty())
                return;
            foreach (var file in SkJsFiles)
            {
                if (file.JsFile == null || file.JsFile.Units.IsNullOrEmpty())
                    continue;
                foreach (var unit in file.JsFile.Units)
                {
                    OptimizeNamespaceVerification(unit);
                }
            }
        }

        string GetNamespaceVerification(JsStatement st)
        {
            var ex = st.Annotation<NamespaceVerificationAnnotation>();
            if (ex != null)
                return ex.Namespace;
            return null;
        }

        void OptimizeNamespaceVerification(JsUnit unit)
        {
            if (unit.Statements.IsNullOrEmpty())
                return;
            unit.Statements.RemoveDoublesByKey(t => GetNamespaceVerification(t));

        }
        #endregion

        void SaveJsFiles()
        {
            TriggerEvent(BeforeSaveJsFiles);
            var att = GetJsExportAttribute();
            string format = null;
            if (att != null)
                format = att.JsCodeFormat;
            foreach (var file in SkJsFiles)
            {
                file.Format = format;
                file.Save();
            }
            TriggerEvent(AfterSaveJsFiles);
        }

        void EmbedResources()
        {
            TriggerEvent(BeforeEmbedResources);
            var atts = Project.Compilation.MainAssembly.GetMetadatas<JsEmbeddedResourceAttribute>().ToList();
            if (atts.IsNotNullOrEmpty())
            {
                var asmFilename = Args.Output;
                Log.WriteLine("Loading assembly {0}", asmFilename);
                var asm = ModuleDefinition.ReadModule(asmFilename);
                var changed = false;
                foreach (var att in atts)
                {
                    if (att.Filename.IsNullOrEmpty())
                        throw new CompilerException(att.SourceAttribute, "JsEmbeddedResourceAttribute.Filename must be set");
                    EmbeddedResourceFiles.Add(att.Filename);
                    var resName = att.ResourceName ?? att.Filename;
                    Log.WriteLine("Embedding {0} -> {1}", att.Filename, resName);
                    var res = new EmbeddedResource(resName, ManifestResourceAttributes.Public, File.ReadAllBytes(att.Filename));
                    var res2 = asm.Resources.Where(t => t.Name == res.Name).OfType<EmbeddedResource>().FirstOrDefault();
                    if (res2 == null)
                    {
                        asm.Resources.Add(res);
                    }
                    else
                    {
                        IStructuralEquatable data2 = res2.GetResourceData();
                        IStructuralEquatable data = res.GetResourceData();

                        if (data.Equals(data2))
                            continue;
                        asm.Resources.Remove(res2);
                        asm.Resources.Add(res);
                    }
                    changed = true;

                }
                if (changed)
                {
                    var prms = new WriterParameters { };//TODO:StrongNameKeyPair = new StrongNameKeyPair("Foo.snk") };
                    var snkFile = Args.NoneFiles.Where(t => t.EndsWith(".snk", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (snkFile != null)
                    {
                        Log.WriteLine("Signing assembly with strong-name keyfile: {0}", snkFile);
                        prms.StrongNameKeyPair = new StrongNameKeyPair(snkFile);
                    }
                    Log.WriteLine("Saving assembly {0}", asmFilename);
                    asm.Write(asmFilename, prms);
                }
            }
            TriggerEvent(AfterEmbedResources);

        }

        #region NativeImage

        bool ShouldCreateNativeImage()
        {
            if (Args != null && Args.CreateNativeImage)
                return true;
            return false;
            //if (Debug)
            //{
            //    Log.WriteLine("ShouldCreateNativeImage? no - debug mode");
            //    return false;
            //}
            //if (!CompilerConfiguration.Current.CreateNativeImage)
            //{
            //    Log.WriteLine("ShouldCreateNativeImage? no - config says not to");
            //    return false;
            //}
            //var dir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            //Skc5CacheDataFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SharpKit\\5\\skc5.exe.cache");

            //Skc5CacheData = new Skc5CacheData();
            //if (!File.Exists(Skc5CacheDataFilename))
            //{
            //    Log.WriteLine("ShouldCreateNativeImage? yes - cache file not found");
            //    return true;
            //}
            //Skc5CacheData.Load(Skc5CacheDataFilename);
            //if (VersionKey != Skc5CacheData.VersionKey)
            //{
            //    if (Skc5CacheData.NGenRetries.GetValueOrDefault() > 3)
            //    {
            //        Log.WriteLine("ShouldCreateNativeImage? false - NGenRetries>3");
            //        return false;
            //    }
            //    Log.WriteLine("ShouldCreateNativeImage? true - NGenRetries<=3");
            //    return true;
            //}
            //Log.WriteLine("ShouldCreateNativeImage? false - already created");
            //return false;
        }
        public void VerifyNativeImage()
        {
            try
            {
                if (ShouldCreateNativeImage())
                {
                    CreateNativeImage();
                    //Skc5CacheData.NGenRetries = 0;
                    //Skc5CacheData.CreatedNativeImage = true;
                    //Skc5CacheData.VersionKey = VersionKey;
                    //Skc5CacheData.Save(Skc5CacheDataFilename);
                }
            }
            catch (Exception e)
            {
                //Skc5CacheData.NGenRetries++;
                //Log.Debug("VerifyNativeImage failed: " + e);
                //try
                //{
                //    Skc5CacheData.Save(Skc5CacheDataFilename);
                //}
                //catch (Exception ee)
                //{
                //    Log.Debug("Save skc.exe.cache failed: " + ee);
                //    Log.Warn(ee.ToString());
                //}
                Log.Warn(e.ToString());
            }
        }
        void CreateNativeImage()
        {
            Log.WriteLine("CreateNativeImage started");
            var windir = Environment.ExpandEnvironmentVariables("%windir%");
            var ngen = Path.Combine(windir, @"Microsoft.NET\Framework\v4.0.30319\ngen.exe");
            var skc = Process.GetCurrentProcess().MainModule.FileName;
            var args = "install " + skc;
            var pi = new ProcessStartInfo { CreateNoWindow = true };

            var p = Process.Start(ngen, args);
            p.WaitForExit();
            if (p.ExitCode != 0)
                throw new Exception("CreateNativeImage failed - command=" + ngen + " " + args);
            Log.WriteLine("CreateNativeImage finished");
        }
        #endregion

        #region ICompiler Members

        public event Action BeforeParseCs;
        public event Action BeforeApplyExternalMetadata;
        public event Action BeforeConvertCsToJs;
        public event Action BeforeMergeJsFiles;
        public event Action BeforeInjectJsCode;
        public event Action BeforeOptimizeJsFiles;
        public event Action BeforeSaveJsFiles;
        public event Action BeforeEmbedResources;
        public event Action BeforeSaveNewManifest;

        public event Action AfterParseCs;
        public event Action AfterApplyExternalMetadata;
        public event Action AfterConvertCsToJs;
        public event Action AfterMergeJsFiles;
        public event Action AfterInjectJsCode;
        public event Action AfterOptimizeJsFiles;
        public event Action AfterSaveJsFiles;
        public event Action AfterEmbedResources;
        public event Action AfterSaveNewManifest;


        public ICompilation CsCompilation
        {
            get
            {
                if (Project == null)
                    return null;
                return Project.Compilation;
            }
        }

        #endregion

        #region Utils

        [DebuggerStepThrough]
        void Time(Action action)
        {
            var stopwatch = new Stopwatch();
            Log.WriteLine("{0:HH:mm:ss.fff}: {1}: Start: ", DateTime.Now, action.Method.Name);
            stopwatch.Start();
            action();
            stopwatch.Stop();
            Log.WriteLine("{0:HH:mm:ss.fff}: {1}: End: {2}ms", DateTime.Now, action.Method.Name, stopwatch.ElapsedMilliseconds);
        }

        [DebuggerStepThrough]
        T Time2<T>(Func<T> action)
        {
            var stopwatch = new Stopwatch();
            Log.WriteLine("{0:HH:mm:ss.fff}: {1}: Start: ", DateTime.Now, action.Method.Name);
            stopwatch.Start();
            var t = action();
            stopwatch.Stop();
            Log.WriteLine("{0:HH:mm:ss.fff}: {1}: End: {2}ms", DateTime.Now, action.Method.Name, stopwatch.ElapsedMilliseconds);
            return t;
        }

        void TriggerEvent(Action ev)
        {
            if (ev != null)
                ev();
        }

        #endregion

        #region Code Injection resources

        string NativeAnonymousDelegateSupportCode = @"if (typeof ($CreateAnonymousDelegate) == 'undefined') {
    var $CreateAnonymousDelegate = function (target, func) {
        if (target == null || func == null)
            return func;
        var delegate = function () {
            return func.apply(target, arguments);
        };
        delegate.func = func;
        delegate.target = target;
        delegate.isDelegate = true;
        return delegate;
    }
}
";
        string NativeDelegateSupportCode = @"if (typeof($CreateDelegate)=='undefined'){
    if(typeof($iKey)=='undefined') var $iKey = 0;
    if(typeof($pKey)=='undefined') var $pKey = String.fromCharCode(1);
    var $CreateDelegate = function(target, func){
        if (target == null || func == null) 
            return func;
        if(func.target==target && func.func==func)
            return func;
        if (target.$delegateCache == null)
            target.$delegateCache = {};
        if (func.$key == null)
            func.$key = $pKey + String(++$iKey);
        var delegate;
        if(target.$delegateCache!=null)
            delegate = target.$delegateCache[func.$key];
        if (delegate == null){
            delegate = function(){
                return func.apply(target, arguments);
            };
            delegate.func = func;
            delegate.target = target;
            delegate.isDelegate = true;
            if(target.$delegateCache!=null)
                target.$delegateCache[func.$key] = delegate;
        }
        return delegate;
    }
}
";
        string NativeExtensionDelegateSupportCode = @"if (typeof($CreateExtensionDelegate)=='undefined'){
    if(typeof($iKey)=='undefined') var $iKey = 0;
    if(typeof($pKey)=='undefined') var $pKey = String.fromCharCode(1);
    var $CreateExtensionDelegate = function(target, func){
        if (target == null || func == null) 
            return func;
        if(func.target==target && func.func==func)
            return func;
        if (target.$delegateCache == null)
            target.$delegateCache = {};
        if (func.$key == null)
            func.$key = $pKey + String(++$iKey);
        var delegate;
        if(target.$delegateCache!=null)
            delegate = target.$delegateCache[func.$key];
        if (delegate == null){
            delegate = function(){
                var args = [target];
                for(var i=0;i<arguments.length;i++)
                    args.push(arguments[i]);
                return func.apply(null, args);
            };
            delegate.func = func;
            delegate.target = target;
            delegate.isDelegate = true;
            delegate.isExtensionDelegate = true;
            if(target.$delegateCache!=null)
                target.$delegateCache[func.$key] = delegate;
        }
        return delegate;
    }
}
";

        //        string NativeInheritanceSupportCode =
        //@"if (typeof($Inherit)=='undefined') {
        //    var $Inherit = function(ce, ce2) {
        //        for (var p in ce2.prototype)
        //            if (typeof(ce.prototype[p]) == 'undefined' || ce.prototype[p]==Object.prototype[p])
        //                ce.prototype[p] = ce2.prototype[p];
        //        for (var p in ce2)
        //            if (typeof(ce[p]) == 'undefined')
        //                ce[p] = ce2[p];
        //        ce.$baseCtor = ce2;
        //    }
        //}";

        string NativeInheritanceSupportCode =
@"if (typeof ($Inherit) == 'undefined') {
	var $Inherit = function (ce, ce2) {

		if (typeof (Object.getOwnPropertyNames) == 'undefined') {

			for (var p in ce2.prototype)
				if (typeof (ce.prototype[p]) == 'undefined' || ce.prototype[p] == Object.prototype[p])
					ce.prototype[p] = ce2.prototype[p];
			for (var p in ce2)
				if (typeof (ce[p]) == 'undefined')
					ce[p] = ce2[p];
			ce.$baseCtor = ce2;

		} else {

			var props = Object.getOwnPropertyNames(ce2.prototype);
			for (var i = 0; i < props.length; i++)
				if (typeof (Object.getOwnPropertyDescriptor(ce.prototype, props[i])) == 'undefined')
					Object.defineProperty(ce.prototype, props[i], Object.getOwnPropertyDescriptor(ce2.prototype, props[i]));

			for (var p in ce2)
				if (typeof (ce[p]) == 'undefined')
					ce[p] = ce2[p];
			ce.$baseCtor = ce2;

		}

	}
};
";


        string CreateExceptionSupportCode =
@"if (typeof($CreateException)=='undefined') 
{
    var $CreateException = function(ex, error) 
    {
        if(error==null)
            error = new Error();
        if(ex==null)
            ex = new System.Exception.ctor();       
        error.message = ex.message;
        for (var p in ex)
           error[p] = ex[p];
        return error;
    }
}
";

        string CreateAnonymousObjectSupportCode =
@"if (typeof($CreateAnonymousObject)=='undefined') 
{
    var $CreateAnonymousObject = function(json)
    {
        var obj = new System.Object.ctor();
        obj.d = json;
        for(var p in json){
            obj['get_'+p] = new Function('return this.d.'+p+';');
        }
        return obj;
    }
}
";


        string RemoveDelegateSupportCode =
@"function $RemoveDelegate(delOriginal,delToRemove)
{
    if(delToRemove == null || delOriginal == null)
        return delOriginal;
    if(delOriginal.isMulticastDelegate)
    {
        if(delToRemove.isMulticastDelegate)
            throw new Error(""Multicast to multicast delegate removal is not implemented yet"");
        var del=$CreateMulticastDelegateFunction();
        for(var i=0;i < delOriginal.delegates.length;i++)
        {
            var del2=delOriginal.delegates[i];
            if(del2 != delToRemove)
            {
                if(del.delegates == null)
                    del.delegates = [];
                del.delegates.push(del2);
            }
        }
        if(del.delegates == null)
            return null;
        if(del.delegates.length == 1)
            return del.delegates[0];
        return del;
    }
    else
    {
        if(delToRemove.isMulticastDelegate)
            throw new Error(""single to multicast delegate removal is not supported"");
        if(delOriginal == delToRemove)
            return null;
        return delOriginal;
    }
};
";
        string CombineDelegatesSupportCode =
        @"function $CombineDelegates(del1,del2)
{
    if(del1 == null)
        return del2;
    if(del2 == null)
        return del1;
    var del=$CreateMulticastDelegateFunction();
    del.delegates = [];
    if(del1.isMulticastDelegate)
    {
        for(var i=0;i < del1.delegates.length;i++)
            del.delegates.push(del1.delegates[i]);
    }
    else
    {
        del.delegates.push(del1);
    }
    if(del2.isMulticastDelegate)
    {
        for(var i=0;i < del2.delegates.length;i++)
            del.delegates.push(del2.delegates[i]);
    }
    else
    {
        del.delegates.push(del2);
    }
    return del;
};
";
        string CreateMulticastDelegateFunctionSupportCode =
        @"function $CreateMulticastDelegateFunction()
{
    var del2 = null;
    
    var del=function()
    {
        var x=undefined;
        for(var i=0;i < del2.delegates.length;i++)
        {
            var del3=del2.delegates[i];
            x = del3.apply(null,arguments);
        }
        return x;
    };
    del.isMulticastDelegate = true;
    del2 = del;   
    
    return del;
};
";
        #endregion



        #region ICompiler Members


        public event Action<IEntity> BeforeConvertCsToJsEntity;

        public event Action<IEntity, JsNode> AfterConvertCsToJsEntity;



        public event Action BeforeExit;



        public event Action<ICSharpCode.NRefactory.CSharp.AstNode> BeforeConvertCsToJsAstNode;

        public event Action<ICSharpCode.NRefactory.CSharp.AstNode, JsNode> AfterConvertCsToJsAstNode;

        public event Action<ICSharpCode.NRefactory.Semantics.ResolveResult> BeforeConvertCsToJsResolveResult;

        public event Action<ICSharpCode.NRefactory.Semantics.ResolveResult, JsNode> AfterConvertCsToJsResolveResult;


        CustomAttributeProvider _CustomAttributeProvider;
        public ICustomAttributeProvider CustomAttributeProvider
        {
            get { return _CustomAttributeProvider; }
        }

        #endregion

        Lazy<JsExportAttribute> _JsExportAttribute;
        private JsStatement CreateAnonymousObjectSupportStatement;
        public JsExportAttribute GetJsExportAttribute()
        {
            if (_JsExportAttribute == null)
            {
                _JsExportAttribute = new Lazy<JsExportAttribute>(() => Sk.GetJsExportAttribute(Project.Compilation));
            }
            return _JsExportAttribute.Value;
        }

    }



}
