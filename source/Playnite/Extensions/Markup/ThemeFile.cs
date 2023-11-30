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
using System.Diagnostics;
using System.Text.RegularExpressions;
using CommandLine;

namespace Playnite.Extensions.Markup
{
    public class ThemeFile : MarkupExtension
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static FileInfo lastUserTheme = null;
        private static bool? lastUserThemeFound = null;

        internal ThemeManifest CurrentTheme { get; set; }
        internal ThemeManifest DefaultTheme { get; set; }

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

        public static ThemeManifest GetDesignTimeDefaultTheme(ApplicationMode mode)
        {
            if (lastUserThemeFound == null)
            {
                if (Process.GetCurrentProcess().TryGetParentId(out var parentId))
                {
                    var proc = Process.GetProcessById(parentId);
                    var cmdline = proc.GetCommandLine();
                    var regEx = Regex.Match(cmdline, @"([^""]+\.sln?)""");
                    if (regEx.Success)
                    {
                        var spath = regEx.Groups[1].Value;
                        if (spath.Contains($"Themes\\{mode.GetDescription()}"))
                        {
                            lastUserTheme = new FileInfo(spath);
                            lastUserThemeFound = true;
                        }
                    }
                }
            }

            if (lastUserThemeFound == true)
            {
                return new ThemeManifest()
                {
                    DirectoryName = lastUserTheme.DirectoryName,
                    DirectoryPath = lastUserTheme.Directory.FullName,
                    Name = "Default"
                };
            }

            lastUserThemeFound = false;
            var defaultTheme = "Default";
            var projectName = mode == ApplicationMode.Fullscreen ? "Playnite.FullscreenApp" : "Playnite.DesktopApp";
            var slnPath = Path.Combine(Environment.GetEnvironmentVariable("PLAYNITE_SLN", EnvironmentVariableTarget.User), projectName);
            var themePath = Path.Combine(slnPath, "Themes", ThemeManager.GetThemeRootDir(mode), defaultTheme);
            return new ThemeManifest()
            {
                DirectoryName = defaultTheme,
                DirectoryPath = themePath,
                Name = defaultTheme
            };
        }

        public static string GetFilePath(string relPath, bool checkExistance = true, bool matchByRegex = false)
        {
            return GetFilePath(relPath, ThemeManager.DefaultTheme, ThemeManager.CurrentTheme, checkExistance, matchByRegex);
        }

        public static string GetFilePath(string relPath, ThemeManifest defaultTheme, bool checkExistance = true, bool matchByRegex = false)
        {
            return GetFilePath(relPath, defaultTheme, ThemeManager.CurrentTheme, checkExistance, matchByRegex);
        }

        public static string GetFilePath(string relPath, ThemeManifest defaultTheme, ThemeManifest currentTheme, bool checkExistance = true, bool matchByRegex = false)
        {
            if (matchByRegex)
            {
                relPath = relPath.TrimStart(new char[] { Path.DirectorySeparatorChar });
                string searchFile(string dir)
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        if (Regex.IsMatch(file.Replace(dir, string.Empty, StringComparison.OrdinalIgnoreCase), relPath, RegexOptions.IgnoreCase))
                        {
                            return file;
                        }
                    }

                    return null;
                }

                if (currentTheme != null)
                {
                    var match = searchFile(currentTheme.DirectoryPath.EndWithDirSeparator());
                    if (!match.IsNullOrEmpty())
                    {
                        return match;
                    }
                }

                if (defaultTheme != null)
                {
                    var match = searchFile(defaultTheme.DirectoryPath.EndWithDirSeparator());
                    if (!match.IsNullOrEmpty())
                    {
                        return match;
                    }
                }

                return null;
            }
            else
            {
                relPath = Paths.FixSeparators(relPath).TrimStart(new char[] { Path.DirectorySeparatorChar });
                string searchFile(string dir)
                {
                    var themePath = Path.Combine(dir, relPath);
                    if (File.Exists(themePath) && checkExistance)
                    {
                        return themePath;
                    }
                    else if (!checkExistance)
                    {
                        return themePath;
                    }

                    return null;
                }

                if (currentTheme != null)
                {
                    var match = searchFile(currentTheme.DirectoryPath);
                    if (!match.IsNullOrEmpty())
                    {
                        return match;
                    }
                }

                if (defaultTheme != null)
                {
                    var match = searchFile(defaultTheme.DirectoryPath);
                    if (!match.IsNullOrEmpty())
                    {
                        return match;
                    }
                }

                return null;
            }
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

            try
            {
                return converter.ConvertFrom(path);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to provide value for theme file {path}");
                return null;
            }
        }
    }
}
