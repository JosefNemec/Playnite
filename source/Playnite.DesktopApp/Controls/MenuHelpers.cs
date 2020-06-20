using Playnite.Common;
using Playnite.DesktopApp.Markup;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.Controls
{
    public class MenuHelpers
    {
        public static object GetIcon(string iconName)
        {
            if (iconName.IsNullOrEmpty())
            {
                return null;
            }

            var resource = ResourceProvider.GetResource(iconName);
            if (resource != null)
            {
                if (resource is string stringIcon)
                {
                    return Images.GetImageFromFile(ThemeFile.GetFilePath(stringIcon));
                }
                else if (resource is BitmapImage bitmap)
                {
                    var image = new System.Windows.Controls.Image() { Source = bitmap };
                    RenderOptions.SetBitmapScalingMode(image, RenderOptions.GetBitmapScalingMode(bitmap));
                    return image;
                }
                else if (resource is TextBlock textIcon)
                {
                    return textIcon;
                }
            }
            else if (System.IO.File.Exists(iconName))
            {
                return BitmapExtensions.BitmapFromFile(iconName)?.ToImage();
            }
            else
            {
                var themeFile = ThemeFile.GetFilePath(iconName);
                if (themeFile != null)
                {
                    return Images.GetImageFromFile(themeFile);
                }
            }

            return null;
        }

        public static MenuItem GenerateMenuParents(Dictionary<string, MenuItem> existingItems, string menuSection, ItemCollection root, int startIndex = -1)
        {
            var sections = menuSection.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToArray();
            var current = "";
            MenuItem prev = null;
            for (int i = 0; i < sections.Length; i++)
            {
                var sec = sections[i];
                current += sec;
                if (existingItems.TryGetValue(current, out var menuItem))
                {
                }
                else
                {
                    menuItem = new MenuItem { Header = sec };
                    existingItems.Add(current, menuItem);
                    if (i == 0)
                    {
                        if (startIndex >= 0)
                        {
                            root.Insert(startIndex, menuItem);
                        }
                        else
                        {
                            root.Add(menuItem);
                        }
                    }

                    if (prev != null)
                    {
                        prev.Items.Add(menuItem);
                    }
                }

                prev = menuItem;
                if (i == sections.Length - 1)
                {
                    return menuItem;
                }
            }

            return null;
        }
    }
}
