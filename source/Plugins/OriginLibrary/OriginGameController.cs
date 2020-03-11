using OriginLibrary.Models;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class OriginGameController : BaseGameController
    {
        private ILogger logger = LogManager.GetLogger();
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private OriginLibrary origin;
        private IPlayniteAPI api;

        public OriginGameController(OriginLibrary library, Game game, IPlayniteAPI api) : base (game)
        {
            origin = library;
            this.api = api;
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            procMon?.Dispose();
        }

        public override void Play()
        {
            ReleaseResources();
            OnStarting(this, new GameControllerEventArgs(this, 0));
            if (Directory.Exists(Game.InstallDirectory))
            {
                var playAction = api.ExpandGameVariables(Game, Game.PlayAction);
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeDestroyed += ProcMon_TreeDestroyed;
                procMon.TreeStarted += ProcMon_TreeStarted;
                GameActionActivator.ActivateAction(playAction);
                StartRunningWatcher();
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }
        }

        public async void StartRunningWatcher()
        {
            if (Origin.GetGameUsesEasyAntiCheat(Game.InstallDirectory))
            {
                // Games with EasyAntiCheat take longer to be re-executed by Origin
                await Task.Delay(12000);
            }
            else if (Origin.GetGameRequiresOrigin(Game.InstallDirectory))
            {
                // Solves issues with game process being started/shutdown multiple times during startup via Origin
                await Task.Delay(5000);
            }

            procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
        }

        public override void Install()
        {
            ReleaseResources();
            ProcessStarter.StartUrl($"origin2://game/launch?offerIds={Game.GameId}&autoDownload=true");
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            ProcessStarter.StartProcess("appwiz.cpl", string.Empty);
            StartUninstallWatcher();
        }

        private void ProcMon_TreeStarted(object sender, EventArgs args)
        {
            OnStarted(this, new GameControllerEventArgs(this, 0));
        }

        private void ProcMon_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            var manifest = origin.GetLocalManifest(Game.GameId);
            if (manifest?.publishing == null)
            {
                logger.Error($"No publishing manifest found for Origin game {Game.GameId}, stopping installation check.");
                OnUninstalled(this, new GameControllerEventArgs(this, 0));
                return;
            }

            var platform = manifest.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var executablePath = origin.GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);
                if (!executablePath?.CompletePath.IsNullOrEmpty() != null)
                {
                    if (File.Exists(executablePath.CompletePath))
                    {
                        var installInfo = new GameInfo()
                        {
                            PlayAction = origin.GetGamePlayTask(manifest),
                            InstallDirectory = Path.GetDirectoryName(executablePath.CompletePath)
                        };

                        OnInstalled(this, new GameInstalledEventArgs(installInfo, this, 0));
                        return;
                    }
                }

                await Task.Delay(2000);
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            var manifest = origin.GetLocalManifest(Game.GameId);
            if (manifest?.publishing == null)
            {
                logger.Error($"No publishing manifest found for Origin game {Game.GameId}, stopping uninstallation check.");
                OnUninstalled(this, new GameControllerEventArgs(this, 0));
                return;
            }

            var platform = manifest.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");
            var executablePath = origin.GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                if (executablePath?.CompletePath == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }
                else
                {
                    if (!File.Exists(executablePath.CompletePath))
                    {
                        OnUninstalled(this, new GameControllerEventArgs(this, 0));
                        return;
                    }
                }

                await Task.Delay(2000);
            }
        }
    }
}
