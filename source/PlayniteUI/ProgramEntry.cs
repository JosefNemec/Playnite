using Playnite;
using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public class ProgramEntry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            PlayniteSettings.ConfigureLogger();
            LogManager.Init(new NLogLogProvider());

            var app = new App();
            app.Run();
        }
    }
}
