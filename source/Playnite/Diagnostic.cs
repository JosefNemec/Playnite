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
using System.Diagnostics;

namespace Playnite
{
    public static class Diagnostic
    {
        private static void AddFolderToZip(ZipArchive archive, string zipRoot, string path, string filter, SearchOption searchOption)
        {
            IEnumerable<string> files;

            if (filter.Contains('|'))
            {
                var filters = filter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                files = Directory.EnumerateFiles(path, "*.*", searchOption).Where(a =>
                {
                    return filters.Contains(Path.GetExtension(a));
                });
            }
            else
            {
                files = Directory.EnumerateFiles(path, filter, searchOption);
            }

            foreach (var file in files)
            {
                var archiveName = zipRoot + file.Replace(path, "").Replace(@"\", @"/");
                archive.CreateEntryFromFile(file, archiveName);
            }
        }

        public static void CreateDiagPackage(string path)
        {
            var diagTemp = Path.Combine(Paths.TempPath, "diag");

            FileSystem.CreateFolder(diagTemp, true);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            ZipFile.CreateFromDirectory(diagTemp, path);
            
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    // Add log files
                    foreach (var logFile in Directory.GetFiles(Paths.ConfigRootPath, "*.log", SearchOption.TopDirectoryOnly))
                    {
                        if (Path.GetFileName(logFile) == "cef.log")
                        {
                            continue;
                        }

                        archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                    }

                    // Config file
                    archive.CreateEntryFromFile(Paths.ConfigFilePath, Path.GetFileName(Paths.ConfigFilePath));

                    // Origin data
                    var originContentPath = Path.Combine(Providers.Origin.OriginPaths.DataPath, "LocalContent");
                    if (Directory.Exists(originContentPath))
                    {
                        AddFolderToZip(archive, "Origin", originContentPath, ".dat|.mfst", SearchOption.AllDirectories);
                    }

                    // GOG data
                    if (GogSettings.IsInstalled)
                    {
                        archive.CreateEntryFromFile(Path.Combine(GogSettings.DBStoragePath, "index.db"), "index.db");
                    }

                    // Steam data
                    if (SteamSettings.IsInstalled)
                    {
                        foreach (var folder in (new SteamLibrary()).GetLibraryFolders())
                        {
                            var appsFolder = Path.Combine(folder, "steamapps");
                            AddFolderToZip(archive, "Steam", appsFolder, "appmanifest*", SearchOption.TopDirectoryOnly);
                        }
                    }

                    // dxdiag
                    var diagPath = Path.Combine(diagTemp, "dxdiag.txt");
                    Process.Start("dxdiag", "/dontskip /whql:off /t " + diagPath).WaitForExit();
                    archive.CreateEntryFromFile(diagPath, Path.GetFileName(diagPath));
                }
            }
        }
    }
}
