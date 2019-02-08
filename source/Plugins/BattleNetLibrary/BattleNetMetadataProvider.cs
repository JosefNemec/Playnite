using Playnite;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary
{
    public class BattleNetMetadataProvider : ILibraryMetadataProvider
    {
        public BattleNetMetadataProvider()
        {
        }

        #region IMetadataProvider

        public GameMetadata GetMetadata(Game game)
        {
            var gameData = new Game("BattleNetGame")
            {
                GameId = game.GameId
            };

            var data = UpdateGameWithMetadata(gameData);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        #endregion IMetadataProvider

        public GameMetadata UpdateGameWithMetadata(Game game)
        {
            var metadata = new GameMetadata();
            var product = BattleNetGames.GetAppDefinition(game.GameId);
            if (product == null)
            {
                return metadata;
            }

            if (string.IsNullOrEmpty(product.IconUrl))
            {
                return metadata;
            }

            game.Name = product.Name;
            var icon = HttpDownloader.DownloadData(product.IconUrl);
            var iconFile = Path.GetFileName(product.IconUrl);
            metadata.Icon = new MetadataFile(iconFile, icon);
            var cover = HttpDownloader.DownloadData(product.CoverUrl);
            var coverFile = Path.GetFileName(product.CoverUrl);
            metadata.Image = new MetadataFile(coverFile, cover);
            game.BackgroundImage = product.BackgroundUrl;
            metadata.BackgroundImage = product.BackgroundUrl;
            game.Links = new ObservableCollection<Link>(product.Links);
            return metadata;
        }
    }
}
