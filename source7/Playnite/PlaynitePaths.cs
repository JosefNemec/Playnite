using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace Playnite;

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
    public const string AppConfigFileName = "AppConfig.json";

    public static string UserProgramDataDir { get; }
    public static string ProgramDir { get; }
    public static string ConfigRootDir { get; }
    public static string LocalizationsDir { get; }
    public static string DataCacheDir { get; }

    public static string DesktopExecutableFile { get; }
    public static string FullscreenExecutableFile { get; }
    public static string PlayniteAssemblyFile { get; }
    public static string PlayniteSDKAssemblyFile { get; }
    public static string ExtensionsUserDataDir { get; }
    public static string ExtensionsProgramDir { get; }
    public static string ExtensionsDataDir { get; }
    public static string ExtensionQueueFile { get; }
    public static string AddonLicenseAgreementsFile { get; }
    public static string LocalizationsStatusFile { get; }
    public static string ThemesProgramDir { get; }
    public static string ThemesUserDataDir { get; }
    public static string UninstallerFile { get; }
    public static string BrowserCacheDir { get; }
    public static string TempDir { get; }
    public static string LogFile { get; }
    public static string ConfigFile { get; }
    public static string FullscreenConfigFile { get; }
    public static string WindowPositionsFile { get; }
    public static string BackupConfigFile { get; }
    public static string BackupFullscreenConfigFile { get; }
    public static string BackupWindowPositionsFile { get; }
    public static string ImagesCacheDir { get; }
    public static string IconsCacheDir { get; }
    public static string JitProfilesDir { get; }
    public static string EmulationDatabaseDir { get; }
    public static string SafeStartupFlagFile { get; }
    public static string BackupActionFile { get; }
    public static string RestoreBackupActionFile { get; }
    public static string AppConfigFile { get; }

    public static bool IsPortable { get; }

    static PlaynitePaths()
    {
        UserProgramDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite");
        ProgramDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        UninstallerFile = Path.Combine(ProgramDir, "unins000.exe");
        IsPortable = !File.Exists(UninstallerFile);
        ConfigRootDir = IsPortable ? ProgramDir : UserProgramDataDir;
        LocalizationsDir = Path.Combine(ProgramDir, LocalizationsDirName);
        DataCacheDir = Path.Combine(ConfigRootDir, "cache");

        DesktopExecutableFile = Path.Combine(ProgramDir, "Playnite.DesktopApp.exe");
        FullscreenExecutableFile = Path.Combine(ProgramDir, "Playnite.FullscreenApp.exe");
        PlayniteAssemblyFile = Path.Combine(ProgramDir, "Playnite.dll");
        PlayniteSDKAssemblyFile = Path.Combine(ProgramDir, "Playnite.SDK.dll");
        ExtensionsUserDataDir = Path.Combine(ConfigRootDir, ExtensionsDirName);
        ExtensionsProgramDir = Path.Combine(ProgramDir, ExtensionsDirName);
        ExtensionsDataDir = Path.Combine(ConfigRootDir, ExtensionsDataDirName);
        ExtensionQueueFile = Path.Combine(ConfigRootDir, "extinstalls.json");
        AddonLicenseAgreementsFile = Path.Combine(ConfigRootDir, "licenseagreements.json");
        LocalizationsStatusFile = Path.Combine(LocalizationsDir, "locstatus.json");
        ThemesProgramDir = Path.Combine(ProgramDir, ThemesDirName);
        ThemesUserDataDir = Path.Combine(ConfigRootDir, ThemesDirName);

        BrowserCacheDir = Path.Combine(ConfigRootDir, "browsercache");
        TempDir = Path.Combine(Path.GetTempPath(), "Playnite");
        LogFile = Path.Combine(ConfigRootDir, "playnite.log");
        ConfigFile = Path.Combine(ConfigRootDir, ConfigFileName);
        FullscreenConfigFile = Path.Combine(ConfigRootDir, FullscreenConfigFileName);
        WindowPositionsFile = Path.Combine(ConfigRootDir, WindowPositionsFileName);
        BackupConfigFile = Path.Combine(ConfigRootDir, "Backup", ConfigFileName);
        BackupFullscreenConfigFile = Path.Combine(ConfigRootDir, "Backup", FullscreenConfigFileName);
        BackupWindowPositionsFile = Path.Combine(ConfigRootDir, "Backup", WindowPositionsFileName);
        ImagesCacheDir = Path.Combine(DataCacheDir, "images");
        IconsCacheDir = Path.Combine(DataCacheDir, "icons");
        JitProfilesDir = Path.Combine(ConfigRootDir, "JITProfiles");
        EmulationDatabaseDir = Path.Combine(ProgramDir, "Emulation", "Database");
        SafeStartupFlagFile = Path.Combine(ConfigRootDir, "safestart.flag");
        BackupActionFile = Path.Combine(ConfigRootDir, "backup.json");
        RestoreBackupActionFile = Path.Combine(ConfigRootDir, "restoreBackup.json");
        AppConfigFile = Path.Combine(ProgramDir, AppConfigFileName);
    }

    public static string ExpandVariables(string inputString, string? emulatorDir = null, bool fixSeparators = false)
    {
        if (string.IsNullOrEmpty(inputString) || !inputString.Contains('{', StringComparison.Ordinal))
        {
            return inputString;
        }

        var result = inputString;
        if (!emulatorDir.IsNullOrEmpty())
        {
            emulatorDir = emulatorDir.Replace(ExpandableVariables.PlayniteDirectory, ProgramDir, StringComparison.Ordinal);
        }

        result = result.Replace(ExpandableVariables.PlayniteDirectory, ProgramDir, StringComparison.Ordinal);
        result = result.Replace(ExpandableVariables.EmulatorDirectory, emulatorDir, StringComparison.Ordinal);
        return fixSeparators ? Paths.FixSeparators(result) : result;
    }
}