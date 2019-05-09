using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class CmdLineOptions
    {
        [Option("start")]
        public string Start { get; set; }

        [Option("nolibupdate")]
        public bool SkipLibUpdate { get; set; }

        public override string ToString()
        {
            return Parser.Default.FormatCommandLine(this);
        }
    }
}
