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

namespace UplayLibrary
{
    public class UplayMetadataProvider : ILibraryMetadataProvider
    {
        public GameMetadata GetMetadata(Game game)
        {
            var program = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.RegistryKeyName == "Uplay Install " + game.GameId);
            if (program == null)
            {
                return null;
            }

            var gameInfo = new GameInfo
            {
                Name = StringExtensions.NormalizeGameName(program.DisplayName)
            };

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo
            };

            if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
            {
                var iconPath = program.DisplayIcon;
                var iconFile = Path.GetFileName(iconPath);
                var data = File.ReadAllBytes(iconPath);
                metadata.Icon = new MetadataFile(iconFile, data);
            }

            return metadata;
        }
    }
}
