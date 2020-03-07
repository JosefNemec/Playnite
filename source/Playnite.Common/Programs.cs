using Microsoft.Win32;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Playnite.Common
{
    public class Program
    {
        public string Path { get; set; }
        public string Arguments { get; set; }
        public string Icon { get; set; }
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

    public class Programs
    {
        private static readonly string[] uninstallerMasks = new string[]
        {
            "uninst",
            "setup",
            @"unins\d+"
        };

        private static ILogger logger = LogManager.GetLogger();

        public static bool IsPathUninstaller(string path)
        {
            return uninstallerMasks.Any(a => Regex.IsMatch(path, a, RegexOptions.IgnoreCase));
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

        public static void CreateShortcut(string executablePath, string arguments, string iconPath, string shortcutPath)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            link.TargetPath = executablePath;
            link.WorkingDirectory = Path.GetDirectoryName(executablePath);
            link.Arguments = arguments;
            link.IconLocation = string.IsNullOrEmpty(iconPath) ? executablePath + ",0" : iconPath;
            link.Save();
        }

        public static async Task<List<Program>> GetExecutablesFromFolder(string path, SearchOption searchOption, CancellationTokenSource cancelToken = null)
        {
            return await Task.Run(() =>
            {
                var execs = new List<Program>();
                var files = new SafeFileEnumerator(path, "*.exe", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        return null;
                    }

                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    if (IsPathUninstaller(file.FullName))
                    {
                        continue;
                    }

                    var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);
                    var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : new DirectoryInfo(Path.GetDirectoryName(file.FullName)).Name;

                    execs.Add(new Program()
                    {
                        Path = file.FullName,
                        Icon = file.FullName,
                        WorkDir = Path.GetDirectoryName(file.FullName),
                        Name = programName
                    });
                }

                return execs;
            });
        }

        public static Program GetLnkShortcutData(string lnkPath)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkPath);
            return new Program()
            {
                Path = link.TargetPath,
                Icon = link.IconLocation,
                Arguments = link.Arguments,
                WorkDir = link.WorkingDirectory
            };
        }

        public static async Task<List<Program>> GetShortcutProgramsFromFolder(string path, CancellationTokenSource cancelToken = null)
        {
            return await Task.Run(() =>
            {
                var folderExceptions = new string[]
                {
                    @"\Accessibility\",
                    @"\Accessories\",
                    @"\Administrative Tools\",
                    @"\Maintenance\",
                    @"\StartUp\",
                    @"\Windows ",
                    @"\Microsoft ",
                };

                var pathExceptions = new string[]
                {
                    @"\system32\",
                    @"\windows\",
                };

                var shell = new IWshRuntimeLibrary.WshShell();
                var apps = new List<Program>();
                var shortucts = new SafeFileEnumerator(path, "*.lnk", SearchOption.AllDirectories);

                foreach (var shortcut in shortucts)
                {
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        return null;
                    }

                    if (shortcut.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    var fileName = shortcut.Name;
                    var Directory = Path.GetDirectoryName(shortcut.FullName);

                    if (folderExceptions.FirstOrDefault(a => shortcut.FullName.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                    {
                        continue;
                    }

                    var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcut.FullName);
                    var target = link.TargetPath;

                    if (pathExceptions.FirstOrDefault(a => target.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                    {
                        continue;
                    }

                    // Ignore uninstallers
                    if (IsPathUninstaller(target))
                    {
                        continue;
                    }

                    // Ignore duplicates
                    if (apps.FirstOrDefault(a => a.Path == target) != null)
                    {
                        continue;
                    }

                    // Ignore non-application links
                    if (Path.GetExtension(target) != ".exe")
                    {
                        continue;
                    }

                    var app = new Program()
                    {
                        Path = target,
                        Icon = link.IconLocation,
                        Name = Path.GetFileNameWithoutExtension(shortcut.Name),
                        WorkDir = link.WorkingDirectory
                    };

                    apps.Add(app);
                }

                return apps;
            });
        }

        public static async Task<List<Program>> GetInstalledPrograms(CancellationTokenSource cancelToken = null)
        {
            var apps = new List<Program>();

            // Get apps from All Users
            var allPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            var allApps = await GetShortcutProgramsFromFolder(allPath);
            if (cancelToken?.IsCancellationRequested == true)
            {
                return null;
            }
            else
            {
                apps.AddRange(allApps);
            }

            // Get current user apps
            var userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            var userApps = await GetShortcutProgramsFromFolder(userPath);
            if (cancelToken?.IsCancellationRequested == true)
            {
                return null;
            }
            else
            {
                apps.AddRange(userApps);
            }

            return apps;
        }

        private static string GetUWPGameIcon(string defPath)
        {
            if (File.Exists(defPath))
            {
                return defPath;
            }

            var folder = Path.GetDirectoryName(defPath);
            var fileMask = Path.GetFileNameWithoutExtension(defPath) + ".scale*.png";
            var files = Directory.GetFiles(folder, fileMask);

            if (files == null || files.Count() == 0)
            {
                return string.Empty;
            }
            else
            {
                var icons = files.Where(a => Regex.IsMatch(a, @"\.scale-\d+\.png"));
                if (icons.Any())
                {
                    return icons.OrderBy(a => a).Last();
                }

                return string.Empty;
            }
        }

        public static List<Program> GetUWPApps()
        {
            var apps = new List<Program>();

            var manager = new PackageManager();
            IEnumerable<Package> packages = manager.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value);
            foreach (var package in packages)
            {
                if (package.IsFramework || package.IsResourcePackage || package.SignatureKind != PackageSignatureKind.Store)
                {
                    continue;
                }

                try
                {
                    if (package.InstalledLocation == null)
                    {
                        continue;
                    }
                }
                catch
                {
                    // InstalledLocation accessor may throw Win32 exception for unknown reason
                    continue;
                }

                try
                {
                    string manifestPath;
                    if (package.IsBundle)
                    {
                        manifestPath = @"AppxMetadata\AppxBundleManifest.xml";
                    }
                    else
                    {
                        manifestPath = "AppxManifest.xml";
                    }

                    manifestPath = Path.Combine(package.InstalledLocation.Path, manifestPath);
                    var manifest = new XmlDocument();
                    manifest.Load(manifestPath);

                    var apxApp = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Applications']//*[local-name() = 'Application'][1]");
                    var appId = apxApp.Attributes["Id"].Value;

                    var visuals = apxApp.SelectSingleNode(@"//*[local-name() = 'VisualElements']");
                    var iconPath = visuals.Attributes["Square150x150Logo"]?.Value;
                    if (iconPath.IsNullOrEmpty())
                    {
                        iconPath = visuals.Attributes["Square70x70Logo"]?.Value;
                        if (iconPath.IsNullOrEmpty())
                        {
                            iconPath = visuals.Attributes["Square44x44Logo"]?.Value;
                            if (iconPath.IsNullOrEmpty())
                            {
                                iconPath = visuals.Attributes["Logo"]?.Value;
                            }
                        }
                    }

                    if (!iconPath.IsNullOrEmpty())
                    {
                        iconPath = Path.Combine(package.InstalledLocation.Path, iconPath);
                        iconPath = GetUWPGameIcon(iconPath);
                    }

                    var name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Properties']/*[local-name() = 'DisplayName']").InnerText;
                    if (name.StartsWith("ms-resource"))
                    {
                        name = Resources.GetIndirectResourceString(package.Id.FullName, package.Id.Name, name);
                        if (name.IsNullOrEmpty())
                        {
                            name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Identity']").Attributes["Name"].Value;
                        }
                    }

                    var app = new Program()
                    {
                        Name = StringExtensions.NormalizeGameName(name),
                        WorkDir = package.InstalledLocation.Path,
                        Path = "explorer.exe",
                        Arguments = $"shell:AppsFolder\\{package.Id.FamilyName}!{appId}",
                        Icon = iconPath,
                        AppId = package.Id.FamilyName
                    };

                    apps.Add(app);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to parse UWP game info.");
                }
            }

            return apps;
        }

        private static List<UninstallProgram> GetUninstallProgsFromView(RegistryView view)
        {
            var rootString = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            void SearchRoot(RegistryHive hive, List<UninstallProgram> programs)
            {
                var root = RegistryKey.OpenBaseKey(hive, view);
                var keyList = root.OpenSubKey(rootString);
                if (keyList == null)
                {
                    return;
                }

                foreach (var key in keyList.GetSubKeyNames())
                {
                    var prog = root.OpenSubKey(rootString + key);
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

        public static Program ParseShortcut(string path)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(path);

            return new Program()
            {
                Path = link.TargetPath,
                Icon = link.IconLocation,
                Name = link.FullName,
                WorkDir = link.WorkingDirectory,
                Arguments = link.Arguments
            };
        }
    }
}
