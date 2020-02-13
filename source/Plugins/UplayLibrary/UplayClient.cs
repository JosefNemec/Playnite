using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UplayLibrary
{
    public class UplayClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => Uplay.Icon;

        public override bool IsInstalled => Uplay.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Uplay.ClientExecPath, string.Empty);
        }

        public override void Shutdown()
        {
            var mainProc = Process.GetProcessesByName("upc").FirstOrDefault();
            if (mainProc == null)
            {
                logger.Info("Uplay client is no longer running, no need to shut it down.");
                return;
            }

            var webCoreCount = Process.GetProcessesByName("UplayWebCore").Count();
            if (webCoreCount > 1)
            {
                // If multiple UI processes are running then we can close them gracefully.
                // Only one running means that Uply is closed to tray and we have to kill it.
                var coreRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/pid {mainProc.Id}", null, out var stdOut1, out var stdErr1);
                if (coreRes != 0)
                {
                    logger.Error($"Failed to close uplay UI processes: {coreRes}, {stdErr1}");
                }

                // TODO change this to proper wait for all processes to close
                Thread.Sleep(3000);
            }

            var mainRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {mainProc.Id}", null, out var stdOut, out var stdErr);
            if (mainRes != 0)
            {
                logger.Error($"Failed to close uplay client: {mainRes}, {stdErr}");
            }
        }
    }
}
