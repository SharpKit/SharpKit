using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Amazon;
//using Amazon.S3;
//using Amazon.S3.Util;
//using Amazon.S3.Model;
using System.Net;
using System.Net.Security;
using System.Diagnostics;
using System.Xml.Linq;
//using Amazon.S3.Transfer;
using SharpKit.Release.Utils;
using System.Xml;
using SharpKit.Utils;
using SharpKit.Installer.Builder;
using SharpKit.Installer;
using Octokit;
using System.Text.RegularExpressions;

namespace SharpKit.Release
{
    class Program
    {

        string ProductVersion { get; set; }
        string SetupFilename { get; set; }
        public string InstallerProjectDir { get; set; }

        string GitRoot;

        static void Main(string[] args)
        {
            new Program().Run();
        }

        void Run()
        {
            GitRoot = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "..", "..", "..")).FullName + Path.DirectorySeparatorChar;
            InstallerProjectDir = Path.Combine(GitRoot, "Installer", "Installer") + Path.DirectorySeparatorChar;
            ReleaseLogsDir = GitRoot + @"Installer\Packager\ReleaseLogs\";
            ReadVersion();

            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                ProcessCommand(args[1]);
                return;
            }

            Console.WriteLine("Note: you can automate all commands via packager.exe <command>");

            while (true)
            {
                ReadVersion();

                if (Config.GitHubAccessToken.IsNullOrEmpty())
                {
                    WriteLine("WARNING: GitHubAccessToken in config.xml not set!", ConsoleColor.White);
                }

                Console.WriteLine("CurrentVersion " + ProductVersion);
                Console.WriteLine();
                Console.WriteLine("Please choose a command:");
                Console.WriteLine();
                WriteCommand("create-version", "Creates a new version. Note: Compiler and SDK will be recompiled, because js headers will change");
                WriteCommand("commit", "Commits the modified files with commit message containing the version. You can do this via external tool. Note: Not implemented yet.");
                WriteCommand("push", "Pushes the changes to github. You can do this via external tool. Note: Not implemented yet.");
                WriteCommand("create-release", "Creates a github release, with the changelog as description. Note: Before running this command, a push is requied!");
                WriteCommand("create-installer");
                WriteCommand("upload");
                WriteCommand("rollback", "Reverts all changed version files to its original state. Assumes, that they are not commited. Note: It's not fully implemented.");
                WriteCommand("release", "runs create-version, create-installer, create-release and upload.");
                WriteCommand("exit");

                Console.WriteLine();
                Console.Write("Command: ");
                var cmd = Console.ReadLine();
                if (cmd == "exit") return;
                ProcessCommand(cmd);
                Console.WriteLine();
                Console.WriteLine();
            }

        }

        public void WriteCommand(string cmd, string description = "")
        {
            Write(cmd, ConsoleColor.Green);
            if (description.IsNullOrEmpty())
                Console.WriteLine();
            else
                Console.WriteLine(": " + description);
        }

        public void WriteLine(string text, ConsoleColor color)
        {
            Write(text + Environment.NewLine, color);
        }

        public void Write(string text, ConsoleColor color)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = old;
        }

        public void ReadVersion()
        {
            ProductVersion = File.ReadAllLines(Path.Combine(GitRoot, "VERSION"))[0];
        }

        public void ProcessCommand(string cmd)
        {
            ReadVersion();

            switch (cmd)
            {

                case "create-version":
                    {
                        CreateNewVersion();
                        break;
                    }

                case "create-installer":
                    {
                        CreateInstaller();
                        break;
                    }

                case "create-release":
                    {
                        CreateGitHubRelease();
                        break;
                    }

                case "upload":
                    {
                        Upload();
                        break;
                    }

                case "rollback":
                    {
                        Rollback();
                        break;
                    }

                case "release":
                    {
                        Release();
                        break;
                    }

                default:
                    Console.WriteLine("Unknown command / not implemented");
                    return;
            }
            Console.WriteLine("Command finished");
        }

        public void Release()
        {
            CreateNewVersion();
            CreateInstaller();
            CreateGitHubRelease();
            Upload();
        }

        #region "CreateNewVersion"

        public void CreateNewVersion()
        {
            Console.WriteLine("Enter new version (optional): ");
            var v = Console.ReadLine();
            if (v.IsNotNullOrEmpty())
            {
                CreateNewVersion(v);
                return;
            }
        }

        public void CreateNewVersion(string version)
        {
            if (version[0] == 'v')
            {
                Console.WriteLine("Version should begin with a number, not with a 'v'");
                return;
            }

            ProductVersion = version;
            ReleaseLog = new ReleaseLog { Created = DateTime.Now, Filename = Path.Combine(ReleaseLogsDir, ProductVersion + ".xml"), Version = ProductVersion };
            FillLog();
            ReleaseLog.Save();

            UpdateVersionFiles();

            SharpKit.Installer.Builder.Utils.CallMake(Path.Combine(GitRoot, "Compiler"));
            SharpKit.Installer.Builder.Utils.CallMake(Path.Combine(GitRoot, "SDK")); //Because the the js files contains the version in the header, the files need to be regenerated before commit

            WriteLine("******************************************************************************", ConsoleColor.Green);
            WriteLine("The new version is now changed in all files. Please commit and push it to git now BEFORE you create a github release/tag!", ConsoleColor.Green);
            WriteLine("******************************************************************************", ConsoleColor.Green);

            Console.WriteLine("Press enter key to continue...");
            Console.ReadLine();

            //CommitGit();
        }

        public void UpdateVersionFiles()
        {
            File.WriteAllText(Path.Combine(GitRoot, "VERSION"), ProductVersion);
            //UpdateSharpKitVersionInfoSourceFiles(ReleaseLog);
            UpdateAssemblyFileVersions(ProductVersion);
            SetupBuilder.CreateConfig(Path.Combine(GitRoot, "Installer", "Installer", "res", "Config.xml"), ProductVersion);
        }

        void UpdateAssemblyFileVersions(string version)
        {
            //foreach (var dir in Directory.GetDirectories(SdkSrcDir))
            //{
            //    var file = Path.Combine(dir, "Properties\\AssemblyInfo");
            //    if (File.Exists(file))
            //    {
            //        UpdateAssemblyFileVersion(file, version);
            //    }
            //}

            foreach (var project in SharpKitCompilerProjectNames)
            {
                var file = GitRoot + "\\Compiler\\" + project + "\\Properties\\AssemblyInfo.cs";
                if (File.Exists(file))
                    UpdateAssemblyFileVersion(file, version);
            }
            //UpdateAssemblyFileVersion(@"C:\Projects\SharpJs\lib\NRefactory\ICSharpCode.NRefactory\Properties\GlobalAssemblyInfo.cs", version);
            //UpdateAssemblyFileVersion(@"C:\Projects\SharpJs\lib\Mono.Cecil\Mono.Cecil\AssemblyInfo.cs", version);

        }

        private void UpdateAssemblyFileVersion(string file, string version)
        {
            Console.WriteLine("Updating {0}", file);
            var lines = File.ReadAllLines(file).ToList();
            var index = lines.FindIndex(t => t.StartsWith("[assembly: AssemblyFileVersion("));
            if (index < 0)
            {
                lines.Add("");
                index = lines.Count - 1;
            }
            Console.WriteLine(lines[index]);
            lines[index] = String.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version);
            Console.WriteLine(lines[index]);
            File.WriteAllLines(file, lines);
        }

        #endregion

        #region "ReleaseLog"

        ReleaseLog ReleaseLog;

        void UpdateSharpKitVersionInfoSourceFiles(ReleaseLog log)
        {
            UpdateSharpKitVersionInfoSourceFiles(log.Version, log.Created);
        }
        void UpdateSharpKitVersionInfoSourceFiles(string version, DateTime dt)
        {
            var line = VersionInfoToCode(version, dt);
            line = "            " + line + ",";
            var files = GitRoot.ToDirectoryInfo().GetFiles("SharpKitVersionInfo.cs", SearchOption.AllDirectories).Select(t => t.FullName).ToList();
            files.ForEach(t => InsertLinesAfterPlaceHolder(t, new[] { line }));
        }

        private void InsertLinesAfterPlaceHolder(string filename, string[] lines2)
        {
            var lines = File.ReadAllLines(filename).ToList();
            var index = lines.FindIndex(t => t.Contains("<!--Placeholder-->"));
            lines.InsertRange(index + 1, lines2);
            File.WriteAllLines(filename, lines.ToArray());
        }

        private string VersionInfoToCode(string version, DateTime dt)
        {
            var line = String.Format("new SharpKitVersionInfo {{ Version = \"{0}\", Date = new DateTime({1}, {2}, {3}) }}", version, dt.Year, dt.Month, dt.Day);
            return line;
        }

        private void FillLog()
        {
            //var lastReleaseLog = LastReleaseLog;
            ReleaseLog.SharpKit5 = CreateSolutionInfo(GitRoot);
            ReleaseLog.SharpKit_Sdk = CreateSolutionInfo(Path.Combine(GitRoot, "SDK"));

            ShowLogMessages(ReleaseLog.SharpKit5.SvnLogEntries);
            ShowLogMessages(ReleaseLog.SharpKit_Sdk.SvnLogEntries);
        }

        void ShowLogMessages(List<VersionControlLogEntry> log)
        {
            var msgs = GetLogMessages(log);
            msgs.ForEach(t => Console.WriteLine(t));
        }

        private List<string> GetLogMessages(List<VersionControlLogEntry> log)
        {
            var msgs = log.Select(t => t.msg).Where(t => t.IsNotNullOrEmpty()).ToList();
            return msgs;
        }

        SolutionInfo CreateSolutionInfo(string srcDir)
        {
            var si = new SolutionInfo { HeadRevision = GetHeadRevision(srcDir) };
            si.SvnLogEntries = GetGitLog(srcDir, GetLastVersion()); //hack by dan-el
            return si;
        }

        List<VersionControlLogEntry> GetGitLog(string dir, string fromRevision)
        {
            //Sample output
            /*
             * commit d34362948c6d2a40527c2b77dc270da61e9f40fb
             * Author: Sebastian Loncar <sebastian.loncar@gmail.com>
             * Date:   2013-11-23 17:30:12 +0100
             *     (empty line)
             * commit test
             *     (empty line)
             * line1
             * line2
             * "line3"
             *     (empty line)
             */

            //git --no-pager log fromRevision..HEAD --pretty=format:"COMMIT=%h;AUTHOR=%an;DATE=%ad;COMMENT=%s" --date=iso //Has no extended descriptions!
            //git --no-pager log fromRevision..HEAD --date=iso //contains also the extended descriptions
            //git --no-pager log --date=iso
            var cmd = String.Format("--no-pager log {0}..HEAD --date=iso", fromRevision);
            var res = Execute(dir, GitExecutable, cmd);
            var lines = res.Output.Select(t => t == null ? "" : t).ToArray();

            var list = new List<VersionControlLogEntry>();

            VersionControlLogEntry entry = null;
            var comments = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("commit "))
                {
                    if (entry != null)
                    {
                        entry.msg = string.Join(Environment.NewLine, comments.Where(t => t.IsNotNullOrEmpty()));
                        list.Add(entry);
                    }
                    entry = new VersionControlLogEntry();
                    comments.Clear();

                    entry.revision = line.Substring("commit ".Length);
                }
                else if (line.StartsWith("Author: "))
                {
                    entry.author = line.Substring("Author: ".Length);
                }
                else if (line.StartsWith("Date: "))
                {
                    entry.date = DateTime.Parse(line.Substring("Date: ".Length));
                }
                else if (line.StartsWith("    "))
                {
                    comments.Add(line.Trim());
                }
            }

            if (entry != null)
            {
                entry.msg = string.Join(Environment.NewLine, comments.Where(t => t.IsNotNullOrEmpty()));
                list.Add(entry);
            }

            //var list = doc.Root.Elements().Select(el => new SvnLogEntry
            //{
            //    author = el.GetChildValue<string>("author"),
            //    date = el.GetChildValue<DateTime>("date"),
            //    msg = el.GetChildValue<string>("msg"),
            //    revision = el.GetChildValue<string>("revision"),
            //}).ToList();

            list.Where(t => t.msg != null && t.msg.EndsWith("\n")).ForEach(t => t.msg = t.msg.RemoveLast(1));
            return list;
        }

        string GetHeadRevision(string dir)
        {
            return GetGitHeadRevision(dir);
        }

        string GitExecutable = @"C:\Program Files (x86)\Git\bin\git.exe";
        string GetGitHeadRevision(string dir)
        {
            //git --no-pager log HEAD -1 --pretty=format:"%h"
            var res = Execute(dir, GitExecutable, "--no-pager log HEAD -1 --pretty=format:\"%H\"");
            return res.Output[0];
        }

        public static ExecuteResult Execute(string dir, string file, string args)
        {
            Console.WriteLine("Executing: {0} {1} {2}", dir, file, args);
            var process = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = dir,
                FileName = file,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });
            var res = new ExecuteResult { Output = new List<string>(), Error = new List<string>() };

            Console.WriteLine("{0}>{1} {2}", process.StartInfo.WorkingDirectory, process.StartInfo.FileName, process.StartInfo.Arguments);
            process.OutputDataReceived += (s, e) => { Console.WriteLine(e.Data); res.Output.Add(e.Data); };
            process.ErrorDataReceived += (s, e) => { Console.WriteLine(e.Data); res.Error.Add(e.Data); };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            res.ExitCode = process.ExitCode;
            if (process.ExitCode != 0)
                throw new Exception(String.Format("Error during execution, exit code={0}", process.ExitCode));
            Console.WriteLine("Finished execution. Exit code: {0}", process.ExitCode);
            return res;
        }

        public string ReleaseLogsDir;

        #endregion


        public void CommitGit()
        {
            CallGit("add .");
            CallGit("commit -am 'New Release " + ProductVersion + "'");
            CallGit("push origin master");
        }

        public void CallGit(string args)
        {
            //
        }

        //TODO
        public void Rollback()
        {

            ProductVersion = GetLastVersion();
            File.WriteAllText(Path.Combine(GitRoot, "VERSION"), ProductVersion);
            UpdateAssemblyFileVersions(ProductVersion);
        }

        string[] SharpKitCompilerProjectNames = new string[]
        {
            "CSharp.Tasks",
            "skc5",
        };

        #region "GitHubRelease"

        private GitHubClient _GitHubClient;
        public GitHubClient GitHubClient
        {
            get
            {
                if (_GitHubClient == null)
                {
                    if (Config.GitHubAccessToken.IsNullOrEmpty())
                        throw new Exception("GitHubAccessToken not set!");

                    _GitHubClient = new GitHubClient(new ProductHeaderValue("TestGitHutAPI"));
                    _GitHubClient.Credentials = new Credentials(Config.GitHubAccessToken);
                }
                return _GitHubClient;
            }
        }

        public string GetLastVersion()
        {
            var rels = GitHubClient.Release.GetAll(Config.GitHubUser, Config.GitHubRepoCompiler);
            rels.Wait();
            return rels.Result.Last().TagName;
        }

        public void CreateGitHubRelease()
        {
            DeleteGitHubRelease(Config.GitHubRepoCompiler, ProductVersion);
            DeleteGitHubRelease(Config.GitHubRepoSDK, ProductVersion);

            GitHubClient.Release.Create(Config.GitHubUser, Config.GitHubRepoCompiler, new ReleaseUpdate(ProductVersion)
            {
                Name = ProductVersion,
                Body = GenerateGitHubReleaseLogDescription()
            }).Wait();

            GitHubClient.Release.Create(Config.GitHubUser, Config.GitHubRepoSDK, new ReleaseUpdate(ProductVersion)
            {
                Name = ProductVersion
            }).Wait();
        }

        public void DeleteGitHubRelease(string repo, string version)
        {
            var rels = GitHubClient.Release.GetAll(Config.GitHubUser, repo);
            rels.Wait();
            foreach (var rel in rels.Result)
            {
                if (rel.TagName == version)
                {
                    GitHubClient.Release.Delete(Config.GitHubUser, repo, rel.Id);
                }
            }
        }

        public string GenerateGitHubReleaseLogDescription()
        {
            ReleaseLog = ReleaseLog.Load(Path.Combine(ReleaseLogsDir, ProductVersion + ".xml"));
            var sb = new StringBuilder();
            GenerateGitHubReleaseLogDescription(sb, ReleaseLog.SharpKit5, "Compiler", "SharpKit");
            GenerateGitHubReleaseLogDescription(sb, ReleaseLog.SharpKit_Sdk, "SDK", "SharpKit-SDK");
            return sb.ToString();
        }

        public void GenerateGitHubReleaseLogDescription(StringBuilder sb, SolutionInfo info, string name, string repo)
        {
            sb.AppendLine();
            sb.AppendLine("##### " + name + " changes");

            var dic = new Dictionary<string, List<VersionControlLogEntry>>();
            var groups = new List<List<VersionControlLogEntry>>();

            foreach (var itm in info.SvnLogEntries)
            {
                List<VersionControlLogEntry> childs;
                if (!dic.TryGetValue(itm.msg.ToLower().Trim(), out childs))
                {
                    childs = new List<VersionControlLogEntry>();
                    dic.Add(itm.msg.ToLower().Trim(), childs);
                    groups.Add(childs);
                }
                childs.Add(itm);
            }

            foreach (var childs in groups)
            {
                var itm = childs[0];
                var msg = itm.msg;
                var reg = new System.Text.RegularExpressions.Regex(@"(\(|\s)(#[0-9]+)(\)\s|.|,)", RegexOptions.RightToLeft);
                var msgSB = new StringBuilder(msg);
                foreach (Match m in reg.Matches(msg))
                {
                    var hash = m.Groups[2].Value;
                    var issue = "[" + hash + "](../../../../" + Config.GitHubUser + "/" + repo + "/issues/" + hash.Replace("#", "") + ")";
                    msgSB.Replace(m.Groups[2].Value, issue, m.Groups[2].Index, m.Groups[2].Length);
                }
                msg = msgSB.ToString();

                //msg = "* " + msg + " ([view](../../commit/" + itm.revision + "))";

                msg = "* " + msg + " (";
                var revList = new List<string>();
                foreach (var child in childs)
                {
                    revList.Add("[" + child.revision.Substring(0, 7) + "](../../../../" + Config.GitHubUser + "/" + repo + "/commit/" + child.revision + ")");
                }
                msg += string.Join(", ", revList);
                msg += ")";

                sb.AppendLine(msg);
            }
        }

        public void Upload()
        {
            Console.WriteLine("Uploading setup file. This can take several minutes!");
            try
            {
                var rels = GitHubClient.Release.GetAll(Config.GitHubUser, Config.GitHubRepoCompiler);
                rels.Wait();
                foreach (var rel in rels.Result)
                {
                    if (rel.TagName == ProductVersion)
                    {
                        Octokit.Internal.Request.DefaultTimeout = TimeSpan.FromSeconds(1000);
                        Stream str = new MemoryStream();
                        var _bytes = File.ReadAllBytes(SetupFilename);
                        str.Write(_bytes, 0, _bytes.Length);
                        str.Seek(0, SeekOrigin.Begin);

                        GitHubClient.Release.UploadAsset(rel, new ReleaseAssetUpload() { ContentType = "application/exe", FileName = Path.GetFileName(SetupFilename), RawData = str }).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        #endregion

        void CreateInstaller()
        {
            var maker = new SetupMaker
            {
                ProductVersion = ProductVersion,
                GitRoot = GitRoot,
                InstallerProjectDir = InstallerProjectDir,
            };
            maker.Run();
            SetupFilename = maker.SetupFilename;
        }


    }
}