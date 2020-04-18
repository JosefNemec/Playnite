using CommandLine;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if (PlaynitePaths.ProgramPath.Contains(@"temp\rar$", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Playnite is not allowed to run from temporary extracted archive.\rInstall/Extract application properly before starting it.",
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

            SplashScreen splash = null;
            var procCount = Process.GetProcesses().Where(a => a.ProcessName.StartsWith("Playnite.")).Count();
            if (cmdLine.Start.IsNullOrEmpty() && !cmdLine.HideSplashScreen && procCount == 1)
            {
                splash = new SplashScreen("SplashScreen.png");
                splash.Show(false);
            }

            PlayniteSettings.ConfigureLogger();
            var app = new FullscreenApplication(new App(), splash, cmdLine);
            app.Run();
        }
    }
}
