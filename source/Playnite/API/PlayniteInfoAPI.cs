using Playnite.App;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class PlayniteInfoAPI : IPlayniteInfoAPI
    {
        public System.Version ApplicationVersion { get => Updater.GetCurrentVersion(); }
    }
}
