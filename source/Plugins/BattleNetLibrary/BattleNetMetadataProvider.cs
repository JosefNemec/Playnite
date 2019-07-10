using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary
{
    public class BattleNetMetadataProvider : LibraryMetadataProvider
    {
        public override GameMetadata GetMetadata(Game game)
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

            gameInfo.Links.Add(new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + product.Name));

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo
            };

            if (!string.IsNullOrEmpty(product.IconUrl))
            {
                metadata.Icon = new MetadataFile(product.IconUrl);
            }

            if (!string.IsNullOrEmpty(product.CoverUrl))
            {
                metadata.CoverImage = new MetadataFile(product.CoverUrl);
            }

            if (!string.IsNullOrEmpty(product.BackgroundUrl))
            {
                metadata.BackgroundImage = new MetadataFile(product.BackgroundUrl);
            }

            return metadata;
        }
    }
}
