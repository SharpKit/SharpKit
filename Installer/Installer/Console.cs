using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace SharpKit.Installer
{

    class ConsoleInterface
    {

        private Installer installer;

        public void Main()
        {
            installer = new Installer();
            installer.OnLog += Log;
            installer.OnFinished += () =>
            {
                Environment.Exit(0);
            };

            installer.EnsureInited();

            Log(string.Format("Welcome to SharpKit {0} Installation!", installer.ProductVersion));

            while (true)
            {
                Console.WriteLine("Press 'i' for install/upgrade/repair");
                Console.WriteLine("Press 'u' for uninstall");
                Console.WriteLine("Press 'q' for quit");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.I:
                        installer.Install();
                        break;
                    case ConsoleKey.U:
                        installer.Uninstall();
                        break;
                    case ConsoleKey.Q:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private void Log(string text)
        {
            Console.WriteLine(text);
        }

    }

}
