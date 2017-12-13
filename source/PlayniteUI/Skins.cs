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
    public class Skin
    {
        public string Name
        {
            get; set;
        }

        public List<string>Profiles
        {
            get; set;
        }

        public Skin(string name, List<string> profiles)
        {
            Name = name;
            Profiles = profiles;
        }
    }

    public class Skins
    {
        public static string CurrentSkin
        {
            get;
            private set;
        }

        public static string CurrentColor
        {
            get;
            private set;
        }

        public static string CurrentFullscreenSkin
        {
            get;
            private set;
        }

        public static string CurrentFullscreenColor
        {
            get;
            private set;
        }

        public static List<Skin> AvailableFullscreenSkins
        {
            get
            {
                return GetSkinsFromFolder(Paths.SkinsFullscreenPath);
            }
        }

        public static List<Skin> AvailableSkins
        {
            get
            {
                return GetSkinsFromFolder(Paths.SkinsPath);
            }
        }

        private static List<Skin> GetSkinsFromFolder(string path)
        {
            var skins = new List<Skin>();
            if (!Directory.Exists(path))
            {
                return skins;
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                var dirInfo = new DirectoryInfo(dir);
                var rootFile = Path.Combine(dir, dirInfo.Name + ".xaml");
                if (!File.Exists(rootFile))
                {
                    continue;
                }

                var skinName = dirInfo.Name;
                var profiles = new List<string>();
                foreach (var file in Directory.GetFiles(dir))
                {
                    var fileInfo = new FileInfo(file);
                    if (file == rootFile)
                    {
                        continue;
                    }

                    var match = Regex.Match(fileInfo.Name, $"{skinName}\\.(.*)\\.xaml", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var profile = match.Groups[1].Value;
                        profiles.Add(profile);
                    }
                }

                skins.Add(new Skin(skinName, profiles));
            }

            return skins;
        }

        public static void ApplyFullscreenSkin(string skinName, string color, bool forceReload = false)
        {
            if (CurrentFullscreenSkin == skinName && CurrentFullscreenColor == color && forceReload == false)
            {
                return;
            }

            if (Application.Current != null)
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var currentSkinDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenName"));
                var currentSkinColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenColorName"));

                if (currentSkinColorDict != null)
                {
                    dictionaries.Remove(currentSkinColorDict);
                }

                if (currentSkinDict != null)
                {
                    dictionaries.Remove(currentSkinDict);
                }

                // Also remove any non-fullscreen resources
                // we don't want to have fullscreen and non-fullscreen resources loaded at the same time
                // to prevent possible conflicts.
                currentSkinDict = dictionaries.FirstOrDefault(a => a.Contains("SkinName"));
                currentSkinColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinColorName"));

                if (currentSkinColorDict != null)
                {
                    dictionaries.Remove(currentSkinColorDict);
                }

                if (currentSkinDict != null)
                {
                    dictionaries.Remove(currentSkinDict);
                }


                var skinPath = GetSkinPath(skinName, true);
                dictionaries.Add(LoadXaml(skinPath));

                if (string.IsNullOrEmpty(color))
                {
                    return;
                }

                var fullColorPath = GetColorPath(skinName, color, true);
                dictionaries.Add(LoadXaml(fullColorPath));
            }

            CurrentSkin = skinName;
            CurrentColor = color;
            CurrentFullscreenSkin = CurrentSkin;
            CurrentFullscreenColor = CurrentColor;
        }

        public static void ApplySkin(string skinName, string color, bool forceReload = false)
        {
            if (CurrentSkin == skinName && CurrentColor == color && forceReload == false)
            {
                return;
            }

            if (Application.Current != null)
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var currentSkinDict = dictionaries.FirstOrDefault(a => a.Contains("SkinName"));
                var currentSkinColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinColorName"));

                if (currentSkinColorDict != null)
                {
                    dictionaries.Remove(currentSkinColorDict);
                }

                if (currentSkinDict != null)
                {
                    dictionaries.Remove(currentSkinDict);
                }

                // Also remove any fullscreen resources
                // we don't want to have fullscreen and non-fullscreen resources loaded at the same time
                // to prevent possible conflicts.
                currentSkinDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenName"));
                currentSkinColorDict = dictionaries.FirstOrDefault(a => a.Contains("SkinFullscreenColorName"));

                if (currentSkinColorDict != null)
                {
                    dictionaries.Remove(currentSkinColorDict);
                }

                if (currentSkinDict != null)
                {
                    dictionaries.Remove(currentSkinDict);
                }

                var skinPath = GetSkinPath(skinName, false);
                dictionaries.Add(LoadXaml(skinPath));                

                if (string.IsNullOrEmpty(color))
                {
                    return;
                }

                var fullColorPath = GetColorPath(skinName, color, false);
                dictionaries.Add(LoadXaml(fullColorPath));
            }

            CurrentSkin = skinName;
            CurrentColor = color;
            CurrentFullscreenSkin = null;
            CurrentFullscreenColor = null;
        }

        private static ResourceDictionary LoadXaml(string path)
        {
            using (var stream = new StreamReader(path))
            {
                return XamlReader.Load(stream.BaseStream) as ResourceDictionary;
            }
        }

        public static string GetSkinPath(string skinName, bool fullscreen)
        {
            return Path.Combine(fullscreen ? Paths.SkinsFullscreenPath : Paths.SkinsPath, skinName, skinName + ".xaml");
        }

        public static string GetColorPath(string skinName, string color, bool fullscreen)
        {
            var colorFile = skinName + $".{color}.xaml";
            return Path.Combine(fullscreen ? Paths.SkinsFullscreenPath : Paths.SkinsPath, skinName, colorFile);
        }

        public static Tuple<bool, string> IsSkinValid(string skinName, bool fullscreen)
        {
            try
            {
                LoadXaml(GetSkinPath(skinName, fullscreen));
                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }

        public static Tuple<bool, string> IsColorProfileValid(string skinName, string color, bool fullscreen)
        {
            try
            {
                LoadXaml(GetColorPath(skinName, color, fullscreen));
                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }
    }
}
