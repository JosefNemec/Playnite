using Playnite.MetaProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;
using Playnite.Providers.Steam;
using Playnite.Services;
using Playnite.Models;

namespace Playnite.Providers.Steam
{
    public class SteamMetadataProvider : IMetadataProvider
    {
        private ServicesClient playniteServices;
        private SteamSettings settings;

        public SteamMetadataProvider()
        {
            settings = new SteamSettings();
        }

        public SteamMetadataProvider(ServicesClient playniteServices, SteamSettings settings)
        {
            this.settings = settings;
            this.playniteServices = playniteServices;
        }

        public GameMetadata GetGameData(string gameId)
        {
            var gameData = new Game("SteamGame")
            {
                Provider = Provider.Steam,
                ProviderId = gameId
            };

            var steamLib = new SteamLibrary(playniteServices);
            var data = steamLib.UpdateGameWithMetadata(gameData, settings);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        public bool GetSupportsIdSearch()
        {
            return true;
        }

        public List<MetadataSearchResult> SearchGames(string gameName)
        {
            throw new NotSupportedException();
        }
    }
}
