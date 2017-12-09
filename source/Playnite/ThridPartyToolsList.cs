using Playnite.Providers.BattleNet;
using Playnite.Providers.EpicLauncher;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;
using Playnite.Providers.Uplay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class ThirdPartyTool
    {
        public string Name
        {
            get; set;
        }

        public string Path
        {
            get; set;
        }

        public string Arguments
        {
            get; set;
        }

        public string WorkDir
        {
            get; set;
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(WorkDir) && string.IsNullOrEmpty(Arguments))
            {
                Process.Start(Path);
            }
            else
            {
                
            }
        }
    }

    public class ThirdPartyToolsList
    {
        public RangeObservableCollection<ThirdPartyTool> Tools
        {
            get; private set;
        } = new RangeObservableCollection<ThirdPartyTool>();

        public void SetTools(IEnumerable<ThirdPartyTool> tools)
        {
            Tools = new RangeObservableCollection<ThirdPartyTool>();
            Tools.AddRange(tools);
        }

        public static List<ThirdPartyTool> GetDefaultInstalledTools()
        {
            var tools = new List<ThirdPartyTool>();

            if (SteamSettings.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = Path.Combine(SteamSettings.InstallationPath, "steam.exe"),
                    Name = "Steam"
                };

                tools.Add(tool);
            }

            if (OriginSettings.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = OriginSettings.ClientExecPath,
                    Name = "Origin"
                };

                tools.Add(tool);
            }

            if (GogSettings.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = Path.Combine(GogSettings.InstallationPath, "GalaxyClient.exe"),
                    Name = "GOG Galaxy"
                };

                tools.Add(tool);
            }

            if (BattleNetSettings.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = BattleNetSettings.ClientExecPath,
                    Name = "Battle.net"
                };

                tools.Add(tool);
            }

            if (UplaySettings.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = UplaySettings.ClientExecPath,
                    Name = "Uplay"
                };

                tools.Add(tool);
            }

            if (EpicLauncherSettings.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = EpicLauncherSettings.ClientExecPath,
                    Name = "Epic Games Launcher"
                };

                tools.Add(tool);
            }

            return tools;
        }
    }
}
