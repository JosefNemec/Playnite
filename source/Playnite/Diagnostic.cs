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
using YamlDotNet.Serialization;
using Newtonsoft.Json;

namespace Playnite
{
    public static class Diagnostic
    {
        public static void CreateDiagPackage(string path)
        {
            var diagTemp = Path.Combine(Paths.TempPath, "diag");
            FileSystem.CreateFolder(diagTemp, true);
            FileSystem.DeleteFile(path);    
            
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


                    // Config 
                    if (File.Exists(Paths.ConfigFilePath))
                    {
                        archive.CreateEntryFromFile(Paths.ConfigFilePath, Path.GetFileName(Paths.ConfigFilePath));
                    }

                    // Origin data
                    var originContentPath = Path.Combine(Providers.Origin.OriginPaths.DataPath, "LocalContent");
                    if (Directory.Exists(originContentPath))
                    {
                        FileSystem.AddFolderToZip(archive, "Origin", originContentPath, ".dat|.mfst", SearchOption.AllDirectories);
                    }

                    // GOG data
                    if (GogSettings.IsInstalled)
                    {
                        var dbPath = Path.Combine(GogSettings.DBStoragePath, "index.db");
                        if (File.Exists(dbPath))
                        {
                            archive.CreateEntryFromFile(dbPath, "index.db");
                        }
                    }

                    // Steam data
                    if (SteamSettings.IsInstalled)
                    {
                        foreach (var folder in (new SteamLibrary()).GetLibraryFolders())
                        {
                            var appsFolder = Path.Combine(folder, "steamapps");
                            FileSystem.AddFolderToZip(archive, "Steam", appsFolder, "appmanifest*", SearchOption.TopDirectoryOnly);
                        }

                        if (File.Exists(SteamSettings.LoginUsersPath))
                        {
                            archive.CreateEntryFromFile(SteamSettings.LoginUsersPath, "loginusers.vdf");
                        }
                    }

                    // dxdiag
                    var diagPath = Path.Combine(diagTemp, "dxdiag.txt");
                    Process.Start("dxdiag", "/dontskip /whql:off /t " + diagPath).WaitForExit();
                    archive.CreateEntryFromFile(diagPath, Path.GetFileName(diagPath));

                    // Uninstall regkey export
                    var regKeyPath = Path.Combine(diagTemp, "uninstall.json");
                    var programs = Programs.GetUnistallProgramsList();
                    File.WriteAllText(regKeyPath, JsonConvert.SerializeObject(programs, Formatting.Indented));
                    archive.CreateEntryFromFile(regKeyPath, Path.GetFileName(regKeyPath));
                }
            }
        }
    }
}
