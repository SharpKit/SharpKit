using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKit.Utils
{
    class StopwatchHelper
    {
        [DebuggerStepThrough]
        public static long TimeInMs(Action action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }


    }
}
