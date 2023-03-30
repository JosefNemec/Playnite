using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Vanara.Windows.Shell;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Playnite;

public class Program
{
    public string? Path { get; set; }
    public string? Arguments { get; set; }
    public string? Icon { get; set; }
    public int IconIndex { get; set; }
    public string? WorkDir { get; set; }
    public string? Name { get; set; }
    public string? AppId { get; set; }

    public override string? ToString()
    {
        return Name ?? base.ToString();
    }
}

public class UninstallProgram
{
    public string? DisplayIcon { get; set; }
    public string? DisplayName { get; set; }
    public string? DisplayVersion { get; set; }
    public string? InstallLocation { get; set; }
    public string? Publisher { get; set; }
    public string? UninstallString { get; set; }
    public string? URLInfoAbout { get; set; }
    public string? RegistryKeyName { get; set; }
    public string? Path { get; set; }

    public override string? ToString()
    {
        return DisplayName ?? RegistryKeyName ?? base.ToString();
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

    private static readonly ILogger logger = LogManager.GetLogger();

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
            using var root = RegistryKey.OpenBaseKey(hive, view);
            var keyList = root.OpenSubKey(rootString);
            if (keyList == null)
            {
                return;
            }

            foreach (var key in keyList.GetSubKeyNames())
            {
                try
                {
                    using var prog = root.OpenSubKey(rootString + key);
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
                catch (System.Security.SecurityException e)
                {
                    logger.Warn(e, $"Failed to read registry key {rootString + key}");
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
        progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry64));
        progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry32));
        return progs;
    }

    public static async Task<List<Program>> GetExecutablesFromFolder(string path, SearchOption searchOption, CancellationToken cancelToken)
    {
        return await Task.Run(() =>
        {
            var execs = new List<Program>();
            var files = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (cancelToken.IsCancellationRequested == true)
                {
                    return execs;
                }

                if (file.Attributes.HasFlag(FileAttributes.Directory))
                {
                    continue;
                }

                if (IsFileScanExcluded(file.Name))
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
            var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : new DirectoryInfo(Path.GetDirectoryName(file.FullName)!).Name;
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
            var program = GetLnkShortcutData(file.FullName);
            var name = Path.GetFileNameWithoutExtension(file.Name);
            if (File.Exists(program.Path))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(program.Path);
                name = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : Path.GetFileNameWithoutExtension(file.FullName);
            }

            program.Name = name;
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

    public static void CreateShortcut(string outLinkPath, string executablePath, string? arguments = null, string? workingDir = null, string? iconPath = null, int iconIndex = 0)
    {
        FileSystem.PrepareSaveFile(outLinkPath);
        using var link = ShellLink.Create(outLinkPath, executablePath, null, workingDir, arguments);
        if (!iconPath.IsNullOrEmpty())
        {
            link.IconLocation = new IconLocation(iconPath, iconIndex);
        }
    }

    public static Program GetLnkShortcutData(string lnkPath)
    {
        using var link = new ShellLink(lnkPath);
        var program = new Program()
        {
            Path = link.TargetPath,
            Arguments = link.Arguments,
            WorkDir = link.WorkingDirectory,
            Name = link.Name,
            Icon = link.TargetPath
        };

        if (!link.IconLocation.RawValue.IsNullOrWhiteSpace())
        {
            var reg = Regex.Match(link.IconLocation.RawValue, @"^@(.+),(\d+)$");
            if (reg.Success)
            {
                program.Icon = reg.Groups[1].Value;
                program.IconIndex = int.Parse(reg.Groups[2].Value);
            }
        }

        return program;
    }

    public static async Task<List<Program>> GetShortcutProgramsFromFolder(string path, CancellationToken cancelToken)
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

            var apps = new List<Program>();
            var shortucts = new SafeFileEnumerator(path, "*.lnk", SearchOption.AllDirectories);

            foreach (var shortcut in shortucts)
            {
                if (cancelToken.IsCancellationRequested == true)
                {
                    return apps;
                }

                if (shortcut.Attributes.HasFlag(FileAttributes.Directory))
                {
                    continue;
                }

                var fileName = shortcut.Name;
                var Directory = Path.GetDirectoryName(shortcut.FullName);

                if (folderExceptions.FirstOrDefault(a => shortcut.FullName.Contains(a, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    continue;
                }

                var link = GetLnkShortcutData(shortcut.FullName);
                var target = link.Path;
                if (target.IsNullOrEmpty())
                {
                    continue;
                }

                if (pathExceptions.FirstOrDefault(a => target.Contains(a, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    continue;
                }

                // Ignore uninstallers, config, redistributables and game engine executables
                if (IsFileScanExcluded(Path.GetFileName(target)))
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
                    Icon = link.Icon,
                    Name = Path.GetFileNameWithoutExtension(shortcut.Name),
                    WorkDir = link.WorkDir
                };

                apps.Add(app);
            }

            return apps;
        });
    }

    public static async Task<List<Program>> GetInstalledPrograms(CancellationToken cancelToken)
    {
        var apps = new List<Program>();

        // Get apps from All Users
        var allPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
        var allApps = await GetShortcutProgramsFromFolder(allPath, cancelToken);
        if (cancelToken.IsCancellationRequested == true)
        {
            return apps;
        }
        else
        {
            apps.AddRange(allApps);
        }

        // Get current user apps
        var userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
        var userApps = await GetShortcutProgramsFromFolder(userPath, cancelToken);
        if (cancelToken.IsCancellationRequested == true)
        {
            return apps;
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

        var folder = Path.GetDirectoryName(defPath)!;
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
            using var indentity = WindowsIdentity.GetCurrent();
            if (indentity.User == null)
            {
                logger.Error("Can't get UWP apps, not user returned by identity API.");
                return apps;
            }

            foreach (var package in manager.FindPackagesForUser(indentity.User.Value))
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
                    using (var stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        manifest.Load(stream);
                    }

                    var apxApp = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Applications']//*[local-name() = 'Application'][1]");
                    if (apxApp?.Attributes == null)
                    {
                        continue;
                    }

                    var appId = apxApp.Attributes["Id"]?.Value;
                    if (appId.IsNullOrEmpty())
                    {
                        continue;
                    }

                    string? iconPath = null;
                    var visuals = apxApp.SelectSingleNode(@"//*[local-name() = 'VisualElements']");
                    if (visuals?.Attributes != null)
                    {
                        iconPath = visuals.Attributes["Square150x150Logo"]?.Value;
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
                    }

                    var name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Properties']/*[local-name() = 'DisplayName']")?.InnerText;
                    if (name?.StartsWith("ms-resource", StringComparison.Ordinal) == true)
                    {
                        name = Resources.GetIndirectResourceString(package.Id.FullName, package.Id.Name, name);
                        if (name.IsNullOrEmpty())
                        {
                            name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Identity']")?.Attributes?["Name"]?.Value;
                        }
                    }

                    var app = new Program()
                    {
                        Name = StringExtensions.NormalizeGameName(name ?? package.DisplayName),
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
                    logger.Error(e, $"Failed to parse UWP app {package.Id.FullName} info.");
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
