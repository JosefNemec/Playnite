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
    public class ItchioLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "itchiolibImportError";
        internal readonly ItchioLibrarySettings LibrarySettings;

        public ItchioLibrary(IPlayniteAPI api) : base(api)
        {
            LibrarySettings = new ItchioLibrarySettings(this, api);
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
                                WorkingDir = action.path.IsHttpUrl() ? null : ExpandableVariables.InstallationDirectory,
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
                                WorkingDir = action.path.IsHttpUrl() ? null : ExpandableVariables.InstallationDirectory,
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

        internal Dictionary<string, GameInfo> GetInstalledGames()
        {
            var games = new Dictionary<string, GameInfo>();
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

                    var game = new GameInfo()
                    {
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
                        game.OtherActions = new List<GameAction>(others);
                    }

                    games.Add(game.GameId, game);
                }
            }

            return games;
        }

        internal List<GameInfo> GetLibraryGames()
        {
            var games = new List<GameInfo>();
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
                    if (!keys.HasItems())
                    {
                        continue;
                    }

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

                        var game = new GameInfo()
                        {
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

        public override LibraryClient Client => new ItchioClient();

        public override string LibraryIcon => Itch.Icon;

        public override string Name => "itch.io";

        public override Guid Id => Guid.Parse("00000001-EBB2-4EEC-ABCB-7C89937A42BB");

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = false
        };

        public override IGameController GetGameController(Game game)
        {
            return new ItchioGameController(game, PlayniteApi);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            var installedGames = new Dictionary<string, GameInfo>();
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
                    catch (Exception e) when (!PlayniteApi.ApplicationInfo.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to import installed itch.io games.");
                        importError = e;
                    }
                }

                if (LibrarySettings.ConnectAccount)
                {
                    try
                    {
                        var libraryGames = GetLibraryGames();
                        logger.Debug($"Found {libraryGames.Count} library itch.io games.");

                        if (!LibrarySettings.ImportUninstalledGames)
                        {
                            libraryGames = libraryGames.Where(lg => installedGames.ContainsKey(lg.GameId)).ToList();
                        }

                        foreach (var game in libraryGames)
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
                    catch (Exception e) when (!PlayniteApi.ApplicationInfo.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to import linked account itch.io games details.");
                        importError = e;
                    }
                }
            }
            else
            {
                importError = new Exception(
                    PlayniteApi.Resources.GetString("LOCItchioClientNotInstalledError"));
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
            return new ItchioMetadataProvider(this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new ItchioLibrarySettingsView();
        }

        #endregion ILibraryPlugin
    }
}
