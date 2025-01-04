using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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
        [Description("LOCDetailsViewLabel")] Details = 0,
        /// <summary>
        ///
        /// </summary>
        [Description("LOCGridViewLabel")] Grid = 1,
        /// <summary>
        ///
        /// </summary>
        [Description("LOCListViewLabel")] List = 2
    }

    /// <summary>
    ///
    /// </summary>
    public enum FullscreenView : int
    {
        /// <summary>
        ///
        /// </summary>
        List = 0,
        /// <summary>
        ///
        /// </summary>
        Details = 1,
    }

    /// <summary>
    /// Describes object providing API for main UI view.
    /// </summary>
    public interface IMainViewAPI
    {
        /// <summary>
        /// Gets currently active Desktop mode view.
        /// </summary>
        DesktopView ActiveDesktopView { get; set; }

        /// <summary>
        /// Gets currently active Fullscreen mode view.
        /// </summary>
        FullscreenView ActiveFullscreenView { get; }

        /// <summary>
        /// Gets currently active sorting order.
        /// </summary>
        SortOrder SortOrder { get; }

        /// <summary>
        /// Gets currently active sorting order direction.
        /// </summary>
        SortOrderDirection SortOrderDirection { get; set; }

        /// <summary>
        /// Gets currently active grouping field.
        /// </summary>
        GroupableField Grouping { get; set; }

        /// <summary>
        /// Gets UI thread dispatcher.
        /// </summary>
        Dispatcher UIDispatcher { get; }

        /// <summary>
        /// Gets list of currently selected games.
        /// </summary>
        IEnumerable<Game> SelectedGames { get; }

        /// <summary>
        /// Gets list of games currently available in game list.
        /// </summary>
        List<Game> FilteredGames { get; }

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

        /// <summary>
        /// Selects multiple games.
        /// </summary>
        /// <param name="gameIds">List of game IDs to select.</param>
        void SelectGames(IEnumerable<Guid> gameIds);

        /// <summary>
        /// Applies filter preset.
        /// </summary>
        /// <param name="filterId">Filter ID.</param>
        void ApplyFilterPreset(Guid filterId);

        /// <summary>
        /// Applies filter preset.
        /// </summary>
        /// <param name="preset">Filter preset.</param>
        void ApplyFilterPreset(FilterPreset preset);

        /// <summary>
        /// Gets ID of currently active filter preset.
        /// </summary>
        /// <returns></returns>
        Guid GetActiveFilterPreset();

        /// <summary>
        /// Gets current filter settings.
        /// </summary>
        /// <returns></returns>
        FilterPresetSettings GetCurrentFilterSettings();

        /// <summary>
        /// Opens global search view.
        /// </summary>
        /// <param name="searchTerm">Default search term.</param>
        void OpenSearch(string searchTerm);

        /// <summary>
        /// Opens global search view.
        /// </summary>
        /// <param name="context">Search context to be activated after opening the view.</param>
        /// <param name="searchTerm">Default search term.</param>
        void OpenSearch(SearchContext context, string searchTerm);
    }
}
