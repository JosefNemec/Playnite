using Playnite.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Models;

namespace Playnite.Providers.GOG
{
    public class GogMetadataProvider : IMetadataProvider
    {
        public GameMetadata GetMetadata(string metadataId)
        {
            var gameData = new Game("GOGGame")
            {
                Provider = Provider.GOG,
                ProviderId = metadataId
            };

            var gogLib = new GogLibrary();
            var data = gogLib.UpdateGameWithMetadata(gameData);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        public GameMetadata GetMetadata(Game game)
        {
            if (game.Provider == Provider.GOG)
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
