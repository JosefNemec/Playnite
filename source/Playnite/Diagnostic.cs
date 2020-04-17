using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

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

        public static void CreateDiagPackage(string path, string userActionsDescription)
        {
            var diagTemp = Path.Combine(PlaynitePaths.TempPath, "diag");
            FileSystem.CreateDirectory(diagTemp, true);
            FileSystem.DeleteFile(path);

            ZipFile.CreateFromDirectory(diagTemp, path);
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
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

                    // System Info
                    var infoPath = Path.Combine(diagTemp, "sysinfo.txt");
                    File.WriteAllText(infoPath, Serialization.ToJson(Computer.GetSystemInfo(), true));
                    archive.CreateEntryFromFile(infoPath, Path.GetFileName(infoPath));

                    // Uninstall regkey export
                    var regKeyPath = Path.Combine(diagTemp, "uninstall.txt");
                    var programs = Programs.GetUnistallProgramsList();
                    File.WriteAllText(regKeyPath, Serialization.ToJson(programs, true));
                    archive.CreateEntryFromFile(regKeyPath, Path.GetFileName(regKeyPath));

                    // UWP app info
                    if (Computer.WindowsVersion == WindowsVersion.Win10)
                    {
                        var uwpInfoPath = Path.Combine(diagTemp, "uwp.txt");
                        var uwpApps = Programs.GetUWPApps();
                        File.WriteAllText(uwpInfoPath, Serialization.ToJson(uwpApps, true));
                        archive.CreateEntryFromFile(uwpInfoPath, Path.GetFileName(uwpInfoPath));
                    }

                    // Playnite info
                    var playnitePath = Path.Combine(diagTemp, "playniteInfo.txt");
                    var playniteInfo = new Dictionary<string, object>
                    {
                        { "Version", Updater.GetCurrentVersion().ToString() },
                        { "Portable", PlayniteSettings.IsPortable },
                        { "Memory", (PlayniteProcess.WorkingSetMemory / 1024f) / 1024f },
                        { "Path", PlayniteProcess.Path },
                        { "Cmdline", PlayniteProcess.Cmdline }
                    };

                    File.WriteAllText(playnitePath, Serialization.ToJson(playniteInfo, true));
                    archive.CreateEntryFromFile(playnitePath, Path.GetFileName(playnitePath));

                    // Program file list
                    var fileListPath = Path.Combine(diagTemp, "fileList.txt");
                    File.WriteAllText(fileListPath, string.Join(Environment.NewLine, GetPlayniteFilesList()));
                    archive.CreateEntryFromFile(fileListPath, Path.GetFileName(fileListPath));

                    // User actions description
                    if (!string.IsNullOrWhiteSpace(userActionsDescription))
                    {
                        var descriptionPath = Path.Combine(diagTemp, "userActions.txt");
                        File.WriteAllText(descriptionPath, userActionsDescription);
                        archive.CreateEntryFromFile(descriptionPath, Path.GetFileName(descriptionPath));
                    }

                    // Add log files
                    foreach (var logFile in Directory.GetFiles(PlaynitePaths.ConfigRootPath, "*.log", SearchOption.TopDirectoryOnly))
                    {
                        if (Path.GetFileName(logFile) == "cef.log")
                        {
                            continue;
                        }

                        archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                    }
                }
            }

            FileSystem.DeleteDirectory(diagTemp);
        }
    }
}
