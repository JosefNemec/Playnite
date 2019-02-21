using Playnite;
using Playnite.Common.System;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary
{
    public class TwitchMetadataProvider : ILibraryMetadataProvider
    {
        public void Dispose()
        {
        }

        public GameMetadata GetMetadata(Game game)
        {
            var program = Twitch.GetUninstallRecord(game.GameId);
            if (program == null)
            {
                return null;
            }

            var gameInfo = new GameInfo
            {
                Name = StringExtensions.NormalizeGameName(program.DisplayName),
                Links = new List<Link>()
            };

            gameInfo.Links.Add(new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + gameInfo.Name));
            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo
            };

            if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
            {
                var iconPath = program.DisplayIcon;
                if (iconPath.EndsWith("ico", StringComparison.OrdinalIgnoreCase))
                {
                    metadata.Icon = new MetadataFile(program.DisplayIcon);
                }
                else
                {
                    var exeIcon = IconExtension.ExtractIconFromExe(iconPath, true);
                    if (exeIcon != null)
                    {
                        var iconName = Guid.NewGuid() + ".png";
                        metadata.Icon = new MetadataFile(iconName, exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));
                    }
                }
            }

            return metadata;
        }
    }
}
