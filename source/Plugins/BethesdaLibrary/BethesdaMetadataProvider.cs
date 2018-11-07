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

namespace BethesdaLibrary
{
    public class BethesdaMetadataProvider : ILibraryMetadataProvider
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
            var program = Bethesda.GetBethesdaInstallEntried().FirstOrDefault(a => a.UninstallString?.Contains($"uninstall/{game.GameId}") == true);
            if (program == null)
            {
                return metadata;
            }

            if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
            {
                var iconPath = program.DisplayIcon;
                var iconFile = Path.GetFileName(iconPath);
                var data = File.ReadAllBytes(iconPath);
                metadata.Icon = new MetadataFile(iconFile, data);
            }

            game.Name = StringExtensions.NormalizeGameName(program.DisplayName);
            return metadata;
        }
    }
}
