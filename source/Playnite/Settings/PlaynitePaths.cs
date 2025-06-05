using Playnite.Common;
using Playnite.SDK;
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
        public const string LocalizationsDirName = "Localization";

        public static string ProgramPath { get; }
        public static string ConfigRootPath { get; private set; }
        public static string LocalizationsPath { get; }
        public static string DataCachePath { get; private set; }

        public static string DesktopExecutablePath { get; }
        public static string FullscreenExecutablePath { get; }
        public static string PlayniteAssemblyPath { get; }
        public static string PlayniteSDKAssemblyPath { get; }
        public static string ExtensionsUserDataPath { get; private set; }
        public static string ExtensionsProgramPath { get; }
        public static string ExtensionsDataPath { get; private set; }
        public static string ExtensionQueueFilePath { get; private set; }
        public static string AddonLicenseAgreementsFilePath { get; private set; }
        public static string LocalizationsStatusPath { get; }
        public static string ThemesProgramPath { get; }
        public static string ThemesUserDataPath { get; private set; }
        public static string UninstallerPath { get; }
        public static string BrowserCachePath { get; private set; }
        public static string TempPath { get; }
        public static string LogPath { get; private set; }
        public static string ConfigFilePath { get; private set; }
        public static string FullscreenConfigFilePath { get; private set; }
        public static string WindowPositionsPath { get; private set; }
        public static string BackupConfigFilePath { get; private set; }
        public static string BackupFullscreenConfigFilePath { get; private set; }
        public static string BackupWindowPositionsPath { get; private set; }
        public static string ImagesCachePath { get; private set; }
        public static string IconsCachePath { get; private set; }
        public static string JitProfilesPath { get; private set; }
        public static string EmulationDatabasePath { get; }
        public static string SafeStartupFlagFile { get; private set; }
        public static string BackupActionFile { get; private set; }
        public static string RestoreBackupActionFile { get; private set; }

        public static bool IsPortable { get; }

        static PlaynitePaths()
        {
            ProgramPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            UninstallerPath = Path.Combine(ProgramPath, "unins000.exe");
            IsPortable = !File.Exists(UninstallerPath);

            LocalizationsPath = Path.Combine(ProgramPath, LocalizationsDirName);
            DesktopExecutablePath = Path.Combine(ProgramPath, "Playnite.DesktopApp.exe");
            FullscreenExecutablePath = Path.Combine(ProgramPath, "Playnite.FullscreenApp.exe");
            PlayniteAssemblyPath = Path.Combine(ProgramPath, "Playnite.dll");
            PlayniteSDKAssemblyPath = Path.Combine(ProgramPath, "Playnite.SDK.dll");
            ExtensionsProgramPath = Path.Combine(ProgramPath, ExtensionsDirName);
            LocalizationsStatusPath = Path.Combine(LocalizationsPath, "locstatus.json");
            ThemesProgramPath = Path.Combine(ProgramPath, ThemesDirName);
            EmulationDatabasePath = Path.Combine(ProgramPath, "Emulation", "Database");
            TempPath = Path.Combine(Path.GetTempPath(), "Playnite");

            // We need to always initialize some default set for environments like Blend or Rider
            UpdateUserDataDir(IsPortable ? ProgramPath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite"));
        }

        public static void UpdateUserDataDir(string dir)
        {
            ConfigRootPath = dir;
            DataCachePath = Path.Combine(ConfigRootPath, "cache");
            ThemesUserDataPath = Path.Combine(ConfigRootPath, ThemesDirName);
            ExtensionsDataPath = Path.Combine(ConfigRootPath, ExtensionsDataDirName);
            ExtensionQueueFilePath = Path.Combine(ConfigRootPath, "extinstalls.json");
            AddonLicenseAgreementsFilePath = Path.Combine(ConfigRootPath, "licenseagreements.json");
            ExtensionsUserDataPath = Path.Combine(ConfigRootPath, ExtensionsDirName);
            BrowserCachePath = Path.Combine(ConfigRootPath, "browsercache");
            LogPath = Path.Combine(ConfigRootPath, "playnite.log");
            ConfigFilePath = Path.Combine(ConfigRootPath, ConfigFileName);
            FullscreenConfigFilePath = Path.Combine(ConfigRootPath, FullscreenConfigFileName);
            WindowPositionsPath = Path.Combine(ConfigRootPath, WindowPositionsFileName);
            BackupConfigFilePath = Path.Combine(ConfigRootPath, "Backup", ConfigFileName);
            BackupFullscreenConfigFilePath = Path.Combine(ConfigRootPath, "Backup", FullscreenConfigFileName);
            BackupWindowPositionsPath = Path.Combine(ConfigRootPath, "Backup", WindowPositionsFileName);
            ImagesCachePath = Path.Combine(DataCachePath, "images");
            IconsCachePath = Path.Combine(DataCachePath, "icons");
            JitProfilesPath = Path.Combine(ConfigRootPath, "JITProfiles");
            SafeStartupFlagFile = Path.Combine(ConfigRootPath, "safestart.flag");
            BackupActionFile = Path.Combine(ConfigRootPath, "backup.json");
            RestoreBackupActionFile = Path.Combine(ConfigRootPath, "restoreBackup.json");
        }

        public static string ExpandVariables(string inputString, string emulatorDir = null, bool fixSeparators = false)
        {
            if (string.IsNullOrEmpty(inputString) || !inputString.Contains('{'))
            {
                return inputString;
            }

            var result = inputString;
            if (!emulatorDir.IsNullOrEmpty())
            {
                emulatorDir = emulatorDir.Replace(ExpandableVariables.PlayniteDirectory, ProgramPath, StringComparison.Ordinal);
            }

            result = result.Replace(ExpandableVariables.PlayniteDirectory, ProgramPath, StringComparison.Ordinal);
            result = result.Replace(ExpandableVariables.EmulatorDirectory, emulatorDir, StringComparison.Ordinal);
            return fixSeparators ? Paths.FixSeparators(result) : result;
        }
    }
}