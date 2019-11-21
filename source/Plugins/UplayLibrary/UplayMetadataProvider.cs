using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplayLibrary
{
    public class UplayMetadataProvider : LibraryMetadataProvider
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly List<Models.ProductInformation> productInfo;

        public UplayMetadataProvider()
        {
            try
            {
                productInfo = Uplay.GetLocalProductCache();
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get Uplay product info from cache.");
            }
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var gameInfo = new GameInfo
            {
                Links = new List<Link>()
            };

            gameInfo.Links.Add(new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + gameInfo.Name));
            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo
            };

            var prod = productInfo?.FirstOrDefault(a => a.uplay_id.ToString() == game.GameId);
            if (prod != null)
            {
                if (!prod.root.icon_image.IsNullOrEmpty())
                {
                    metadata.Icon = new MetadataFile(prod.root.icon_image);
                }

                if (!prod.root.thumb_image.IsNullOrEmpty())
                {
                    metadata.CoverImage = new MetadataFile(prod.root.thumb_image);
                }

                if (!prod.root.background_image.IsNullOrEmpty())
                {
                    metadata.BackgroundImage = new MetadataFile(prod.root.background_image);
                }
            }
            else
            {
                var program = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.RegistryKeyName == "Uplay Install " + game.GameId);
                if (program != null)
                {
                    if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
                    {
                        metadata.Icon = new MetadataFile(program.DisplayIcon);
                    }
                }
            }

            return metadata;
        }
    }
}
