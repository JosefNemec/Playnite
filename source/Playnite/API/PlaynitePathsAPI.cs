using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class PlaynitePathsAPI : IPlaynitePathsAPI
    {
        public bool IsPortable { get => PlayniteSettings.IsPortable; }

        public string ApplicationPath { get => PlaynitePaths.ProgramPath; }

        public string ConfigurationPath { get => PlaynitePaths.ConfigRootPath; }

        public string ExtensionsDataPath { get => PlaynitePaths.ExtensionsDataPath; }
    }
}
