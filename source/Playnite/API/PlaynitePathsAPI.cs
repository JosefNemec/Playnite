using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class PlaynitePathsAPI : IPlaynitePathsAPI
    {
        public bool IsPortable { get => Settings.IsPortable; }

        public string ApplicationPath { get => Paths.ProgramPath; }

        public string ConfigurationPath { get => Paths.ConfigRootPath; }
    }
}
