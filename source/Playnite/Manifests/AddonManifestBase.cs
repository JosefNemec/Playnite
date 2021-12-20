using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public enum AddonType
    {
        GameLibrary,
        MetadataProvider,
        Generic,
        ThemeDesktop,
        ThemeFullscreen
    }

    public class AddonManifestBase : ObservableObject
    {
        public class AddonUserAgreement
        {
            public DateTime Updated { get; set; }
            public string AgreementUrl { get; set; }
        }

        public class AddonScreenshot
        {
            public string Thumbnail { get; set; }
            public string Image { get; set; }
        }

        public string IconUrl { get; set; }
        public List<AddonScreenshot> Screenshots { get; set; }
        public AddonType Type { get; set; }
        public string InstallerManifestUrl { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string AddonId { get; set; }
        public string Author { get; set; }
        public Dictionary<string, string> Links { get; set; }
        public List<string> Tags { get; set; }
        public AddonUserAgreement UserAgreement { get; set; }
        public string SourceUrl { get; set; }
    }

    public class AddonInstallerPackage
    {
        public Version Version { get; set; }
        public string PackageUrl { get; set; }
        public Version RequiredApiVersion { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Changelog { get; set; }
    }

    public class AddonInstallerManifestBase
    {
        public string AddonId { get; set; }
        public List<AddonInstallerPackage> Packages { get; set; }
    }
}
