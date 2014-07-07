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
            ProductVersion = File.ReadAllLines(Path.Combine(GitRoot, "VERSION"))[0];
            CreateInstaller();
        }

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

        //public static void BuildProject(string slnFilename, string configuration, string projectName, string action = "build")
        //{
        //    Console.WriteLine("Building: {0} {1} {2}", slnFilename, configuration, projectName);
        //    var args = String.Format("\"{0}\" /{1} \"{2}\"", Path.GetFileName(slnFilename), action, configuration);
        //    if (projectName.IsNotNullOrEmpty())
        //        args += String.Format(" /Project \"{0}\"", projectName);
        //    //args += " /consoleloggerparameters:ErrorsOnly";
        //    var outFile = @"C:\temp\BuildOutput.txt";
        //    if (File.Exists(outFile)) File.Delete(outFile);
        //    args += @" /Out " + outFile;
        //    var res = Execute(Path.GetDirectoryName(slnFilename), Vs2013Exe, args);
        //    if (res.ExitCode != 0)
        //        throw new Exception(String.Format("Error during build, exit code={0}", res.ExitCode));
        //    Console.WriteLine("Finished build.");
        //}

    }
}