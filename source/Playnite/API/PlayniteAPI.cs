using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite.Plugins;
using System.Threading;

namespace Playnite.API
{
    public class PlayniteAPI : IPlayniteAPI
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly GamesEditor gameEditor;
        private readonly ExtensionFactory extensions;
        private readonly SynchronizationContext execContext;

        public PlayniteAPI(
            IGameDatabaseAPI databaseApi,
            IDialogsFactory dialogs,
            IMainViewAPI mainViewApi,
            IPlayniteInfoAPI infoApi,
            IPlaynitePathsAPI pathsApi,
            IWebViewFactory webViewFactory,
            IResourceProvider resources,
            INotificationsAPI notifications,
            GamesEditor gameEditor,
            IUriHandlerAPI uriHandler,
            IPlayniteSettingsAPI settingsApi,
            IAddons addonsApi,
            IEmulationAPI emulation,
            ExtensionFactory extensions)
        {
            WebViews = webViewFactory;
            Paths = pathsApi;
            ApplicationInfo = infoApi;
            MainView = mainViewApi;
            Dialogs = dialogs;
            Database = databaseApi;
            Resources = resources;
            Notifications = notifications;
            this.gameEditor = gameEditor;
            UriHandler = uriHandler;
            ApplicationSettings = settingsApi;
            Addons = addonsApi;
            Emulation = emulation;
            this.extensions = extensions;
            SDK.API.Instance = this;
            execContext = SynchronizationContext.Current;
        }

        public IDialogsFactory Dialogs { get; }

        public IGameDatabaseAPI Database { get; }

        public IMainViewAPI MainView { get; set; }

        public IPlaynitePathsAPI Paths { get; }

        public IPlayniteInfoAPI ApplicationInfo { get; }

        public IWebViewFactory WebViews { get; }

        public IResourceProvider Resources { get; }

        public INotificationsAPI Notifications { get; }

        public IUriHandlerAPI UriHandler { get; }

        public IPlayniteSettingsAPI ApplicationSettings { get; }

        public IAddons Addons { get; }

        public IEmulationAPI Emulation { get; }

        public string ExpandGameVariables(Game game, string inputString)
        {
            return game?.ExpandVariables(inputString);
        }

        public GameAction ExpandGameVariables(Game game, GameAction action)
        {
            return action?.ExpandVariables(game);
        }

        public void StartGame(Guid gameId)
        {
            var game = Database.Games.Get(gameId);
            if (game == null)
            {
                logger.Error($"Can't start game, game ID {gameId} not found.");
            }
            else
            {
                // Run on main thread for edge cases like this:
                // https://www.reddit.com/r/playnite/comments/slyg99/boot_another_game_as_a_play_action/
                execContext.Send((_) => gameEditor.PlayGame(game), null);
            }
        }

        public void InstallGame(Guid gameId)
        {
            var game = Database.Games.Get(gameId);
            if (game == null)
            {
                logger.Error($"Can't install game, game ID {gameId} not found.");
            }
            else
            {
                gameEditor.InstallGame(game);
            }
        }

        public void UninstallGame(Guid gameId)
        {
            var game = Database.Games.Get(gameId);
            if (game == null)
            {
                logger.Error($"Can't uninstall game, game ID {gameId} not found.");
            }
            else
            {
                gameEditor.UnInstallGame(game);
            }
        }

        public void AddCustomElementSupport(Plugin source, AddCustomElementSupportArgs args)
        {
            extensions.AddCustomElementSupport(source, args);
        }

        public void AddSettingsSupport(Plugin source, AddSettingsSupportArgs args)
        {
            extensions.AddSettingsSupport(source, args);
        }
    }
}