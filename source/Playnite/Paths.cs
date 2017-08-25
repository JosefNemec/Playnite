using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class Paths
    {
        public static string ProgramFolder
        {
            get
            {
                return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        public static string ExecutablePath
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly().Location;
            }
        }

        public static string UninstallerPath
        {
            get
            {
                return Path.Combine(ProgramFolder, "uninstall.exe");
            }
        }

        public static string BrowserCachePath
        {
            get
            {
                return Path.Combine(ConfigRootPath, "browsercache");
            }
        }

        public static string TempPath
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), "Playnite");
            }
        }

        public static string ConfigFilePath
        {
            get
            {
                return Path.Combine(ConfigRootPath, "config.json");
            }
        }

        public static string DataCachePath
        {
            get
            {
                return Path.Combine(ConfigRootPath, "cache");
            }
        }

        public static string ImagesCachePath
        {
            get
            {
                return Path.Combine(DataCachePath, "images");
            }
        }

        public static string ConfigRootPath
        {
            get
            {
                if (Settings.IsPortable)
                {
                    return ProgramFolder;
                }
                else
                {
                    return UserProgramDataPath;
                }
            }
        }

        public static string UserProgramDataPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite");
            }
        }

        public static bool GetValidFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }            
            
            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                return false;
            }

            string drive = Path.GetPathRoot(path);
            if (!string.IsNullOrEmpty(drive) && !Directory.Exists(drive))
            {
                return false;
            }

            return true;
        }
    }
}
