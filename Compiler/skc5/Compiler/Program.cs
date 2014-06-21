using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using System.IO;
using System.CodeDom.Compiler;
using Mirrored.SharpKit.JavaScript;
using System.Diagnostics;
using System.Configuration;
using System.Threading;
using System.Globalization;

namespace SharpKit.Compiler
{
    class Program
    {
        public static int Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            CollectionExtensions.Parallel = ConfigurationManager.AppSettings["Parallel"] == "true";
            CollectionExtensions.ParallelPreAction = () => Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //Console.AutoFlush = true;
            System.Console.WriteLine("Parallel=" + CollectionExtensions.Parallel);
            var skc = new CompilerTool { CommandLineArguments = args };
            skc.Init();
#if DEBUG
            skc.Debug = true;
#endif
            var res = skc.Run();
            stopwatch.Stop();
            System.Console.WriteLine("Total: {0}ms", stopwatch.ElapsedMilliseconds);
            //System.Console.Flush();
            return res;

        }

    }


}
