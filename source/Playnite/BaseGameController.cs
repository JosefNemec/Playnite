using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite
{
    public abstract class BaseGameController : IGameController
    {
        protected readonly SynchronizationContext execContext;

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

        public BaseGameController(Game game)
        {
            Game = game;
            execContext = SynchronizationContext.Current;
        }

        public abstract void ActivateAction(GameAction action);

        public abstract void Play();

        public abstract void Install();

        public abstract void Uninstall();

        public virtual void Dispose()
        {
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
    }
}
