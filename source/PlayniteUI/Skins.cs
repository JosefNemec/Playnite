using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite;
using System.Windows;
using System.Windows.Markup;

namespace PlayniteUI
{
    public class Skins
    {        
        public static List<string> AvailableSkins
        {
            get
            {
                var skins = new List<string>();
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

                    skins.Add(dirInfo.Name);
                }

                return skins;
            }
        }

        public static void ApplySkin(string name)
        {
            var path = Path.Combine(Paths.SkinsPath, name, name + ".xaml");
            using (var stream = new StreamReader(path))
            {
                var dict = XamlReader.Load(stream.BaseStream) as ResourceDictionary;
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
        }
    }
}
