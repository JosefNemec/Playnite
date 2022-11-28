using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
        /// Gets main view API.
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

        /// <summary>
        /// Gets paths API.
        /// </summary>
        IPlaynitePathsAPI Paths { get; }

        /// <summary>
        /// Gets notification API.
        /// </summary>
        INotificationsAPI Notifications { get; }

        /// <summary>
        /// Gets application info API.
        /// </summary>
        IPlayniteInfoAPI ApplicationInfo { get; }

        /// <summary>
        /// Gets web view API.
        /// </summary>
        IWebViewFactory WebViews { get; }

        /// <summary>
        /// Gets resources API.
        /// </summary>
        IResourceProvider Resources { get; }

        /// <summary>
        /// Gets URI handler API.
        /// </summary>
        IUriHandlerAPI UriHandler { get; }

        /// <summary>
        /// Get application settings API.
        /// </summary>
        IPlayniteSettingsAPI ApplicationSettings { get; }

        /// <summary>
        /// Gets addons API.
        /// </summary>
        IAddons Addons { get; }

        /// <summary>
        /// Gets emulation API.
        /// </summary>
        IEmulationAPI Emulation { get; }

        /// <summary>
        /// Expands dynamic game variables in specified string.
        /// </summary>
        /// <param name="game">Game to use dynamic variables from.</param>
        /// <param name="inputString">String containing dynamic variables.</param>
        /// <returns>String with replaces variables.</returns>
        string ExpandGameVariables(Game game, string inputString);

        /// <summary>
        /// Expands dynamic game variables in specified string.
        /// </summary>
        /// <param name="game">Game to use dynamic variables from.</param>
        /// <param name="inputString">String containing dynamic variables.</param>
        /// <param name="emulatorDir">String to be used to expand {EmulatorDir} variable if present.</param>
        /// <returns>String with replaces variables.</returns>
        string ExpandGameVariables(Game game, string inputString, string emulatorDir);

        /// <summary>
        /// Expands dynamic game variables in specified game action.
        /// </summary>
        /// <param name="game">Game to use dynamic variables from.</param>
        /// <param name="action">Game action to expand variables to.</param>
        /// <returns>Game action with expanded variables.</returns>
        Models.GameAction ExpandGameVariables(Game game, Models.GameAction action);

        /// <summary>
        /// Starts game.
        /// </summary>
        /// <param name="gameId">Game's database ID.</param>
        void StartGame(Guid gameId);

        /// <summary>
        /// Installs game.
        /// </summary>
        /// <param name="gameId">Game's database ID.</param>
        void InstallGame(Guid gameId);

        /// <summary>
        /// Uninstalls game.
        /// </summary>
        /// <param name="gameId">Game's database ID.</param>
        void UninstallGame(Guid gameId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        void AddCustomElementSupport(Plugin source, AddCustomElementSupportArgs args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        void AddSettingsSupport(Plugin source, AddSettingsSupportArgs args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        void AddConvertersSupport(Plugin source, AddConvertersSupportArgs args);
    }

    /// <summary>
    /// Represents access class to API instances.
    /// </summary>
    public static class API
    {
        /// <summary>
        /// Gets Playnite API.
        /// </summary>
        public static IPlayniteAPI Instance { get; internal set; }
    }
}
