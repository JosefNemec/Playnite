﻿using System;
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
using System.Windows.Markup;
using Playnite.Settings;

namespace PlayniteUI
{
    public class SkinMediaPathConverter : MarkupExtension, IValueConverter
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
            if (string.IsNullOrEmpty(skinName))
            {
                return DependencyProperty.UnsetValue;
            }

            var filePath = Path.Combine(PlaynitePaths.ProgramPath, skinFolder, skinName, path);
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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
