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
    public class BethesdaLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "bethesdalibImportError";

        internal BethesdaLibrarySettings LibrarySettings { get; private set; }

        public BethesdaLibrary(IPlayniteAPI api) : base(api)
        {
            LibrarySettings = new BethesdaLibrarySettings(this, PlayniteApi);
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

        public List<GameInfo> GetInstalledGames()
        {
            var games = new List<GameInfo>();

            foreach (var program in Bethesda.GetBethesdaInstallEntried())
            {
                var installDir = program.Path.Trim('"');
                if (!Directory.Exists(installDir))
                {
                    continue;
                }

                var match = Regex.Match(program.UninstallString, @"uninstall\/(\d+)");
                var gameId = match.Groups[1].Value;
                var newGame = new GameInfo()
                {
                    GameId = gameId,
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

        public override LibraryClient Client => new BethesdaClient();

        public override string LibraryIcon => Bethesda.Icon;

        public override string Name => "Bethesda";

        public override Guid Id => Guid.Parse("0E2E793E-E0DD-4447-835C-C44A1FD506EC");

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = true
        };

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return firstRunSettings ? null : LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return firstRunView ? null : new BethesdaLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new BethesdaGameController(this, game);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    var installed = GetInstalledGames();
                    logger.Debug($"Found {installed.Count} installed Bethesda games.");
                    PlayniteApi.Notifications.Remove(dbImportMessageId);
                    return installed;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import uninstalled Bethesda games.");
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        dbImportMessageId,
                        string.Format(PlayniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                        System.Environment.NewLine + e.Message,
                        NotificationType.Error,
                        () => OpenSettingsView()));
                }
            }

            return allGames;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new BethesdaMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
