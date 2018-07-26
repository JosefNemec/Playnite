using NLog;
using Playnite.Database;
using Playnite.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Providers
{
    public class GenericGameController : BaseGameController
    {
        protected CancellationTokenSource watcherToken;
        protected Stopwatch stopWatch;
        protected ProcessMonitor procMon;
        private GameDatabase database;

        public GenericGameController(GameDatabase db, Game game) : base (game)
        {
            database = db;
        }

        public override void ActivateAction(GameAction action)
        {
            GameActionActivator.ActivateAction(action, Game);
        }

        public override void Play()
        {            
            if (Game.PlayAction == null)
            {
                throw new Exception("Cannot start game without play action.");
            }

            Dispose();
            OnStarting(this, new GameControllerEventArgs(this, 0));
            var emulators = database.GetEmulators();
            var proc = GameActionActivator.ActivateAction(Game.PlayAction, Game, emulators);
            OnStarted(this, new GameControllerEventArgs(this, 0));

            if (Game.PlayAction.Type != GameActionType.URL)
            {
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeDestroyed += Monitor_TreeDestroyed;

                // Handle Windows store apps
                if (Game.PlayAction.Path == "explorer.exe" &&
                    Game.PlayAction.Arguments.StartsWith("shell:") &&
                    !string.IsNullOrEmpty(Game.InstallDirectory))
                {
                    procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
                }
                else
                {
                    procMon.WatchProcessTree(proc);
                }
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }
        }

        public override void Install()
        {
        }

        public override void Uninstall()
        {
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            watcherToken?.Cancel();
            procMon?.Dispose();
        }

        private void Monitor_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }
    }
}
