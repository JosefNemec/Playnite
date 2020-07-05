using Playnite.Common;
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

        public System.Windows.Controls.Image Icon { get; set; }

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
                foreach (var plugin in plugins.OrderBy(a => a.Name))
                {
                    if (plugin.Client != null && plugin.Client.IsInstalled)
                    {
                        var tool = new ThirdPartyTool()
                        {
                            Client = plugin.Client,
                            Name = plugin.Name
                        };

                        if (plugin.Client?.Icon != null && File.Exists(plugin.Client.Icon))
                        {
                            tool.Icon = Images.GetImageFromFile(
                                plugin.Client.Icon,
                                System.Windows.Media.BitmapScalingMode.Fant,
                                double.NaN,
                                double.NaN);
                        }

                        tools.Add(tool);
                    }
                }
            }

            return tools;
        }
    }
}
