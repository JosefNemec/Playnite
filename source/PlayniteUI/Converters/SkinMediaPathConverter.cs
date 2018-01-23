using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Playnite.Database;
using NLog;
using System.IO;
using Playnite;

namespace PlayniteUI
{
    public class SkinMediaPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var path = parameter as string;
            if (string.IsNullOrEmpty(path))
            {
                return DependencyProperty.UnsetValue;
            }

            var skinName = string.IsNullOrEmpty(Themes.CurrentFullscreenTheme) ? Themes.CurrentTheme : Themes.CurrentFullscreenTheme;
            var skinFolder = string.IsNullOrEmpty(Themes.CurrentFullscreenTheme) ? "Skins" : "SkinsFullscreen";
            var filePath = Path.Combine(Paths.ProgramFolder, skinFolder, skinName, path);
            if (File.Exists(filePath))
            {
                return filePath;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
