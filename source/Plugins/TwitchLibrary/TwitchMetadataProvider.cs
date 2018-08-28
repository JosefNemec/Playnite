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
        #region IMetadataProvider
        
        public GameMetadata GetMetadata(Game game)
        {
            var gameData = game.CloneJson();
            var data = UpdateGameWithMetadata(gameData);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        #endregion IMetadataProvider

        public GameMetadata UpdateGameWithMetadata(Game game)
        {
            var metadata = new GameMetadata();
            var program = Twitch.GetUninstallRecord(game.GameId);
            if (program == null)
            {
                return metadata;
            }

            if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
            {
                var iconPath = program.DisplayIcon;
                var iconFile = Path.GetFileName(iconPath);
                if (iconPath.EndsWith("ico", StringComparison.InvariantCultureIgnoreCase))
                {
                    var data = File.ReadAllBytes(iconPath);
                    metadata.Icon = new MetadataFile($"images/twitch/{game.GameId}/{iconFile}", iconFile, data);
                }
                else
                {
                    var exeIcon = IconExtension.ExtractIconFromExe(iconPath, true);
                    if (exeIcon != null)
                    {
                        var iconName = Guid.NewGuid() + ".png";
                        metadata.Icon = new MetadataFile($"images/twitch/{game.GameId}/{iconFile}", iconName, exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));                       
                    }
                }
            }

            game.Name = StringExtensions.NormalizeGameName(program.DisplayName);
            return metadata;
        }
    }
}
