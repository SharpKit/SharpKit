using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SharpKit.Utils;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SharpKit.Installer
{

    public static class Utils
    {

        public static bool RootRestart()
        {
            if (IsUnix)
            {
                if (!isUnixRoot)
                {
                    var errorMessage = "Permission denied. Are you root?";
                    if (IsConsole)
                    {
                        //sudo will not work for security reasons, because it could be possible to sniff the password
                        Console.WriteLine(errorMessage);
                        return true;
                    }
                    var args = new List<string>(Environment.GetCommandLineArgs());
                    if (!args[0].Contains("mono")) args.Insert(0, "mono");
                    var command = (IsConsole ? "sudo" : "gksudo");
                    if (!File.Exists(command)) {
                        if (IsConsole)
                            Console.WriteLine(errorMessage);
                        else
                            MessageBox.Show(errorMessage);
                        return true;
                    }
                    //MessageBox.Show(string.Join(" ", args));
                    Process.Start(command, string.Join(" ", args));
                    return true;
                }
            }
            else
            {
                //TODO: needed windows admin privileges?
            }
            return false;
        }

        [DllImport("libc")]
        public static extern uint getuid();

        private static bool isUnixRoot
        {
            get
            {
                return getuid() == 0;
            }
        }

        public static void GiveUnixDirectoryReadPermission(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);
            foreach (var file in dirInfo.GetFiles())
                GiveUnixFileReadPermission(file.FullName);

            foreach (var subDir in dirInfo.GetDirectories())
            {
                GiveUnixDirectoryReadPermission(subDir.FullName);
            }
        }

        public static void GiveUnixFileReadPermission(string file)
        {
            Process.Start("chmod", "+r " + file).WaitForExit();
        }

        private static string _CurrentProcessFile;
        public static string CurrentProcessFile
        {
            get
            {
                if (_CurrentProcessFile == null)
                    _CurrentProcessFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return _CurrentProcessFile;
            }
        }

        public static SuccessSkip UICopyOverwrite(string source, string dest)
        {
            var checkedEqual = false;
            while (true)
            {
                try
                {
                    File.Copy(source, dest, true);
                    return SuccessSkip.Success;
                }
                catch (Exception e)
                {
                    if (!checkedEqual && FileHelper.IsFilesContentEqual(source, dest))
                        return SuccessSkip.Skip;
                    else
                        checkedEqual = true;
                    var btn = new Dialog
                    {
                        Title = "Copy file failed",
                        Message = string.Format("Filename: {0}\r\nError: {1}", dest, e.Message),
                        Buttons = new List<DialogButton>
                        {
                            new DialogButton{Text="Retry"},
                            new DialogButton{Text="Abort"},
                            new DialogButton{Text="Skip"},
                        }
                    }.ShowDialog();
                    if (btn == null || btn.Text == "Abort")
                    {
                        throw new Exception("Installation aborted");
                    }
                    else if (btn.Text == "Skip")
                    {
                        return SuccessSkip.Skip;
                    }
                    else
                    {
                        //retry
                    }
                }
            }
        }

        public static bool IsConsole = false;

        public static SuccessSkip UIDeleteFile(string file)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
                return SuccessSkip.Success;
            new FileInfo(file).Attributes = FileAttributes.Normal;

            while (true)
            {
                try
                {
                    File.Delete(file);
                    return SuccessSkip.Success;
                }
                catch (Exception ex)
                {
                    var btn = new Dialog
                    {
                        Title = "Delete file failed",
                        Message = string.Format("File: {0}\r\nError: {1}", file, ex.Message),
                        Buttons = new List<DialogButton>
                        {
                            new DialogButton{Text="Retry"},
                            new DialogButton{Text="Abort"},
                            new DialogButton{Text="Skip"},
                        }
                    }.ShowDialog();
                    if (btn == null || btn.Text == "Abort")
                    {
                        throw new Exception("Installation aborted");

                    }
                    else if (btn.Text == "Skip")
                    {
                        return SuccessSkip.Skip;
                    }
                    else
                    {
                        //retry
                    }
                };
            }
        }

        public static string GetFolderPath(Environment.SpecialFolder folder)
        {
            switch (folder)
            {
                case Environment.SpecialFolder.ProgramFilesX86:
                    if (IsUnix)
                        return "/usr/lib";
                    else
                    {
                        var dir = Environment.GetFolderPath(folder);
                        if (!dir.IsNullOrEmpty()) return dir;
                        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    }
                case Environment.SpecialFolder.ProgramFiles:
                    if (IsUnix)
                        return "/usr/lib";
                    else
                    {
                        var dir = Environment.GetFolderPath(folder);
                        if (!dir.IsNullOrEmpty()) return dir;
                        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    }
                default:
                    return Environment.GetFolderPath(folder);
            }
        }

        public static SuccessSkip UIDeleteDirectory(string dir, bool recursive = true)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return SuccessSkip.Success;
            new DirectoryInfo(dir).Attributes = FileAttributes.Normal;

            while (true)
            {
                try
                {
                    Directory.Delete(dir, recursive);
                    return SuccessSkip.Success;
                }
                catch (Exception ex)
                {
                    var btn = new Dialog
                    {
                        Title = "Delete directory failed",
                        Message = string.Format("Directory: {0}\r\nError: {1}", dir, ex.Message),
                        Buttons = new List<DialogButton>
                        {
                            new DialogButton{Text="Retry"},
                            new DialogButton{Text="Abort"},
                            new DialogButton{Text="Skip"},
                        }
                    }.ShowDialog();
                    if (btn == null || btn.Text == "Abort")
                    {
                        throw new Exception("Installation aborted");

                    }
                    else if (btn.Text == "Skip")
                    {
                        return SuccessSkip.Skip;
                        //TODO:
                    }
                    else
                    {

                        //retry
                    }
                };
            }
        }

        public static bool IsUnix
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Unix;
            }
        }

    }


    public enum SuccessSkip
    {
        Success,
        //Abort,
        //Retry,
        Skip,
    }
}
