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
using System.Threading;

namespace SharpKit.Installer
{

    public partial class FormMain : Form
    {

        private Installer installer;
        private Thread InstallerThread;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            installer = new Installer();
            installer.OnLog += LogAsync;
            installer.OnFinished += () => Invoke(new Action(() =>
            {
                buttonAction = ButtonAction.Close;
                butInstall.Text = "Close";
            }));

            installer.EnsureInited();

            Log(string.Format("Welcome to SharpKit {0} Installation!", installer.ProductVersion));

            butInstall.Focus();

            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && args[1] == "/uninstall") radioUninstall.Checked = true;
        }

        private ButtonAction buttonAction = ButtonAction.Install;
        private enum ButtonAction
        {
            Install,
            Abort,
            Close
        }

        private void butInstall_Click(object sender, EventArgs e)
        {
            switch (buttonAction)
            {
                case ButtonAction.Install:
                    buttonAction = ButtonAction.Abort;
                    butInstall.Text = "Abort";
                    cbWebInstaller.Enabled = false;
                    radioInstall.Enabled = false;
                    radioUninstall.Enabled = false;

                    if (radioInstall.Checked)
                        InstallAsync();
                    else if (radioUninstall.Checked)
                        UninstallAsync();

                    break;
                case ButtonAction.Abort:
                    buttonAction = ButtonAction.Close;
                    InstallerThread.Abort();
                    butInstall.Text = "Close";
                    Log("Aborted");
                    break;
                case ButtonAction.Close:
                    Close();
                    break;
                default: throw new NotImplementedException();
            }
        }

        private void InstallAsync()
        {
            InstallerThread = new Thread(InstallSync);
            InstallerThread.Start();
        }

        private void InstallSync(object state)
        {
            installer.Install();
        }

        private void UninstallAsync()
        {
            InstallerThread = new Thread(UninstallSync);
            InstallerThread.Start();
        }

        private void UninstallSync(object state)
        {
            installer.Uninstall();
        }

        private void LogAsync(string text)
        {
            this.Invoke(new Action<string>(Log), text);
        }

        private void Log(string text)
        {
            tbLog.Text += text + "\r\n";
            tbLog.Select(tbLog.Text.Length, 0);
            tbLog.ScrollToCaret();
        }

        private void cbWebInstaller_CheckedChanged(object sender, EventArgs e)
        {
            installer.CheckForUpdates = cbWebInstaller.Checked;
        }

    }

}
