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

        public List<GameInfo> GetLibraryGames()
        {
            var games = new List<GameInfo>();
            var dlcsToIgnore = new List<uint>();
            foreach (var item in Uplay.GetLocalProductCache())
            {
                if (item.root.addons.HasItems())
                {
                    foreach (var dlc in item.root.addons.Select(a => a.id))
                    {
                        dlcsToIgnore.AddMissing(dlc);
                    }
                }

                if (item.root.third_party_platform != null)
                {
                    continue;
                }

                if (item.root.is_ulc)
                {
                    dlcsToIgnore.AddMissing(item.uplay_id);
                    continue;
                }

                if (dlcsToIgnore.Contains(item.uplay_id))
                {
                    continue;
                }

                if (item.root.start_game == null)
                {
                    continue;
                }

                var newGame = new GameInfo
                {
                    Name = item.root.name,
                    GameId = item.uplay_id.ToString(),
                    BackgroundImage = item.root.background_image,
                    Icon = item.root.icon_image,
                    CoverImage = item.root.thumb_image,
                    Source = "Uplay"
                };

                games.Add(newGame);
            }

            return games;
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
            }

            if (installsKey != null)
            {
                foreach (var install in installsKey.GetSubKeyNames())
                {
                    var gameData = installsKey.OpenSubKey(install);
                    var installDir = (gameData.GetValue("InstallDir") as string)?.Replace('/', Path.DirectorySeparatorChar);
                    if (!installDir.IsNullOrEmpty() && Directory.Exists(installDir))
                    {
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
                }
            }

            return games;
        }

        #region ILibraryPlugin

        public override LibraryClient Client => new UplayClient();

        public override string LibraryIcon => Uplay.Icon;

        public override string Name => "Uplay";

        public override Guid Id => Guid.Parse("C2F038E5-8B92-4877-91F1-DA9094155FC5");

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = true
        };

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new UplayLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new UplayGameController(this, game);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            var installedGames = new List<GameInfo>();
            Exception importError = null;

            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    installedGames = GetInstalledGames();
                    logger.Debug($"Found {installedGames.Count} installed Uplay games.");
                    allGames.AddRange(installedGames);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed Uplay games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ImportUninstalledGames)
            {
                try
                {
                    var libraryGames = GetLibraryGames();
                    logger.Debug($"Found {libraryGames.Count} library Uplay games.");
                    foreach (var libGame in libraryGames)
                    {
                        var installed = installedGames.FirstOrDefault(a => a.GameId == libGame.GameId);
                        if (installed != null)
                        {
                            installed.Icon = libGame.Icon;
                            installed.BackgroundImage = libGame.BackgroundImage;
                            installed.CoverImage = libGame.CoverImage;
                        }
                        else
                        {
                            allGames.Add(libGame);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import uninstalled Uplay games.");
                    importError = e;
                }
            }

            if (importError != null)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    string.Format(PlayniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error,
                    () => OpenSettingsView()));
            }
            else
            {
                PlayniteApi.Notifications.Remove(dbImportMessageId);
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
