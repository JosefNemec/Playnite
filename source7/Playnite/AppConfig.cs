using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace Playnite;

public enum ReleaseChannel
{
    Stable,
    Beta,
    Patreon
}

public static class AppConfig
{
    public class AppConfigData
    {
        public string UpdateUrl { get; set; } = "http://localhost/update/";
        public string UpdateUrl2 { get; set; } = "http://localhost:8081/update/";
        public string ServicesUrl { get; set; } = "http://localhost:5000/";
        public string UpdateBranch { get; set; } = "stable";
        public bool ThrowAllErrors { get; set; } = true;
        public bool OfflineMode { get; set; } = true;
        public string PipeEndpoint { get; set; } = "net.pipe://localhost/PlaynitePipe";
        public string AppBranch { get; set; } = "devel";
    }

    private static readonly ILogger logger = LogManager.GetLogger();
    public static AppConfigData Config { get; private set; } = new AppConfigData();

    static AppConfig()
    {
        try
        {
            if (File.Exists(PlaynitePaths.AppConfigFile))
            {
                var cfg = Serialization.FromJsonFile<AppConfigData>(PlaynitePaths.AppConfigFile);
                if (cfg == null)
                {
                    throw new Exception("App config deserialization provided not data.");
                }
                else
                {
                    Config = cfg;
                }
            }
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to load app config file.");
        }
    }

    public static string AppBranch => Config.AppBranch;
    public static bool ThrowAllErrors => Config.ThrowAllErrors;
    public static bool InOfflineMode => Config.OfflineMode;
    public static bool IsDebuggerAttached => Debugger.IsAttached;

    public static ReleaseChannel ReleaseChannel
    {
        get
        {
            return Config!.UpdateBranch switch
            {
                "stable" => ReleaseChannel.Stable,
                "patreon" => ReleaseChannel.Patreon,
                "beta" => ReleaseChannel.Beta,
                _ => ReleaseChannel.Stable,
            };
        }
    }

    public static bool IsDebugBuild
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsElevated
    {
        get
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
