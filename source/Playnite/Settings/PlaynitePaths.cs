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

        public const string ExtensionsDirName = "Extensions";
        public const string ExtensionsDataDirName = "ExtensionsData";
        public const string ThemesDirName = "Themes";
        public const string ConfigFileName = "config.json";
        public const string FullscreenConfigFileName = "fullscreenConfig.json";
        public const string WindowPositionsFileName = "windowPositions.json";

        public static string ProgramPath => Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        public static string DesktopExecutablePath => Path.Combine(ProgramPath, "Playnite.DesktopApp.exe");
        public static string FullscreenExecutablePath => Path.Combine(ProgramPath, "Playnite.FullscreenApp.exe");
        public static string PlayniteAssemblyPath => Path.Combine(ProgramPath, "Playnite.dll");
        public static string PlayniteSDKAssemblyPath => Path.Combine(ProgramPath, "Playnite.SDK.dll");
        public static string ExtensionsUserDataPath => Path.Combine(ConfigRootPath, ExtensionsDirName);
        public static string ExtensionsProgramPath => Path.Combine(ProgramPath, ExtensionsDirName);
        public static string ExtensionsDataPath => Path.Combine(ConfigRootPath, ExtensionsDataDirName);
        public static string ExtensionQueueFilePath => Path.Combine(ConfigRootPath, "extinstalls.json");
        public static string AddonLicenseAgreementsFilePath => Path.Combine(ConfigRootPath, "licenseagreements.json");
        public const string LocalizationsDirName = "Localization";
        public static string LocalizationsPath => Path.Combine(ProgramPath, LocalizationsDirName);
        public static string LocalizationsStatusPath => Path.Combine(LocalizationsPath, "locstatus.json");
        public static string ThemesProgramPath => Path.Combine(ProgramPath, ThemesDirName);
        public static string ThemesUserDataPath => Path.Combine(ConfigRootPath, ThemesDirName);
        public static string UninstallerPath => Path.Combine(ProgramPath, "unins000.exe");
        public static string BrowserCachePath => Path.Combine(ConfigRootPath, "browsercache");
        public static string TempPath => Path.Combine(Path.GetTempPath(), "Playnite");
        public static string LogPath => Path.Combine(ConfigRootPath, "playnite.log");
        public static string ConfigFilePath => Path.Combine(ConfigRootPath, ConfigFileName);
        public static string FullscreenConfigFilePath => Path.Combine(ConfigRootPath, FullscreenConfigFileName);
        public static string WindowPositionsPath => Path.Combine(ConfigRootPath, WindowPositionsFileName);
        public static string BackupConfigFilePath => Path.Combine(ConfigRootPath, "Backup", ConfigFileName);
        public static string BackupFullscreenConfigFilePath => Path.Combine(ConfigRootPath, "Backup", FullscreenConfigFileName);
        public static string BackupWindowPositionsPath => Path.Combine(ConfigRootPath, "Backup", WindowPositionsFileName);
        public static string DataCachePath => Path.Combine(ConfigRootPath, "cache");
        public static string ImagesCachePath => Path.Combine(DataCachePath, "images");
        public static string IconsCachePath => Path.Combine(DataCachePath, "icons");
        public static string UserProgramDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite");
        public static string JitProfilesPath => Path.Combine(ConfigRootPath, "JITProfiles");
        public static string EmulationDatabasePath => Path.Combine(ProgramPath, "Emulation", "Database");
        public static string ConfigRootPath => PlayniteSettings.IsPortable ? ProgramPath : UserProgramDataPath;
        public static string SafeStartupFlagFile => Path.Combine(ConfigRootPath, "safestart.flag");
        public static string BackupActionFile => Path.Combine(ConfigRootPath, "backup.json");
        public static string RestoreBackupActionFile => Path.Combine(ConfigRootPath, "restoreBackup.json");
    }
}