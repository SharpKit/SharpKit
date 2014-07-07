using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace SharpKit.Installer
{
    static class ProgramPreInit
    {

        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender2, args) =>
            {
                if (args.Name.Contains("ICSharpCode.SharpZipLib"))
                    return Assembly.Load(Properties.Resources.ICSharpCode_SharpZipLib);
                else if (args.Name.Contains("IWshRuntimeLibrary"))
                    return Assembly.Load(Properties.Resources.Interop_IWshRuntimeLibrary);
                else if (args.Name.Contains("Shell32"))
                    return Assembly.Load(Properties.Resources.Interop_Shell32);
                else if (args.Name.Contains("corex"))
                    return Assembly.Load(Properties.Resources.corex);
                return null;
            };

            Program.Main2();
        }

    }

    static class Program
    {
        public static void Main2()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }
            catch
            {
                Utils.IsConsole = true;
            }

            if (!Utils.IsConsole)
            {
                Application.ThreadException += (sender, e) =>
                {
                    if (MessageBox.Show("A error occured", e.Exception.ToString(), MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                    {
                        Application.Exit();
                    }
                };
            }

            if (Utils.RootRestart())
            {
                Environment.Exit(0);
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender2, args) =>
            {
                if (args.Name.Contains("ICSharpCode.SharpZipLib"))
                    return Assembly.Load(Properties.Resources.ICSharpCode_SharpZipLib);
                else if (args.Name.Contains("IWshRuntimeLibrary"))
                    return Assembly.Load(Properties.Resources.Interop_IWshRuntimeLibrary);
                else if (args.Name.Contains("Shell32"))
                    return Assembly.Load(Properties.Resources.ICSharpCode_SharpZipLib);
                else if (args.Name.Contains("coreex"))
                    return Assembly.Load(Properties.Resources.corex);
                return null;
            };

            var minVersion = new Corex.Helpers.FrameworkVersion(new Version("4.5"));
            if (!Corex.Helpers.FrameworkVersion.HasVersionkOrBetter(minVersion))
            {
                if (MessageBox.Show(string.Format("Minimum .NET Framework version required: {0}\nCurrent .NET Framework version installed: {1}\nDo you want to continue installation at your own risk?", minVersion.ToString(), Corex.Helpers.FrameworkVersion.Current.ToString()), "Framework version not supported", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    Application.Exit();
                    return;
                }
            }

#if UNIX
            if (!Utils.IsUnix)
            {
                MessageBox.Show("This installer was compiled on an unix system. Installation on windows is not supported.", "Not supported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
#endif

            //MessageBox.Show(string.Format("currProcessPath: {0}\nProgramFilesX86Path: {1}\nProgramFilesPath: {2}", Utils.CurrentProcessFile.ToLower(), Utils.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower(), Utils.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower()));

            if (Utils.CurrentProcessFile.ToLower().Contains(Utils.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower()))
            {
                //var tempFile = Path.GetTempFileName();
                //tempFile = Path.Combine(Path.GetDirectoryName(tempFile), "SharpKitSetup_" + Path.GetFileNameWithoutExtension(tempFile) + ".exe");

                var tempFile = Path.GetTempPath() + Path.DirectorySeparatorChar + "SharpKitSetup.exe";

                File.Copy(Utils.CurrentProcessFile, tempFile, true);
                var args = new List<string>(Environment.GetCommandLineArgs());
                if (Utils.IsUnix)
                {
                    args[0] = tempFile;
                    Process.Start("mono", string.Join(" ", args.ToArray()));
                }
                else
                {
                    args.RemoveAt(0);
                    Process.Start(tempFile, string.Join(" ", args.ToArray()));
                }
                Environment.Exit(0);
                return;
            }

            if (Utils.IsConsole)
            {
                Utils.IsConsole = true;
                var con = new ConsoleInterface();
                con.Main();
            }
            else
            {
                Application.Run(new FormMain());
            }
        }
    }
}
