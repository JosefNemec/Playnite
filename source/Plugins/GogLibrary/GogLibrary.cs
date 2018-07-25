using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GogLibrary
{
    public class GogLibrary : ILibraryPlugin
    {
        private ILogger logger;
        private readonly IPlayniteAPI playniteApi;

        public GogLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            logger = playniteApi.CreateLogger("GogLibrary");
            var configPath = Path.Combine(api.GetPluginStoragePath(this), "config.json");
            Settings = new GogLibrarySettings(configPath);
        }

        #region ILibraryPlugin

        public UserControl SettingsView
        {
            get => new GogLibrarySettingsView();
        }

        public IEditableObject Settings { get; private set; }

        public string Name { get; } = "GOG";

        public Guid Id { get; } = Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");

        public void Dispose()
        {

        }

        public IGameController GetGameController(Game game)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Game> GetGames()
        {
            throw new NotImplementedException();
        }

        public IMetadataProvider GetMetadataDownloader()
        {
            throw new NotImplementedException();
        }

        #endregion ILibraryPlugin

    }
}
