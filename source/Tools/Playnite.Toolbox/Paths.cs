using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Toolbox
{
    public class Paths
    {
        public static string GetThemesPath(ApplicationMode mode)
        {
            return Path.Combine(PlaynitePaths.ThemesProgramPath, ThemeManager.GetThemeRootDir(mode));
        }

        public static string GetThemeTemplateDir(ApplicationMode mode)
        {
            return Path.Combine(PlaynitePaths.ProgramPath, "Templates", "Themes", ThemeManager.GetThemeRootDir(mode));
        }

        public static string GetThemeTemplateFilePath(ApplicationMode mode, string fileName)
        {
            return Path.Combine(PlaynitePaths.ProgramPath, "Templates", "Themes", ThemeManager.GetThemeRootDir(mode), fileName);
        }

        public static string GetThemeTemplatePath(string fileName)
        {
            return Path.Combine(PlaynitePaths.ProgramPath, "Templates", "Themes", fileName);
        }
    }
}
