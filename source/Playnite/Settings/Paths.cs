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
        public static string ProgramPath
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

        public static string PluginsUserDataPath
        {
            get
            {
                return Path.Combine(ConfigRootPath, "Plugins");
            }
        }

        public static string PluginsProgramPath
        {
            get
            {
                return Path.Combine(ProgramPath, "Plugins");
            }
        }

        public static string ScriptsUserDataPath
        {
            get
            {
                return Path.Combine(ConfigRootPath, "Scripts");
            }
        }

        public static string ScriptsProgramPath
        {
            get
            {
                return Path.Combine(ProgramPath, "Scripts");
            }
        }

        public static string LocalizationsPath
        {
            get
            {
                return Path.Combine(ProgramPath, "Localization");
            }
        }

        public static string ThemesPath
        {
            get
            {
                return Path.Combine(ProgramPath, "Skins");
            }
        }

        public static string ThemesFullscreenPath
        {
            get
            {
                return Path.Combine(ProgramPath, "SkinsFullscreen");
            }
        }

        public static string UninstallerInnoPath
        {
            get
            {
                return Path.Combine(ProgramPath, "unins000.exe");
            }
        }

        public static string UninstallerNsisPath
        {
            get
            {
                return Path.Combine(ProgramPath, "uninstall.exe");
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
                    return ProgramPath;
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

        public static string FixSeparators(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var newPath = path.Replace('\\', Path.DirectorySeparatorChar);
            return newPath.Replace('/', Path.DirectorySeparatorChar);
        }

        private static string Normalize(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
        }

        public static bool AreEqual(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) && !string.IsNullOrEmpty(path2))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
            {
                return false;
            }

            // Empty string is not valid path, return false even when both are null
            if (string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
            {
                return false;
            }

            return Normalize(path1) == Normalize(path2);
        }
    }
}
