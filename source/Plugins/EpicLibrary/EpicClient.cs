using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary
{
    public class EpicClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => EpicLauncher.Icon;

        public override bool IsInstalled => EpicLauncher.IsInstalled;

        public override void Open()
        {
            EpicLauncher.StartClient();
        }

        public override void Shutdown()
        {
            var mainProc = Process.GetProcessesByName("EpicGamesLauncher").FirstOrDefault();
            if (mainProc == null)
            {
                logger.Info("Epic client is no longer running, no need to shut it down.");
                return;
            }

            var procRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {mainProc.Id}", null, out var stdOut, out var stdErr);
            if (procRes != 0)
            {
                logger.Error($"Failed to close Epic client: {procRes}, {stdErr}");
            }
        }
    }
}
