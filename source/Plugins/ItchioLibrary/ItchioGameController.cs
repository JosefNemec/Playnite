using ItchioLibrary.Models;
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

namespace ItchioLibrary
{
    public class ItchioGameController : BaseGameController
    {
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private readonly IPlayniteAPI api;
        private readonly Game game;
        private Butler butler;

        public const string DynamicLaunchActionStr = "dynamic";

        public ItchioGameController(Game game, IPlayniteAPI api) : base(game)
        {
            this.api = api;
            this.game = game;            
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            procMon?.Dispose();
            if (butler != null)
            {
                butler.RequestReceived -= Butler_RequestReceived;
                butler.NotificationReceived -= Butler_NotificationReceived;
                butler.Dispose();
            }
        }

        private void CheckItchInstallStatus()
        {
            if (!Itch.IsInstalled)
            {
                throw new Exception(
                    api.Resources.GetString("LOCItchioClientNotInstalledError"));
            }
        }

        public override void Play()
        {
            CheckItchInstallStatus();
            ReleaseResources();
            if (Game.PlayAction.Path == DynamicLaunchActionStr ||
                Game.PlayAction.Type == GameActionType.File ||
                Game.PlayAction.Type == GameActionType.URL)
            {
                OnStarting(this, new GameControllerEventArgs(this, 0));

                if (Game.PlayAction.Path == DynamicLaunchActionStr)
                {
                    butler = new Butler();
                    butler.RequestReceived += Butler_RequestReceived;
                    butler.NotificationReceived += Butler_NotificationReceived;
                    butler.LaunchAsync(Game.PlayAction.Arguments);
                }
                else
                {
                    if (!Directory.Exists(Game.InstallDirectory))
                    {
                        throw new DirectoryNotFoundException(api.Resources.GetString("LOCInstallDirNotFoundError"));
                    }
                    
                    GameActionActivator.ActivateAction(api.ExpandGameVariables(Game, Game.PlayAction));
                    if (Directory.Exists(Game.InstallDirectory))
                    {
                        procMon = new ProcessMonitor();
                        procMon.TreeStarted += ProcMon_TreeStarted;
                        procMon.TreeDestroyed += Monitor_TreeDestroyed;
                        procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
                    }
                    else
                    {
                        OnStopped(this, new GameControllerEventArgs(this, 0));
                    }
                }
            }
            else
            {
                throw new Exception(api.Resources.GetString("LOCInvalidGameActionSettings"));
            }
        }

        private void Butler_NotificationReceived(object sender, JsonRpcNotificationEventArgs e)
        {
            if (e.Notification.Method == Butler.Methods.LaunchRunning)
            {
                OnStarted(this, new GameControllerEventArgs(this, 0));
                stopWatch = Stopwatch.StartNew();
            }
            else if (e.Notification.Method == Butler.Methods.LaunchExited)
            {
                stopWatch.Stop();
                OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
            }
        }

        private void Butler_RequestReceived(object sender, JsonRpcRequestEventArgs e)
        {
            switch (e.Request.Method)
            {
                case Butler.Methods.PickManifestAction:
                    var pick = e.Request.GetParams<PickManifestAction>();
                    butler.SendResponse(e.Request, new Dictionary<string, int>
                    {
                        { "index",  0 }
                    });
                    break;

                case Butler.Methods.HTMLLaunch:
                    var html = e.Request.GetParams<HTMLLaunch>();
                    ProcessStarter.StartProcess(Path.Combine(html.rootFolder, html.indexPath));
                    butler.SendResponse(e.Request);
                    break;

                case Butler.Methods.URLLaunch:
                    var url = e.Request.GetParams<URLLaunch>();
                    ProcessStarter.StartUrl(url.url);
                    butler.SendResponse(e.Request);
                    break;

                case Butler.Methods.ShellLaunch:
                    var shell = e.Request.GetParams<ShellLaunch>();
                    ProcessStarter.StartProcess(shell.itemPath);
                    butler.SendResponse(e.Request);
                    break;

                case Butler.Methods.PrereqsFailed:
                    var error = e.Request.GetParams<PrereqsFailed>();
                    butler.SendResponse(e.Request, new Dictionary<string, bool>
                    {
                        { "continue",  true }
                    });
                    break;
            }
        }

        public override void Install()
        {
            CheckItchInstallStatus();
            ProcessStarter.StartUrl("itch://library/owned");
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            CheckItchInstallStatus();
            ProcessStarter.StartUrl("itch://library/installed");
            StartUninstallWatcher();
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            using (var butler = new Butler())
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var installed = butler.GetCaves();
                    var cave = installed?.FirstOrDefault(a => a.game.id.ToString() == Game.GameId);
                    if (cave != null)
                    {
                        var installInfo = new GameInfo
                        {
                            InstallDirectory = cave.installInfo.installFolder,
                            PlayAction = new GameAction()
                            {
                                Type = GameActionType.URL,
                                Path = DynamicLaunchActionStr,
                                Arguments = cave.id,
                                IsHandledByPlugin = true
                            }
                        };

                        OnInstalled(this, new GameInstalledEventArgs(installInfo, this, 0));
                        return;
                    }

                    await Task.Delay(5000);
                }
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            using (var butler = new Butler())
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var installed = butler.GetCaves();
                    var cave = installed?.FirstOrDefault(a => a.game.id.ToString() == Game.GameId);
                    if (cave == null)
                    {
                        OnUninstalled(this, new GameControllerEventArgs(this, 0));
                        return;
                    }

                    await Task.Delay(2000);
                }
            }
        }

        private void ProcMon_TreeStarted(object sender, EventArgs args)
        {
            OnStarted(this, new GameControllerEventArgs(this, 0));
            stopWatch = Stopwatch.StartNew();
        }

        private void Monitor_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }
    }
}
