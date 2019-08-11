using Playnite.SDK;
using Playnite.SDK.Plugins;
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
        public LibraryClient Client { get; set; }

        public string Name { get; set; }

        public void Start()
        {
            Client.Open();
        }
    }

    public class ThirdPartyToolsList
    {
        public static List<ThirdPartyTool> GetTools(IEnumerable<LibraryPlugin> plugins)
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

            return tools;
        }
    }
}
