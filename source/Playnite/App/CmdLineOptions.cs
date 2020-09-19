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

        [Option("uridata")]
        public string UriData { get; set; }

        [Option("nolibupdate")]
        public bool SkipLibUpdate { get; set; }

        [Option("startdesktop")]
        public bool StartInDesktop { get; set; }

        [Option("startfullscreen")]
        public bool StartInFullscreen { get; set; }

        [Option("forcesoftrender")]
        public bool ForceSoftwareRender { get; set; }

        [Option("forcedefaulttheme")]
        public bool ForceDefaultTheme { get; set; }

        [Option("hidesplashscreen")]
        public bool HideSplashScreen { get; set; }

        [Option("installext")]
        public string InstallExtension { get; set; }

        [Option("clearwebcache")]
        public bool ClearWebCache { get; set; }

        [Option("shutdown")]
        public bool Shutdown { get; set; }

        [Option("safestartup")]
        public bool SafeStartup { get; set; }

        public override string ToString()
        {
            return Parser.Default.FormatCommandLine(this);
        }
    }
}
