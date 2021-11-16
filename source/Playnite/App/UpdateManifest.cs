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

        private Version version;
        private Version sdkVersion;
        private Version fullscreenThemeVersion;
        private Version desktopThemeVersion;
        private string checksum;
        private List<string> packageUrls;
        private List<Version> versionHistory;

        public Version Version                  { get => version; set => SetValue(ref version, value); }
        public Version SdkVersion               { get => sdkVersion; set => SetValue(ref sdkVersion, value); }
        public Version FullscreenThemeVersion   { get => fullscreenThemeVersion; set => SetValue(ref fullscreenThemeVersion, value); }
        public Version DesktopThemeVersion      { get => desktopThemeVersion; set => SetValue(ref desktopThemeVersion, value); }
        public string Checksum                  { get => checksum; set => SetValue(ref checksum, value); }
        public List<string> PackageUrls         { get => packageUrls; set => SetValue(ref packageUrls, value); }
        public List<Version> VersionHistory     { get => versionHistory; set => SetValue(ref versionHistory, value); }
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
