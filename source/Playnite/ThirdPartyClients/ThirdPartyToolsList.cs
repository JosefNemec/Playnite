using Playnite.Providers.EpicLauncher;
using Playnite.SDK;
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

            if (SteamLibrary.Steam.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = Path.Combine(SteamLibrary.Steam.InstallationPath, "steam.exe"),
                    Name = "Steam"
                };

                tools.Add(tool);
            }

            if (GogLibrary.Gog.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = Path.Combine(GogLibrary.Gog.InstallationPath, "steam.exe"),
                    Name = "GOG Galaxy"
                };

                tools.Add(tool);
            }

            if (OriginLibrary.Origin.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = OriginLibrary.Origin.ClientExecPath,
                    Name = "Origin"
                };

                tools.Add(tool);
            }

            if (BattleNetLibrary.BattleNet.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = BattleNetLibrary.BattleNet.ClientExecPath,
                    Name = "Battle.net"
                };

                tools.Add(tool);
            }

            if (UplayLibrary.Uplay.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = UplayLibrary.Uplay.ClientExecPath,
                    Name = "Uplay.net"
                };

                tools.Add(tool);
            }

            if (EpicLauncher.IsInstalled)
            {
                var tool = new ThirdPartyTool()
                {
                    Path = EpicLauncher.ClientExecPath,
                    Name = "Epic Games Launcher"
                };

                tools.Add(tool);
            }

            return tools;
        }
    }
}
