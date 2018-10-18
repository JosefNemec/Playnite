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
                    GameDatabase.CloneLibrary(sourceDb, targetDb);
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
                    GameDatabase.CloneLibrary(dbTempPath, dbPath);
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
    }
}
