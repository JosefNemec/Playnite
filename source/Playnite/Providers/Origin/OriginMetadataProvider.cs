using Playnite.MetaProviders;
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
        public GameMetadata GetGameData(string gameId)
        {
            var gameData = new Game("OriginGame")
            {
                Provider = Provider.Steam,
                ProviderId = gameId
            };

            var steamLib = new OriginLibrary();
            var data = steamLib.UpdateGameWithMetadata(gameData);
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
