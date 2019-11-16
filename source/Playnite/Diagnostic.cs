using Playnite.Common;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Playnite
{
    public static class Diagnostic
    {
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
                    // Add log files
                    foreach (var logFile in Directory.GetFiles(PlaynitePaths.ConfigRootPath, "*.log", SearchOption.TopDirectoryOnly))
                    {
                        if (Path.GetFileName(logFile) == "cef.log")
                        {
                            continue;
                        }

                        archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                    }

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
                    File.WriteAllText(infoPath, Serialization.ToYaml(Computer.GetSystemInfo()));
                    archive.CreateEntryFromFile(infoPath, Path.GetFileName(infoPath));

                    // Uninstall regkey export
                    var regKeyPath = Path.Combine(diagTemp, "uninstall.txt");
                    var programs = Programs.GetUnistallProgramsList();
                    File.WriteAllText(regKeyPath, Serialization.ToYaml(programs));
                    archive.CreateEntryFromFile(regKeyPath, Path.GetFileName(regKeyPath));

                    // Playnite info
                    var playnitePath = Path.Combine(diagTemp, "playniteInfo.txt");
                    var playniteInfo = new Dictionary<string, object>
                    {
                        { "Version", Updater.GetCurrentVersion().ToString() },
                        { "Portable", PlayniteSettings.IsPortable }
                    };

                    File.WriteAllText(playnitePath, Serialization.ToYaml(playniteInfo));
                    archive.CreateEntryFromFile(playnitePath, Path.GetFileName(playnitePath));

                    // User actions description
                    if (!string.IsNullOrWhiteSpace(userActionsDescription))
                    {
                        var descriptionPath = Path.Combine(diagTemp, "userActions.txt");
                        File.WriteAllText(descriptionPath, userActionsDescription);
                        archive.CreateEntryFromFile(descriptionPath, Path.GetFileName(descriptionPath));
                    }
                }
            }

            FileSystem.DeleteDirectory(diagTemp);
        }
    }
}
