using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
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
            var splash = new SplashScreen("SplashScreen.png");
            splash.Show(false);
            PlayniteSettings.ConfigureLogger();
            var app = new FullscreenApplication(new App(), splash);
            app.Run();
        }
    }
}
