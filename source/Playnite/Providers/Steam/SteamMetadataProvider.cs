using Playnite.Metadata;
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

        public GameMetadata GetMetadata(string metadataId)
        {
            var gameData = new Game("SteamGame")
            {
                Provider = Provider.Steam,
                ProviderId = metadataId
            };

            var steamLib = new SteamLibrary(playniteServices);
            var data = steamLib.UpdateGameWithMetadata(gameData, settings);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        public GameMetadata GetMetadata(Game game)
        {
            if (game.Provider == Provider.Steam)
            {
                return GetMetadata(game.ProviderId);
            }

            throw new NotImplementedException();
        }

        public ICollection<MetadataSearchResult> SearchMetadata(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
