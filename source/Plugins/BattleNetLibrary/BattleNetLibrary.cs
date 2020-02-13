using BattleNetLibrary.Models;
using BattleNetLibrary.Services;
using Playnite.Common;
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
using System.Windows.Controls;

namespace BattleNetLibrary
{
    public class BattleNetLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "bnetlibImportError";

        internal BattleNetLibrarySettings LibrarySettings { get; private set; }

        public BattleNetLibrary(IPlayniteAPI api) : base(api)
        {
            LibrarySettings = new BattleNetLibrarySettings(this, PlayniteApi);
        }

        public static UninstallProgram GetUninstallEntry(BNetApp app)
        {
            foreach (var prog in Programs.GetUnistallProgramsList())
            {
                if (app.Type == BNetAppType.Classic)
                {
                    if (prog.DisplayName == app.InternalId)
                    {
                        return prog;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(prog.UninstallString))
                    {
                        continue;
                    }

                    var match = Regex.Match(prog.UninstallString, string.Format(@"Battle\.net.*--uid={0}.*\s", app.InternalId));
                    if (match.Success)
                    {
                        return prog;
                    }
                }
            }

            return null;
        }

        public static GameAction GetGamePlayTask(string id)
        {
            return new GameAction()
            {
                Type = GameActionType.URL,
                Path = $"battlenet://{id}/",
                IsHandledByPlugin = true
            };
        }

        public Dictionary<string, GameInfo> GetInstalledGames()
        {
            var games = new Dictionary<string, GameInfo>();
            foreach (var prog in Programs.GetUnistallProgramsList())
            {
                if (string.IsNullOrEmpty(prog.UninstallString))
                {
                    continue;
                }

                if (prog.Publisher == "Blizzard Entertainment" && BattleNetGames.Games.Any(a => a.Type == BNetAppType.Classic && prog.DisplayName == a.InternalId))
                {
                    var products = BattleNetGames.Games.Where(a => a.Type == BNetAppType.Classic && prog.DisplayName == a.InternalId);
                    foreach (var product in products)
                    {
                        if (!Directory.Exists(prog.InstallLocation))
                        {
                            continue;
                        }

                        var game = new GameInfo()
                        {
                            GameId = product.ProductId,
                            Source = "Battle.net",
                            Name = product.Name,
                            PlayAction = new GameAction()
                            {
                                Type = GameActionType.File,
                                WorkingDir = ExpandableVariables.InstallationDirectory,
                                Path = product.ClassicExecutable,
                                IsHandledByPlugin = true
                            },
                            InstallDirectory = prog.InstallLocation,
                            IsInstalled = true
                        };

                        // Check in case there are more versions of single games installed.
                        if (!games.TryGetValue(game.GameId, out var _))
                        {
                            games.Add(game.GameId, game);
                        }
                    }
                }
                else
                {
                    var match = Regex.Match(prog.UninstallString, @"Battle\.net.*--uid=(.*?)\s");
                    if (!match.Success)
                    {
                        continue;
                    }

                    if (prog.DisplayName.EndsWith("Test") || prog.DisplayName.EndsWith("Beta"))
                    {
                        continue;
                    }

                    var iId = match.Groups[1].Value;
                    var product = BattleNetGames.Games.FirstOrDefault(a => a.Type == BNetAppType.Default && iId.StartsWith(a.InternalId));
                    if (product == null)
                    {
                        continue;
                    }

                    if (!Directory.Exists(prog.InstallLocation))
                    {
                        continue;
                    }

                    var game = new GameInfo()
                    {
                        GameId = product.ProductId,
                        Source = "Battle.net",
                        Name = product.Name,
                        PlayAction = GetGamePlayTask(product.ProductId),
                        InstallDirectory = prog.InstallLocation,
                        IsInstalled = true
                    };

                    // Check in case there are more versions of single games installed.
                    if (!games.TryGetValue(game.GameId, out var _))
                    {
                        games.Add(game.GameId, game);
                    }
                }
            }

            return games;
        }

        public List<GameInfo> GetLibraryGames()
        {
            using (var view = PlayniteApi.WebViews.CreateOffscreenView())
            {
                var api = new BattleNetAccountClient(view);
                var games = new List<GameInfo>();
                if (!api.GetIsUserLoggedIn())
                {
                    throw new Exception("User is not logged in.");
                }

                var accountGames = api.GetOwnedGames();
                if (accountGames?.Any() == true)
                {
                    foreach (var product in accountGames)
                    {
                        var gameInfo = BattleNetGames.Games.FirstOrDefault(a => a.ApiId == product.titleId);
                        if (gameInfo == null)
                        {
                            logger.Warn($"Unknown game found on the account: {product.localizedGameName}/{product.titleId}, skipping import.");
                            continue;
                        }

                        // To avoid duplicates like multiple WoW accounts
                        if (!games.Any(a => a.GameId == gameInfo.ProductId))
                        {
                            games.Add(new GameInfo()
                            {
                                Source = "Battle.net",
                                GameId = gameInfo.ProductId,
                                Name = gameInfo.Name
                            });
                        }
                    }
                }

                var classicGames = api.GetOwnedClassicGames();
                if (classicGames?.Any() == true)
                {
                    // W3
                    var w3Games = classicGames.Where(a => a.regionalGameFranchiseIconFilename.Contains("warcraft-iii"));
                    if (w3Games.Any())
                    {
                        var w3 = BattleNetGames.Games.FirstOrDefault(a => a.ProductId == "W3");
                        games.Add(new GameInfo()
                        {
                            Source = "Battle.net",
                            GameId = w3.ProductId,
                            Name = w3.Name
                        });

                        if (w3Games.Count() == 2)
                        {
                            var w3x = BattleNetGames.Games.FirstOrDefault(a => a.ProductId == "W3X");
                            games.Add(new GameInfo()
                            {
                                Source = "Battle.net",
                                GameId = w3x.ProductId,
                                Name = w3x.Name
                            });
                        }
                    }

                    // D2
                    var d2Games = classicGames.Where(a => a.regionalGameFranchiseIconFilename.Contains("diablo-ii"));
                    if (d2Games.Any())
                    {
                        var d2 = BattleNetGames.Games.FirstOrDefault(a => a.ProductId == "D2");
                        games.Add(new GameInfo()
                        {
                            Source = "Battle.net",
                            GameId = d2.ProductId,
                            Name = d2.Name
                        });

                        if (d2Games.Count() == 2)
                        {
                            var d2x = BattleNetGames.Games.FirstOrDefault(a => a.ProductId == "D2X");
                            games.Add(new GameInfo()
                            {
                                Source = "Battle.net",
                                GameId = d2x.ProductId,
                                Name = d2x.Name
                            });
                        }
                    }
                }

                return games;
            }
        }

        #region ILibraryPlugin

        public override LibraryClient Client => new BattleNetClient();

        public override string LibraryIcon => BattleNet.Icon;

        public override string Name => "Battle.net";

        public override Guid Id => Guid.Parse("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD");

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
            return new BattleNetLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new BattleNetGameController(game, PlayniteApi);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            var installedGames = new Dictionary<string, GameInfo>();
            Exception importError = null;

            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    installedGames = GetInstalledGames();
                    logger.Debug($"Found {installedGames.Count} installed Battle.net games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed Battle.net games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ConnectAccount)
            {
                try
                {
                    var libraryGames = GetLibraryGames();
                    logger.Debug($"Found {libraryGames.Count} library Battle.net games.");

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
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import linked account Battle.net games details.");
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
            return new BattleNetMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
