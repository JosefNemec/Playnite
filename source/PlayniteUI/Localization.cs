using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public static class Localization
    {
        public static string CurrentLanguage
        {
            get
            {
                var dictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(a => a.Contains("LocalizationLanguage"));
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

        public static void SetLanguage(string language)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var currentDict = dictionaries.FirstOrDefault(a => a.Contains("LocalizationLanguage"));
            var desiredDict = new ResourceDictionary()
            {
                Source = new Uri(string.Format("pack://application:,,,/Localization/{0}.xaml", language))
            };

            if (currentDict == null)
            {
                dictionaries.Add(desiredDict);
            }
            else
            {
                dictionaries[dictionaries.IndexOf(currentDict)] = desiredDict;
            }
        }
    }
}
