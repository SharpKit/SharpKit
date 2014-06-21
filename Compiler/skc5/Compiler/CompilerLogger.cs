using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp;
using System.Collections.Concurrent;
using ICSharpCode.NRefactory;
using System.Diagnostics;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler
{

    class CompilerLogger
    {
        public CompilerLogger()
        {
            Items = new ConcurrentQueue<CompilerLogItem>();
            Console = new Console { AutoFlush = true };
        }
        public ConcurrentQueue<CompilerLogItem> Items { get; set; }
        public void Log(CompilerLogItem item)
        {
            if (item == null)
                return;
            TryResolveMissingInfo(item);
            Items.Enqueue(item);
            var sb = new StringBuilder();
            if (item.ProjectRelativeFilename.IsNotNullOrEmpty())
                sb.AppendFormat("{0}", item.ProjectRelativeFilename);
            if (item.Line > 0 && item.Column > 0)
                sb.AppendFormat("({0},{1})", item.Line, item.Column);
            if (item.ProjectRelativeFilename.IsNotNullOrEmpty() || (item.Line > 0 && item.Column > 0))
                sb.Append(": ");

            sb.AppendFormat("{0} {1}{2}: {3}", item.Type.ToString().ToLower(), "SK", item.Code.ToString("0000"), item.Text);
            if (item.AbsoluteFilename.IsNotNullOrEmpty())
                sb.AppendFormat(" [{0}]", item.AbsoluteFilename);
            var ss = sb.ToString();
            Console.WriteLine(ss);
            if (CompilerConfiguration.Current.EnableLogging)
                Debug(ss);
            //"Compilation\JsCompiler.cs(175,26): warning CS0169: The field 'SharpKit.JavaScript.Compilation.JsCompiler.__LastException' is never used [C:\Projects\SharpKit\googlecode\trunk\SharpKit.JsClr-4.1.0\SharpKit.JsClr-4.1.0.csproj]
        }

        private void TryResolveMissingInfo(CompilerLogItem item)
        {
            try
            {
                if (item.Line == 0 && item.Column == 0)
                {
                    if (item.Entity != null && !item.Entity.Region.IsEmpty)
                    {
                        item.Line = item.Entity.Region.BeginLine;
                        item.Column = item.Entity.Region.BeginColumn;
                    }
                    else if (item.Node != null && !item.Node.StartLocation.IsEmpty)
                    {
                        item.Line = item.Node.StartLocation.Line;
                        item.Column = item.Node.StartLocation.Column;
                    }
                }
                if (item.ProjectRelativeFilename == null && item.AbsoluteFilename == null)
                {
                    if (item.Node != null)
                        item.ProjectRelativeFilename = item.Node.GetFileName();
                    else if (item.Entity != null && !item.Entity.Region.IsEmpty)
                        item.ProjectRelativeFilename = item.Entity.Region.FileName;
                    if (item.ProjectRelativeFilename.IsNotNullOrEmpty())
                        item.AbsoluteFilename = Path.GetFullPath(item.ProjectRelativeFilename);
                }
            }
            catch
            {
            }
        }

        public void Warn(string text)
        {
            Log(new CompilerLogItem { Type = CompilerLogItemType.Warning, Text = text });
        }
        public void Warn(IEntity me, string text)
        {
            Log(new CompilerLogItem { Type = CompilerLogItemType.Warning, Entity = me, Text = text });
        }
        public void Warn(AstNode node, string text)
        {
            Log(new CompilerLogItem { Type = CompilerLogItemType.Warning, Node = node, Text = text });
        }
        public void Error(string text)
        {
            Log(new CompilerLogItem { Type = CompilerLogItemType.Error, Text = text });
        }
        public void Error(AstNode node, string text)
        {
            Log(new CompilerLogItem { Type = CompilerLogItemType.Error, Node = node, Text = text });
        }
        public void Error(IEntity me, string text)
        {
            Log(new CompilerLogItem { Type = CompilerLogItemType.Error, Entity = me, Text = text });
        }

        public void Log(Exception e)
        {
            if (e is CompilerException)
            {
                var ee = (CompilerException)e;
                if (ee.AstNode != null)
                    Error(ee.AstNode, e.Message);
                else if (ee.Entity != null)
                    Error(ee.Entity, e.Message);
                else
                {
                    Log(new CompilerLogItem { ProjectRelativeFilename = ee.Filename, Column = ee.Column.GetValueOrDefault(), Line = ee.Line.GetValueOrDefault(), Text = ee.Message, Type = CompilerLogItemType.Error });
                }
            }
            else if (e is AggregateException)
            {
                var ae = (AggregateException)e;
                Log(ae.InnerException);
            }

            else
            {
                Error(e.Message);
            }
        }

        public void Message(string msg)
        {
            Log(new CompilerLogItem { Type = CompilerLogItemType.Message, Text = msg });
        }

        public void WriteLine(string format, object prm1)
        {
            WriteLine(String.Format(format, prm1));
        }
        public void WriteLine(string format, object prm1, object prm2)
        {
            WriteLine(String.Format(format, prm1, prm2));
        }
        public void WriteLine(string format, object prm1, object prm2, object prm3)
        {
            WriteLine(String.Format(format, prm1, prm2, prm3));
        }
        public void WriteLine(string s)
        {
            Console.WriteLine(s);
            if (CompilerConfiguration.Current.EnableLogging)
                Debug(s);
        }

        string LoggingFilename;
        public void Init()
        {
            if (CompilerConfiguration.Current.EnableLogging)
            {
                LoggingFilename = Process.GetCurrentProcess().MainModule.FileName + ".log";
                File.Delete(LoggingFilename);
            }
        }
        public void Debug(string s)
        {
            if (!CompilerConfiguration.Current.EnableLogging)
                return;
            File.AppendAllLines(LoggingFilename, new string[] { s });
        }
        public Console Console { get; set; }
    }

    class CompilerLogItem
    {
        public AstNode Node { get; set; }
        public string Text { get; set; }
        public int Code { get; set; }
        public CompilerLogItemType Type { get; set; }
        public IEntity Entity { get; set; }
        public string ProjectRelativeFilename { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string AbsoluteFilename { get; set; }

    }

    enum CompilerLogItemType
    {
        Message,
        Warning,
        Error
    }
}
