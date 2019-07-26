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
        Theme,
        Script,
        Plugin
    }

    [Verb("new")]
    public class NewCmdLineOptions
    {
        [Value(0, Required = true)]
        public ItemType Type { get; set; }
        [Value(1, Required = true)]
        public string TargetType { get; set; }
        [Value(2, Required = true)]
        public string Name { get; set; }
    }

    [Verb("pack")]
    public class PackCmdLineOptions
    {
        [Value(0, Required = true)]
        public ItemType Type { get; set; }
        [Value(1, Required = true)]
        public string TargetType { get; set; }
        [Value(2, Required = true)]
        public string Name { get; set; }
        [Value(2, Required = true)]
        public string DestinationPath { get; set; }
    }
}
