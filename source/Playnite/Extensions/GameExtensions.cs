using Playnite.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class GameExtensions
    {
        public static string ExpandVariables(this Game game, string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }

            var result = inputString;
            result = result.Replace("{InstallDir}", game.InstallDirectory);
            result = result.Replace("{InstallDirName}", Path.GetFileName(Path.GetDirectoryName(game.InstallDirectory)));
            result = result.Replace("{ImagePath}", game.IsoPath);
            result = result.Replace("{ImageNameNoExt}", Path.GetFileNameWithoutExtension(game.IsoPath));
            result = result.Replace("{ImageName}", Path.GetFileName(game.IsoPath));
            result = result.Replace("{PlayniteDir}", Paths.ProgramPath);
            result = result.Replace("{Name}", game.Name);
            return result;
        }

        public static string GetIdentifierInfo(this Game game)
        {
            return $"{game.Name}, {game.Id}, {game.GameId}, {game.PluginId}";
        }
    }
}
