using ItchioLibrary.Models;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ItchioLibrary
{
    public class ItchioLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private const string dbImportMessageId = "itchiolibImportError";
        internal readonly ItchioLibrarySettings LibrarySettings;

        public ItchioLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibrarySettings = new ItchioLibrarySettings(this, api);
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\itchioicon.png");
        }

        public static bool TryGetGameActions(string installDir, out GameAction playAction, out List<GameAction> otherActions)
        {
            var fileEnum = new SafeFileEnumerator(installDir, ".itch.toml", SearchOption.AllDirectories);
            if (fileEnum.Any())
            {
                var strMan = File.ReadAllText(fileEnum.First().FullName);
                var manifest = Serialization.FromToml<LaunchManifest>(strMan);
                if (manifest.actions?.Any() == true)
                {
                    playAction = null;
                    otherActions = new List<GameAction>();
                    foreach (var action in manifest.actions)
                    {
                        if (action.name.Equals("play", StringComparison.OrdinalIgnoreCase))
                        {
                            playAction = new GameAction
                            {
                                IsHandledByPlugin = true,
                                Name = "Play",
                                Path = action.path,
                                WorkingDir = action.path.IsHttpUrl() ? null : "{InstallDir}",
                                Type = action.path.IsHttpUrl() ? GameActionType.URL : GameActionType.File,
                                Arguments = action.args?.Any() == true ? string.Join(" ", action.args) : null
                            };
                        }
                        else
                        {
                            otherActions.Add(new GameAction
                            {
                                Name = action.name,
                                Path = action.path,
                                WorkingDir = action.path.IsHttpUrl() ? null : "{InstallDir}",
                                Type = action.path.IsHttpUrl() ? GameActionType.URL : GameActionType.File,
                                Arguments = action.args?.Any() == true ? string.Join(" ", action.args) : null
                            });
                        }
                    }

                    return true;
                }
            }

            playAction = null;
            otherActions = null;
            return false;
        }

        internal Dictionary<string, Game> GetInstalledGames()
        {
            var games = new Dictionary<string, Game>();
            using (var butler = new Butler())
            {
                var caves = butler.GetCaves();
                if (caves?.Any() != true)
                {
                    return games;
                }

                foreach (var cave in caves)
                {
                    if (cave.game.classification != Models.GameClassification.game &&
                        cave.game.classification != Models.GameClassification.tool)
                    {
                        continue;
                    }

                    // TODO: We don't support multiple version of one game at moment
                    if (games.ContainsKey(cave.game.id.ToString()))
                    {
                        continue;
                    }

                    var installDir = cave.installInfo.installFolder;
                    if (!Directory.Exists(installDir))
                    {
                        continue;
                    }

                    var game = new Game()
                    {
                        PluginId = Id,
                        Source = "itch.io",
                        GameId = cave.game.id.ToString(),
                        Name = cave.game.title,
                        InstallDirectory = installDir,
                        IsInstalled = true,
                        CoverImage = cave.game.coverUrl,
                        PlayAction = new GameAction
                        {
                            Type = GameActionType.URL,
                            Path = ItchioGameController.DynamicLaunchActionStr,
                            Arguments = cave.id,
                            IsHandledByPlugin = true
                        }
                    };

                    if (TryGetGameActions(installDir, out var play, out var others))
                    {
                        game.OtherActions = new ObservableCollection<GameAction>(others);
                    }

                    games.Add(game.GameId, game);
                }
            }

            return games;
        }

        internal List<Game> GetLibraryGames()
        {
            var games = new List<Game>();
            using (var butler = new Butler())
            {
                var profiles = butler.GetProfiles();
                if (profiles?.Any() != true)
                {
                    throw new Exception("User is not authenticated.");
                }

                foreach (var profile in profiles)
                {
                    var keys = butler.GetOwnedKeys(profile.id);
                    foreach (var key in keys)
                    {
                        if (key.game == null)
                        {
                            continue;
                        }

                        if (key.game.classification != GameClassification.game &&
                            key.game.classification != GameClassification.tool)
                        {
                            continue;
                        }

                        if (games.Any(a => a.GameId == key.game.id.ToString()))
                        {
                            continue;
                        }

                        var game = new Game()
                        {
                            PluginId = Id,
                            Source = "itch.io",
                            GameId = key.game.id.ToString(),
                            Name = key.game.title,
                            CoverImage = key.game.coverUrl
                        };

                        games.Add(game);
                    }
                }
            }

            return games;
        }

        #region ILibraryPlugin

        public ILibraryClient Client { get; } = new ItchioClient();

        public string LibraryIcon { get; }

        public string Name { get; } = "itch.io";

        public bool IsClientInstalled => Itch.IsInstalled;

        public Guid Id { get; } = Guid.Parse("00000001-EBB2-4EEC-ABCB-7C89937A42BB");

        public void Dispose()
        {
        }
               
        public IGameController GetGameController(Game game)
        {
            return new ItchioGameController(game, playniteApi);
        }

        public IEnumerable<Game> GetGames()
        {
            var allGames = new List<Game>();
            var installedGames = new Dictionary<string, Game>();
            Exception importError = null;

            if (!LibrarySettings.ImportInstalledGames && !LibrarySettings.ImportUninstalledGames)
            {
                return allGames;
            }

            if (Itch.IsInstalled)
            {
                if (LibrarySettings.ImportInstalledGames)
                {
                    try
                    {
                        installedGames = GetInstalledGames();
                        logger.Debug($"Found {installedGames.Count} installed itch.io games.");
                        allGames.AddRange(installedGames.Values.ToList());
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to import installed itch.io games.");
                        importError = e;
                    }
                }

                if (LibrarySettings.ImportUninstalledGames)
                {
                    try
                    {
                        var uninstalled = GetLibraryGames();
                        logger.Debug($"Found {uninstalled.Count} library itch.io games.");

                        foreach (var game in uninstalled)
                        {
                            if (installedGames.TryGetValue(game.GameId, out var installed))
                            {
                                installed.Playtime = game.Playtime;
                                installed.LastActivity = game.LastActivity;
                            }
                            else
                            {
                                allGames.Add(game);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to import uninstalled itch.io games.");
                        importError = e;
                    }
                }
            }
            else
            {
                importError = new Exception(
                    playniteApi.Resources.FindString("LOCItchioClientNotInstalledError"));
            }

            if (importError != null)
            {
                playniteApi.Notifications.Add(
                    dbImportMessageId,
                    string.Format(playniteApi.Resources.FindString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error);
            }
            else
            {
                playniteApi.Notifications.Remove(dbImportMessageId);
            }

            return allGames;
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return new ItchioMetadataProvider();
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return new ItchioLibrarySettingsView();
        }

        #endregion ILibraryPlugin
    }
}
