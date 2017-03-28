using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using NLog;
using Playnite.Providers.GOG;
using Playnite.Providers.Steam;
using Playnite.Providers.Origin;

namespace Playnite
{
    public static class Diagnostic
    {
        private static void addFolderToZip(ZipArchive archive, string zipRoot, string path, string filter)
        {
            IEnumerable<string> files;

            if (filter.Contains('|'))
            {
                var filters = filter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(a =>
                {
                    return filters.Contains(Path.GetExtension(a));
                });
            }
            else
            {
                files = Directory.EnumerateFiles(path, filter, SearchOption.AllDirectories);
            }

            foreach (var file in files)
            {
                var archiveName = zipRoot + file.Replace(path, "").Replace(@"\", @"/");
                archive.CreateEntryFromFile(file, archiveName);
            }
        }

        public static void CreateDiagPackage(string path)
        {
            FileSystem.CreateFolder(Paths.TempPath, true);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            ZipFile.CreateFromDirectory(Paths.TempPath, path);
            
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    foreach (var logFile in Directory.GetFiles(Paths.ConfigRootPath, "*.log", SearchOption.TopDirectoryOnly))
                    {
                        if (Path.GetFileName(logFile) == "cef.log")
                        {
                            continue;
                        }

                        archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                    }

                    archive.CreateEntryFromFile(Paths.ConfigFilePath, Path.GetFileName(Paths.ConfigFilePath));

                    var originContentPath = Path.Combine(Providers.Origin.OriginPaths.DataPath, "LocalContent");
                    if (Directory.Exists(originContentPath))
                    {
                        addFolderToZip(archive, "Origin", originContentPath, ".dat|.mfst");
                    }

                    if (GogSettings.IsInstalled)
                    {
                        archive.CreateEntryFromFile(Path.Combine(GogSettings.DBStoragePath, "index.db"), "index.db");
                    }

                    if (SteamSettings.IsInstalled)
                    {
                        foreach (var folder in SteamSettings.GameDatabases)
                        {
                            var appsFolder = Path.Combine(folder, "steamapps");
                            addFolderToZip(archive, "Steam", appsFolder, "appmanifest*");
                        }
                    }
                }
            }
        }
    }
}
