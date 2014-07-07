using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace SharpKit.Installer.Builder
{


    static class Extensions
    {
        public static IEnumerable<ObjectProperty<T>> EnumerateProperties<T>(this T obj)
        {
            foreach (var pe in obj.GetType().GetProperties())
            {
                yield return new ObjectProperty<T>
                {
                    Object = obj,
                    Property = pe,
                    Getter = () => pe.GetValue(obj, null),
                    Setter = t => pe.SetValue(obj, t, null),
                };
            }
        }
        public static bool IsEmpty(this string self)
        {
            return self == null || self == "";
        }

    }
    struct ObjectProperty<T>
    {
        public T Object { get; set; }
        public PropertyInfo Property { get; set; }
        public Func<object> Getter { get; set; }
        public Action<object> Setter { get; set; }
    }

    static class Utils
    {

        public static string CorrectPathSeparator(string path)
        {
            if (IsUnix)
                return path.Replace("\\", "/");
            else
                return path.Replace("/", "\\");
        }

        public static bool IsUnix
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Unix;
            }
        }

        public static ExecuteResult ExecuteProcess(string dir, string file, string args)
        {
            Console.WriteLine("Executing: {0} {1} {2}", dir, file, args);
            var process = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = dir,
                FileName = file,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });
            var res = new ExecuteResult { Output = new List<string>(), Error = new List<string>() };

            Console.WriteLine("{0}>{1} {2}", process.StartInfo.WorkingDirectory, process.StartInfo.FileName, process.StartInfo.Arguments);
            process.OutputDataReceived += (s, e) => { Console.WriteLine(e.Data); res.Output.Add(e.Data); };
            process.ErrorDataReceived += (s, e) => { Console.WriteLine(e.Data); res.Error.Add(e.Data); };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            res.ExitCode = process.ExitCode;
            Console.WriteLine("Finished execution. Exit code: {0}", process.ExitCode);
            return res;
        }

    }

    public class ExecuteResult
    {
        public int ExitCode { get; set; }
        public List<string> Output { get; set; }
        public List<string> Error { get; set; }
    }

}
