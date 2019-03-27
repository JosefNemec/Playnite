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
    public class UplayMetadataProvider : ILibraryMetadataProvider
    {
        public void Dispose()
        {
        }

        public GameMetadata GetMetadata(Game game)
        {
            var program = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.RegistryKeyName == "Uplay Install " + game.GameId);
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
                metadata.Icon = new MetadataFile(program.DisplayIcon);
            }

            return metadata;
        }
    }
}
