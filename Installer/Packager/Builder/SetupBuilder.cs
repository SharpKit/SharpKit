using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using SharpKit.Utils;
using SharpKit.Release;

namespace SharpKit.Installer.Builder
{

    class SetupBuilder
    {

        public string GitRoot;
        public string InstallerProjectDir { get; set; }
        public string TempBinDir { get; set; }
        public string SourceFilesDir { get; set; }
        public string ProductVersion { get; set; }
        public string InstallerNeedsMinVersion { get; set; }

        private string ZipPath;
        private string ConfigPath;

        public void build()
        {
            ZipPath = Path.Combine(TempBinDir, "Files.zip");
            ConfigPath = Path.Combine(TempBinDir, "Config.xml");
            if (InstallerNeedsMinVersion.IsEmpty())
                InstallerNeedsMinVersion = ProductVersion;

            DirectoryHelper.VerifyDir(TempBinDir);
            CreateConfig();
            CreateZip();
            CreateExe();

            Console.WriteLine("installer created at " + OutputFilename);
        }

        void CreateConfig()
        {
            Console.WriteLine("creating config file");
            if (File.Exists(ConfigPath))
                File.Delete(ConfigPath);
            var xdoc = new XDocument();
            var root = new XElement("config");
            xdoc.Add(root);
            root.Add(new XElement("param", new XAttribute("key", "ProductVersion"), new XAttribute("value", ProductVersion)));
            root.Add(new XElement("param", new XAttribute("key", "InstallerNeedsMinVersion"), new XAttribute("value", InstallerNeedsMinVersion)));
            xdoc.Save(ConfigPath);
        }

        void CreateZip()
        {
            Console.WriteLine("creating zip file");

            // Needed contents at ZipPath:
            // Files\Application\ --> All contents requied for Programm Files (x86)\Sharpkit\
            // Files\NET\ --> the content of FrameworkDir\SharpKit
            // Files\NET_Unix\ --> only the modified files for unix. At this time, it will contain only the file SharpKit.Build.targets.
            // Files\Templates\ --> contains 'SharpKit Web Application.zip' and 'SharpKit 5 Web Application.zip'

            // Just copy the needed above files to @SourceFilesDir() and run this code.
            // @SourceFilesDir must be the parent directory of Files\

            using (var zip = new ZipArchive(ZipPath) { AddFileCallback = t => Console.WriteLine(t) })
            {
                zip.BeginUpdate();
                zip.AddDirectory(SourceFilesDir);
                zip.EndUpdate();
            }
        }

        void CreateExe()
        {
            File.Copy(ZipPath, Path.Combine(InstallerProjectDir, "res", Path.GetFileName(ZipPath)), true);
            File.Copy(ConfigPath, Path.Combine(InstallerProjectDir, "res", Path.GetFileName(ConfigPath)), true);
            //Program.BuildProject(SkSlnFilename, "Release", "Installer");
            Console.WriteLine("creating executable");
            //System.Diagnostics.Process.Start(Path.Combine(InstallerProjectDir, "make"), "release");

            if (Utils.IsUnix)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("make", "release") { WorkingDirectory = InstallerProjectDir, UseShellExecute = true });
            else
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", "/c make release") { WorkingDirectory = InstallerProjectDir, UseShellExecute = true });

            //var runner = new MSBuildRunner(InstallerProjectDir);
            //runner.Execute();
            File.Copy(Path.Combine(InstallerProjectDir, "bin", "SharpKitSetup.exe"), OutputFilename, true);
        }
        public string OutputFilename { get; set; }

    }

}
