using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace SharpKit.Compiler
{
    class Console
    {
        public bool AutoFlush { get; set; }
        public ConcurrentQueue<string> Items = new ConcurrentQueue<string>();
        public void WriteLine(object p)
        {
            Items.Enqueue(p.ToString());
            DoAutoFlush();
        }

        private void DoAutoFlush()
        {
            if (!AutoFlush)
                return;
            Flush();
        }

        public void Flush()
        {
            while (Items.Count > 0)
            {
                string item;
                if (Items.TryDequeue(out item))
                {
                    System.Console.WriteLine(item);
                }
            }
        }
        public void WriteLine(string msg)
        {
            Items.Enqueue(msg);
            DoAutoFlush();
        }
        public void FormatLine(string msg, params object[] args)
        {
            Items.Enqueue(String.Format(msg, args));
            DoAutoFlush();
        }
    }
}
