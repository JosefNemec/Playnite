using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Toolbox
{
    public class FileChange
    {
        public string ChangeType { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return $"{ChangeType}: {Path}";
        }
    }

    public class Themes
    {
        public const string ThemeSlnName = "Theme.sln";
        public const string ThemeProjName = "Theme.csproj";
        public const string AppXamlName = "App.xaml";
        public const string LocSourceName = "LocSource.xaml";
        public const string GlobalResourcesName = "GlobalResources.xaml";
        
        public static List<FileChange> GetThemeChangelog(Version baseVersion, ApplicationMode mode, string changelogDir)
        {
            if (!Directory.Exists(changelogDir))
            {
                throw new FileNotFoundException("Theme changelog not found.");
            }

            var changes = new List<FileChange>();
            foreach (var changeFile in Directory.GetFiles(changelogDir, "*.txt").OrderBy(a => a))
            {
                var match = Regex.Match(Path.GetFileName(changeFile), @"(.+)-(.+)\.txt");
                if (!match.Success)
                {
                    continue;
                }

                var oldVersion = new Version(match.Groups[1].Value);
                var newVersion = new Version(match.Groups[2].Value);
                if (oldVersion < baseVersion)
                {
                    continue;
                }

                foreach (var line in File.ReadAllLines(changeFile))
                {
                    var lineMatch = Regex.Match(line, $"([A-Z])\\s(.+{ThemeManager.GetThemeRootDir(mode)}\\/Default.+)");
                    if (!lineMatch.Success)
                    {
                        continue;
                    }

                    var changeType = lineMatch.Groups[1].Value;
                    var changePath = lineMatch.Groups[2].Value;

                    var exist = changes.FirstOrDefault(a => a.Path == changePath);
                    if (exist != null)
                    {
                        changes.Remove(exist);
                    }

                    changes.Add(new FileChange
                    {
                        ChangeType = changeType,
                        Path = changePath
                    });
                }
            }

            return changes;
        }

        public static List<FileChange> GetThemeChangelog(Version baseVersion, ApplicationMode mode)
        {
            return GetThemeChangelog(baseVersion, mode, Paths.ChangeLogsDir);
        }
    }
}
