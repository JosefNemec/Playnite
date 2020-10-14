using Microsoft.Win32;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class SystemIntegration
    {
        public static void RegisterPlayniteUriProtocol()
        {
            var view = RegistryView.Registry32;
            if (Environment.Is64BitOperatingSystem)
            {
                view = RegistryView.Registry64;
            }

            using (var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
            using (var classes = root.OpenSubKey(@"Software\Classes", true))
            {
                var openString = $"\"{PlaynitePaths.DesktopExecutablePath}\" --uridata \"%1\"";
                var existing = classes.OpenSubKey(@"Playnite\shell\open\command");
                if (existing != null && existing.GetValue(string.Empty)?.ToString() == openString)
                {
                    existing.Dispose();
                    return;
                }

                using (var newEntry = classes.CreateSubKey("Playnite"))
                {
                    newEntry.SetValue(string.Empty, "URL:playnite");
                    newEntry.SetValue("URL Protocol", string.Empty);
                    using (var command = newEntry.CreateSubKey(@"shell\open\command"))
                    {
                        command.SetValue(string.Empty, openString);
                    }
                }
            }
        }

        public static void SetBootupStateRegistration(bool runOnBootup)
        {
            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var shortcutPath = Path.Combine(startupPath, "Playnite.lnk");
            if (runOnBootup)
            {
                var args = new CmdLineOptions()
                {
                    HideSplashScreen = true
                }.ToString();

                if (File.Exists(shortcutPath))
                {
                    var existLnk = Programs.GetLnkShortcutData(shortcutPath);
                    if (existLnk.Path == PlaynitePaths.DesktopExecutablePath &&
                        existLnk.Arguments == args)
                    {
                        return;
                    }
                }

                FileSystem.DeleteFile(shortcutPath);
                Programs.CreateShortcut(PlaynitePaths.DesktopExecutablePath, args, "", shortcutPath);
            }
            else
            {
                FileSystem.DeleteFile(shortcutPath);
            }
        }

        public static void RegisterFileExtensions()
        {
            var view = RegistryView.Registry32;
            if (Environment.Is64BitOperatingSystem)
            {
                view = RegistryView.Registry64;
            }

            using (var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view))
            using (var classes = root.OpenSubKey(@"Software\Classes", true))
            {
                var openString = $"\"{PlaynitePaths.DesktopExecutablePath}\" --installext \"%1\"";
                var existing = classes.OpenSubKey(@"Playnite.ext\shell\open\command");
                if (existing != null && existing.GetValue(string.Empty)?.ToString() == openString)
                {
                    existing.Dispose();
                    return;
                }

                using (var newEntry = classes.CreateSubKey("Playnite.ext"))
                {
                    newEntry.SetValue(string.Empty, "Playnite extension file");
                    using (var command = newEntry.CreateSubKey(@"DefaultIcon"))
                    {
                        var icoPath = Path.Combine(PlaynitePaths.ProgramPath, "Resources", "playnite_extension.ico");
                        command.SetValue(string.Empty, $"\"{icoPath}\"");
                    }

                    using (var command = newEntry.CreateSubKey(@"shell\open\command"))
                    {
                        command.SetValue(string.Empty, openString);
                    }
                }

                using (var newEntry = classes.CreateSubKey(PlaynitePaths.PackedExtensionFileExtention))
                using (var command = newEntry.CreateSubKey(@"OpenWithProgids"))
                {
                    command.SetValue("Playnite.ext", string.Empty);
                }

                using (var newEntry = classes.CreateSubKey(PlaynitePaths.PackedThemeFileExtention))
                using (var command = newEntry.CreateSubKey(@"OpenWithProgids"))
                {
                    command.SetValue("Playnite.ext", string.Empty);
                }
            }
        }
    }
}
