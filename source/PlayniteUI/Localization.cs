using Microsoft.Win32;
using Playnite;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace PlayniteUI
{
    public class PlayniteLanguage
    {
        public string LocaleString
        {
            get; set;
        }

        public string Id
        {
            get; set;
        }
    }

    public static class Localization
    {
        public static List<PlayniteLanguage> AvailableLanguages
        {
            get
            {
                return GetLanguagesFromFolder(PlaynitePaths.LocalizationsPath);
            }
        }

        public static string CurrentLanguage
        {
            get
            {
                var dictionary = Application.Current.Resources.MergedDictionaries.
                    FirstOrDefault(a => a.Contains("LocalizationLanguage") && a["LocalizationLanguage"].ToString() != "english");
                if (dictionary == null)
                {
                    return string.Empty;
                }
                else
                {
                    return dictionary["LocalizationLanguage"].ToString();
                }
            }

            set
            {
                SetLanguage(value);
            }
        }

        public static List<PlayniteLanguage> GetLanguagesFromFolder(string path)
        {
            var langs = new List<PlayniteLanguage>();
            foreach (var file in Directory.GetFiles(path, "*.xaml"))
            {
                var langsPath = Path.Combine(path, file);

                using (var stream = new StreamReader(langsPath))
                {
                    var res = XamlReader.Load(stream.BaseStream) as ResourceDictionary;
                    langs.Add(new PlayniteLanguage()
                    {
                        Id = Path.GetFileNameWithoutExtension(langsPath),
                        LocaleString = res["LocalizationString"].ToString()
                    });
                }
            }

            return langs.OrderBy(a => a.LocaleString).ToList();
        }

        public static void SetLanguage(string language)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var currentLang = dictionaries.FirstOrDefault(a => a.Contains("LocalizationLanguage") && a["LocalizationLanguage"].ToString() != "english");
            if (currentLang != null)
            {
                dictionaries.Remove(currentLang);
            }

            var langFile = Path.Combine(PlaynitePaths.LocalizationsPath, language + ".xaml");
            if (File.Exists(langFile) && language != "english")
            {
                var newLang = new ResourceDictionary() { Source = new Uri(langFile) };
                dictionaries.Add(newLang);
            }
        }
    }
}
