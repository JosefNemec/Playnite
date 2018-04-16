using Playnite.MetaProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Models;
using Playnite.SDK.Models;

namespace Playnite.Providers.BattleNet
{
    public class BattleNetMetadataProvider : IMetadataProvider
    {
        public GameMetadata GetGameData(string gameId)
        {
            var gameData = new Game("BattleNetGame")
            {
                Provider = Provider.Steam,
                ProviderId = gameId
            };

            var steamLib = new BattleNetLibrary();
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
