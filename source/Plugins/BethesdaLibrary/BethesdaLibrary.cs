using Microsoft.Win32;
using Playnite.Common.System;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BethesdaLibrary
{
    public class BethesdaLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;

        internal BethesdaLibrarySettings LibrarySettings { get; private set; }

        public BethesdaLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\bethesdaicon.png");
            LibrarySettings = new BethesdaLibrarySettings(this, playniteApi);
        }

        public GameAction GetGamePlayTask(string id)
        {
            return new GameAction()
            {
                Type = GameActionType.URL,
                Path = @"bethesdanet://run/" + id,
                IsHandledByPlugin = true
            };
        }

        public List<Game> GetInstalledGames()
        {
            var games = new List<Game>();

            foreach (var program in Bethesda.GetBethesdaInstallEntried())
            {
                var installDir = program.Path.Trim('"');
                if (!Directory.Exists(installDir))
                {
                    continue;
                }

                var match = Regex.Match(program.UninstallString, @"uninstall\/(\d+)");
                var gameId = match.Groups[1].Value;
                var newGame = new Game()
                {
                    GameId = gameId,
                    PluginId = Id,
                    Source = "Bethesda",
                    InstallDirectory = installDir,
                    PlayAction = GetGamePlayTask(gameId),
                    Name = program.DisplayName,
                    IsInstalled = true
                };

                games.Add(newGame);
            }

            return games;
        }

        #region ILibraryPlugin

        public ILibraryClient Client { get; } = new BethesdaClient();

        public string LibraryIcon { get; }

        public string Name { get; } = "Bethesda";

        public Guid Id { get; } = Guid.Parse("0E2E793E-E0DD-4447-835C-C44A1FD506EC");

        public void Dispose()
        {

        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return firstRunSettings ? null : LibrarySettings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return firstRunView ? null : new BethesdaLibrarySettingsView();
        }

        public IGameController GetGameController(Game game)
        {
            return new BethesdaGameController(this, game);
        }

        public IEnumerable<Game> GetGames()
        {
            var allGames = new List<Game>();
            if (LibrarySettings.ImportInstalledGames)
            {
                return GetInstalledGames();
            }

            return allGames;
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return new BethesdaMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
