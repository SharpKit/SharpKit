using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace SharpKit.Installer
{

    public class TZipArchive : IDisposable
    {

        private ICSharpCode.SharpZipLib.Zip.ZipFile zip;

        public TZipArchive(string ZipFile)
        {
            zip = new ICSharpCode.SharpZipLib.Zip.ZipFile(ZipFile);
        }

        protected TZipArchive(ICSharpCode.SharpZipLib.Zip.ZipFile Zip)
        {
            this.zip = Zip;
        }

        public static TZipArchive Create(string ZipFile)
        {
            if (File.Exists(ZipFile))
            {
                Utils.UIDeleteFile(ZipFile);
            }
            return new TZipArchive(ICSharpCode.SharpZipLib.Zip.ZipFile.Create(ZipFile));
        }

        public TZipArchive(Stream s)
        {
            zip = new ICSharpCode.SharpZipLib.Zip.ZipFile(s);
        }

        public void AddFile(string File)
        {
            zip.Add(File, Path.GetFileName(File));
        }

        public void AddFile(string File, string EntryName)
        {
            zip.Add(File, EntryName);
        }

        public void BeginUpdate()
        {
            zip.BeginUpdate();
        }

        public void EndUpdate()
        {
            zip.CommitUpdate();
        }

        private char dsc = Path.DirectorySeparatorChar;

        public void ExtractFile(string ArchiveFile, string DestDir, string DestFile = "")
        {
            if (DestFile == "")
            {
                DestFile = ArchiveFile;
            }
            if (DestDir.Length > 0 && DestDir[DestDir.Length - 1] != dsc)
            {
                DestDir += dsc;
            }
            DestFile = DestDir + DestFile;

            if (DestDir != "" && !Directory.Exists(System.IO.Path.GetDirectoryName(DestFile)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DestFile));
            }

            int idx = zip.FindEntry(ArchiveFile, true);
            if (idx < 0)
            {
                return;
            }
            ICSharpCode.SharpZipLib.Zip.ZipEntry entry = zip[idx];

            var tmpFile = Path.GetTempFileName();
            using (Stream stream = zip.GetInputStream(entry))
            {
                byte[] bytes = new byte[Convert.ToInt32(entry.Size)];
                stream.Read(bytes, 0, bytes.Length);
                System.IO.File.WriteAllBytes(tmpFile, bytes);
            }
            Utils.UICopyOverwrite(tmpFile, DestFile);
            //if (File.Exists(DestFile))
            //{
            //    Utils.UIDeleteFile(DestFile);
            //}
            //File.Copy(tmpFile, DestFile, true);
        }

        public void ExtractDirectory(string ArchiveDir, string DestDir, Action<string> logger = null)
        {
            if (ArchiveDir.Last() != '/') ArchiveDir += "/";
            if (DestDir.Last() != dsc) DestDir += dsc;
            foreach (var entry in GetFiles())
            {
                if (entry.StartsWith(ArchiveDir) && !entry.EndsWith("/"))
                {
                    var DestFile = entry.Substring(ArchiveDir.Length);
                    if (logger != null)
                        logger(Path.Combine(DestDir, DestFile));
                    ExtractFile(entry, DestDir, DestFile);
                }
            }
        }

        public string[] GetFiles()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < Convert.ToInt32(zip.Count); i++)
            {
                ICSharpCode.SharpZipLib.Zip.ZipEntry entry = zip[i];
                string fileName = entry.Name;
                list.Add(fileName);
            }
            return list.ToArray();
        }


        public void Dispose()
        {
            zip.Close();
        }

    }

}
