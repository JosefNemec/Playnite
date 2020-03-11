using Playnite.Common.Media.Icons;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLibrary.Models;

namespace AmazonGamesLibrary
{
    public class AmazonGamesMetadataProvider : LibraryMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger();
        private AmazonGamesLibrary library;

        public AmazonGamesMetadataProvider(AmazonGamesLibrary library)
        {
            this.library = library;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var gameInfo = new GameInfo
            {
                Links = new List<Link>()
            };

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo
            };

            gameInfo.Links.Add(new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + game.Name));

            var program = AmazonGames.GetUninstallRecord(game.GameId);
            if (program != null)
            {
                gameInfo.Name = StringExtensions.NormalizeGameName(program.DisplayName);
                if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
                {
                    var iconPath = program.DisplayIcon;
                    if (iconPath.EndsWith("ico", StringComparison.OrdinalIgnoreCase))
                    {
                        metadata.Icon = new MetadataFile(program.DisplayIcon);
                    }
                    else
                    {
                        using (var ms = new MemoryStream())
                        {
                            if (IconExtractor.ExtractMainIconFromFile(iconPath, ms))
                            {
                                var iconName = Guid.NewGuid() + ".ico";
                                metadata.Icon = new MetadataFile(iconName, ms.ToArray());
                            }
                        }
                    }
                }
            }

            return metadata;
        }
    }
}
