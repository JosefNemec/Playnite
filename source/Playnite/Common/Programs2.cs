﻿using Microsoft.Win32;
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
    public partial class Programs
    {
        public static async Task<List<Program>> GetExecutablesFromFolder(string path, SearchOption searchOption, CancellationTokenSource cancelToken = null)
        {
            return await Task.Run(() =>
            {
                var execs = new List<Program>();
                var files = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);

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

                    if (file.Extension.IsNullOrEmpty())
                    {
                        continue;
                    }

                    if (file.Extension.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true ||
                        file.Extension.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) == true ||
                        file.Extension.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        execs.Add(GetProgramData(file.FullName));
                    }
                }

                return execs;
            });
        }

        public static Program GetProgramData(string filePath)
        {
            var file = new FileInfo(filePath);
            if (file.Extension?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);
                var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : new DirectoryInfo(Path.GetDirectoryName(file.FullName)).Name;
                return new Program
                {
                    Path = file.FullName,
                    Icon = file.FullName,
                    WorkDir = Path.GetDirectoryName(file.FullName),
                    Name = programName
                };
            }
            else if (file.Extension?.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) == true)
            {
                var data = GetLnkShortcutData(file.FullName);
                var name = Path.GetFileNameWithoutExtension(file.Name);
                if (File.Exists(data.Path))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(data.Path);
                    name = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : Path.GetFileNameWithoutExtension(file.FullName);
                }

                var program = new Program
                {
                    Path = data.Path,
                    WorkDir = data.WorkDir,
                    Arguments = data.Arguments,
                    Name = name
                };

                if (!data.Icon.IsNullOrEmpty())
                {
                    var reg = Regex.Match(data.Icon, @"^(.+),(\d+)$");
                    if (reg.Success)
                    {
                        program.Icon = reg.Groups[1].Value;
                        program.IconIndex = int.Parse(reg.Groups[2].Value);
                    }
                    else
                    {
                        program.Icon = data.Icon;
                    }
                }
                else
                {
                    program.Icon = data.Path;
                }

                return program;
            }
            else if (file.Extension?.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new Program
                {
                    Path = file.FullName,
                    Name = Path.GetFileNameWithoutExtension(file.FullName),
                    WorkDir = Path.GetDirectoryName(file.FullName)
                };
            }

            throw new NotSupportedException("Only exe, bat and lnk files are supported.");
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

        public static Program GetLnkShortcutData(string lnkPath)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkPath);
            return new Program()
            {
                Path = link.TargetPath,
                Icon = link.IconLocation == ",0" ? link.TargetPath : link.IconLocation,
                Arguments = link.Arguments,
                WorkDir = link.WorkingDirectory,
                Name = link.FullName
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

            try
            {
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
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to get list of installed UWP apps.");
            }

            return apps;
        }
    }
}
