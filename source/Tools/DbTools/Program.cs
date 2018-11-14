using LiteDB;
using Playnite;
using Playnite.Database;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 2)
            {
                var sourceDb = args[0];
                var targetDb = args[1];

                if (!File.Exists(sourceDb))
                {
                    Console.WriteLine("Source database file not found!");
                    return;
                }

                try
                {
                    CloneLibrary(sourceDb, targetDb);
                }
                catch (Exception e) when (!Debugger.IsAttached)
                {
                    Console.WriteLine($"Failed to clone database: {e.Message}");
                }
            }
            else
            {
                try
                {
                    var settings = PlayniteSettings.LoadSettings();
                    var dbPath = settings.DatabasePath;
                    var dbTempPath = dbPath + "temp";

                    if (File.Exists(dbTempPath))
                    {
                        File.Delete(dbTempPath);
                    }

                    if (!File.Exists(dbPath))
                    {
                        Console.WriteLine($"Couldn't find {dbPath} database file.");
                        return;
                    }

                    File.Move(dbPath, dbTempPath);

                    Console.WriteLine($"Fixing {dbPath} file...");
                    CloneLibrary(dbTempPath, dbPath);
                    File.Delete(dbTempPath);
                    Console.WriteLine("Finished fixing database");
                }
                catch (Exception e) when (!Debugger.IsAttached)
                {
                    Console.WriteLine($"Failed to clone database: {e.Message}");
                }
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        public static void CloneLibrary(string dbPath, string targetPath)
        {
            using (var sourceDb = new LiteDatabase($"Filename={dbPath};Mode=Exclusive"))
            {
                using (var targetDb = new LiteDatabase($"Filename={targetPath};Mode=Exclusive"))
                {
                    var games = sourceDb.GetCollection("games").FindAll();
                    var targetGames = targetDb.GetCollection("games");
                    foreach (var game in games)
                    {
                        targetGames.Insert(game);
                    }

                    var targetPlatforms = targetDb.GetCollection("platforms");
                    foreach (var platform in sourceDb.GetCollection("platforms").FindAll())
                    {
                        targetPlatforms.Insert(platform);
                    }

                    var targetEmulators = targetDb.GetCollection("emulators");
                    foreach (var emulator in sourceDb.GetCollection("emulators").FindAll())
                    {
                        targetEmulators.Insert(emulator);
                    }

                    var targetSettings = targetDb.GetCollection("settings");
                    foreach (var setting in sourceDb.GetCollection("settings").FindAll())
                    {
                        targetSettings.Insert(setting);
                    }

                    foreach (var file in sourceDb.FileStorage.FindAll())
                    {
                        using (var fileStream = file.OpenRead())
                        {
                            targetDb.FileStorage.Upload(file.Id, file.Filename, fileStream);
                        }
                    }

                    targetDb.Engine.UserVersion = sourceDb.Engine.UserVersion;
                }
            }
        }
    }
}
