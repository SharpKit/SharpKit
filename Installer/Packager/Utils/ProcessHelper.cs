using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security;
using System.IO;

namespace SharpKit.Utils
{
    class ExecuteProcessResult
    {
        public int ExitCode { get; set; }
        public List<string> OutputLines { get; set; }
    }

    class ExecuteProcessInfo
    {
        public string Filename { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public TimeSpan? Timeout { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    static class ProcessHelper
    {
        public static ExecuteProcessResult ExecuteProgramWithOutput(ExecuteProcessInfo info)
        {
            long timestamp = DateTime.Now.Ticks;
            Trace.TraceInformation("{3}: Executing {0} {1} at {2}", info.Filename, info.Arguments, info.WorkingDirectory, timestamp);
            var entrance = new object();
            var result = new ExecuteProcessResult();
            DataReceivedEventHandler dataHandler = (s, e) =>
            {
                if (e.Data == null)
                    return;
                lock (entrance)
                    result.OutputLines.AddRange(e.Data.Split('\n'));
            };
            result.OutputLines = new List<string>();
            try
            {
                int? ti = null;
                if (info.Timeout != null)
                    ti = (int)info.Timeout.Value.TotalMilliseconds;
                result.ExitCode = ProcessHelper.ExecuteAndWaitForExit(info.Filename, info.Arguments, info.WorkingDirectory, dataHandler, dataHandler, info.Username, info.Password, ti, info.EnvironmentVariables);
                Trace.TraceInformation("{0}: Finished with exit code {1}", timestamp, result.ExitCode);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("{0}: Exception while executing program: {1}", timestamp, e.Message);
                result.ExitCode = 1;
            }
            return result;
        }



        public static int ExecuteAndWaitForExit(string filename, string arguments, string workingDirectory, DataReceivedEventHandler outputDataRecieved, DataReceivedEventHandler errorDataRecieved)
        {
            return ExecuteAndWaitForExit(filename, arguments, workingDirectory, outputDataRecieved, errorDataRecieved, null, null, null);
        }
        public static int ExecuteAndWaitForExit(string filename, string arguments, string workingDirectory, DataReceivedEventHandler outputDataRecieved, DataReceivedEventHandler errorDataRecieved, int? timeout)
        {
            return ExecuteAndWaitForExit(filename, arguments, workingDirectory, outputDataRecieved, errorDataRecieved, null, null, timeout);
        }
        public static int ExecuteAndWaitForExit(string filename, string arguments, string workingDirectory, TextWriter outputData, TextWriter outputError, int? timeout)
        {
            var entrance = new object();
            DataReceivedEventHandler d1 = null;
            if (outputData != null)
            {
                d1 = new DataReceivedEventHandler(delegate(object x1, DataReceivedEventArgs e)
                     {
                         lock (entrance)
                         {
                             outputData.Write(e.Data);
                         }
                     });
            }
            DataReceivedEventHandler d2 = null;
            if (outputError != null)
            {
                d2 = new DataReceivedEventHandler(delegate(object x1, DataReceivedEventArgs e)
                {
                    lock (entrance)
                    {
                        outputError.Write(e.Data);
                    }
                });
            }
            return ExecuteAndWaitForExit(filename, arguments, workingDirectory, d1, d2, null, null, timeout);
        }
        public static int ExecuteAndWaitForExit(string filename, string arguments, string workingDirectory, DataReceivedEventHandler outputDataRecieved, DataReceivedEventHandler errorDataRecieved, string username, string password, int? timeout)
        {
            return ExecuteAndWaitForExit(filename, arguments, workingDirectory, outputDataRecieved, errorDataRecieved, username, password, timeout, null);
        }
        public static int ExecuteAndWaitForExit(string filename, string arguments, string workingDirectory, DataReceivedEventHandler outputDataRecieved, DataReceivedEventHandler errorDataRecieved, string username, string password, int? timeout, Dictionary<string, string> environmentVariables)
        {
            var si = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments
                };
            Process process = new Process { StartInfo = si };

            if (workingDirectory.IsNotNullOrEmpty())
                si.WorkingDirectory = workingDirectory;
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            if (environmentVariables != null)
            {
                foreach (var k in environmentVariables.Keys)
                {
                    si.EnvironmentVariables[k] = environmentVariables[k];
                }
            }
            if (username.IsNotNullOrEmpty())
            {
                si.UserName = username;
                var ss = new SecureString();
                if (password.IsNotNullOrEmpty())
                {
                    foreach (var ch in password)
                    {
                        ss.AppendChar(ch);
                    }
                }
                si.Password = ss;
            }
            if (outputDataRecieved != null)
            {
                si.RedirectStandardOutput = true;
                process.OutputDataReceived += new DataReceivedEventHandler(outputDataRecieved);
            }
            if (errorDataRecieved != null)
            {
                si.RedirectStandardError = true;
                process.ErrorDataReceived += new DataReceivedEventHandler(errorDataRecieved);
            }
            if (!process.Start())
                throw new Exception("could not start");
            if (outputDataRecieved != null)
                process.BeginOutputReadLine();
            if (errorDataRecieved != null)
                process.BeginErrorReadLine();
            if (timeout != null)
            {
                var exited = process.WaitForExit(timeout.Value);
                if (!exited)
                {
                    process.Kill();
                    throw new Exception(String.Format("The process {0} {1} did not exit by the specified timeout {2}", filename, arguments, timeout.Value));
                }
            }
            else
            {
                process.WaitForExit();
            }
            return process.ExitCode;
        }

        public static string Quote(string argument)
        {
            return "\"" + argument + "\"";
        }
    }
}
