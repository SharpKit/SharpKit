using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using System.Net;
using System.Net.Security;
using System.Diagnostics;
using System.Xml.Linq;
using Amazon.S3.Transfer;
using SharpKit.Release.Utils;
using System.Xml;
using SharpKit.Utils;
using SharpKit.Installer.Builder;

namespace SharpKit.Release
{
    class Program
    {
        bool TestOnly = false;
        bool UseGit = true;
        bool SkipLog = true;
        bool BuildSdk35 = false;
        static string Vs2013Exe = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com";
        //static string Vs2010Exe = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.com";
        public string SkRootDir { get { return Config.SkRootDir; } }
        public string SkTrunkDir { get { return Config.SkTrunkDir; } }
        public string OldWebsite = @"C:\Projects\SharpKit_Prev\SharpKit\trunk\src\SharpKit.Website\";
        public string SkSlnFilename { get; set; }
        public string SdkTrunkDir { get { return Config.SdkTrunkDir; } }
        public string SdkSlnFilename { get; set; }
        public string WebConfigFilename { get; set; }
        public string ReleaseLogsDir { get; set; }
        public string SetupDir { get; set; }
        public string InstallerProjectDir { get; set; }

        string[] SharpKitCompilerProjectNames = new string[]
        {
            //"Mono.Cecil",
            //"ICSharpCode.NRefactory",
            //"ICSharpCode.NRefactory.CSharp",
            //"SharpKitActivation",
            "SharpKit.CSharp.Tasks",
            "skc5",
            //"SharpKit.NuGet",
        };


        static void Main(string[] args)
        {
            new Program().Run();
        }

        ReleaseLog LastReleaseLog;
        void Run()
        {
            if (TestOnly)
                Console.WriteLine("TEST MODE!!!");

            SkSlnFilename = @"C:\Projects\GitHub\SharpKit\Compiler\Compiler.sln";
            SdkSlnFilename = @"C:\Projects\GitHub\SharpKit\Defs\Defs.sln";

            WebConfigFilename = SkTrunkDir + @"Website\SharpKit.WebSite\Web.config";
            OldWebConfigFilename = OldWebsite + @"Web.config";
            ReleaseLogsDir = SkTrunkDir + @"Compiler\Packager\ReleaseLogs\";
            InstallerProjectDir = SkTrunkDir + @"Compiler\Installer\";
            SetupDir = SkTrunkDir + @"setup\";
            try
            {
                var buildSdk = AskBoolean("Build SDK?");
                if (!SkipLog)
                {
                    if (!AskBoolean("Did you remember to get latest on SharpKit SDK?"))
                        return;
                    var logFiles = Directory.GetFiles(ReleaseLogsDir, "*.xml").OrderBy(t => t).ToList();
                    var lastReleaseLogFilename = logFiles.LastOrDefault();
                    LastReleaseLog = ReleaseLog.Load(lastReleaseLogFilename);
                    SetupVersion = AskString(String.Format("Last version is: {0}, from:{1:dd-MM-yyyy}, enter new version:", LastReleaseLog.Version, LastReleaseLog.Created));
                    if (SetupVersion.IsNullOrEmpty())
                        throw new Exception();
                    if (SetupVersion == LastReleaseLog.Version)
                    {
                        if (!AskBoolean("You have selected to create the same version, are you sure?"))
                            return;
                    }
                }
                else
                {
                    SetupVersion = AskString("enter new version:");
                }
                ReleaseLog = new ReleaseLog { Created = DateTime.Now, Filename = Path.Combine(ReleaseLogsDir, SetupVersion + ".xml"), Version = SetupVersion };
                if (!SkipLog)
                {
                    FillLog();
                }

                if (!TestOnly)
                {
                    //UpdateSharpKitVersionInfoSourceFiles(ReleaseLog);
                    //UpdateAssemblyFileVersions(SetupVersion);
                }

                if (!TestOnly)
                {
                    BuildSolution(SkSlnFilename, "Release");
                    if (buildSdk)
                        BuildSdk();
                }
                CreateInstaller();

                if (!TestOnly)
                {
                    ReleaseLog.Save();
                    var oldFile = OldWebsite + "ReleaseNotes.aspx";
                    if (File.Exists(oldFile))
                        WriteToReleaseNotesPage(ReleaseLog, oldFile);
                    WriteToReleaseNotesPage(ReleaseLog, SkTrunkDir + @"Website\SharpKit.Website\ReleaseNotes.aspx");
                    UpdateWebConfig(SetupVersion, SetupCloudFrontUrl, WebConfigFilename);
                    UpdateWebConfig(SetupVersion, SetupCloudFrontUrl, OldWebConfigFilename);
                }
                CopyInstallerToReleaseFolder();
                if (!TestOnly)
                    Upload();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Finished....");
            Console.ReadLine();
        }

        private void FillLog()
        {
            var lastReleaseLog = LastReleaseLog;
            ReleaseLog.SharpKit_Sdk = CreateSolutionInfo(SdkTrunkDir, lastReleaseLog.SharpKit_Sdk);
            ReleaseLog.SharpKit5 = CreateSolutionInfo(SkTrunkDir, lastReleaseLog.SharpKit5, UseGit);

            ShowLogMessages(ReleaseLog.SharpKit_Sdk.SvnLogEntries);
            ShowLogMessages(ReleaseLog.SharpKit5.SvnLogEntries);
        }


        #region Utils
        //private void CreateVersionInfos(List<string> logFiles)
        //{
        //    foreach (var file in logFiles)
        //    {
        //        var log = SharpKit.Release.ReleaseLog.Load(file);
        //        UpdateSharpKitVersionInfoSourceFiles(log);
        //    }
        //}


        //string UpdateVersionInfo(ReleaseLog log, string contents)
        //{
        //    var version = log.Version;
        //    contents = contents.Replace("CreateHeader(\"" + version + "\")", "CreateHeader(" + VersionInfoToCode(version, log.Created) + ")");
        //    UpdateSharpKitVersionInfoSourceFiles(log.Version, log.Created);
        //    return contents;
        //}
        void UpdateSharpKitVersionInfoSourceFiles(ReleaseLog log)
        {
            UpdateSharpKitVersionInfoSourceFiles(log.Version, log.Created);
        }
        void UpdateSharpKitVersionInfoSourceFiles(string version, DateTime dt)
        {
            var line = VersionInfoToCode(version, dt);
            line = "            " + line + ",";
            var files = SkTrunkDir.ToDirectoryInfo().GetFiles("SharpKitVersionInfo.cs", SearchOption.AllDirectories).Select(t => t.FullName).ToList();
            var old = OldWebsite + "Server/SharpKitVersionInfo.cs";
            if (File.Exists(old))
                files.Add(old);
            files.ForEach(t => InsertLinesAfterPlaceHolder(t, new[] { line }));
        }

        private string VersionInfoToCode(string version, DateTime dt)
        {
            var line = String.Format("new SharpKitVersionInfo {{ Version = \"{0}\", Date = new DateTime({1}, {2}, {3}) }}", version, dt.Year, dt.Month, dt.Day);
            return line;
        }
        ReleaseLog ReleaseLog;

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
        private List<string> GetLogMessages(ReleaseLog log)
        {
            var list = new List<string>();
            //if (log.SharpKit != null)
            //    list.AddRange(GetLogMessages(log.SharpKit.SvnLogEntries));
            if (log.SharpKit_Sdk != null)
                list.AddRange(GetLogMessages(log.SharpKit_Sdk.SvnLogEntries));
            if (log.SharpKit5 != null)
                list.AddRange(GetLogMessages(log.SharpKit5.SvnLogEntries));
            return list;
        }

        void CreateLogHtml(List<string> files)
        {
            var file = "ReleaseNotes.ascx";
            using (var writer = new StreamWriter(file, false))
            {
                files.ForEach(t =>
                {
                    var log = ReleaseLog.Load(t);
                    LogToHtml(writer, log);

                });
            }
        }

        private void WriteToReleaseNotesPage(ReleaseLog log, string releaseNotesFilename)
        {
            var tmpFilename = Path.ChangeExtension(log.Filename, ".ascx");
            LogToHtml(tmpFilename, log);
            InsertLinesAfterPlaceHolder(releaseNotesFilename, File.ReadAllLines(tmpFilename));
            File.Delete(tmpFilename);
        }

        private void InsertLinesAfterPlaceHolder(string filename, string[] lines2)
        {
            var lines = File.ReadAllLines(filename).ToList();
            var index = lines.FindIndex(t => t.Contains("<!--Placeholder-->"));
            lines.InsertRange(index + 1, lines2);
            File.WriteAllLines(filename, lines.ToArray());
        }
        private void LogToHtml(string filename, ReleaseLog log)
        {
            using (var writer = new StreamWriter(filename, false))
                LogToHtml(writer, log);
        }
        private void LogToHtml(StreamWriter writer, ReleaseLog log)
        {
            writer.WriteLine("<div><%=CreateHeader({0})%>", VersionInfoToCode(log.Version, log.Created));
            var list = GetLogMessages(log);
            LogProjectToHtml(writer, log.SharpKit5, "SharpKit 5");
            LogProjectToHtml(writer, log.SharpKit_Sdk, "SharpKit SDK");
            writer.WriteLine("</div>");
        }

        private void LogProjectToHtml(StreamWriter writer, SolutionInfo info, string name)
        {
            if (info != null && info.SvnLogEntries != null && info.SvnLogEntries.Count > 0)
            {
                writer.WriteLine("<h4>{0}</h4>", name);
                LogItemsToHtml(writer, GetLogMessages(info.SvnLogEntries));
            }
        }

        private void LogItemsToHtml(StreamWriter writer, List<string> list)
        {
            list.Reverse();
            if (list.Count > 0)
            {
                writer.WriteLine("<ul>");
                foreach (var msg in list)
                {
                    foreach (var line in msg.Split('\n'))
                        writer.WriteLine("    {0}", new XElement("li", line).ToString());
                }
                writer.WriteLine("</ul>");
            }
        }

        string GetHeadRevision(string dir, bool useGit = false)
        {
            if (UseGit)
                return GetGitHeadRevision(dir);
            else
            {
                return GetSvnHeadRevision(dir);
            }
        }

        string GetSvnHeadRevision(string dir)
        {
            //svn info -r HEAD
            var res = Execute(dir, "svn", "info -r HEAD");
            return res.Output.Where(t => t.StartsWith("Revision: ")).First().Substring("Revision: ".Length);
        }

        string ToXmlDateTime(DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        SolutionInfo CreateSolutionInfo(string srcDir, SolutionInfo lastSolutionInfo, bool useGit = false)
        {
            var si = new SolutionInfo { HeadRevision = GetHeadRevision(srcDir, useGit) };
            if (lastSolutionInfo == null || si.HeadRevision == lastSolutionInfo.HeadRevision)
                si.SvnLogEntries = new List<VersionControlLogEntry>();
            else
                si.SvnLogEntries = GetSvnLog(srcDir, (int.Parse(lastSolutionInfo.HeadRevision) + 1).ToString()); //hack by dan-el
            return si;
        }

        List<VersionControlLogEntry> GetLog(string dir, string fromRevision, bool useGit)
        {
            if (useGit)
                return GetGitLog(dir, fromRevision);
            else
                return GetSvnLog(dir, fromRevision);
        }

        List<VersionControlLogEntry> GetSvnLog(string dir, string fromRevision)
        {
            //svn info -r HEAD
            var res = Execute(dir, "svn", String.Format("log -r {0}:HEAD --xml", fromRevision));
            var xml = String.Join("\r\n", res.Output.ToArray());
            var doc = XDocument.Parse(xml);
            var list = doc.Root.Elements().Select(el => new VersionControlLogEntry
            {
                author = el.GetChildValue<string>("author"),
                date = el.GetChildValue<DateTime>("date"),
                msg = el.GetChildValue<string>("msg"),
                revision = el.GetChildValue<string>("revision"),
            }).ToList();
            list.Where(t => t.msg != null && t.msg.EndsWith("\n")).ForEach(t => t.msg = t.msg.RemoveLast(1));
            return list;
        }

        string GitExecutable = @"C:\Program Files (x86)\Git\bin\git.exe";
        string GetGitHeadRevision(string dir)
        {
            //git --no-pager log HEAD -1 --pretty=format:"%h"
            var res = Execute(dir, GitExecutable, "--no-pager log HEAD -1 --pretty=format:\"%H\"");
            return res.Output[0];
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
            var res = Execute(dir, GitExecutable, String.Format("--no-pager log {0}..HEAD --date=iso", fromRevision));
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

        private void BuildSdk()
        {
            BuildSolution(SdkSlnFilename, "v4.0");
            if (BuildSdk35)
                BuildSolution(SdkSlnFilename, "v3.5");
        }

        void UpdateWebConfig(string version, string downloadUrl, string webConfigFileName)
        {
            Console.WriteLine("Updating {0}", webConfigFileName);
            var lines = File.ReadAllLines(webConfigFileName).ToList();
            var i1 = lines.FindIndex(t => t.Contains("SharpKitDownloadUrl"));
            Console.WriteLine(lines[i1]);
            lines[i1] = String.Format("    <add key=\"SharpKitDownloadUrl\" value=\"{0}\"/>", downloadUrl);
            Console.WriteLine(lines[i1]);
            var i2 = lines.FindIndex(t => t.Contains("SharpKitLatestVersion"));
            Console.WriteLine(lines[i2]);
            lines[i2] = String.Format("    <add key=\"SharpKitLatestVersion\" value=\"{0}\"/>", version);
            Console.WriteLine(lines[i2]);
            File.WriteAllLines(webConfigFileName, lines);
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
                var file = SkTrunkDir + "\\Compiler\\" + project + "\\Properties\\AssemblyInfo.cs";
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
        private void TryBuildProjects(string slnFile, string[] projectNames)
        {
            foreach (var projectName in projectNames)
            {
                TryBuildProject(slnFile, "Release", projectName);
            }
        }

        void TryBuildProject(string slnFile, string config, string projectName)
        {
            Try(() => BuildProject(slnFile, config, projectName));
        }
        void Try(Action action)
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    var s = AskString("Error occured (retry / ignore / abort):");
                    if (s == "r")
                        continue;
                    if (s == "i")
                        break;
                    if (s == "a")
                        throw;
                }
            }
        }

        void CreateInstaller()
        {
            var maker = new SetupMaker
            {
                ProductVersion = SetupVersion,
                SdkTrunkDir = SdkTrunkDir,
                SkTrunkDir = SkTrunkDir,
                SkSlnFilename = SkSlnFilename,
                InstallerProjectDir = InstallerProjectDir,
            };
            maker.Run();
            SetupFilename = maker.SetupFilename;

        }

        public static void BuildSolution(string slnFilename, string configuration)
        {
            BuildProject(slnFilename, configuration, null, "build");
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
        public static void BuildProject(string slnFilename, string configuration, string projectName, string action = "build")
        {
            Console.WriteLine("Building: {0} {1} {2}", slnFilename, configuration, projectName);
            var args = String.Format("\"{0}\" /{1} \"{2}\"", Path.GetFileName(slnFilename), action, configuration);
            if (projectName.IsNotNullOrEmpty())
                args += String.Format(" /Project \"{0}\"", projectName);
            //args += " /consoleloggerparameters:ErrorsOnly";
            var outFile = @"C:\temp\BuildOutput.txt";
            if (File.Exists(outFile)) File.Delete(outFile);
            args += @" /Out " + outFile;
            var res = Execute(Path.GetDirectoryName(slnFilename), Vs2013Exe, args);
            if (res.ExitCode != 0)
                throw new Exception(String.Format("Error during build, exit code={0}", res.ExitCode));
            Console.WriteLine("Finished build.");
        }
        string VersionToString(int version)
        {
            return version.ToString().Insert(1, ".").Insert(4, ".");
        }


        string SetupCloudFrontUrl
        {
            get
            {
                return "http://download2.sharpkit.net/" + SetupFilename.ToFileInfo().Name;
            }
        }
        string SetupVersion { get; set; }
        string SetupFilename { get; set; }

        private void CopyInstallerToReleaseFolder()
        {
            Console.WriteLine("Copying setup file");
            var releasesDir = Path.Combine(SkTrunkDir, "releases");
            DirectoryHelper.VerifyDir(releasesDir);
            SetupFilename.ToFileInfo().CopyToDirectory(new DirectoryInfo(releasesDir), true);

        }
        private void Upload()
        {
            var filepath = SetupFilename;
            var s3Url = "http://download.sharpkit.net/" + SetupFilename.ToFileInfo().Name;
            var cloudFrontUrl = SetupCloudFrontUrl;
            Console.WriteLine("Uploading to " + s3Url);
            S3Upload(filepath, s3Url);
            //s3.PutObject(new PutObjectRequest { BucketName = "download.sharpkit.net", Key = filename, FilePath = filepath, CannedACL = S3CannedACL.PublicRead, Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds });


            Console.WriteLine("Finished, file is available at:");
            Console.WriteLine("http://download2.sharpkit.net/" + SetupFilename.ToFileInfo().Name);
            Console.WriteLine("**********************************************************************");
        }

        private void S3Upload(string filename, string url)
        {
            Console.WriteLine("Uploading to " + url);
            S3Helper.Upload(filename, url, (s, e) => Console.WriteLine("{0}% {1}kb/{2}kb", e.PercentDone, e.TransferredBytes / 1024, e.TotalBytes / 1024));
            Console.WriteLine("Finished uploading to " + url);
        }


        /// <summary>
        /// Extracts a zipped template, and merges better info from another vstemplate file, then rezips the file
        /// C:\Users\dkhen\Documents\Visual Studio 2010\My Exported Templates
        /// </summary>
        /// <param name="zipFile"></param>
        void FixTemplate(string zipFile, bool isItemTemplate)
        {
            var vsTemplateFilename = "MyTemplate.vstemplate";
            var dir = SevenZipHelper.ExtractToDir(zipFile);
            vsTemplateFilename = Path.Combine(dir, vsTemplateFilename);
            var doc1 = XDocument.Load(vsTemplateFilename);
            var doc2 = XDocument.Load(@"C:\Projects\SharpKit\trunk\src\SharpKit.Release\VSTemplates10\" + Path.GetFileNameWithoutExtension(zipFile) + ".vstemplate");
            XNamespace xn = "http://schemas.microsoft.com/developer/vstemplate/2005";
            var el1 = doc1.Root.Element(xn + "TemplateData");
            var el2 = doc2.Root.Element(xn + "TemplateData");
            foreach (var ch2 in el2.Elements())
            {
                var ch1 = el1.Element(ch2.Name);
                ch1.Value = ch2.Value;
            }
            doc1.Save(vsTemplateFilename);
            FileHelper.DeleteFileIfExists(zipFile + ".bak");
            File.Move(zipFile, zipFile + ".bak");
            SevenZipHelper.ZipDirectory(dir, zipFile);
            Directory.Delete(dir, true);

            var newZipFile = el1.Element(xn + "Name").Value + ".zip";

            var zipDir = @"C:\Projects\SharpKit\trunk\src\SharpKit.Release\VSTemplates10\ProjectTemplates\";
            if (isItemTemplate)
                zipDir = zipDir.Replace("ProjectTemplates", "ItemTemplates");
            newZipFile = Path.Combine(zipDir, newZipFile);

            File.Copy(zipFile, newZipFile, true);
        }

        bool AskBoolean(string question)
        {
            Console.WriteLine(question);
            var s = Console.ReadLine().ToLower();
            return s == "y" || s == "yes";
        }
        string AskString(string question)
        {
            Console.WriteLine(question);
            var s = Console.ReadLine().ToLower();
            return s;
        }

        #endregion


        public string OldWebConfigFilename { get; set; }
    }

    class Config
    {

        private static string _ApplicationDirectory;
        public static string ApplicationDirectory
        {
            get
            {
                if (_ApplicationDirectory == null)
                    _ApplicationDirectory = GetDirectoryOfAssemblyOfType(typeof(Config));
                return _ApplicationDirectory;
            }
        }

        public static string GetDirectoryOfAssemblyOfType(Type t)
        {
            Uri FileUri = new Uri(t.Assembly.CodeBase);
            System.IO.FileInfo File = new System.IO.FileInfo(System.Web.HttpUtility.UrlDecode(FileUri.AbsolutePath));
            return File.DirectoryName + "\\";
        }

        static Dictionary<string, string> XmlToHash(string xmlString)
        {
            var xmlDoc = XDocument.Parse(xmlString);
            var hash = new Dictionary<string, string>();

            foreach (var node in xmlDoc.Root.Elements())
            {
                hash.Add(node.Attribute("key").Value, node.Attribute("value").Value);
            }
            return hash;
        }

        static Dictionary<string, string> _hash;
        static Dictionary<string, string> hash
        {
            get
            {
                if (_hash == null)
                {
                    var cfgFile = Path.Combine(ApplicationDirectory, "..", "config.xml");
                    if (File.Exists(cfgFile))
                        _hash = XmlToHash(File.ReadAllText(cfgFile));
                    else
                        _hash = new Dictionary<string, string>();
                }
                return _hash;
            }
        }

        static string CheckAppendSlash(string path)
        {
            return path.EndsWith("\\") ? path : path + "\\";
        }

        static string GetValue(string key, string defaultValue)
        {
            string value;
            if (hash.TryGetValue(key, out value))
                if (value != "") return value;
            return defaultValue;
        }

        public static string SkRootDir
        {
            get
            {
                return CheckAppendSlash(GetValue("SkRootDir", @"C:\Projects\SharpKit\"));
            }
        }

        public static string SkTrunkDir
        {
            get
            {
                return CheckAppendSlash(GetValue("SkTrunkDir", SkRootDir + @"trunk\"));
            }
        }

        public static string SdkTrunkDir
        {
            get
            {
                return CheckAppendSlash(GetValue("SdkTrunkDir", SkRootDir + @"googlecode\trunk\"));
            }
        }

    }

}