using Playnite.MetaProviders;
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
        public GameMetadata GetGameData(string gameId)
        {
            var gameData = new Game("GOGGame")
            {
                Provider = Provider.Steam,
                ProviderId = gameId
            };

            var steamLib = new GogLibrary();
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
