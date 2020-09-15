using Playnite.Common;
using Playnite.Converters;
using Playnite.DesktopApp.Markup;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.Controls
{
    public class MenuHelpers
    {
        public static object GetIcon(string iconName, double imageHeight = 16, double imageWidth = 16)
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
                    return Images.GetImageFromFile(ThemeFile.GetFilePath(stringIcon), BitmapScalingMode.Fant, imageHeight, imageWidth);
                }
                else if (resource is BitmapImage bitmap)
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
            else if (System.IO.File.Exists(iconName))
            {
                return BitmapExtensions.BitmapFromFile(iconName)?.ToImage();
            }
            else
            {
                var themeFile = ThemeFile.GetFilePath(iconName);
                if (themeFile != null)
                {
                    return Images.GetImageFromFile(themeFile, BitmapScalingMode.Fant, imageHeight, imageWidth);
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

        public static void SetEnumBinding(
            MenuItem target,
            string bindingPath,
            object bindingSource,
            object bindingEnum)
        {
            BindingOperations.SetBinding(target, MenuItem.IsCheckedProperty,
                new Binding
                {
                    Source = bindingSource,
                    Path = new PropertyPath(bindingPath),
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = bindingEnum
                });
        }

        public static void PopulateEnumOptions<T>(
            ItemCollection parent,
            string bindingPath,
            object bindingSource,
            bool sorted = false,
            List<T> ignoreValues = null) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            if (sorted)
            {
                values = values.OrderBy(a => a.GetDescription());
            }

            foreach (T type in values)
            {
                if (ignoreValues?.Contains(type) == true)
                {
                    continue;
                }

                var item = new MenuItem
                {
                    Header = type.GetDescription(),
                    IsCheckable = true
                };

                SetEnumBinding(item, bindingPath, bindingSource, type);
                parent.Add(item);
            }
        }
    }
}
