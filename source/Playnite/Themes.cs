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

        public static void ApplyFullscreenButtonPrompts(Application app, FullscreenButtonPrompts prompts)
        {
            if (prompts == FullscreenSettings.DefaultButtonPrompts)
            {
                var defaultXaml = $"{FullscreenSettings.DefaultButtonPrompts.ToString()}.xaml";
                foreach (var dir in PlayniteApplication.CurrentNative.Resources.MergedDictionaries.ToList())
                {
                    if (dir.Source.OriginalString.Contains("ButtonPrompts") &&
                        !dir.Source.OriginalString.EndsWith(defaultXaml))
                    {
                        PlayniteApplication.CurrentNative.Resources.MergedDictionaries.Remove(dir);
                    }
                }
            }
            else
            {
                var promptsPath = Path.Combine(ThemeManager.DefaultTheme.DirectoryPath, "Images", "ButtonPrompts");
                foreach (var dir in Directory.GetDirectories(promptsPath))
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var promptXaml = Path.Combine(dir, $"{dirInfo.Name}.xaml");
                    if (File.Exists(promptXaml) && dirInfo.Name == prompts.ToString())
                    {
                        var xaml = Xaml.GetObjectFromFile(promptXaml);
                        if (xaml is ResourceDictionary xamlDir)
                        {
                            xamlDir.Source = new Uri(promptXaml, UriKind.Absolute);
                            PlayniteApplication.CurrentNative.Resources.MergedDictionaries.Add(xamlDir);
                        }
                    }
                }
            }
        }

        public static void ApplyTheme(Application app, ThemeDescription theme, ApplicationMode mode)
        {
            var allLoaded = true;
            var loadedXamls = new List<ResourceDictionary>();
            var xamlFiles = Directory.GetFiles(theme.DirectoryPath, "*.xaml", SearchOption.AllDirectories);
            foreach (var xamlFile in xamlFiles)
            {
                try
                {
                    var xaml = Xaml.GetObjectFromFile(xamlFile);
                    if (xaml is ResourceDictionary xamlDir)
                    {
                        xamlDir.Source = new Uri(xamlFile, UriKind.Absolute);
                        loadedXamls.Add(xamlDir as ResourceDictionary);
                    }
                    else
                    {
                        logger.Error($"Skipping theme file {xamlFile}, it's not resource dictionary.");
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
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
