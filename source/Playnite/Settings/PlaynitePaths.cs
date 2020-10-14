using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class PlaynitePaths
    {
        public const string ExtensionManifestFileName = "extension.yaml";
        public const string ThemeManifestFileName = "theme.yaml";
        public const string PackedThemeFileExtention = ".pthm";
        public const string PackedExtensionFileExtention = ".pext";
        public const string EngLocSourceFileName = "LocSource.xaml";

        public const string ThemeSlnFileName = "Theme.sln";
        public const string ThemeProjFileName = "Theme.csproj";
        public const string AppXamlFileName = "App.xaml";

        public static string ProgramPath => Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        public static string DesktopExecutablePath => Path.Combine(ProgramPath, "Playnite.DesktopApp.exe");
        public static string FullscreenExecutablePath => Path.Combine(ProgramPath, "Playnite.FullscreenApp.exe");
        public static string PlayniteAssemblyPath => Path.Combine(ProgramPath, "Playnite.dll");
        public static string PlayniteCommonAssemblyPath => Path.Combine(ProgramPath, "Playnite.Common.dll");
        public static string PlayniteSDKAssemblyPath => Path.Combine(ProgramPath, "Playnite.SDK.dll");
        public static string ExtensionsUserDataPath => Path.Combine(ConfigRootPath, "Extensions");
        public static string ExtensionsProgramPath => Path.Combine(ProgramPath, "Extensions");
        public static string ExtensionsDataPath => Path.Combine(ConfigRootPath, "ExtensionsData");
        public static string ExtensionQueueFilePath => Path.Combine(ConfigRootPath, "extinstalls.json");
        public static string LocalizationsPath => Path.Combine(ProgramPath, "Localization");
        public static string LocalizationsStatusPath => Path.Combine(LocalizationsPath, "locstatus.json");
        public static string ThemesProgramPath => Path.Combine(ProgramPath, "Themes");
        public static string ThemesUserDataPath => Path.Combine(ConfigRootPath, "Themes");
        public static string UninstallerPath => Path.Combine(ProgramPath, "unins000.exe");
        public static string BrowserCachePath => Path.Combine(ConfigRootPath, "browsercache");
        public static string TempPath => Path.Combine(Path.GetTempPath(), "Playnite");
        public static string LogPath => Path.Combine(ConfigRootPath, "playnite.log");
        public static string ConfigFilePath => Path.Combine(ConfigRootPath, "config.json");
        public static string FullscreenConfigFilePath => Path.Combine(ConfigRootPath, "fullscreenConfig.json");
        public static string WindowPositionsPath => Path.Combine(ConfigRootPath, "windowPositions.json");
        public static string ExclusionListConfigFilePath => Path.Combine(ConfigRootPath, "exclusionList.json");
        public static string BackupConfigFilePath => Path.Combine(ConfigRootPath, "Backup", "config.json");
        public static string BackupFullscreenConfigFilePath => Path.Combine(ConfigRootPath, "Backup", "fullscreenConfig.json");
        public static string BackupWindowPositionsPath => Path.Combine(ConfigRootPath, "Backup", "windowPositions.json");
        public static string BackupExclusionListConfigFilePath => Path.Combine(ConfigRootPath, "Backup", "exclusionList.json");
        public static string DataCachePath => Path.Combine(ConfigRootPath, "cache");
        public static string ImagesCachePath => Path.Combine(DataCachePath, "images");
        public static string IconsCachePath => Path.Combine(DataCachePath, "icons");
        public static string UserProgramDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite");
        public static string JitProfilesPath => Path.Combine(ConfigRootPath, "JITProfiles");
        public static string ConfigRootPath
        {
            get
            {
                if (PlayniteSettings.IsPortable)
                {
                    return ProgramPath;
                }
                else
                {
                    return UserProgramDataPath;
                }
            }
        }
    }
}