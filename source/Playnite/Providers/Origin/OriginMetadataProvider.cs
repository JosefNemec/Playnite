using Playnite.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Models;

namespace Playnite.Providers.Origin
{
    public class OriginMetadataProvider : IMetadataProvider
    {
        public GameMetadata GetMetadata(string metadataId)
        {
            var gameData = new Game("OriginGame")
            {
                Provider = Provider.Origin,
                ProviderId = metadataId
            };

            var originLib = new OriginLibrary();
            var data = originLib.UpdateGameWithMetadata(gameData);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        public GameMetadata GetMetadata(Game game)
        {
            if (game.Provider == Provider.Origin)
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
