using CefSharp;
using CefSharp.Wpf;
using Playnite.Common;
using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class CefTools
    {
        private static ILogger logger = LogManager.GetLogger();
        public static bool IsInitialized { get; private set; }

        public static void ConfigureCef(bool traceLogsEnabled)
        {
            FileSystem.CreateDirectory(PlaynitePaths.BrowserCachePath);
            var settings = new CefSettings();
            settings.WindowlessRenderingEnabled = true;

            if (settings.CefCommandLineArgs.ContainsKey("disable-gpu"))
            {
                settings.CefCommandLineArgs.Remove("disable-gpu");
            }

            if (settings.CefCommandLineArgs.ContainsKey("disable-gpu-compositing"))
            {
                settings.CefCommandLineArgs.Remove("disable-gpu-compositing");
            }

            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");

            // This is needed since Chromium 138 and up automatically de-elevates elevated instances.
            // This however breaks webviews in case Playnite is started as admin.
            // This in unsafe, but we already warn users to not run Playnite as admin with explicit dialog on startup...
            settings.CefCommandLineArgs.Add("do-not-de-elevate");

            settings.CachePath = PlaynitePaths.BrowserCachePath;
            settings.PersistSessionCookies = true;
            settings.LogFile = Path.Combine(PlaynitePaths.ConfigRootPath, "cef.log");
            settings.LogSeverity =  traceLogsEnabled ? LogSeverity.Verbose : LogSeverity.Info;
            // Firefox user agent gives the best compatibility because some websites complain
            // about unsecure browser if we try to pretend to be Chrome (which is CefSharp's default).
            // Plugins can change this on an individual level anyways.
            settings.UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:139.0) Gecko/20100101 Firefox/139.0 Playnite/{PlayniteApplication.CurrentVersion.ToString(2)}";
            IsInitialized = Cef.Initialize(settings, performDependencyCheck: true);
            if (!IsInitialized)            
                logger.Error($"CEF failed to initialize: {Cef.GetExitCode()}");            
        }

        public static void Shutdown()
        {
            Cef.Shutdown();
            IsInitialized = false;
        }
    }
}
