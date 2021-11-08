using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Playnite
{
    public static class Diagnostic
    {
        private static ILogger logger = LogManager.GetLogger();

        private static List<string> GetPlayniteFilesList()
        {
            var progPath = PlaynitePaths.ProgramPath;
            var allFiles = new List<string>();
            foreach (var file in Directory.GetFiles(progPath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    var subName = file.Replace(progPath, "").Trim(Path.DirectorySeparatorChar);
                    if (subName.StartsWith("library", StringComparison.OrdinalIgnoreCase) ||
                        subName.StartsWith("browsercache", StringComparison.OrdinalIgnoreCase) ||
                        subName.StartsWith("cache", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var fInfo = new FileInfo(file);
                    allFiles.Add(subName + ", " + fInfo.Length);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get information about Playnite file {file}");
                }
            }

            return allFiles;
        }

        private static string GetManifestInfo(string rootDir, string manFileName)
        {
            if (!Directory.Exists(rootDir))
            {
                return string.Empty;
            }

            var total = new StringBuilder();
            foreach (var dir in Directory.GetDirectories(rootDir))
            {
                var manifest = Path.Combine(dir, manFileName);
                if (File.Exists(manifest))
                {
                    total.Append(Path.GetFileName(dir));
                    total.AppendLine();
                    total.AppendLine("--------------------------");
                    total.Append(File.ReadAllText(manifest));
                    total.AppendLine();
                    total.AppendLine();
                }
            }

            return total.ToString();
        }

        public static void CreateDiagPackage(string path, string userActionsDescription, DiagnosticPackageInfo packageInfo)
        {
            var diagTemp = Path.Combine(PlaynitePaths.TempPath, "diag");
            FileSystem.CreateDirectory(diagTemp, true);
            FileSystem.DeleteFile(path);

            ZipFile.CreateFromDirectory(diagTemp, path);
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    // Package info
                    var packagePath = Path.Combine(diagTemp, DiagnosticPackageInfo.PackageInfoFileName);
                    File.WriteAllText(packagePath, Serialization.ToJson(packageInfo));
                    archive.CreateEntryFromFile(packagePath, Path.GetFileName(packagePath));

                    // Config
                    if (Directory.Exists(PlaynitePaths.ConfigRootPath))
                    {
                        foreach (var cfg in Directory.GetFiles(PlaynitePaths.ConfigRootPath, "*.json"))
                        {
                            var fileInfo = new FileInfo(cfg);
                            archive.CreateEntryFromFile(cfg, fileInfo.Name);
                        }
                    }

                    // Extension configs
                    if (Directory.Exists(PlaynitePaths.ExtensionsDataPath))
                    {
                        foreach (var cfg in Directory.GetFiles(PlaynitePaths.ExtensionsDataPath, "config.json", SearchOption.AllDirectories))
                        {
                            var fileInfo = new FileInfo(cfg);
                            archive.CreateEntryFromFile(cfg, Path.Combine("extensions", fileInfo.Directory.Name, fileInfo.Name));
                        }
                    }

                    // Installed extensions/themes
                    try
                    {
                        var extensionsPath = Path.Combine(diagTemp, "extensions.txt");
                        File.WriteAllText(extensionsPath, GetManifestInfo(PlaynitePaths.ExtensionsProgramPath, PlaynitePaths.ExtensionManifestFileName));
                        File.AppendAllText(extensionsPath, GetManifestInfo(PlaynitePaths.ThemesProgramPath, PlaynitePaths.ThemeManifestFileName));
                        if (!PlayniteSettings.IsPortable)
                        {
                            File.AppendAllText(extensionsPath, GetManifestInfo(PlaynitePaths.ExtensionsUserDataPath, PlaynitePaths.ExtensionManifestFileName));
                            File.AppendAllText(extensionsPath, GetManifestInfo(PlaynitePaths.ThemesUserDataPath, PlaynitePaths.ThemeManifestFileName));
                        }

                        archive.CreateEntryFromFile(extensionsPath, Path.GetFileName(extensionsPath));
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to package extensions list.");
                    }

                    // System Info
                    try
                    {
                        var infoPath = Path.Combine(diagTemp, "sysinfo.txt");
                        File.WriteAllText(infoPath, Serialization.ToJson(Computer.GetSystemInfo(), true));
                        archive.CreateEntryFromFile(infoPath, Path.GetFileName(infoPath));
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed gather system info.");
                    }

                    // Uninstall regkey export
                    try
                    {
                        var regKeyPath = Path.Combine(diagTemp, "uninstall.txt");
                        var programs = Programs.GetUnistallProgramsList();
                        File.WriteAllText(regKeyPath, Serialization.ToJson(programs, true));
                        archive.CreateEntryFromFile(regKeyPath, Path.GetFileName(regKeyPath));
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed gather install app list.");
                    }

                    // UWP app info
                    try
                    {
                        if (Computer.WindowsVersion == WindowsVersion.Win10 || Computer.WindowsVersion == WindowsVersion.Win11)
                        {
                            var uwpInfoPath = Path.Combine(diagTemp, "uwp.txt");
                            var uwpApps = Programs.GetUWPApps();
                            File.WriteAllText(uwpInfoPath, Serialization.ToJson(uwpApps, true));
                            archive.CreateEntryFromFile(uwpInfoPath, Path.GetFileName(uwpInfoPath));
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed gather UWP install list.");
                    }

                    // Playnite info
                    var playnitePath = Path.Combine(diagTemp, "playniteInfo.txt");
                    var playniteInfo = new Dictionary<string, object>
                    {
                        { "Version", Updater.CurrentVersion.ToString() },
                        { "Portable", PlayniteSettings.IsPortable },
                        { "Memory", (PlayniteProcess.WorkingSetMemory / 1024f) / 1024f },
                        { "Path", PlayniteProcess.Path },
                        { "Cmdline", PlayniteProcess.Cmdline },
                        { "Playnite.DesktopApp.exe_MD5", FileSystem.GetMD5(PlaynitePaths.DesktopExecutablePath) },
                        { "Playnite.FullscreenApp.exe_MD5", FileSystem.GetMD5(PlaynitePaths.FullscreenExecutablePath) },
                        { "Playnite.dll_MD5", FileSystem.GetMD5(PlaynitePaths.PlayniteAssemblyPath) },
                        { "Playnite.SDK.dll_MD5", FileSystem.GetMD5(PlaynitePaths.PlayniteSDKAssemblyPath) }
                    };

                    File.WriteAllText(playnitePath, Serialization.ToJson(playniteInfo, true));
                    archive.CreateEntryFromFile(playnitePath, Path.GetFileName(playnitePath));

                    // Program file list
                    try
                    {
                        var fileListPath = Path.Combine(diagTemp, "fileList.txt");
                        File.WriteAllText(fileListPath, string.Join(Environment.NewLine, GetPlayniteFilesList()));
                        archive.CreateEntryFromFile(fileListPath, Path.GetFileName(fileListPath));
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to pack app file list.");
                    }

                    // User actions description
                    if (!string.IsNullOrWhiteSpace(userActionsDescription))
                    {
                        var descriptionPath = Path.Combine(diagTemp, "userActions.txt");
                        File.WriteAllText(descriptionPath, userActionsDescription);
                        archive.CreateEntryFromFile(descriptionPath, Path.GetFileName(descriptionPath));
                    }

                    void addCefLog(string logPath, ZipArchive archiveObj)
                    {
                        try
                        {
                            var cefEntry = archive.CreateEntry(Path.GetFileName(logPath));
                            using (var cefS = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var writer = new StreamWriter(cefEntry.Open()))
                            {
                                cefS.CopyTo(writer.BaseStream);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Failed to pack CEF log.");
                        }
                    }

                    // Add log files
                    foreach (var logFile in Directory.GetFiles(PlaynitePaths.ConfigRootPath, "*.log", SearchOption.TopDirectoryOnly))
                    {
                        if (Path.GetFileName(logFile) == "cef.log" || Path.GetFileName(logFile) == "debug.log")
                        {
                            addCefLog(logFile, archive);
                        }
                        else
                        {
                            archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                        }
                    }
                }
            }

            FileSystem.DeleteDirectory(diagTemp);
        }

        public static void CreateLogPackage(string path)
        {
            FileSystem.DeleteFile(path);
            using (FileStream zipToOpen = new FileStream(path, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
            {
                foreach (var logFile in Directory.GetFiles(PlaynitePaths.ConfigRootPath, "*.log", SearchOption.TopDirectoryOnly))
                {
                    if (Path.GetFileName(logFile) == "cef.log" || Path.GetFileName(logFile) == "debug.log")
                    {
                        continue;
                    }
                    else
                    {
                        archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                    }
                }
            }
        }
    }
}