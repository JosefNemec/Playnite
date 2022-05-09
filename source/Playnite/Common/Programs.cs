using Microsoft.Win32;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class Program
    {
        public string Path { get; set; }
        public string Arguments { get; set; }
        public string Icon { get; set; }
        public int IconIndex { get; set; }
        public string WorkDir { get; set; }
        public string Name { get; set; }
        public string AppId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class UninstallProgram
    {
        public string DisplayIcon { get; set; }
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public string InstallLocation { get; set; }
        public string Publisher { get; set; }
        public string UninstallString { get; set; }
        public string URLInfoAbout { get; set; }
        public string RegistryKeyName { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return DisplayName ?? RegistryKeyName;
        }
    }

    public partial class Programs
    {
        private static readonly string[] scanFileExclusionMasks = new string[]
        {
            "uninst",
            "setup",
            @"unins\d+",
            "Config",
            "DXSETUP",
            @"vc_redist\.x64",
            @"vc_redist\.x86",
            @"^UnityCrashHandler32\.exe$",
            @"^UnityCrashHandler64\.exe$",
            @"^notification_helper\.exe$",
            @"^python\.exe$",
            @"^pythonw\.exe$",
            @"^zsync\.exe$",
            @"^zsyncmake\.exe$"
        };

        private static ILogger logger = LogManager.GetLogger();

        public static bool IsFileScanExcluded(string path)
        {
            return scanFileExclusionMasks.Any(a => Regex.IsMatch(path, a, RegexOptions.IgnoreCase));
        }

        public static void CreateUrlShortcut(string url, string iconPath, string shortcutPath)
        {
            FileSystem.PrepareSaveFile(shortcutPath);
            var content = @"[InternetShortcut]
IconIndex=0";
            if (!iconPath.IsNullOrEmpty())
            {
                content += Environment.NewLine + $"IconFile={iconPath}";
            }

            content += Environment.NewLine + $"URL={url}";
            File.WriteAllText(shortcutPath, content);
        }

        private static List<UninstallProgram> GetUninstallProgsFromView(RegistryView view)
        {
            var rootString = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            void SearchRoot(RegistryHive hive, List<UninstallProgram> programs)
            {
                using (var root = RegistryKey.OpenBaseKey(hive, view))
                {
                    var keyList = root.OpenSubKey(rootString);
                    if (keyList == null)
                    {
                        return;
                    }

                    foreach (var key in keyList.GetSubKeyNames())
                    {
                        try
                        {
                            using (var prog = root.OpenSubKey(rootString + key))
                            {
                                if (prog == null)
                                {
                                    continue;
                                }

                                var program = new UninstallProgram()
                                {
                                    DisplayIcon = prog.GetValue("DisplayIcon")?.ToString(),
                                    DisplayVersion = prog.GetValue("DisplayVersion")?.ToString(),
                                    DisplayName = prog.GetValue("DisplayName")?.ToString(),
                                    InstallLocation = prog.GetValue("InstallLocation")?.ToString(),
                                    Publisher = prog.GetValue("Publisher")?.ToString(),
                                    UninstallString = prog.GetValue("UninstallString")?.ToString(),
                                    URLInfoAbout = prog.GetValue("URLInfoAbout")?.ToString(),
                                    Path = prog.GetValue("Path")?.ToString(),
                                    RegistryKeyName = key
                                };

                                programs.Add(program);
                            }
                        }
                        catch (System.Security.SecurityException e)
                        {
                            logger.Warn(e, $"Failed to read registry key {rootString + key}");
                        }
                    }
                }
            }

            var progs = new List<UninstallProgram>();
            SearchRoot(RegistryHive.LocalMachine, progs);
            SearchRoot(RegistryHive.CurrentUser, progs);
            return progs;
        }

        public static List<UninstallProgram> GetUnistallProgramsList()
        {
            var progs = new List<UninstallProgram>();

            if (Environment.Is64BitOperatingSystem)
            {
                progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry64));
            }

            progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry32));
            return progs;
        }
    }
}
