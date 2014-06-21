using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpKit.Release.Utils;

namespace SharpKit.Release
{

    class ReleaseLog
    {
        public static ReleaseLog Load(string filename)
        {
            return new XSerializer().DeserializeFromFile<ReleaseLog>(filename);
        }

        public void Save(string filename)
        {
            new XSerializer().SerializeToFile(this, filename);
        }
        public void Save()
        {
            if (Filename.IsNullOrEmpty())
                throw new Exception();
            Save(Filename);
        }
        public string Filename { get; set; }
        public string Version { get; set; }
        //public SolutionInfo SharpKit { get; set; }
        public SolutionInfo SharpKit_Sdk { get; set; }
        public SolutionInfo SharpKit5 { get; set; }
        public DateTime Created { get; set; }
    }

    class SolutionInfo
    {
        public List<VersionControlLogEntry> SvnLogEntries { get; set; }
        public string HeadRevision { get; set; }
    }

    class VersionControlLogEntry
    {
        public string revision { get; set; }
        public string author { get; set; }
        public DateTime date { get; set; }
        public string msg { get; set; }
    }
}
