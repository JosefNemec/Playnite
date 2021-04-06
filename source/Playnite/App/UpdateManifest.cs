using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class UpdateManifest
    {
        public Version Version { get; set; }
        public string Checksum { get; set; }
        public List<string> PackageUrls { get; set; }
        public List<Version> VersionHistory { get; set; }
    }

    public class ReleaseNoteData
    {
        public Version Version
        {
            get; set;
        }

        public string Note
        {
            get; set;
        }
    }
}
