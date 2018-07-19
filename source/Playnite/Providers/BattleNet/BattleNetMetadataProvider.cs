//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Playnite.Models;
//using Playnite.SDK.Models;
//using Playnite.Metadata;

//namespace Playnite.Providers.BattleNet
//{
//    public class BattleNetMetadataProvider : IMetadataProvider
//    {
//        public GameMetadata GetMetadata(string metadataId)
//        {
//            var gameData = new Game("BattleNetGame")
//            {
//                Provider = Provider.BattleNet,
//                GameId = metadataId
//            };

//            var bnetLib = new BattleNetLibrary();
//            var data = bnetLib.UpdateGameWithMetadata(gameData);
//            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
//        }

//        public GameMetadata GetMetadata(Game game)
//        {
//            if (game.Provider == Provider.BattleNet)
//            {
//                return GetMetadata(game.GameId);
//            }

//            throw new NotImplementedException();
//        }

//        public ICollection<MetadataSearchResult> SearchMetadata(Game game)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
