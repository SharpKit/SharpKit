using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpKit.Utils
{
    public static class FileHelper
    {
        public static void VerifyDir(string filename)
        {
            DirectoryHelper.VerifyDir(Path.GetDirectoryName(filename));
        }
        public static void DeleteFileIfExists(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}
