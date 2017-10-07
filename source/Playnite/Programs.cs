using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Playnite
{
    public class Program
    {
        public string Path
        {
            get; set;
        }

        public string Arguments
        {
            get; set;
        }

        public string Icon
        {
            get; set;
        }

        public string WorkDir
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class UninstallProgram
    {
        public string DisplayIcon
        {
            get; set;
        }

        public string DisplayName
        {
            get; set;
        }

        public string DisplayVersion
        {
            get; set;
        }

        public string InstallLocation
        {
            get; set;
        }

        public string Publisher
        {
            get; set;
        }

        public string UninstallString
        {
            get; set;
        }

        public string URLInfoAbout
        {
            get; set;
        }

        public string RegistryKeyName
        {
            get; set;
        }
    }

    public class Programs
    {
        public static void CreateShortcut(string executablePath, string arguments, string iconPath, string shortuctPath)
        {
            var shell = new WshShell();
            var link = (IWshShortcut)shell.CreateShortcut(shortuctPath);
            link.TargetPath = executablePath;
            link.WorkingDirectory = Path.GetDirectoryName(executablePath);
            link.Arguments = arguments;
            link.IconLocation = string.IsNullOrEmpty(iconPath) ? executablePath + ",0" : iconPath;
            link.Save();
        }

        public static List<Program> GetExecutablesFromFolder(string path, SearchOption searchOption)
        {
            var execs = new List<Program>();
            var files = new SafeFileEnumerator(path, "*.exe", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                execs.Add(new Program()
                {
                    Path = file.FullName,
                    Icon = file.FullName,
                    WorkDir = Path.GetDirectoryName(file.FullName),
                    Name = new DirectoryInfo(Path.GetDirectoryName(file.FullName)).Name
                });
            }

            return execs;
        }

        public static List<Program> GetShortcutProgramsFromFolder(string path)
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

            var nameExceptions = new string[]
            {
                "uninstall",
                "setup"
            };

            var pathExceptions = new string[]
            {
                @"\system32\",
                @"\windows\",
            };

            var shell = new WshShell();
            var apps = new List<Program>();
            var shortucts = new SafeFileEnumerator(path, "*.lnk", SearchOption.AllDirectories);

            foreach (var shortcut in shortucts)
            {
                var fileName = shortcut.Name;
                var Directory = Path.GetDirectoryName(shortcut.FullName);

                if (folderExceptions.FirstOrDefault(a => shortcut.FullName.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                {
                    continue;
                }

                if (nameExceptions.FirstOrDefault(a => shortcut.FullName.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                {
                    continue;
                }

                var link = (IWshShortcut)shell.CreateShortcut(shortcut.FullName);               
                var target = link.TargetPath;
                
                if (pathExceptions.FirstOrDefault(a => target.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
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
        }

        public static List<Program> GetInstalledPrograms()
        {
            var apps = new List<Program>();

            // Get apps from All Users
            var allPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            apps.AddRange(GetShortcutProgramsFromFolder(allPath));

            // Get current user apps
            var userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            apps.AddRange(GetShortcutProgramsFromFolder(userPath));

            return apps;
        }

        private static string GetUWPGameIcon(string defPath)
        {
            if (System.IO.File.Exists(defPath))
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
                return files.Where(a => Regex.IsMatch(a, @"\.scale-\d+\.png"))?.OrderBy(a => a).Last();
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
                var iconPath = visuals.Attributes["Square44x44Logo"]?.Value;
                if (string.IsNullOrEmpty(iconPath))
                {
                    iconPath = visuals.Attributes["Logo"]?.Value;
                }

                if (!string.IsNullOrEmpty(iconPath))
                {
                    iconPath = Path.Combine(package.InstalledLocation.Path, iconPath);
                    iconPath = GetUWPGameIcon(iconPath);
                }

                var name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Properties']/*[local-name() = 'DisplayName']").InnerText;
                if (name.StartsWith("ms-resource"))
                {
                    name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Identity']").Attributes["Name"].Value;
                }

                var app = new Program()
                {
                    Name = name,
                    WorkDir = string.Empty,
                    Path = "explorer.exe",
                    Arguments = $"shell:AppsFolder\\{package.Id.FamilyName}!{appId}",
                    Icon = iconPath
                };

                apps.Add(app);
            }

            return apps;
        }

        private static List<UninstallProgram> GetUninstallProgsFromView(RegistryView view)
        {
            var rootString = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            var progs = new List<UninstallProgram>();
            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            var keyList = root.OpenSubKey(rootString);

            foreach (var key in keyList.GetSubKeyNames())
            {
                var prog = root.OpenSubKey(rootString + key);
                var program = new UninstallProgram()
                {
                    DisplayIcon = prog.GetValue("DisplayIcon")?.ToString(),
                    DisplayVersion = prog.GetValue("DisplayVersion")?.ToString(),
                    DisplayName = prog.GetValue("DisplayName")?.ToString(),
                    InstallLocation = prog.GetValue("InstallLocation")?.ToString(),
                    Publisher = prog.GetValue("Publisher")?.ToString(),
                    UninstallString = prog.GetValue("UninstallString")?.ToString(),
                    URLInfoAbout = prog.GetValue("URLInfoAbout")?.ToString(),
                    RegistryKeyName = key
                };

                progs.Add(program);
            }

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
