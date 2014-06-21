using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpKit.Utils;
using System.Diagnostics;

namespace SharpKit.Compiler
{
    [Serializable]
    class Manifest
    {
        public Manifest()
        {
            ExternalFiles = new List<ManifestFile>();
            SourceFiles = new List<ManifestFile>();
            ContentFiles = new List<ManifestFile>();
            ReferencedFiles = new List<ManifestFile>();
            NoneFiles = new List<ManifestFile>();
        }
        public string SkcVersion { get; set; }
        public ManifestFile SkcFile { get; set; }
        public string DefinedSymbols { get; set; }
        public List<ManifestFile> SourceFiles { get; set; }
        public List<ManifestFile> ContentFiles { get; set; }
        public List<ManifestFile> ReferencedFiles { get; set; }
        //public List<string> OutputFiles { get; set; }
        public List<ManifestFile> NoneFiles { get; set; }
        public List<ManifestFile> ExternalFiles { get; set; }

        public void SaveToFile(string filename)
        {
            var dir = Path.GetDirectoryName(filename);
            if (dir.IsNotNullOrEmpty() && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            new XSerializer().SerializeToFile(this, filename);
        }
        public static Manifest LoadFromFile(string filename)
        {
            try
            {
                return new XSerializer().DeserializeFromFile<Manifest>(filename);
            }
            catch
            {
                return new Manifest();
            }
        }
        public ManifestDiff GetManifestDiff(Manifest other)
        {
            var ret = new ManifestDiff();
            if (SkcVersion != other.SkcVersion)
            {
                ret.NewVersion = SkcVersion;
                ret.OldVersion = other.SkcVersion;
            }
            if (SkcFile != other.SkcFile)
            {
                ret.NewVersion = SkcFile.ToString();
                ret.OldVersion = other.SkcFile == null ? "" : other.SkcFile.ToString();
            }
            if (DefinedSymbols != other.DefinedSymbols)
            {
                ret.NewDefines = DefinedSymbols;
                ret.OldDefines = other.DefinedSymbols;
            }

            var sourceDiffs = new CollectionDiff<ManifestFile>(SourceFiles, other.SourceFiles);
            ret.AddedSourceFilenames = sourceDiffs.NewItems.ToList();
            if (ret.AddedSourceFilenames.Count == 0)
                ret.AddedSourceFilenames = null;
            ret.RemovedSourceFilenames = sourceDiffs.RemovedItems.ToList();
            if (ret.RemovedSourceFilenames.Count == 0)
                ret.RemovedSourceFilenames = null;

            var contentDiffs = new CollectionDiff<ManifestFile>(ExternalFiles, other.ExternalFiles);
            ret.AddedExternalFiles = contentDiffs.NewItems.ToList();
            if (ret.AddedExternalFiles.Count == 0)
                ret.AddedExternalFiles = null;
            ret.RemovedExternalFiles = contentDiffs.RemovedItems.ToList();
            if (ret.RemovedExternalFiles.Count == 0)
                ret.RemovedExternalFiles = null;

            //var contentDiffs = new CollectionDiff<ManifestFile>(ContentFiles, other.ContentFiles);
            //ret.AddedContentFiles = contentDiffs.NewItems.ToList();
            //if (ret.AddedContentFiles.Count == 0)
            //    ret.AddedContentFiles = null;
            //ret.RemovedContentFiles = contentDiffs.RemovedItems.ToList();
            //if (ret.RemovedContentFiles.Count == 0)
            //    ret.RemovedContentFiles = null;

            var referenceDiffs = new CollectionDiff<ManifestFile>(ReferencedFiles, other.ReferencedFiles);
            ret.AddedReferences = referenceDiffs.NewItems.ToList();
            if (ret.AddedReferences.Count == 0)
                ret.AddedReferences = null;
            ret.RemovedReferences = referenceDiffs.RemovedItems.ToList();
            if (ret.RemovedReferences.Count == 0)
                ret.RemovedReferences = null;

            //var outputFileDiff = new CollectionDiff<string>(this.OutputFiles, other.OutputFiles);
            //ret.NewOutputFiles = referenceDiffs.NewItems.ToList();
            //if (ret.NewOutputFiles.Count == 0)
            //    ret.NewOutputFiles = null;
            //ret.OldOutputFiles = referenceDiffs.RemovedItems.ToList();
            //if (ret.OldOutputFiles.Count == 0)
            //    ret.OldOutputFiles = null;
            return ret;
        }
    }

    class ManifestFile
    {
        public static bool operator ==(ManifestFile x, ManifestFile y)
        {
            return Equals(x, y);
        }
        public static bool operator !=(ManifestFile x, ManifestFile y)
        {
            return !Equals(x, y);
        }
        public string Filename { get; set; }
        public string Modified { get; set; }
        public string Size { get; set; }

        public bool EqualsTo(ManifestFile file)
        {
            return Equals(this, file);
        }
        public override bool Equals(object obj)
        {
            if (obj is ManifestFile)
                return Equals(this, (ManifestFile)obj);
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return (Filename ?? "").GetHashCode() ^ (Modified ?? "").GetHashCode() ^ (Size ?? "").GetHashCode();
        }
        public static bool Equals(ManifestFile x, ManifestFile y)
        {
            if (object.ReferenceEquals(x, y))
                return true;
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                return false;
            return x.Filename == y.Filename && x.Modified == y.Modified && x.Size == y.Size;
        }
        public override string ToString()
        {
            return String.Format("\"{0}\", Modified:{1}, Size:{2}", Filename, Modified, Size);
        }
    }


    class ManifestDiff
    {
        /// <summary>
        /// Only specified if versions differ
        /// </summary>
        public string NewVersion { get; set; }
        /// <summary>
        /// Only specified if versions differ
        /// </summary>
        public string OldVersion { get; set; }
        /// <summary>
        /// Only specified if defines differ
        /// </summary>
        public string NewDefines { get; set; }
        /// <summary>
        /// Only specified if defines differ
        /// </summary>
        public string OldDefines { get; set; }
        /// <summary>
        /// Only specified if plugins differ
        /// </summary>
        public string NewLoadedPlugins { get; set; }
        /// <summary>
        /// Only specified if plugins differ
        /// </summary>
        public string OldLoadedPlugins { get; set; }
        /// <summary>
        /// Files added in new manifest
        /// </summary>
        public List<ManifestFile> AddedSourceFilenames { get; set; }
        /// <summary>
        /// Files removed in new manifest
        /// </summary>
        public List<ManifestFile> RemovedSourceFilenames { get; set; }

        public List<ManifestFile> AddedExternalFiles { get; set; }
        public List<ManifestFile> RemovedExternalFiles { get; set; }
        /// <summary>
        /// Content files in new manifest
        /// </summary>
        public List<ManifestFile> AddedContentFiles { get; set; }
        /// <summary>
        /// Content files removed in new manifest
        /// </summary>
        public List<ManifestFile> RemovedContentFiles { get; set; }
        /// <summary>
        /// References added in new manifest
        /// </summary>
        public List<ManifestFile> AddedReferences { get; set; }
        /// <summary>
        /// References removed in new manifest
        /// </summary>
        public List<ManifestFile> RemovedReferences { get; set; }
        public bool AreManifestsEqual
        {
            get
            {
                if (NewVersion != null)
                    return false;
                if (NewDefines != null)
                    return false;
                if (NewLoadedPlugins != null)
                    return false;
                if (AddedReferences != null && AddedReferences.Count > 0)
                    return false;
                if (RemovedReferences != null && RemovedReferences.Count > 0)
                    return false;
                if (AddedSourceFilenames != null && AddedSourceFilenames.Count > 0)
                    return false;
                if (RemovedSourceFilenames != null && RemovedSourceFilenames.Count > 0)
                    return false;
                //if (AddedContentFiles != null && AddedContentFiles.Count > 0)
                //    return false;
                //if (RemovedContentFiles != null && RemovedContentFiles.Count > 0)
                //    return false;
                if (AddedExternalFiles.IsNotNullOrEmpty())
                    return false;
                if (RemovedExternalFiles.IsNotNullOrEmpty())
                    return false;
                //if (OldManifestVersion != NewManifestVersion)
                //    return false;
                //if (OldOutputFiles != null && OldOutputFiles.Count > 0)
                //    return false;
                //if (NewOutputFiles != null && NewOutputFiles.Count > 0)
                //    return false;
                return true;
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (NewVersion != null)
                sb.AppendFormat("New version: {0} (Old:{1})\n", NewVersion, OldVersion);
            if (NewDefines != null)
            {
                var defs = NewDefines.Split(';');
                var oldDefs = OldDefines.IsNullOrEmpty() ? new string[] { } : OldDefines.Split(';');
                var defDiff = new CollectionDiff<string>(defs, oldDefs);
                if (defDiff.NewItems.Count() > 0)
                    sb.AppendFormat(defDiff.NewItems.StringConcat("Defined symbols added: ", ", ", "\n"));
                if (defDiff.RemovedItems.Count() > 0)
                    sb.AppendFormat(defDiff.RemovedItems.StringConcat("Defined symbols removed: ", ", ", "\n"));
            }
            if (NewLoadedPlugins != null)
            {
                var plgs = NewLoadedPlugins.Split(',');
                var olgPlgs = OldLoadedPlugins.IsNullOrEmpty() ? new string[] { } : OldLoadedPlugins.Split(';');
                var plgDiff = new CollectionDiff<string>(plgs, olgPlgs);
                if (plgDiff.NewItems.Count() > 0)
                    sb.AppendFormat(plgDiff.NewItems.StringConcat("Defined symbols added:\n", ",\n", "\n"));
                if (plgDiff.RemovedItems.Count() > 0)
                    sb.AppendFormat(plgDiff.RemovedItems.StringConcat("Defined symbols removed:\n", ",\n", "\n"));
            }
            if (AddedReferences != null && AddedReferences.Count > 0)
                sb.AppendFormat(AddedReferences.Select(t => t.ToString()).StringConcat("Added references:\n", ",\n", "\n"));
            if (RemovedReferences != null && RemovedReferences.Count > 0)
                sb.AppendFormat(RemovedReferences.Select(t => t.ToString()).StringConcat("Removed references:\n", ",\n", "\n"));
            if (AddedSourceFilenames != null && AddedSourceFilenames.Count > 0)
                sb.AppendFormat(AddedSourceFilenames.Select(t => t.ToString()).StringConcat("Added source files:\n", ",\n", "\n"));
            if (RemovedSourceFilenames != null && RemovedSourceFilenames.Count > 0)
                sb.AppendFormat(RemovedSourceFilenames.Select(t => t.ToString()).StringConcat("Removed source files:\n", ",\n", "\n"));
            if (AddedContentFiles != null && AddedContentFiles.Count > 0)
                sb.AppendFormat(AddedContentFiles.Select(t => t.ToString()).StringConcat("Added content files:\n", ",\n", "\n"));
            if (RemovedContentFiles != null && RemovedContentFiles.Count > 0)
                sb.AppendFormat(RemovedContentFiles.Select(t => t.ToString()).StringConcat("Removed content files:\n", ",\n", "\n"));
            //if (OldOutputFiles != null && OldOutputFiles.Count > 0)
            //    sb.AppendFormat(OldOutputFiles.StringConcat("Old output files:\n", ",\n", "\n"));
            //if (NewOutputFiles != null && NewOutputFiles.Count > 0)
            //    sb.AppendFormat(NewOutputFiles.StringConcat("New output files:\n", ",\n", "\n"));
            return sb.ToString();
        }
    }

    class CollectionDiff<T>
    {
        public CollectionDiff(IEnumerable<T> newCollection, IEnumerable<T> oldCollection)
        {
            if (newCollection == null)
                newCollection = new T[] { };
            if (oldCollection == null)
                oldCollection = new T[] { };
            var newHash = new HashSet<T>(newCollection);
            var oldHash = new HashSet<T>(oldCollection);
            NewItems = newHash.Except(oldHash);
            RemovedItems = oldHash.Except(newHash);
        }
        public IEnumerable<T> NewItems { get; set; }
        public IEnumerable<T> RemovedItems { get; set; }
    }

    class ManifestHelper
    {
        public CompilerToolArgs Args { get; set; }
        public CompilerLogger Log { get; set; }
        public string SkcVersion { get; set; }
        public string SkcFile { get; set; }
        public Manifest CreateManifest()
        {
            var manifest = new Manifest
            {
                DefinedSymbols = Args.define,
                SourceFiles = Args.Files.Select(f => GetFileWithModificationDate(f)).ToList(),
                ReferencedFiles = Args.References.Select(f => GetFileWithModificationDate(f)).ToList(),
                ContentFiles = Args.ContentFiles.Select(cf => GetFileWithModificationDate(cf)).ToList(),
                NoneFiles = Args.NoneFiles.Select(cf => GetFileWithModificationDate(cf)).ToList(),
                SkcVersion = SkcVersion,
                SkcFile = GetFileWithModificationDate(SkcFile),
                ExternalFiles = ExternalFiles.Select(GetFileWithModificationDate).ToList()
            };
            return manifest;
        }
        //string GetFileWithModificationDate(string filename)
        //{
        //    var fi = new FileInfo(filename);
        //    if (!fi.Exists)
        //        return String.Format("\"{0}\", missing", filename);
        //    DateTime dt = fi.LastWriteTime;
        //    return String.Format("\"{0}\", last modified at {1}", filename, dt);
        //}
        //string GetFileWithSize(string filename)
        //{
        //    var fi = new FileInfo(filename);
        //    if (!fi.Exists)
        //        return String.Format("\"{0}\", missing", filename);
        //    long size = fi.Length;
        //    return String.Format("\"{0}\", sized {1} bytes", filename, size);
        //}
        //string GetFileWithModificationDateAndSize(string filename)
        //{
        //    var fi = new FileInfo(filename);
        //    if (!fi.Exists)
        //        return String.Format("\"{0}\", missing", filename);
        //    DateTime dt = fi.LastWriteTime;
        //    long size = fi.Length;
        //    return String.Format("\"{0}\", last modified at {1}, sized {2} bytes", filename, dt, size);
        //}



        ManifestFile GetFileWithModificationDate(string filename)
        {
            var fi = new FileInfo(filename);
            if (!fi.Exists)
                return new ManifestFile { Filename = filename, Modified = "Missing" };// String.Format("\"{0}\", last modified at {1}", filename, dt);
            DateTime dt = fi.LastWriteTime;
            return new ManifestFile { Filename = filename, Modified = dt.ToBinary().ToString() };// String.Format("\"{0}\", last modified at {1}", filename, dt);
        }
        ManifestFile GetFileWithSize(string filename)
        {
            var fi = new FileInfo(filename);
            if (!fi.Exists)
                return new ManifestFile { Filename = filename, Size = "Missing" };// String.Format("\"{0}\", last modified at {1}", filename, dt);
            long size = fi.Length;
            return new ManifestFile { Filename = filename, Size = size.ToString() };// String.Format("\"{0}\", last modified at {1}", filename, dt);
            //return String.Format("\"{0}\", sized {1} bytes", filename, size);
        }
        ManifestFile GetFileWithModificationDateAndSize(string filename)
        {
            var fi = new FileInfo(filename);
            if (!fi.Exists)
                return new ManifestFile { Filename = filename, Size = "Missing", Modified = "Missing" };// String.Format("\"{0}\", last modified at {1}", filename, dt);
            DateTime dt = fi.LastWriteTime;
            long size = fi.Length;
            return new ManifestFile { Filename = filename, Size = size.ToString(), Modified = dt.ToBinary().ToString() };// String.Format("\"{0}\", last modified at {1}", filename, dt);
            //return String.Format("\"{0}\", last modified at {1}, sized {2} bytes", filename, dt, size);
        }

        void DeleteManifest()
        {
            string manifestFileName = Args.ManifestFile;
            FileInfo fi = new FileInfo(manifestFileName);
            if (fi.Exists)
            {
                fi.Attributes = FileAttributes.Normal;
                fi.Delete();
            }
        }

        public CompilerTool Compiler { get; set; }

        public List<string> ExternalFiles { get; set; }
    }
    static class Extensions
    {
        /// <summary>
        /// Concatenates string values that are selected from an IEnumerable (e.g CSV parameter list, with ( and ) )
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="stringSelector"></param>
        /// <param name="prefix"></param>
        /// <param name="delim"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string StringConcat<T>(this IEnumerable<T> list, Func<T, string> stringSelector, string prefix, string delim, string suffix)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(prefix))
                sb.Append(prefix);
            bool first = true, hasDelim = !String.IsNullOrEmpty(delim);
            if (list != null)
            {
                foreach (T item in list)
                {
                    if (hasDelim)
                    {
                        if (first)
                            first = false;
                        else
                            sb.Append(delim);
                    }
                    string s = stringSelector(item);
                    if (!String.IsNullOrEmpty(s))
                        sb.Append(s);
                }
            }
            if (!String.IsNullOrEmpty(suffix))
                sb.Append(suffix);
            return sb.ToString();
        }

        /// <summary>
        /// Concatenates an IEnumerable of strings
        /// </summary>
        /// <param name="list"></param>
        /// <param name="prefix"></param>
        /// <param name="delim"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string StringConcat(this IEnumerable<string> list, string prefix, string delim, string suffix)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(prefix))
                sb.Append(prefix);
            bool first = true, hasDelim = !String.IsNullOrEmpty(delim);
            foreach (string item in list)
            {
                if (String.IsNullOrEmpty(item))
                    continue;
                if (hasDelim)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(delim);
                }
                sb.Append(item);
            }
            if (!String.IsNullOrEmpty(suffix))
                sb.Append(suffix);
            return sb.ToString();
        }
        /// <summary>
        /// Concatenates an IEnumerable of strings
        /// </summary>
        /// <param name="list"></param>
        /// <param name="delim"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string StringConcat(this IEnumerable<string> list, string delim)
        {
            return StringConcat(list, null, delim, null);
        }
    }
}
