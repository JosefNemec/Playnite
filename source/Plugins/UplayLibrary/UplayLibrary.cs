using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UplayLibrary
{
    public class UplayLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;

        internal UplayLibrarySettings LibrarySettings { get; private set; }

        public UplayLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\uplayicon.png");
            LibrarySettings = new UplayLibrarySettings(this, playniteApi);
        }

        public GameAction GetGamePlayTask(string id)
        {
            return new GameAction()
            {
                Type = GameActionType.URL,
                Path = @"uplay://launch/" + id,
                IsHandledByPlugin = true
            };
        }

        public List<Game> GetInstalledGames()
        {
            var games = new List<Game>();

            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var installsKey = root.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");
            if (installsKey == null)
            {
                root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                installsKey = root.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");

                if (installsKey == null)
                {
                    return games;
                }
            }

            foreach (var install in installsKey.GetSubKeyNames())
            {
                var gameData = installsKey.OpenSubKey(install);
                var installDir = (gameData.GetValue("InstallDir") as string).Replace('/', Path.DirectorySeparatorChar);

                var newGame = new Game()
                {
                    GameId = install,
                    PluginId = Id,
                    Source = "Uplay",
                    InstallDirectory = installDir,
                    PlayAction = GetGamePlayTask(install),
                    Name = Path.GetFileName(installDir.TrimEnd(Path.DirectorySeparatorChar)),
                    IsInstalled = true
                };

                games.Add(newGame);
            }

            return games;
        }

        #region ILibraryPlugin

        public ILibraryClient Client { get; } = new UplayClient();

        public string LibraryIcon { get; }

        public string Name { get; } = "Uplay";

        public Guid Id { get; } = Guid.Parse("C2F038E5-8B92-4877-91F1-DA9094155FC5");

        public void Dispose()
        {

        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return firstRunSettings ? null : LibrarySettings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return firstRunView ? null : new UplayLibrarySettingsView();
        }

        public IGameController GetGameController(Game game)
        {
            return new UplayGameController(this, game);
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
            return new UplayMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
