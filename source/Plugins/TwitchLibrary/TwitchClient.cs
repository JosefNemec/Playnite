using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TwitchLibrary
{
    public class TwitchClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => Twitch.Icon;

        public override bool IsInstalled => Twitch.IsInstalled;

        public bool IsRunning
        {
            get => Process.GetProcessesByName("twitch").Count() > 0;
        }            

        public override void Open()
        {
            ProcessStarter.StartProcess(Twitch.ClientExecPath, string.Empty);
        }

        public override void Shutdown()
        {
            // Main Twitch process doesn't react to termination signals, only the main TwitchUI process does.
            var mainProc = Process.GetProcessesByName("Twitch").FirstOrDefault();
            if (mainProc == null)
            {
                logger.Info("Twitch client is no longer running, no need to shut it down.");
                return;
            }

            var mainUiProc = Process.GetProcessesByName("TwitchUI").FirstOrDefault(a =>
            {
                var par = a.TryGetParentId(out var parId);
                return parId == mainProc.Id;
            });

            var procRes = 0;
            var stdErr = string.Empty;
            if (mainUiProc == null)
            {
                logger.Info("TwitchUI process is no longer running, this shouldn't happen.");
                procRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {mainProc.Id}", null, out var stdOut, out stdErr);
            }
            else
            {
                procRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/pid {mainUiProc.Id}", null, out var stdOut, out stdErr);
            }

            if (procRes != 0)
            {
                logger.Error($"Failed to close twitch client: {procRes}, {stdErr}");
            }
        }
    }
}
