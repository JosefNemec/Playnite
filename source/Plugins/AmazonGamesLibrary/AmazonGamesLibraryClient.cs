using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonGamesLibrary
{
    public class AmazonGamesLibraryClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => AmazonGames.Icon;

        public override bool IsInstalled => AmazonGames.IsInstalled;

        public bool IsRunning
        {
            get => Process.GetProcessesByName("Amazon Games").Count() > 0;
        }

        public override void Open()
        {
            AmazonGames.StartClient();
        }

        public override void Shutdown()
        {
            // Main Amazon process doesn't react to termination signals, only the main TwitchUI process does.
            var mainProc = Process.GetProcessesByName("Amazon Games").FirstOrDefault();
            if (mainProc == null)
            {
                logger.Info("Amazon Games client is no longer running, no need to shut it down.");
                return;
            }

            var mainUiProc = Process.GetProcessesByName("Amazon Games UI").FirstOrDefault(a =>
            {
                var par = a.TryGetParentId(out var parId);
                return parId == mainProc.Id;
            });

            var procRes = 0;
            var stdErr = string.Empty;
            if (mainUiProc == null)
            {
                logger.Info("Amazon Games UI process is no longer running, this shouldn't happen.");
                procRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {mainProc.Id}", null, out var stdOut, out stdErr);
            }
            else
            {
                procRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/pid {mainUiProc.Id}", null, out var stdOut, out stdErr);
            }

            if (procRes != 0)
            {
                logger.Error($"Failed to close Amazon Games client: {procRes}, {stdErr}");
            }
        }
    }
}