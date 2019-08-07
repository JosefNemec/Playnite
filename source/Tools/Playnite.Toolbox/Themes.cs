using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Toolbox
{
    public class Themes
    {
        public const string ThemeSlnName = "Theme.sln";
        public const string ThemeProjName = "Theme.csproj";
        public const string AppXamlName = "App.xaml";
        public const string LocSourceName = "LocSource.xaml";
        public const string GlobalResourcesName = "GlobalResources.xaml";
        public static string ThemeChangelogPath => Path.Combine(PlaynitePaths.ProgramPath, "Templates", "Themes", "ThemeChangeLog.txt");

        public static Dictionary<Version, List<string>> GetThemeChangelog(string changelogPath)
        {
            if (!File.Exists(changelogPath))
            {
                throw new FileNotFoundException("Theme changelog not found.");
            }

            var changes = new Dictionary<Version, List<string>>();
            List<string> currentDiff = null;
            Version currentVer = null;
            foreach (var line in File.ReadLines(changelogPath))
            {
                if (line.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (Version.TryParse(line, out var version))
                {
                    if (currentVer != null)
                    {
                        changes.Add(currentVer, currentDiff);
                    }

                    currentVer = version;
                    currentDiff = new List<string>();
                    continue;
                }
                else
                {
                    if (currentDiff != null)
                    {
                        currentDiff.Add(Regex.Replace(line, @"source/Playnite\.(Desktop|Fullscreen)App/Themes/", string.Empty, RegexOptions.IgnoreCase));
                    }
                }
            }

            if (currentVer != null)
            {
                changes.Add(currentVer, currentDiff);
            }

            return changes;
        }

        public static Dictionary<Version, List<string>> GetThemeChangelog()
        {
            return GetThemeChangelog(ThemeChangelogPath);
        }
    }
}
