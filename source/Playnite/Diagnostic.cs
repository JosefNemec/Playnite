using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Newtonsoft.Json;
using Playnite.Settings;

namespace Playnite
{
    public static class Diagnostic
    {
        public static void CreateDiagPackage(string path)
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
                    if (File.Exists(PlaynitePaths.ConfigFilePath))
                    {
                        archive.CreateEntryFromFile(PlaynitePaths.ConfigFilePath, Path.GetFileName(PlaynitePaths.ConfigFilePath));
                    }

                    // dxdiag
                    var diagPath = Path.Combine(diagTemp, "dxdiag.txt");
                    Process.Start("dxdiag", "/dontskip /whql:off /t " + diagPath).WaitForExit();
                    archive.CreateEntryFromFile(diagPath, Path.GetFileName(diagPath));

                    // Uninstall regkey export
                    var regKeyPath = Path.Combine(diagTemp, "uninstall.json");
                    var programs = Common.System.Programs.GetUnistallProgramsList();
                    File.WriteAllText(regKeyPath, JsonConvert.SerializeObject(programs, Formatting.Indented));
                    archive.CreateEntryFromFile(regKeyPath, Path.GetFileName(regKeyPath));
                }
            }
        }
    }
}
