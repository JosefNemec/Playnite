using Playnite.Common;
using Playnite.Extensions;
using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;

namespace Playnite.Extensions.Markup
{
    public class ThemeFile : MarkupExtension
    {
        public ThemeDescription CurrentTheme { get; set; }
        public ThemeDescription DefaultTheme { get; set; }

        public string RelativePath { get; set; }

        public ThemeFile(ApplicationMode mode)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                DefaultTheme = GetDesignTimeDefaultTheme(mode);
            }
            else
            {
                DefaultTheme = ThemeManager.DefaultTheme;
                CurrentTheme = ThemeManager.CurrentTheme;
            }
        }

        public ThemeFile(string path, ApplicationMode mode) : this(mode)
        {
            RelativePath = path;
        }

        public static ThemeDescription GetDesignTimeDefaultTheme(ApplicationMode mode)
        {
            var defaultTheme = "Default";
            var projectName = mode == ApplicationMode.Fullscreen ? "Playnite.FullscreenApp" : "Playnite.DesktopApp";
            var slnPath = Path.Combine(Environment.GetEnvironmentVariable("PLAYNITE_SLN", EnvironmentVariableTarget.User), projectName);
            var themePath = Path.Combine(slnPath, "Themes", ThemeManager.GetThemeRootDir(mode), defaultTheme);
            return new ThemeDescription()
            {
                DirectoryName = defaultTheme,
                DirectoryPath = themePath,
                Name = defaultTheme
            };
        }

        public static string GetFilePath(string relPath, ThemeDescription defaultTheme)
        {
            return GetFilePath(relPath, defaultTheme, null);
        }

        public static string GetFilePath(string relPath, ThemeDescription defaultTheme, ThemeDescription currentTheme)
        {
            var relativePath = Paths.FixSeparators(relPath).TrimStart(new char[] { Path.DirectorySeparatorChar });

            if (currentTheme != null)
            {
                var themePath = Path.Combine(currentTheme.DirectoryPath, relativePath);
                if (File.Exists(themePath))
                {
                    return themePath;
                }
            }

            if (defaultTheme != null)
            {
                var defaultPath = Path.Combine(defaultTheme.DirectoryPath, relativePath);
                if (File.Exists(defaultPath))
                {
                    return defaultPath;
                }
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string path = GetFilePath(RelativePath, DefaultTheme, CurrentTheme);
            if (path.IsNullOrEmpty())
            {
                return null;
            }

            var provider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var type = IProvideValueTargetExtensions.GetTargetType(provider);
            var converter = TypeDescriptor.GetConverter(type);
            return converter.ConvertFrom(path);
        }
    }
}
