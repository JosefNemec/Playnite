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
        private const string dbImportMessageId = "uplaylibImportError";

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

        public List<GameInfo> GetInstalledGames()
        {
            var games = new List<GameInfo>();

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

                var newGame = new GameInfo()
                {
                    GameId = install,
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

        public bool IsClientInstalled => Uplay.IsInstalled;

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

        public IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    var installed = GetInstalledGames();
                    logger.Debug($"Found {installed.Count} installed Uplay games.");
                    playniteApi.Notifications.Remove(dbImportMessageId);
                    return installed;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import uninstalled Uplay games.");
                    playniteApi.Notifications.Add(
                        dbImportMessageId,
                        string.Format(playniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                        System.Environment.NewLine + e.Message,
                        NotificationType.Error);
                }
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
