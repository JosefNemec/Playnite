using Playnite.Models;
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
        public static string ResolveVariables(this IGame game, string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }

            var result = inputString;
            result = result.Replace("{InstallDir}", game.InstallDirectory);
            result = result.Replace("{ImagePath}", game.IsoPath);
            result = result.Replace("{ImageNameNoExt}", Path.GetFileNameWithoutExtension(game.IsoPath));
            result = result.Replace("{ImageName}", Path.GetFileName(game.IsoPath));
            result = result.Replace("{PlayniteDir}", Paths.ProgramFolder);
            return result;
        }
    }
}
