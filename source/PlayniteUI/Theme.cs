using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Models;

namespace PlayniteUI
{
    public static class Theme
    {
        public static readonly Dictionary<Provider, string> BackgroundColors = new Dictionary<Provider, string>()
        {
            { Provider.Steam, "#1B2838" },
            { Provider.GOG, "#CCCCCC"},
            { Provider.Custom, "#1B2838"},
            { Provider.Origin, "#1B2838"}
        };
    }
}
