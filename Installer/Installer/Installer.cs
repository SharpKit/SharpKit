using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Diagnostics;
using SharpKit.Utils;
using System.ServiceProcess;
using System.Runtime.InteropServices;

namespace SharpKit.Installer
{
    class Installer
    {

        public string ApplicationFolder { get; set; } //Program Files (x86)/SharpKit/5
        public string ApplicationCompilerFolder { get; set; } //Program Files (x86)/SharpKit/5/compiler
        public string ApplicationDefsFolder { get; set; } //Program Files (x86)/SharpKit/5/defs

        public string MSBuildFolder35 { get; set; }
        public string MSBuildFolder40 { get; set; }
        public string MSBuildFolder45 { get; set; }

        public string MSBuildSharpKitFolder35 { get; set; }
        public string MSBuildSharpKitFolder40 { get; set; }
        public string MSBuildSharpKitFolder45 { get; set; }

        public string DocumentsFolder { get; set; }
        public string ProductVersion { get; set; }
        public string ProductType { get; set; }
        public bool CheckForUpdates { get; set; }
        public string WebInstallUrl;
        public string StartMenuDir { get; set; }
        public Guid UninstallGUID { get; set; }
        private char dsc = Path.DirectorySeparatorChar;
        private string InstallerNeedsMinVersion;

        public string TemplateDirectoryVS2005 { get; set; }
        public string TemplateDirectoryVS2008 { get; set; }
        public string TemplateDirectoryVS2010 { get; set; }
        public string TemplateDirectoryVS2012 { get; set; }
        public string TemplateDirectoryVS2013 { get; set; }
        public string MonoDevelopPluginPath { get; set; }
        public string MonoDevelopSharpKitPluginPath { get; set; }

        private string WindowsFolder = Utils.GetFolderPath(Environment.SpecialFolder.Windows);
        private Dictionary<string, string> ConfigHash;

        public Installer()
        {
            ApplicationFolder = Utils.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + dsc + "SharpKit" + dsc + "5";
            //ApplicationCompilerFolder = ApplicationFolder + dsc + "Compiler";
            ApplicationCompilerFolder = ApplicationFolder;
            ApplicationDefsFolder = ApplicationFolder + dsc + "Defs";

            if (Utils.IsUnix)
            {
                MSBuildFolder35 = "/usr/lib/mono/3.5";
                MSBuildFolder40 = "/usr/lib/mono/4.0";
                MSBuildFolder45 = "/usr/lib/mono/4.5";
                MonoDevelopPluginPath = "/usr/lib/monodevelop/AddIns";
                    MonoDevelopSharpKitPluginPath = Path.Combine(MonoDevelopPluginPath,"MonoDevelop.SharpKit");
            }
            else
            {
                MSBuildFolder35 = Utils.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework\v3.5";
                MSBuildFolder40 = Utils.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework\v4.0.30319";
                MSBuildFolder45 = @"C:\Program Files (x86)\MSBuild\12.0\bin";
            }
            MSBuildSharpKitFolder35 = Path.Combine(MSBuildFolder35, "SharpKit", "5");
            MSBuildSharpKitFolder40 = Path.Combine(MSBuildFolder40, "SharpKit", "5");
            MSBuildSharpKitFolder45 = Path.Combine(MSBuildFolder45, "SharpKit", "5");

            DocumentsFolder = Utils.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            TemplateDirectoryVS2005 = Path.Combine(DocumentsFolder, @"Visual Studio 2005\Templates\ProjectTemplates\Visual C#\SharpKit");
            TemplateDirectoryVS2008 = Path.Combine(DocumentsFolder, @"Visual Studio 2008\Templates\ProjectTemplates\Visual C#\SharpKit");
            TemplateDirectoryVS2010 = Path.Combine(DocumentsFolder, @"Visual Studio 2010\Templates\ProjectTemplates\Visual C#\SharpKit");
            TemplateDirectoryVS2012 = Path.Combine(DocumentsFolder, @"Visual Studio 2012\Templates\ProjectTemplates\Visual C#\SharpKit");
            TemplateDirectoryVS2013 = Path.Combine(DocumentsFolder, @"Visual Studio 2013\Templates\ProjectTemplates\Visual C#\SharpKit");

            if (Utils.IsUnix)
            {
                StartMenuDir = "/usr/share/applications";
            }
            else
            {
                StartMenuDir = Utils.GetFolderPath(Environment.SpecialFolder.CommonPrograms) + dsc + "SharpKit";
            }
            CheckForUpdates = true;
            WebInstallUrl = null;//TODO: "http://download2.sharpkit.net/latest/";
            UninstallGUID = Guid.Parse("1A765314-EA5C-45D0-A22C-9F60D4E875A4");

            ProductType = "Express";
        }

        private Dictionary<string, string> XmlToHash(string xmlString)
        {
            var xmlDoc = XDocument.Parse(xmlString);
            var hash = new Dictionary<string, string>();

            foreach (var node in xmlDoc.Root.Elements())
            {
                hash.Add(node.Attribute("key").Value, node.Attribute("value").Value);
            }
            return hash;
        }

        private void Init()
        {
            SetConfig(XmlToHash(Properties.Resources.Config));
        }

        private void SetConfig(Dictionary<string, string> hash)
        {
            ConfigHash = hash;
            ProductVersion = ConfigHash["ProductVersion"];
            InstallerNeedsMinVersion = ConfigHash["InstallerNeedsMinVersion"];
        }

        private bool Inited = false;
        public void EnsureInited()
        {
            if (Inited) return;
            Init();
        }

        public void DoCheckForUpdates()
        {
            if (WebInstallUrl.IsNullOrEmpty())
                return;
            Log("Checking for new version");
            if (!WebInstallUrl.EndsWith("/"))
                WebInstallUrl += "/";
            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    var config = wc.DownloadString(WebInstallUrl + "Config.xml");
                    var hash = XmlToHash(config);
                    int onlineVersion = GetVersionInteger(hash["ProductVersion"]);
                    int localVersion = GetVersionInteger(ProductVersion);

                    if (onlineVersion > localVersion)
                    {
                        int InstallerOnlineVersion = GetVersionInteger(hash["InstallerNeedsMinVersion"]);
                        int InstallerLocalVersion = GetVersionInteger(InstallerNeedsMinVersion);

                        SetConfig(hash);
                        Log("Found new version " + ProductVersion);
                        Log("Downloading installation files");

                        if (InstallerOnlineVersion > InstallerLocalVersion)
                        {
                            var fileName = "SharpKitSetup_" + ProductVersion.Replace("v", "").Replace(".", "_") + ".exe";
                            var tempFile = Path.GetTempPath() + fileName;
                            Utils.UIDeleteFile(tempFile);
                            wc.DownloadFile(WebInstallUrl + fileName, tempFile);
                            Log("Starting new setup file");
                            if (Utils.IsUnix)
                            {
                                Process.Start("mono", tempFile);
                            }
                            else
                            {
                                Process.Start(tempFile);
                            }
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                        }
                        else
                        {
                            var data = wc.DownloadData(WebInstallUrl + "Files.zip");
                            Log("Downloading finisehd");
                            WebMemorystram = new MemoryStream(data); //at the moment, sharpkit install files are not very large. When becomes > 100MB, it should be saved to disk.
                        }
                    }
                    else if (onlineVersion == localVersion) Log("No new version available.");
                    else Log("Local version are newer than online version.");
                }
            }
            catch (Exception ex)
            {
                Log("Error while checking for updates. Continue installing offline files. " + ex);
            }
        }

        private int GetVersionInteger(string version)
        {
            var versionNumberStr = version.Replace("v", "").Replace(".", "");
            return int.Parse(versionNumberStr);
        }

        public void Install()
        {
            Uninstall();

            try
            {
                Log(".NET Framework detected: " + Corex.Helpers.FrameworkVersion.Current.ToString());
                Log("Program files folder: " + Utils.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower());
                EnsureInited();

                if (CheckForUpdates)
                    DoCheckForUpdates();
                Try(StopService);
                Log("Extracting and copying files");
                using (var zip = GetZipArchive())
                {
                    zip.ExtractDirectory(@"Files/Application", ApplicationFolder, Log);
                    if (!Utils.IsUnix)
                    {
                        //zip.ExtractDirectory(@"Files/Templates", TemplateDirectoryVS2008, Log);
                        //zip.ExtractDirectory(@"Files/Templates", TemplateDirectoryVS2010, Log);
                        //zip.ExtractDirectory(@"Files/Templates", TemplateDirectoryVS2012, Log);
                        //zip.ExtractDirectory(@"Files/Templates", TemplateDirectoryVS2013, Log);
                    }

                    if (Utils.IsUnix)
                    {
                                //zip.ExtractDirectory(@"Files/MonoDevelopPlugin", MonoDevelopPluginPath, Log);
                        ////zip.ExtractDirectory(@"Files/NET_Unix", SharpKitNETFolder35, Log);
                        //TODO: zip.ExtractDirectory(@"Files/NET_Unix", SharpKitNETFolder40, Log);
                    }
                }

                CreateNETSymbolicLink(MSBuildFolder35, MSBuildSharpKitFolder35, ApplicationCompilerFolder);
                CreateNETSymbolicLink(MSBuildFolder40, MSBuildSharpKitFolder40, ApplicationCompilerFolder);
                CreateNETSymbolicLink(MSBuildFolder45, MSBuildSharpKitFolder45, ApplicationCompilerFolder);

                if(!Utils.IsUnix){
                CreateNETSymbolicLink(Utils.GetParentDir(TemplateDirectoryVS2010), TemplateDirectoryVS2010, Path.Combine(ApplicationFolder, "Templates"));
                CreateNETSymbolicLink(Utils.GetParentDir(TemplateDirectoryVS2012), TemplateDirectoryVS2012, Path.Combine(ApplicationFolder, "Templates"));
                CreateNETSymbolicLink(Utils.GetParentDir(TemplateDirectoryVS2013), TemplateDirectoryVS2013, Path.Combine(ApplicationFolder, "Templates"));
                    }

                if(Utils.IsUnix){
                   CreateNETSymbolicLink(MonoDevelopPluginPath, MonoDevelopSharpKitPluginPath, Path.Combine(ApplicationFolder,"Integration","MonoDevelop"));
                }

                File.Copy(Utils.CurrentProcessFile, ApplicationFolder + dsc + "SharpKitSetup.exe", true);

                if (Utils.IsUnix)
                {

                    CreateBashScript("skc", ApplicationCompilerFolder + "/skc5.exe");
                    CreateBashScript("skc-setup", ApplicationFolder + "/SharpKitSetup.exe");
                    //CreateBashScript("skc-activation", ApplicationFolder + "/SharpKitActivation.exe");

                    Log("Set unix read permission");
                        //Utils.GiveUnixDirectoryReadPermission(ApplicationFolder);
                        //Utils.GiveUnixDirectoryReadPermission(MonoDevelopSharpKitPluginPath);
                        Process.Start("chmod", "-R ugo+rX " + Utils.GetParentDir(ApplicationFolder)).WaitForExit();

                        Process.Start("chmod", "-R ugo+rX " + Utils.GetParentDir(Utils.GetParentDir(MSBuildSharpKitFolder35))).WaitForExit();
                        Process.Start("chmod", "-R ugo+rX " + Utils.GetParentDir(Utils.GetParentDir(MSBuildSharpKitFolder40))).WaitForExit();
                        Process.Start("chmod", "-R ugo+rX " + Utils.GetParentDir(Utils.GetParentDir(MSBuildSharpKitFolder45))).WaitForExit();

                    Log("Set unix execution permission");
                        Process.Start("chmod", "ugo+x " + ApplicationCompilerFolder + "/skc5.exe").WaitForExit();
                        Process.Start("chmod", "ugo+x " + ApplicationFolder + "/SharpKitSetup.exe").WaitForExit();
                    //Process.Start("chmod", "+x " + ApplicationFolder + "/SharpKitActivation.exe").WaitForExit();
                }

                Log("Writing registry entries");
                using (var key = Registry.LocalMachine.CreateSubKey(@"Software\SharpKit\5"))
                {
                    key.SetValue("ProductFolder", ApplicationFolder);
                    key.SetValue("ProductVersion", ProductVersion);
                    key.SetValue("ProductType", ProductType);
                }

                SetMachineKeyRegistryValue(@"Software\Microsoft\.NETFramework\AssemblyFolders\SharpKit5", "", ApplicationDefsFolder);
                SetMachineKeyRegistryValue(@"Software\Microsoft\.NETFramework\v3.5\AssemblyFoldersEx\SharpKit5", "", ApplicationDefsFolder);
                SetMachineKeyRegistryValue(@"Software\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\SharpKit5", "", ApplicationDefsFolder);
                SetMachineKeyRegistryValue(@"Software\Microsoft\Visual Studio\10.0\MSBuild\SafeImports", "SharpKit5", Path.Combine(ApplicationCompilerFolder, "SharpKit.CSharp.targets"));

                Log("Creating shortcuts");
                if (Utils.IsUnix)
                {
                    //CreateShortcutUnix("sharpkit-activate", "Activate SharpKit", ApplicationFolder + @"/SharpKitActivation.exe", "", "Activate SharpKit");
                    CreateShortcutUnix("sharpkit-modify", "Modify SharpKit installation", ApplicationFolder + @"/SharpKitSetup.exe", "", "Modify SharpKit installation");
                    CreateShortcutUnix("sharpkit-update", "Check for Sharpkit updates", ApplicationCompilerFolder + @"/skc5.exe", "/CheckForNewVersion", "Check for Updates");
                }
                else
                {
                        #if WINNT
                    //CreateShortcutWin("Activate SharpKit", ApplicationFolder + @"\SharpKitActivation.exe", "", "Activate SharpKit");
                    CreateShortcutWin("Check for a New Version", ApplicationCompilerFolder + @"\skc5.exe", "/CheckForNewVersion", "Check for Updates");
                    CreateShortcutWin("Modify installation", ApplicationFolder + @"\SharpKitSetup.exe", "", "Modify installation");
                        #endif
                }

                if (!Utils.IsUnix)
                {
                    Log("Register application");
                    RegisterUninstall();
                }

                CreateNativeImage();
                InstallService();
                Log("Installation finished");
            }
            catch (Exception ex)
            {
                LogFormat("Error while installing: {0}", ex.Message);
            }
            if (OnFinished != null)
                OnFinished();
        }

        void CreateNETSymbolicLink(string checkDir, string symbol, string target)
        {
            if (Directory.Exists(checkDir))
            {
                    if (!Directory.Exists(Utils.GetParentDir(symbol)))
                        Directory.CreateDirectory(Utils.GetParentDir(symbol));

                if (Directory.Exists(symbol))
                {
                    Log("Removing old symbolic link: " + symbol);
                    Utils.UIDeleteDirectory(symbol, false); //target is maybe not correct, so remove it
                }
                CreateLink(symbol, target);
            }
        }

        void CreateLink(string symbol, string target){
            Log("Creating symbolic link: " + symbol);
            if (Utils.IsUnix)
            {
                    RunProcess("ln", "-s " + target + " " + symbol);
            }
            else
            {
                if (!CreateSymbolicLink(symbol, target, 1))
                {
                    Log("Error during creating symbolic link: " + GetLastError().ToString());
                }
            }
        }

        void RunProcess(string app, string args){
            Log(String.Format("Running: {0}, Args: {1}",app, args));
            Process.Start(app, args).WaitForExit();
        }

        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        [DllImport("kernel32.dll")]
        static extern int GetLastError();

        private void CreateBashScript(string name, string destination)
        {
            Log("Creating bash script '" + name + "'");
            var script = "#!/bin/bash" + Environment.NewLine + "mono " + destination + " \"$@\"";
            var file = "/usr/bin/" + name;
            if (File.Exists(file)) File.Delete(file);
            File.WriteAllText(file, script);
            Process.Start("chmod", "+rx " + file).WaitForExit();
        }

        private void RemoveBashScript(string name)
        {
            Utils.UIDeleteFile("/usr/bin/" + name);
        }

        void SetMachineKeyRegistryValue(string subKey, string name, string value)
        {
            LogFormat("Setting registry key: MACHINE_KEY/{0}/{1} = {2}", subKey, name, value);
            name = name ?? "";
            using (var key = Registry.LocalMachine.CreateSubKey(subKey))
                key.SetValue(name, value);
        }

        public void Uninstall()
        {
            try
            {
                Log("Uninstalling");
                EnsureInited();
                UninstallService();
                Log("Deleting files");
                Utils.UIDeleteDirectory(MSBuildSharpKitFolder45);
                Utils.UIDeleteDirectory(ApplicationFolder);
                Utils.UIDeleteDirectory(MSBuildSharpKitFolder35);
                Utils.UIDeleteDirectory(MSBuildSharpKitFolder40);
                if (!Utils.IsUnix)
                {
                    Utils.UIDeleteDirectory(TemplateDirectoryVS2010);
                    Utils.UIDeleteDirectory(TemplateDirectoryVS2012);
                    Utils.UIDeleteDirectory(TemplateDirectoryVS2013);
                }
                else
                {
                        Utils.UIDeleteDirectory(MonoDevelopSharpKitPluginPath);
                }

                if (Utils.IsUnix)
                {
                    Log("Removing bash scripts");
                    RemoveBashScript("skc");
                    RemoveBashScript("skc-setup");
                    RemoveBashScript("skc-activation");
                }

                Log("Deleting registry entries");
                DeleteLocalMachineSubKey(@"Software\Microsoft\.NETFramework\AssemblyFolders\SharpKit5");
                DeleteLocalMachineSubKey(@"Software\Microsoft\.NETFramework\v3.5\AssemblyFoldersEx\SharpKit5");
                DeleteLocalMachineSubKey(@"Software\Microsoft\.NETFramework\v4.0\AssemblyFoldersEx\SharpKit5");

                using (var key = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Visual Studio\10.0\MSBuild\SafeImports"))
                    key.DeleteValue("SharpKit5", false);

                using (var key = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Visual Studio\10.0\MSBuild\SafeImports"))
                    key.DeleteValue("SharpKit5", false);

                using (var key = Registry.LocalMachine.CreateSubKey(@"Software\SharpKit\5"))
                {
                    key.DeleteValue("ProductFolder", false);
                    key.SetValue("ProductVersion", false);
                    key.SetValue("ProductType", false);
                }

                Log("Removing shortcuts");
                if (Utils.IsUnix)
                {
                    Utils.UIDeleteFile(StartMenuDir + "/sharpkit-activation.desktop");
                    Utils.UIDeleteFile(StartMenuDir + "/sharpkit-updates.desktop");
                    Utils.UIDeleteFile(StartMenuDir + "/sharpkit-repair.desktop");
                }
                else
                {
                    Utils.UIDeleteDirectory(StartMenuDir);
                }

                if (!Utils.IsUnix)
                {
                    Log("Unregister application");
                    UnregisterUninstall();
                }

                Log("Finished");
            }
            catch (Exception ex)
            {
                LogFormat("Error while uninstalling: {0}", ex.Message);
            }
            if (OnFinished != null)
                OnFinished();
        }

        private void DeleteLocalMachineSubKey(string subKey)
        {
            LogFormat("Deleting registry key: MachineKey/{0}", subKey);
            Registry.LocalMachine.DeleteSubKey(subKey, false);
        }

        private void RegisterUninstall()
        {
            var guid = UninstallGUID.ToString("B");
            using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + guid))
            {
                key.SetValue("DisplayName", "SharpKit");
                //key.SetValue("ApplicationVersion", ProductVersion);
                key.SetValue("Publisher", "SharpKit");
                key.SetValue("DisplayIcon", ApplicationFolder + @"\SharpKitSetup.exe");
                key.SetValue("DisplayVersion", ProductVersion);
                key.SetValue("URLInfoAbout", "http://www.sharpkit.net");
                key.SetValue("Contact", "support@sharpkit.net");
                key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                key.SetValue("UninstallString", ApplicationFolder + @"\SharpKitSetup.exe /uninstall");
            }
        }

        private void UnregisterUninstall()
        {
            var guid = UninstallGUID.ToString("B");
            Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + guid, false);
        }

        public event Action<string> OnLog;
        public event Action OnFinished;

        private void Log(string text)
        {
            if (OnLog != null) OnLog(text);
        }

        private MemoryStream WebMemorystram = null;
        private TZipArchive GetZipArchive()
        {
            MemoryStream ms;
            if (WebMemorystram != null)
                ms = WebMemorystram;
            else
                ms = new MemoryStream(Properties.Resources.Files);

            return new TZipArchive(ms);
        }

        #if WINNT
        private void CreateShortcutWin(string name, string destFile, string arguments = "", string description = "")
        {
            var lnkFile = StartMenuDir + dsc + name + ".lnk";
            if (File.Exists(lnkFile))
                Utils.UIDeleteFile(lnkFile);
            var lnkDir = Path.GetDirectoryName(lnkFile);
            if (!Directory.Exists(lnkDir)) Directory.CreateDirectory(lnkDir);
            var wsh = new IWshRuntimeLibrary.WshShellClass();
            IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(lnkFile) as IWshRuntimeLibrary.IWshShortcut;
            shortcut.TargetPath = destFile;
            shortcut.Arguments = arguments;
            shortcut.Description = description;
            shortcut.WorkingDirectory = Path.GetDirectoryName(destFile); ;
            shortcut.Save();
        }
        #endif

        private void CreateShortcutUnix(string fileName, string name, string destFile, string arguments = "", string description = "")
        {
            var lnkFile = StartMenuDir + dsc + fileName + ".desktop";
            if (File.Exists(lnkFile))
                Utils.UIDeleteFile(lnkFile);
            var lnkDir = Path.GetDirectoryName(lnkFile);
            if (!Directory.Exists(lnkDir)) Directory.CreateDirectory(lnkDir);
            var lines = new List<string>();
            lines.Add("[Desktop Entry]");
            lines.Add("Name=" + name);
            lines.Add("Type=Application");
            lines.Add("Comment=" + description);
            lines.Add("Exec=" + destFile + " " + arguments);
            lines.Add("Terminal=false");
            lines.Add("Categories=Application;System;");
            File.WriteAllLines(lnkFile, lines.ToArray());
        }

        private void StopService()
        {
            if (Utils.IsUnix)
                return;
            //var service = ServiceController.GetServices().Where(t => t.ServiceName == "SharpKit").FirstOrDefault();
            //if (service == null)
            //    return;
            //service.Stop();
            //Log("Stopping SharpKit windows sevice");
            Execute("Stop SharpKit Windows Service", "net", "stop sharpkit");
        }

        void Try(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        private void InstallService()
        {
            if (Utils.IsUnix)
                return;
            var skc = Path.Combine(ApplicationCompilerFolder, "skc5.exe");
            Execute("Install SharpKit Windows Service", skc, "/service:reinstall");
        }

        private void UninstallService()
        {
            if (Utils.IsUnix)
                return;
            StopService();
            var skc = Path.Combine(ApplicationCompilerFolder, "skc5.exe");
            Execute("Install SharpKit Windows Service", skc, "/service:uninstall");

        }

        private void CreateNativeImage()
        {
            string ngen;
            string skc = Path.Combine(ApplicationCompilerFolder, "skc5.exe"); ;
            string args;
            if (Utils.IsUnix)
            {
                return; //skip on unix, because of bug in mono runtime: 

                //Assertion at method-to-ir.c:11612, condition `((sreg == -1) && (regtype == ' ')) || ((sreg != -1) && (regtype != ' '))' not met
                /*=================================================================
                Got a SIGABRT while executing native code. This usually indicates
                a fatal error in the mono runtime or one of the native libraries 
                used by your application.
                =================================================================*/

                //ngen = "mono";
                //args = "-O=all --aot " + skc;
            }
            else
            {
                ngen = Path.Combine(Utils.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET\Framework\v4.0.30319", "ngen.exe");
                args = "install " + "\"" + skc + "\"";
            }
            Execute("CreateNativeImage", ngen, args);
        }

        void Execute(string action, string filename, string args)
        {
            Log(action + " started");
            var res = ProcessHelper.ExecuteProgramWithOutput(new ExecuteProcessInfo { Filename = filename, Arguments = args });
            if (res.OutputLines != null)
                res.OutputLines.ForEach(t => LogFormat("    {0}", t));
            if (res.ExitCode != 0)
                LogFormat(action + " failed - ExitCode={0} command={1} {2}", res.ExitCode, filename, args);
            Log(action + " finished");
        }

        private void LogFormat(string format, params object[] args)
        {
            Log(String.Format(format, args));
        }

    }


}
