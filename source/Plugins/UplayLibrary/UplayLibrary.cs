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
    public class UplayLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "uplaylibImportError";

        internal UplayLibrarySettings LibrarySettings { get; private set; }

        public UplayLibrary(IPlayniteAPI api) : base(api)
        {
            LibrarySettings = new UplayLibrarySettings(this);
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

        public override LibraryClient Client => new UplayClient();

        public override string LibraryIcon => Uplay.Icon;

        public override string Name => "Uplay";

        public override Guid Id => Guid.Parse("C2F038E5-8B92-4877-91F1-DA9094155FC5");

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return firstRunSettings ? null : LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return firstRunView ? null : new UplayLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new UplayGameController(this, game);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    var installed = GetInstalledGames();
                    logger.Debug($"Found {installed.Count} installed Uplay games.");
                    PlayniteApi.Notifications.Remove(dbImportMessageId);
                    return installed;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import uninstalled Uplay games.");
                    PlayniteApi.Notifications.Add(
                        dbImportMessageId,
                        string.Format(PlayniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                        System.Environment.NewLine + e.Message,
                        NotificationType.Error);
                }
            }

            return allGames;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new UplayMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
