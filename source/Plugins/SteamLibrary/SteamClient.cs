using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary
{
    public class SteamClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => Steam.Icon;

        public override bool IsInstalled => Steam.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Steam.ClientExecPath, string.Empty);
        }

        public override void Shutdown()
        {            
            var mainProc = Process.GetProcessesByName("Steam").FirstOrDefault();
            if (mainProc == null)
            {
                logger.Info("Steam client is no longer running, no need to shut it down.");
                return;
            }

            ProcessStarter.StartProcessWait(Steam.ClientExecPath, "-shutdown", null);
        }
    }
}
