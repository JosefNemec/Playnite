using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface IGameController : IDisposable
    {
        bool IsGameRunning { get; }

        Game Game
        {
            get;
        }

        void Install();

        void Uninstall();

        void Play();

        event GameControllerEventHandler Starting;

        event GameControllerEventHandler Started;

        event GameControllerEventHandler Stopped;

        event GameControllerEventHandler Uninstalled;

        event GameControllerEventHandler Installed;
    }
}
