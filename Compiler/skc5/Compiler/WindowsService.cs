using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpKit.Compiler
{
    class WindowsService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            if (StartAction != null)
                StartAction();
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (PauseAction != null)
                PauseAction();
            else if (StopAction != null)
                StopAction();

        }

        protected override void OnStop()
        {
            base.OnStop();
            if (StopAction != null)
                StopAction();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            if (ContinueAction != null)
                ContinueAction();
            else if (StartAction != null)
                StartAction();
        }
        protected override void OnShutdown()
        {
            base.OnShutdown();
            if (ShutdownAction != null)
                ShutdownAction();
        }
        public Action StartAction { get; set; }
        public Action StopAction { get; set; }
        public Action PauseAction { get; set; }
        public Action RestartAction { get; set; }
        public Action ContinueAction { get; set; }
        public Action ShutdownAction { get; set; }

        public void Run()
        {
            ServiceBase.Run(this);
        }
    }
}
