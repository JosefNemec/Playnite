using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite
{
    public class FakePlayniteLibraryPlugin : ILibraryPlugin
    {
        public ILibraryClient Client => null;

        public string LibraryIcon => string.Empty;

        public string Name => "Playnite";

        public bool IsClientInstalled => false;

        public Guid Id => Guid.Empty;

        public void Dispose()
        {            
        }

        public IGameController GetGameController(Game game)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GameInfo> GetGames()
        {
            throw new NotImplementedException();
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            throw new NotImplementedException();
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            throw new NotImplementedException();
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            throw new NotImplementedException();
        }
    }
}
