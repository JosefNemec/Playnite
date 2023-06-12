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
using Playnite.Database;
using static Microsoft.Scripting.Hosting.Shell.ConsoleHostOptions;

namespace Playnite.API
{
    public interface IPlayniteAPIRoot
    {
        string ExpandGameVariables(Game game, string inputString);
        GameAction ExpandGameVariables(Game game, GameAction action);
        string ExpandGameVariables(Game game, string inputString, string emulatorDir);
        void StartGame(Guid gameId);
        void InstallGame(Guid gameId);
        void UninstallGame(Guid gameId);
        void AddCustomElementSupport(Plugin source, AddCustomElementSupportArgs args);
        void AddSettingsSupport(Plugin source, AddSettingsSupportArgs args);
        void AddConvertersSupport(Plugin source, AddConvertersSupportArgs args);
    }

    public class PlayniteApiRoot : IPlayniteAPIRoot
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly GamesEditor gameEditor;
        private readonly ExtensionFactory extensions;
        private readonly GameDatabase database;
        private readonly SynchronizationContext execContext;

        public PlayniteApiRoot(
            GamesEditor gameEditor,
            ExtensionFactory extensions,
            GameDatabase database)
        {
            this.gameEditor = gameEditor;
            this.extensions = extensions;
            this.database = database;
            execContext = SynchronizationContext.Current;
        }

        public string ExpandGameVariables(Game game, string inputString)
        {
            return game?.ExpandVariables(inputString);
        }

        public string ExpandGameVariables(Game game, string inputString, string emulatorDir)
        {
            return game?.ExpandVariables(inputString, emulatorDir: emulatorDir);
        }

        public GameAction ExpandGameVariables(Game game, GameAction action)
        {
            return action?.ExpandVariables(game);
        }

        public void StartGame(Guid gameId)
        {
            var game = database.Games.Get(gameId);
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
            var game = database.Games.Get(gameId);
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
            var game = database.Games.Get(gameId);
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

        public void AddConvertersSupport(Plugin source, AddConvertersSupportArgs args)
        {
            extensions.AddConvertersSupport(source, args);
        }
    }

    public class PlayniteAPI : IPlayniteAPI
    {
        public IPlayniteAPIRoot RootApi { get; set; }
        public IDialogsFactory Dialogs { get; set; }
        public IGameDatabaseAPI Database { get; set; }
        public IMainViewAPI MainView { get; set; }
        public IPlaynitePathsAPI Paths { get; set; }
        public IPlayniteInfoAPI ApplicationInfo { get; set; }
        public IWebViewFactory WebViews { get; set; }
        public IResourceProvider Resources { get; set; }
        public INotificationsAPI Notifications { get; set; }
        public IUriHandlerAPI UriHandler { get; set; }
        public IPlayniteSettingsAPI ApplicationSettings { get; set; }
        public IAddons Addons { get; set; }
        public IEmulationAPI Emulation { get; set; }

        public PlayniteAPI()
        {
        }

        public string ExpandGameVariables(Game game, string inputString)
        {
            return RootApi.ExpandGameVariables(game, inputString);
        }

        public string ExpandGameVariables(Game game, string inputString, string emulatorDir)
        {
            return RootApi.ExpandGameVariables(game, inputString, emulatorDir);
        }

        public GameAction ExpandGameVariables(Game game, GameAction action)
        {
            return RootApi.ExpandGameVariables(game, action);
        }

        public void StartGame(Guid gameId)
        {
            RootApi.StartGame(gameId);
        }

        public void InstallGame(Guid gameId)
        {
            RootApi.InstallGame(gameId);
        }

        public void UninstallGame(Guid gameId)
        {
            RootApi.UninstallGame(gameId);
        }

        public void AddCustomElementSupport(Plugin source, AddCustomElementSupportArgs args)
        {
            RootApi.AddCustomElementSupport(source, args);
        }

        public void AddSettingsSupport(Plugin source, AddSettingsSupportArgs args)
        {
            RootApi.AddSettingsSupport(source, args);
        }

        public void AddConvertersSupport(Plugin source, AddConvertersSupportArgs args)
        {
            RootApi.AddConvertersSupport(source, args);
        }
    }
}