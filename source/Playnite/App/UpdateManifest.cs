using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class UpdateManifest : ObservableObject
    {
        public const string ServerManifestFileName = "update.json";

        public Version Version { get; set; }
        public Version SdkVersion { get; set; }
        public Version FullscreenThemeVersion { get; set; }
        public Version DesktopThemeVersion { get; set; }
        public string Sha256 { get; set; }
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
