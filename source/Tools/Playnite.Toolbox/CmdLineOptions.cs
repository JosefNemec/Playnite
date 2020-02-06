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
        IronPythonScript,
        GenericPlugin,
        MetadataPlugin,
        LibraryPlugin
    }

    [Verb("new")]
    public class NewCmdLineOptions
    {
        [Value(0, Required = true)]
        public ItemType Type { get; set; }
        [Value(1, Required = true)]
        public string Name { get; set; }
        [Value(2, Required = false)]
        public string OutDirectory { get; set; }
    }

    [Verb("pack")]
    public class PackCmdLineOptions
    {
        [Value(0, Required = true)]
        public string Directory { get; set; }
        [Value(1, Required = true)]
        public string Destination { get; set; }
    }

    [Verb("update")]
    public class UpdateCmdLineOptions
    {
        [Value(0, Required = true)]
        public string Directory { get; set; }
    }
}