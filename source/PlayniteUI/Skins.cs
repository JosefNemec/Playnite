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

        public static List<Skin> AvailableSkins
        {
            get
            {
                var skins = new List<Skin>();
                if (!Directory.Exists(Paths.SkinsPath))
                {
                    return skins;
                }

                foreach (var dir in Directory.GetDirectories(Paths.SkinsPath))
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
        }

        public static void ApplySkin(string skinName, string color)
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

            var skinPath = GetSkinPath(skinName);
            dictionaries.Add(LoadXaml(skinPath));
            CurrentSkin = skinName;

            if (string.IsNullOrEmpty(color))
            {
                return;
            }
            
            var fullColorPath = GetColorPath(skinName, color);
            dictionaries.Add(LoadXaml(fullColorPath));
            CurrentColor = color;
        }

        private static ResourceDictionary LoadXaml(string path)
        {
            using (var stream = new StreamReader(path))
            {
                return XamlReader.Load(stream.BaseStream) as ResourceDictionary;
            }
        }

        public static string GetSkinPath(string skinName)
        {
            return Path.Combine(Paths.SkinsPath, skinName, skinName + ".xaml");
        }

        public static string GetColorPath(string skinName, string color)
        {
            var colorFile = skinName + $".{color}.xaml";
            return Path.Combine(Paths.SkinsPath, skinName, colorFile);
        }

        public static Tuple<bool, string> IsSkinValid(string skinName)
        {
            try
            {
                LoadXaml(GetSkinPath(skinName));
                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }

        public static Tuple<bool, string> IsColorProfileValid(string skinName, string color)
        {
            try
            {
                LoadXaml(GetColorPath(skinName, color));
                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }
    }
}
