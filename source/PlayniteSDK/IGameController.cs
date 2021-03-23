using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes game controller.
    /// </summary>
    [Obsolete("Not used anymore in Playnite 9.")]
    public interface IGameController : IDisposable
    {
        /// <summary>
        /// Gets value indicating wheter the game is running.
        /// </summary>
        bool IsGameRunning { get; }

        /// <summary>
        /// Gets game being handled.
        /// </summary>
        Game Game
        {
            get;
        }

        /// <summary>
        /// Installs game.
        /// </summary>
        void Install();

        /// <summary>
        /// Uninstalls game.
        /// </summary>
        void Uninstall();

        /// <summary>
        /// Starts game.
        /// </summary>
        void Play();

        /// <summary>
        /// Occurs when game is being started.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Starting;

        /// <summary>
        /// Occurs when game is started.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Started;

        /// <summary>
        /// Occurs when game stops running.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Stopped;

        /// <summary>
        /// Occurs when game is finished uninstalling.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Uninstalled;

        /// <summary>
        /// Occurs when game is finished installing.
        /// </summary>
        event EventHandler<Events.GameInstalledEventArgs> Installed;
    }
}
