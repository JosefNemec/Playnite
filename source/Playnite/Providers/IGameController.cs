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
    public abstract class GameController : IGameController
    {
        protected readonly SynchronizationContext execContext;
        protected CancellationTokenSource watcherToken;
        protected Stopwatch stopWatch;
        protected ProcessMonitor procMon;
        private GameDatabase database;

        public bool IsGameRunning
        {
            get; private set;
        }

        public Game Game
        {
            get; private set;
        }

        public event GameControllerEventHandler Starting;
        public event GameControllerEventHandler Started;
        public event GameControllerEventHandler Stopped;
        public event GameControllerEventHandler Uninstalled;
        public event GameControllerEventHandler Installed;

        public GameController(GameDatabase db, Game game)
        {
            Game = game;
            execContext = SynchronizationContext.Current;
            database = db;
        }

        public virtual void ActivateAction(GameAction action)
        {
            GameHandler.ActivateTask(action, Game);
        }

        public virtual void Play()
        {            
            if (Game.PlayAction == null)
            {
                throw new Exception("Cannot start game without play task");
            }

            Dispose();
            OnStarting(this, new GameControllerEventArgs(this, 0));
            var emulators = database.GetEmulators();
            var proc = GameHandler.ActivateTask(Game.PlayAction, Game, emulators);
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

        public abstract void Install();

        public abstract void Uninstall();

        public virtual void Dispose()
        {
            ReleaseResources();
        }

        public virtual void ReleaseResources()
        {
            watcherToken?.Cancel();
            procMon?.Dispose();

        }

        public virtual void OnStarting(object sender, GameControllerEventArgs args)
        {
            execContext.Post((a) => Starting?.Invoke(sender, args), null);
        }

        public virtual void OnStarted(object sender, GameControllerEventArgs args)
        {
            IsGameRunning = true;
            execContext.Post((a) => Started?.Invoke(sender, args), null);
        }

        public virtual void OnStopped(object sender, GameControllerEventArgs args)
        {
            IsGameRunning = false;
            execContext.Post((a) => Stopped?.Invoke(sender, args), null);
        }

        public virtual void OnUninstalled(object sender, GameControllerEventArgs args)
        {
            execContext.Post((a) => Uninstalled?.Invoke(sender, args), null);
        }

        public virtual void OnInstalled(object sender, GameControllerEventArgs args)
        {
            execContext.Post((a) => Installed?.Invoke(sender, args), null);
        }

        private void Monitor_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }
    }
}
