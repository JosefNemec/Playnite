using CommandLine;
using Playnite.Common;
using Playnite.FullscreenApp.Windows;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp
{
    public class ProgramEntry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            FileSystem.CreateDirectory(PlaynitePaths.JitProfilesPath);
            ProfileOptimization.SetProfileRoot(PlaynitePaths.JitProfilesPath);
            ProfileOptimization.StartProfile("fullscreen");

            if (Computer.WindowsVersion == WindowsVersion.Win7 || Computer.WindowsVersion == WindowsVersion.Win8)
            {
                MessageBox.Show(
                     "Windows 7 and Windows 8 are no longer supported. Please update your operating system or downgrade to older Playnite version.",
                     "Startup Error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);
                return;
            }

            if (PlaynitePaths.ProgramPath.Contains(@"temp\rar$", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Playnite is not allowed to run from temporary extracted archive.\rInstall/Extract application properly before starting it.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            else if (PlaynitePaths.ProgramPath.Contains("#"))
            {
                MessageBox.Show(
                    "Playnite is unable to run from current directory due to illegal character '#' in the path. Please use different directory.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var cmdLine = new CmdLineOptions();
            var parsed = Parser.Default.ParseArguments<CmdLineOptions>(Environment.GetCommandLineArgs());
            if (parsed is Parsed<CmdLineOptions> options)
            {
                cmdLine = options.Value;
            }

            ExtendedSplashScreen splash = null;
            var procCount = Process.GetProcesses().Where(a => a.ProcessName.StartsWith("Playnite.")).Count();
            if (cmdLine.Start.IsNullOrEmpty() && !cmdLine.HideSplashScreen && procCount == 1)
            {
                splash = new ExtendedSplashScreen("SplashScreen.png");
                splash.Show(false);
            }

            PlayniteSettings.ConfigureLogger();
            LogManager.GetLogger().Info($"App arguments: '{string.Join(",", args)}'");
            var app = new FullscreenApplication(() => new App(), splash, cmdLine);
            app.Run();
        }
    }
}
