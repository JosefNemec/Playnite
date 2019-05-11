using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp
{
    public class ProgramEntry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            PlayniteSettings.ConfigureLogger();
            LogManager.Init(new NLogLogProvider());

            var app = new FullscreenApplication(new App());
            app.Run();
        }
    }
}
