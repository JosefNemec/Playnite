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
        public GameMetadata GetMetadata(Game game)
        {
            var product = BattleNetGames.GetAppDefinition(game.GameId);
            if (product == null)
            {
                return null;
            }

            var gameInfo = new GameInfo
            {
                Name = product.Name,
                Links = new List<Link>(product.Links)
            };

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo
            };

            if (!string.IsNullOrEmpty(product.IconUrl))
            {
                metadata.Icon = new MetadataFile(
                    Path.GetFileName(product.IconUrl),
                    HttpDownloader.DownloadData(product.IconUrl),
                    product.IconUrl);
            }

            if (!string.IsNullOrEmpty(product.CoverUrl))
            {
                metadata.CoverImage = new MetadataFile(
                    Path.GetFileName(product.CoverUrl),
                    HttpDownloader.DownloadData(product.CoverUrl),
                    product.CoverUrl);
            }

            if (!string.IsNullOrEmpty(product.BackgroundUrl))
            {
                metadata.BackgroundImage = new MetadataFile(product.BackgroundUrl);
            }

            return metadata;
        }
    }
}
