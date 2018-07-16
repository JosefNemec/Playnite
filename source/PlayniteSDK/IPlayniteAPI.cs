using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes object providing Playnite API.
    /// </summary>
    public interface IPlayniteAPI
    {
        /// <summary>
        /// Gest main view API.
        /// </summary>
        IMainViewAPI MainView { get; }

        /// <summary>
        /// Gets database API.
        /// </summary>
        IGameDatabaseAPI Database { get; }

        /// <summary>
        /// Gets dialog API.
        /// </summary>
        IDialogsFactory Dialogs { get; }

        IPlaynitePathsAPI Paths { get; }

        IPlayniteInfoAPI ApplicationInfo { get; }

        /// <summary>
        /// Returns string while resolving any dynamic variables supported by Playnite.
        /// </summary>
        /// <param name="game">Game to use dynamic variables from.</param>
        /// <param name="toResolve">String containing dynamic variables.</param>
        /// <returns>String with replaces variables.</returns>
        string ResolveGameVariables(Game game, string toResolve);

        /// <summary>
        /// Returns new instance of Playnite logger.
        /// </summary>
        /// <param name="name">Logger name.</param>
        /// <returns>Logger object.</returns>
        ILogger CreateLogger(string name);
    }
}
