using Playnite.Common;
using Playnite.Database;
using Playnite.Extensions.Markup;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite
{
    public class SdkHelpers
    {
        public static object ResolveUiItemIcon(object icon)
        {
            if (icon == null)
            {
                return null;
            }

            if (icon is string stringIcon)
            {
                var resource = ResourceProvider.GetResource(stringIcon);
                if (resource != null)
                {
                    if (resource is BitmapImage bitmap)
                    {
                        var image = new System.Windows.Controls.Image() { Source = bitmap };
                        RenderOptions.SetBitmapScalingMode(image, RenderOptions.GetBitmapScalingMode(bitmap));
                        return image;
                    }
                    else if (resource is TextBlock textIcon)
                    {
                        var text = new TextBlock
                        {
                            Text = textIcon.Text,
                            FontFamily = textIcon.FontFamily,
                            FontStyle = textIcon.FontStyle
                        };

                        if (textIcon.ReadLocalValue(TextBlock.ForegroundProperty) != DependencyProperty.UnsetValue)
                        {
                            text.Foreground = textIcon.Foreground;
                        }

                        return text;
                    }
                }
                else if (System.IO.File.Exists(stringIcon))
                {
                    return BitmapExtensions.BitmapFromFile(stringIcon)?.ToImage();
                }
                else
                {
                    var themeFile = ThemeFile.GetFilePath(stringIcon);
                    if (themeFile != null)
                    {
                        return Images.GetImageFromFile(themeFile, BitmapScalingMode.Fant, double.NaN, double.NaN);
                    }

                    var dbFile = GameDatabase.Instance.GetFileAsImage(stringIcon);
                    if (dbFile != null)
                    {
                        return dbFile.ToImage();
                    }
                }
            }
            else
            {
                return icon;
            }

            return null;
        }
    }
}
