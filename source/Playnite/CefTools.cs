using CefSharp;
using CefSharp.Wpf;
using Playnite.Common;
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
        public static bool IsInitialized { get; private set; }

        public static void ConfigureCef(bool enableLogs)
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

            // This is because cookies created with Alloy runtime won't work with Chrome runtime
            // which is default since CefSharp 126. Alloy will be completely removed from CEF 128
            // so P10 will be likely stuck forever on 127.
            // https://github.com/cefsharp/CefSharp/issues/4847
            // https://github.com/chromiumembedded/cef/issues/3721
            settings.ChromeRuntime = false;
            CefSharpSettings.RuntimeStyle = CefRuntimeStyle.Alloy;

            settings.CachePath = PlaynitePaths.BrowserCachePath;
            settings.PersistSessionCookies = true;
            settings.LogFile = Path.Combine(PlaynitePaths.ConfigRootPath, "cef.log");
            settings.LogSeverity = enableLogs ? LogSeverity.Error : LogSeverity.Disable;
            // Firefox user agent gives the best compatibility because some websites complain
            // about unsecure browser if we try to pretend to be Chrome (which is CefSharp's default).
            // Plugins can change this on an individual level anyways.
            settings.UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:126.0) Gecko/20100101 Firefox/126.0 Playnite/{PlayniteApplication.CurrentVersion.ToString(2)}";
            IsInitialized = Cef.Initialize(settings);
        }

        public static void Shutdown()
        {
            Cef.Shutdown();
            IsInitialized = false;
        }
    }
}
