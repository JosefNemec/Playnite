using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    public interface IGenericPlugin : IPlugin
    {
        /// <summary>
        /// Returns list of plugin functions.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ExtensionFunction> GetFunctions();

        /// <summary>
        /// Called before game is started.
        /// </summary>
        /// <param name="game">Game that will be started.</param>
        void OnGameStarting(Game game);

        /// <summary>
        /// Called when game has started.
        /// </summary>
        /// <param name="game">Game that started.</param>
        void OnGameStarted(Game game);

        /// <summary>
        /// Called when game stopped running.
        /// </summary>
        /// <param name="game">Game that stopped running.</param>
        /// <param name="ellapsedSeconds">Time in seconds of how long the game was running.</param>
        void OnGameStopped(Game game, long ellapsedSeconds);

        /// <summary>
        /// Called when game has been installed.
        /// </summary>
        /// <param name="game">Game that's been installed.</param>
        void OnGameInstalled(Game game);

        /// <summary>
        /// Called when game has been uninstalled.
        /// </summary>
        /// <param name="game">Game that's been uninstalled.</param>
        void OnGameUninstalled(Game game);

        void OnApplicationStarted();
    }
}
