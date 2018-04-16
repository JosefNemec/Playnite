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

namespace PlayniteUI
{
    public class Theme
    {
        public string Name
        {
            get; set;
        }

        public List<string>Profiles
        {
            get; set;
        }

        public Theme(string name, List<string> profiles)
        {
            Name = name;
            Profiles = profiles;
        }
    }

    public class Themes
    {
        public static string CurrentTheme
        {
            get;
            private set;
        }

        public static string CurrentColor
        {
            get;
            private set;
        }

        public static string CurrentFullscreenTheme
        {
            get;
            private set;
        }

        public static string CurrentFullscreenColor
        {
            get;
            private set;
        }

        public static List<Theme> AvailableFullscreenThemes
        {
            get
            {
                return GetThemesFromFolder(Paths.ThemesFullscreenPath);
            }
        }

        public static List<Theme> AvailableThemes
        {
            get
            {
                return GetThemesFromFolder(Paths.ThemesPath);
            }
        }

        private static List<Theme> GetThemesFromFolder(string path)
        {
            var themes = new List<Theme>();
            if (!Directory.Exists(path))
            {
                return themes;
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                var dirInfo = new DirectoryInfo(dir);
                var rootFile = Path.Combine(dir, dirInfo.Name + ".xaml");
                if (!File.Exists(rootFile))
                {
                    continue;
                }

                var themeName = dirInfo.Name;
                var profiles = new List<string>();
                foreach (var file in Directory.GetFiles(dir))
                {
                    var fileInfo = new FileInfo(file);
                    if (file == rootFile)
                    {
                        continue;
                    }

                    var match = Regex.Match(fileInfo.Name, $"{themeName}\\.(.*)\\.xaml", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var profile = match.Groups[1].Value;
                        profiles.Add(profile);
                    }
                }

                themes.Add(new Theme(themeName, profiles));
            }

            return themes;
        }

        public static void ApplyFullscreenTheme(string themeName, string color, bool forceReload = false)
        {
            if (CurrentFullscreenTheme == themeName && CurrentFullscreenColor == color && forceReload == false)
            {
                return;
            }

            if (Application.Current != null)
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var currentThemeDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenName"));
                var currentThemeColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenColorName"));

                if (currentThemeColorDict != null)
                {
                    dictionaries.Remove(currentThemeColorDict);
                }

                if (currentThemeDict != null)
                {
                    dictionaries.Remove(currentThemeDict);
                }

                // Also remove any non-fullscreen resources
                // we don't want to have fullscreen and non-fullscreen resources loaded at the same time
                // to prevent possible conflicts.
                currentThemeDict = dictionaries.FirstOrDefault(a => a.Contains("SkinName"));
                currentThemeColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinColorName"));

                if (currentThemeColorDict != null)
                {
                    dictionaries.Remove(currentThemeColorDict);
                }

                if (currentThemeDict != null)
                {
                    dictionaries.Remove(currentThemeDict);
                }


                var themePath = GetThemePath(themeName, true);
                dictionaries.Add(LoadXaml(themePath));

                if (string.IsNullOrEmpty(color))
                {
                    return;
                }

                var fullColorPath = GetColorPath(themeName, color, true);
                dictionaries.Add(LoadXaml(fullColorPath));
            }


            CurrentTheme = null;
            CurrentColor = null;
            CurrentFullscreenTheme = themeName;
            CurrentFullscreenColor = color;
        }

        public static void ApplyTheme(string themeName, string color, bool forceReload = false)
        {
            if (CurrentTheme == themeName && CurrentColor == color && forceReload == false)
            {
                return;
            }

            if (Application.Current != null)
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var currentThemeDict = dictionaries.FirstOrDefault(a => a.Contains("SkinName"));
                var currentThemeColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinColorName"));
                if (currentThemeColorDict != null)
                {
                    dictionaries.Remove(currentThemeColorDict);
                }

                var changeThemeBase = string.IsNullOrEmpty(CurrentTheme) || CurrentTheme != currentThemeDict["SkinName"].ToString();
                if (currentThemeDict != null && changeThemeBase)
                {
                    dictionaries.Remove(currentThemeDict);
                }

                // Also remove any fullscreen resources
                // we don't want to have fullscreen and non-fullscreen resources loaded at the same time
                // to prevent possible conflicts.
                currentThemeDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenName"));
                currentThemeColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenColorName"));

                if (currentThemeColorDict != null)
                {
                    dictionaries.Remove(currentThemeColorDict);
                }

                if (currentThemeDict != null)
                {
                    dictionaries.Remove(currentThemeDict);
                }

                if (changeThemeBase)
                {
                    var themePath = GetThemePath(themeName, false);
                    dictionaries.Add(LoadXaml(themePath));
                }

                if (string.IsNullOrEmpty(color))
                {
                    return;
                }

                var fullColorPath = GetColorPath(themeName, color, false);
                dictionaries.Add(LoadXaml(fullColorPath));
            }

            CurrentTheme = themeName;
            CurrentColor = color;
            CurrentFullscreenTheme = null;
            CurrentFullscreenColor = null;
        }

        private static ResourceDictionary LoadXaml(string path)
        {
            using (var stream = new StreamReader(path))
            {
                return XamlReader.Load(stream.BaseStream) as ResourceDictionary;
            }
        }

        public static string GetThemePath(string themeName, bool fullscreen)
        {
            return Path.Combine(fullscreen ? Paths.ThemesFullscreenPath : Paths.ThemesPath, themeName, themeName + ".xaml");
        }

        public static string GetColorPath(string themeName, string color, bool fullscreen)
        {
            var colorFile = themeName + $".{color}.xaml";
            return Path.Combine(fullscreen ? Paths.ThemesFullscreenPath : Paths.ThemesPath, themeName, colorFile);
        }

        public static Tuple<bool, string> IsThemeValid(string themeName, bool fullscreen)
        {
            try
            {
                LoadXaml(GetThemePath(themeName, fullscreen));
                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }

        public static Tuple<bool, string> IsColorProfileValid(string themeName, string color, bool fullscreen)
        {
            try
            {
                LoadXaml(GetColorPath(themeName, color, fullscreen));
                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }
    }
}
