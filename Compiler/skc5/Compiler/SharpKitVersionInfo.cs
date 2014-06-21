using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SharpKit.Compiler
{
    class SharpKitVersionInfo
    {
        public string Version { get; set; }

        static SharpKitVersionInfo _CurrentVersion;
        public static SharpKitVersionInfo CurrentVersion
        {
            get
            {
                if (_CurrentVersion == null)
                {
                    //var SkcVersion = typeof(SharpKitVersionInfo).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
                    var SkcVersion = typeof(SharpKitVersionInfo).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().First().Version;
                    _CurrentVersion = new SharpKitVersionInfo { Version = SkcVersion };
                }
                return _CurrentVersion;
            }
        }
    }
}
