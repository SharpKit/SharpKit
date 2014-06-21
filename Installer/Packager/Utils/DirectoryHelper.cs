using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpKit.Utils
{
    class DirectoryHelper
    {
        public static List<string> GetFiles(string dir, bool excludeHidden)
        {
            var files = Directory.GetFiles(dir).ToList();
            if (excludeHidden)
                files = files.Where(t => !File.GetAttributes(t).HasFlag(FileAttributes.Hidden)).ToList();
            return files;
        }
        public static List<string> GetDirectories(string dir, bool excludeHidden)
        {
            var dirs = Directory.GetDirectories(dir).ToList();
            if (excludeHidden)
                dirs = dirs.Where(t => !File.GetAttributes(t).HasFlag(FileAttributes.Hidden)).ToList();
            return dirs;
        }
        public static void CopyDirContents(string dir1, string dir2, bool recursive, bool excludeHidden)
        {
            VerifyDir(dir2);
            GetFiles(dir1, excludeHidden).ForEach(t => File.Copy(t, Path.Combine(dir2, Path.GetFileName(t))));
            if (recursive)
            {
                GetDirectories(dir1, excludeHidden).ForEach(innerDir =>
                {
                    var innerDir2 = Path.Combine(dir2, Path.GetFileName(innerDir));
                    Directory.CreateDirectory(innerDir2);
                    CopyDirContents(innerDir, innerDir2, true, excludeHidden);
                });
            }
        }
        public static void DeleteDirContents(string dir)
        {
            Directory.GetFiles(dir, "*", SearchOption.AllDirectories).ForEach(t => File.Delete(t));
            Directory.GetDirectories(dir).ForEach(t => Directory.Delete(t, true));
        }

        public static void UseCurrentDirectory(string dir, Action action)
        {
            var prev = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(dir);
                action();
            }
            finally
            {
                Directory.SetCurrentDirectory(prev);
            }
        }
        public static void VerifyDir(string dir)
        {
            if (dir.Length == 0)
                return;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        public static void VerifyEmptyDir(string dir)
        {
            if (dir.Length == 0)
                return;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            else
            {
                DeleteDirContents(dir);
            }
        }
    }
}
