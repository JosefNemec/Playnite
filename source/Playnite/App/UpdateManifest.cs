using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class UpdateManifest
    {
        public class ReleaseNote
        {
            public Version Version
            {
                get; set;
            }

            public string FileName
            {
                get; set;
            }
        }

        public class Package
        {
            public Version BaseVersion
            {
                get; set;
            }

            public string FileName
            {
                get; set;
            }

            public string Checksum
            {
                get; set;
            }
        }

        public Version LatestVersion
        {
            get; set;
        }

        public List<string> DownloadServers
        {
            get; set;
        }

        public List<string> ReleaseNotesUrlRoots
        {
            get; set;
        }

        public List<Package> Packages
        {
            get; set;
        }

        public List<ReleaseNote> ReleaseNotes
        {
            get; set;
        }
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
