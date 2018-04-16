using NLog;
using Playnite.Models;
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
    public class GameControllerEventArgs : EventArgs
    {
        public IGameController Controller
        {
            get; set;
        }

        /// <summary>
        /// Time in seconds for event to be finished.
        /// For example in case of Stopped event it indicates how long was game running until stopped.
        /// </summary>
        public long EllapsedTime
        {
            get; set;
        } = 0;

        public GameControllerEventArgs()
        {
        }

        public GameControllerEventArgs(IGameController controller, long ellapsedTime)
        {
            Controller = controller;
            EllapsedTime = ellapsedTime;
        }

        public GameControllerEventArgs(IGameController controller, double ellapsedTime)
            : this(controller, Convert.ToInt64(ellapsedTime))
        {
        }
    }

    public delegate void GameControllerEventHandler(object sender, GameControllerEventArgs controller);

    public interface IGameController : IDisposable
    {
        Game Game
        {
            get;
        }

        void Install();

        void Uninstall();

        void Play(List<Emulator> emulators);

        void ActivateAction(GameTask action);

        event GameControllerEventHandler Started;

        event GameControllerEventHandler Stopped;

        event GameControllerEventHandler Uninstalled;

        event GameControllerEventHandler Installed;
    }

    public abstract class GameController : IGameController
    {
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected readonly SynchronizationContext execContext;
        protected CancellationTokenSource watcherToken;
        protected Stopwatch stopWatch;
        protected ProcessMonitor procMon;

        public Game Game
        {
            get; private set;
        }

        public event GameControllerEventHandler Started;
        public event GameControllerEventHandler Stopped;
        public event GameControllerEventHandler Uninstalled;
        public event GameControllerEventHandler Installed;

        public GameController(Game game)
        {
            Game = game;
            execContext = SynchronizationContext.Current;
        }

        public virtual void ActivateAction(GameTask action)
        {
            GameHandler.ActivateTask(action, Game);
        }

        public virtual void Play(List<Emulator> emulators)
        {
            if (Game.PlayTask == null)
            {
                throw new Exception("Cannot start game without play task");
            }

            var proc = GameHandler.ActivateTask(Game.PlayTask, Game, emulators);
            OnStarted(this, new GameControllerEventArgs(this, 0));

            if (Game.PlayTask.Type != GameTaskType.URL)
            {
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeDestroyed += Monitor_TreeDestroyed;
                procMon.WatchProcessTree(proc);
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
            watcherToken?.Cancel();
            procMon?.Dispose();
        }

        public virtual void OnStarted(object sender, GameControllerEventArgs args)
        {
            execContext.Post((a) => Started?.Invoke(sender, args), null);
        }

        public virtual void OnStopped(object sender, GameControllerEventArgs args)
        {
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
