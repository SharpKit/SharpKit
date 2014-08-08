using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpKit.Utils;

namespace SharpKit.Installer.Builder
{
    class SetupMaker
    {
        public string GitRoot { get; set; }
        public string TempBinDir { get; set; }
        public string ProductVersion { get; set; }

        public string TempZipDir { get; set; }
        public string TempDir { get; set; }
        public string InstallerProjectDir { get; set; }

        private char dsc = Path.DirectorySeparatorChar;

        public SetupMaker()
        {
            TempDir = Path.Combine(Path.GetTempPath(), "SharpKitInstaller") + Path.DirectorySeparatorChar;
            try
            {
                if (Directory.Exists(TempDir)) Directory.Delete(TempDir, true);
            }
            catch { }
            TempBinDir = TempDir + @"bin" + dsc;
            TempZipDir = TempDir + @"zip" + dsc;
        }

        public void Run()
        {
            if (ProductVersion.IsNullOrEmpty())
                throw new ArgumentNullException("ProductVersion");
            //ParamHelper.EvalInline(Vars);
            DirectoryHelper.VerifyEmptyDir(TempDir);
            PrepareFolder();
            Build();
        }

        void AddFile(string source, string target)
        {
            source = Utils.CorrectPathSeparator(source);
            target = Utils.CorrectPathSeparator(target);
            Files.Add(new FileMapping { Source = source, Target = target });
        }

        void AddDirectory(string source, string target, string filter = "")
        {
            source = Utils.CorrectPathSeparator(source);
            target = Utils.CorrectPathSeparator(target);
            if (filter == "") filter = "*";
            foreach (var file in System.IO.Directory.GetFiles(source, filter))
            {
                AddFile(file, target);
            }
        }

        List<FileMapping> Files;
        private void PrepareFolder()
        {
            Directory.CreateDirectory(TempZipDir);
            //DirectoryHelper.DeleteDirContents(TempZipDir);
            Files = new List<FileMapping>();
            //foreach (var version in NetVersions)
            //{
            //    foreach (var proj in SdkProjectNames)
            //    {
            //        if (version != "4.0")
            //        {
            //            if (proj == "SharpKit.Web")
            //                continue;
            //            if (proj == "SharpKit.JsClr")
            //                continue;
            //        }
            //        AddFile(@"{0}..\bin\v{1}\{2}.dll".FormatWith(SdkTrunkDir, version, proj), @"Files\Application\Assemblies\v{0}\".FormatWith(version));
            //        AddFile(@"{0}..\bin\v{1}\{2}.xml".FormatWith(SdkTrunkDir, version, proj), @"Files\Application\Assemblies\v{0}\".FormatWith(version));
            //    }
            //}
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\AjaxMin.dll", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\ICSharpCode.NRefactory.CSharp.dll", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\ICSharpCode.NRefactory.Cecil.dll", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\ICSharpCode.NRefactory.dll", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\Mono.Cecil.dll", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\corex.dll", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\SharpKitActivation\bin\SharpKitActivation.exe", @"Files\Application\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\SharpKit.Build.targets", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\SharpKit.CSharp.targets", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\SharpKit.CSharp.Tasks", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\SharpKit.CSharp.Tasks.dll", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\skc5.exe", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\skc5\bin\skc5.exe.config", @"Files\NET\");
            //AddFile(SkTrunkDir + @"Compiler\MonoDevelop.SharpKit\bin\MonoDevelop.SharpKit.dll", @"Files\MonoDevelopPlugin\");
            ////TODO: AddFile(@"SharpKit.Build.targets", @"Files\NET_Unix\");
            //AddFile(SkTrunkDir + @"Compiler\Packager\VSTemplates10\ProjectTemplates\SharpKit 5 Web Application.zip", @"Files\Templates\");
            //AddFile(SkTrunkDir + @"scripts\LicenseAgreement.rtf", @"Files\Application\");
            //AddFile(SdkTrunkDir + @"SharpKit.JsClr\res\jsclr.js", @"Files\Application\Scripts\");
            //AddFile(SdkTrunkDir + @"SharpKit.JsClr\res\jsclr.min.js", @"Files\Application\Scripts\");

            ////AddFile(SK5TrunkFolder+@"src\SharpKit.Release\VSTemplates10\ProjectTemplates\SharpKit 5 Web Application.zip", @"Files\Application\Visual Studio 2010\Templates\Project Templates\Visual C#\SharpKit\");

            ////var prms = new { SDKTrunkFolder, SK5TrunkFolder };
            ////var prms2 = new Dictionary<string, string>();
            ////foreach (var prm in prms.EnumerateProperties())
            ////{
            ////    prms2.Add(prm.Property.Name, (string)prm.Getter());
            ////}
            ////foreach (var file in Files)
            ////{
            ////    file.Source = ParamHelper.Eval(file.Source, Vars);
            ////    file.Target = ParamHelper.Eval(file.Target, Vars);
            ////}

            AddDirectory(GitRoot + @"Compiler\skc5\bin", @"Files\Application\Compiler\");
            AddDirectory(GitRoot + @"Compiler\CSharp.Tasks\bin", @"Files\Application\Compiler\");
            AddDirectory(GitRoot + @"SDK\Defs\bin", @"Files\Application\Defs\");
            AddDirectory(GitRoot + @"SDK\Frameworks\JsClr\bin", @"Files\Application\Frameworks\JsClr\bin\", "SharpKit.JsClr.*");
            AddDirectory(GitRoot + @"SDK\Frameworks\JsClr\res", @"Files\Application\Frameworks\JsClr\res\", "jsclr.*");
            AddFile(GitRoot + @"Integration\VisualStudio\ProjectTemplates\SharpKit 5 Web Application.zip", @"Files\Application\Integration\VisualStudio\Templates\");
            AddDirectory(GitRoot + @"Integration\MonoDevelop\bin", @"Files\Application\Integration\MonoDevelop\");

            foreach (var file in Files)
            {
                var targetDir = Path.Combine(TempZipDir, file.Target);
                var targetFile = Path.Combine(targetDir, Path.GetFileName(file.Source));
                DirectoryHelper.VerifyDir(targetDir);
                File.Copy(file.Source, targetFile, true);
            }

        }

        string ReplaceParameters(string s, Dictionary<string, string> prms)
        {
            foreach (var prm in prms)
                s = s.Replace("{" + prm.Key + "}", prm.Value);
            return s;
        }

        private void Build()
        {
            if (SetupFilename == null)
            {
                //SetupFilename = Path.Combine(TempDir, "SharpKitSetup_" + ProductVersion.Replace('.', '_') + ".exe");
                SetupFilename = Path.Combine(GitRoot, "contrib", "SharpKitSetup_" + ProductVersion.Replace('.', '_') + ".exe");
            }

            var builder = new SetupBuilder
            {
                TempBinDir = TempBinDir,
                SourceFilesDir = TempZipDir,
                ProductVersion = ProductVersion,
                InstallerProjectDir = InstallerProjectDir,
                OutputFilename = SetupFilename,
                GitRoot = GitRoot
            };
            builder.build();

        }

        public string SetupFilename { get; set; }
    }


    class FileMapping
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }
}
