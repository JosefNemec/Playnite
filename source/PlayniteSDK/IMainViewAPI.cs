using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    ///
    /// </summary>
    public enum DesktopView : int
    {
        /// <summary>
        ///
        /// </summary>
        Details = 0,
        /// <summary>
        ///
        /// </summary>
        Grid = 1,
        /// <summary>
        ///
        /// </summary>
        List = 2
    }

    /// <summary>
    /// Describes object providing API for main UI view.
    /// </summary>
    public interface IMainViewAPI
    {
        /// <summary>
        /// Gets currently active Desktop mode view.
        /// </summary>
        DesktopView ActiveDesktopView { get; }

        /// <summary>
        /// Gets list of currently selected games.
        /// </summary>
        IEnumerable<Game> SelectedGames { get; }

        /// <summary>
        /// Opens settings view for specified plugin.
        /// </summary>
        /// <param name="pluginId">Plugin ID.</param>
        /// <returns>True if user saved any changes, False if dialog was canceled.</returns>
        bool OpenPluginSettings(Guid pluginId);

        /// <summary>
        /// Switches Playnite to Library view.
        /// </summary>
        void SwitchToLibraryView();

        /// <summary>
        /// Selects game.
        /// </summary>
        /// <param name="gameId">Game's database ID.</param>
        void SelectGame(Guid gameId);
    }
}
