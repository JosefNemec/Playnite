using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite;
using System.Windows;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using Playnite.Settings;
using Playnite.Common;
using Playnite.SDK;

namespace Playnite
{
    public class ThemeDescription
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string DirectoryPath { get; set; }
        public string DirectoryName { get; set; }

        public static ThemeDescription FromFile(string path)
        {
            var theme = Serialization.FromYaml<ThemeDescription>(File.ReadAllText(path));
            theme.DirectoryPath = Path.GetDirectoryName(path);
            theme.DirectoryName = Path.GetFileNameWithoutExtension(theme.DirectoryPath);
            return theme;
        }
    }

    public class ThemeManager
    {
        private static ILogger logger = LogManager.GetLogger();
        private const string themeManifestFileName = "theme.yaml";
        public static ThemeDescription CurrentTheme { get; private set; }
        public static ThemeDescription DefaultTheme { get; private set; }

        private static object LoadXaml(string path)
        {
            using (var stream = new StreamReader(path))
            {
                return XamlReader.Load(stream.BaseStream);
            }
        }

        public static string GetThemeRootDir(ApplicationMode mode)
        {
            return mode == ApplicationMode.Desktop ? "Desktop" : "Fullscreen";
        }

        public static void SetCurrentTheme(ThemeDescription theme)
        {
            CurrentTheme = theme;
        }

        public static void SetDefaultTheme(ThemeDescription theme)
        {
            DefaultTheme = theme;
        }

        public static void ApplyTheme(Application app, ThemeDescription theme, ApplicationMode mode)
        {
            var allLoaded = true;
            var loadedXamls = new List<ResourceDictionary>();
            var xamlFiles = Directory.GetFiles(theme.DirectoryPath, "*.xaml");
            foreach (var xamlFile in xamlFiles)
            {
                try
                {
                    var xaml = LoadXaml(xamlFile);
                    if (xaml is ResourceDictionary)
                    {
                        loadedXamls.Add(xaml as ResourceDictionary);
                    }
                    else
                    {
                        logger.Error($"Skipping theme file {xamlFile}, it's not resource dictionary.");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load xaml {xamlFiles}");
                    allLoaded = false;
                    break;
                }
            }

            if (allLoaded)
            {
                loadedXamls.ForEach(a => app.Resources.MergedDictionaries.Add(a));
            }
        }

        public static List<ThemeDescription> GetAvailableThemes(ApplicationMode mode)
        {
            var modeDir = GetThemeRootDir(mode);
            var added = new List<string>();
            var themes = new List<ThemeDescription>();

            var userPath = Path.Combine(PlaynitePaths.ThemesUserDataPath, modeDir);
            if (!PlayniteSettings.IsPortable && Directory.Exists(userPath))
            {
                foreach (var dir in Directory.GetDirectories(userPath))
                {
                    var descriptorPath = Path.Combine(dir, themeManifestFileName);
                    if (File.Exists(descriptorPath))
                    {
                        var info = new FileInfo(descriptorPath);
                        added.Add(info.Directory.Name);
                        themes.Add(ThemeDescription.FromFile(descriptorPath));
                    }
                }
            }

            var programPath = Path.Combine(PlaynitePaths.ThemesProgramPath, modeDir);
            if (Directory.Exists(programPath))
            {
                foreach (var dir in Directory.GetDirectories(programPath))
                {
                    var descriptorPath = Path.Combine(dir, themeManifestFileName);
                    if (File.Exists(descriptorPath))
                    {
                        var info = new FileInfo(descriptorPath);
                        if (!added.Contains(info.Directory.Name))
                        {
                            themes.Add(ThemeDescription.FromFile(descriptorPath));
                        }
                    }
                }
            }

            return themes;
        }
    }
}
