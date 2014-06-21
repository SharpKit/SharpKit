using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SharpKit.Utils
{
    class SevenZipHelper
    {
        public static string ExeFilename = @"C:\Program Files (x86)\7-Zip\7z.exe";

        static string Quote(string s)
        {
            if (!s.StartsWith("\""))
                return String.Format("\"{0}\"", s);
            return s;
        }
        public static void ZipDirectory(string dir, string zipFile)
        {
            var res = ProcessHelper.ExecuteProgramWithOutput(new ExecuteProcessInfo { Filename = ExeFilename, Arguments = "a " + Quote(zipFile), WorkingDirectory = dir });
        }

        public static void AddFilesToZip(string zipFile, string[] filenamesToAdd)
        {
            var res = ProcessHelper.ExecuteProgramWithOutput(new ExecuteProcessInfo { Filename = ExeFilename, Arguments = "a " + Quote(zipFile) + " " + filenamesToAdd.Select(Quote).StringJoin(",") });
        }
        public static void ExtractFilesFromZip(string zipFile, string[] filenamesToExtract)
        {
            var res = ProcessHelper.ExecuteProgramWithOutput(new ExecuteProcessInfo { Filename = ExeFilename, Arguments = "e " + Quote(zipFile) + " " + filenamesToExtract.Select(Quote).StringJoin(",") });
        }


        public static string ExtractToDir(string zipFile)
        {
            var dir = Path.Combine(Path.GetDirectoryName(zipFile), Path.GetFileNameWithoutExtension(zipFile));
            DirectoryHelper.VerifyDir(dir);
            if (!Path.IsPathRooted(zipFile))
                zipFile = "..\\" + zipFile;
            var res = ProcessHelper.ExecuteProgramWithOutput(new ExecuteProcessInfo { Filename = ExeFilename, Arguments = "e " + Quote(zipFile), WorkingDirectory = dir });
            return dir;
        }

    }
}
