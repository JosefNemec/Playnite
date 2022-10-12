using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Toolbox
{
    public enum ItemType
    {
        Uknown,
        DesktopTheme,
        FullscreenTheme,
        PowerShellScript,
        GenericPlugin,
        MetadataPlugin,
        LibraryPlugin
    }

    public enum ManifestType
    {
        Installer,
        Addon
    }

    [Verb("new", HelpText = "Generate new add-on from template.")]
    public class NewCmdLineOptions
    {
        [Value(0, Required = true, HelpText = "Add-on type.")]
        public ItemType Type { get; set; }
        [Value(1, Required = true, HelpText = "Add-on name.")]
        public string Name { get; set; }
        [Value(2, Required = false, HelpText = "Output directory (for extensions only).")]
        public string OutDirectory { get; set; }
    }

    [Verb("pack", HelpText = "Pack existing add-on.")]
    public class PackCmdLineOptions
    {
        [Value(0, Required = true, HelpText = "Add-on directory to pack.")]
        public string Directory { get; set; }
        [Value(1, Required = true, HelpText = "Destination directory for packaged file to be saved to.")]
        public string Destination { get; set; }
    }

    [Verb("update", HelpText = "Update theme to newer version.")]
    public class UpdateCmdLineOptions
    {
        [Value(0, Required = true, HelpText = "Theme directory to update.")]
        public string Directory { get; set; }
    }

    [Verb("verify", HelpText = "Verify addon manifest.")]
    public class VerifyManifestOptions
    {
        [Value(0, Required = true, HelpText = "Manifest type.")]
        public ManifestType Type { get; set; }
        [Value(1, Required = true, HelpText = "Full manifest file path.")]
        public string ManifestPath { get; set;}
    }
}