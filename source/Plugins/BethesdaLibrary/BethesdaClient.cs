using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BethesdaLibrary
{
    public class BethesdaClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => Bethesda.Icon;

        public override bool IsInstalled => Bethesda.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Bethesda.ClientExecPath, string.Empty);
        }

        public override void Shutdown()
        {
            // There are two processes running, we want to signal the main one, not the web render process.
            var mainProc = Process.GetProcessesByName("BethesdaNetLauncher").FirstOrDefault(a => !a.GetCommandLine().Contains("--type="));
            if (mainProc == null)
            {
                logger.Info("Bethesda client is no longer running, no need to shut it down.");
                return;
            }

            var procRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/pid {mainProc.Id}", null, out var stdOut, out var stdErr);
            if (procRes != 0)
            {
                logger.Error($"Failed to close Bethesda client: {procRes}, {stdErr}");
            }
        }
    }
}
