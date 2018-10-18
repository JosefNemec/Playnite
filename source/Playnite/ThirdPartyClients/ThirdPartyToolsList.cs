using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.ThirdPartyClients;
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
        public ILibraryClient Client { get; set; }

        public string Name { get; set; }

        public void Start()
        {
            Client.Open();
        }
    }

    public class ThirdPartyToolsList
    {
        public static List<ThirdPartyTool> GetTools(IEnumerable<ILibraryPlugin> plugins)
        {
            var tools = new List<ThirdPartyTool>();
            if (plugins?.Any() == true)
            {
                foreach (var plugin in plugins)
                {
                    if (plugin.Client != null && plugin.Client.IsInstalled)
                    {
                        tools.Add(new ThirdPartyTool()
                        {
                            Client = plugin.Client,
                            Name = plugin.Name
                        });
                    }
                }
            }

            var epic = new EpicLauncherClient();
            if (epic.IsInstalled)
            {
                tools.Add(new ThirdPartyTool()
                {
                    Client = epic,
                    Name = "Epic Launcher"
                });
            }

            return tools;
        }
    }
}
