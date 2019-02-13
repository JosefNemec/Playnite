using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SteamLibrary.Models;
using SteamLibrary.Services;
using Playnite.Web;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Playnite;
using System.Windows;
using System.Reflection;
using System.Collections.ObjectModel;

namespace SteamLibrary
{
    public class SteamLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI playniteApi;
        private SteamServicesClient servicesClient;
        private readonly Configuration config;
        private readonly SteamApiClient apiClient = new SteamApiClient();
        private const string dbImportMessageId = "steamlibImportError";

        internal SteamLibrarySettings LibrarySettings { get; private set; }

        public SteamLibrary(IPlayniteAPI api)
        {
            Initialize(api);
            config = api.GetPluginConfiguration<Configuration>(this);
            servicesClient = new SteamServicesClient(config.ServicesEndpoint);
        }

        public SteamLibrary(IPlayniteAPI api, SteamServicesClient client)
        {
            Initialize(api);
            servicesClient = client;
        }

        private void Initialize(IPlayniteAPI api)
        {
            playniteApi = api;
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\steamicon.png");
            LibrarySettings = new SteamLibrarySettings(this, playniteApi)
            {
                SteamUsers = GetSteamUsers()
            };
        }

        internal static GameAction CreatePlayTask(GameID gameId)
        {
            return new GameAction()
            {
                Name = "Play",
                Type = GameActionType.URL,
                Path = @"steam://rungameid/" + gameId,
                IsHandledByPlugin = true
            };
        }

        internal Game GetInstalledGameFromFile(string path)
        {
            var kv = new KeyValue();
            kv.ReadFileAsText(path);

            var name = string.Empty;
            if (string.IsNullOrEmpty(kv["name"].Value))
            {
                if (kv["UserConfig"]["name"].Value != null)
                {
                    name = StringExtensions.NormalizeGameName(kv["UserConfig"]["name"].Value);
                }
            }
            else
            {
                name = StringExtensions.NormalizeGameName(kv["name"].Value);
            }

            var gameId = new GameID(kv["appID"].AsUnsignedInteger());
            var game = new Game()
            {
                PluginId = Id,
                Source = "Steam",
                GameId = gameId.ToString(),
                Name = name,
                InstallDirectory = Path.Combine((new FileInfo(path)).Directory.FullName, "common", kv["installDir"].Value),
                PlayAction = CreatePlayTask(gameId),
                IsInstalled = true
            };

            return game;
        }

        internal List<Game> GetInstalledGamesFromFolder(string path)
        {
            var games = new List<Game>();

            foreach (var file in Directory.GetFiles(path, @"appmanifest*"))
            {
                try
                {
                    var game = GetInstalledGameFromFile(Path.Combine(path, file));
                    games.Add(game);
                }
                catch (Exception exc)
                {
                    // Steam can generate invalid acf file according to issue #37
                    logger.Error(exc, $"Failed to get information about installed game from: {file}");
                }
            }

            return games;
        }

        internal List<Game> GetInstalledGoldSrcModsFromFolder(string path)
        {
            var games = new List<Game>();
            var firstPartyMods = new string[] { "bshift", "cstrike", "czero", "czeror", "dmc", "dod", "gearbox", "ricochet", "tfc", "valve"};
            var dirInfo = new DirectoryInfo(path);

            foreach (var folder in dirInfo.GetDirectories().Where(a => !firstPartyMods.Contains(a.Name)).Select(a => a.FullName))
            {
                try
                {
                    var game = GetInstalledModFromFolder(folder, ModInfo.ModType.HL);
                    if (game != null)
                    {
                        games.Add(game);
                    }
                }
                catch (Exception exc)
                {
                    // gameinfo.txt may not exist or may be invalid
                    logger.Error(exc, $"Failed to get information about installed GoldSrc mod from: {path}");
                }
            }

            return games;
        }

        internal List<Game> GetInstalledSourceModsFromFolder(string path)
        {
            var games = new List<Game>();

            foreach (var folder in Directory.GetDirectories(path))
            {
                try
                {
                    var game = GetInstalledModFromFolder(folder, ModInfo.ModType.HL2);
                    if (game != null)
                    {
                        games.Add(game);
                    }
                }
                catch (Exception exc)
                {
                    // gameinfo.txt may not exist or may be invalid
                    logger.Error(exc, $"Failed to get information about installed Source mod from: {path}");
                }
            }

            return games;
        }

        internal Game GetInstalledModFromFolder(string path, ModInfo.ModType modType)
        {
            var modInfo = ModInfo.GetFromFolder(path, modType);
            if (modInfo == null)
            {
                return null;
            }

            var game = new Game()
            {
                PluginId = Id,
                Source = "Steam",
                GameId = modInfo.GameId.ToString(),
                Name = modInfo.Name,
                InstallDirectory = path,
                PlayAction = CreatePlayTask(modInfo.GameId),
                IsInstalled = true,
                Developers = new ComparableList<string>() { modInfo.Developer },
                Links = modInfo.Links,
                Tags = modInfo.Categories,
                Icon = modInfo.IconPath
            };

            return game;
        }

        internal Dictionary<string, Game> GetInstalledGames(bool includeMods = true)
        {
            var games = new Dictionary<string, Game>();
            if (!Steam.IsInstalled)
            {
                return games;
            }

            foreach (var folder in GetLibraryFolders())
            {
                var libFolder = Path.Combine(folder, "steamapps");
                if (Directory.Exists(libFolder))
                {
                    GetInstalledGamesFromFolder(libFolder).ForEach(a =>
                    {
                        if (!games.ContainsKey(a.GameId))
                        {
                            games.Add(a.GameId, a);
                        }
                    });
                }
                else
                {
                    logger.Warn($"Steam library {libFolder} not found.");
                }
            }

            if (includeMods)
            {
                try
                {
                    // In most cases, this will be inside the folder where Half-Life is installed.
                    var modInstallPath = Steam.ModInstallPath;
                    if (!string.IsNullOrEmpty(modInstallPath) && Directory.Exists(modInstallPath))
                    {
                        GetInstalledGoldSrcModsFromFolder(Steam.ModInstallPath).ForEach(a =>
                        {
                            if (!games.ContainsKey(a.GameId))
                            {
                                games.Add(a.GameId, a);
                            }
                        });
                    }

                    // In most cases, this will be inside the library folder where Steam is installed.
                    var sourceModInstallPath = Steam.SourceModInstallPath;
                    if (!string.IsNullOrEmpty(sourceModInstallPath) && Directory.Exists(sourceModInstallPath))
                    {
                        GetInstalledSourceModsFromFolder(Steam.SourceModInstallPath).ForEach(a =>
                        {
                            if (!games.ContainsKey(a.GameId))
                            {
                                games.Add(a.GameId, a);
                            }
                        });
                    }
                }
                catch (Exception e) when (!Environment.IsDebugBuild)
                {
                    logger.Error(e, "Failed to import Steam mods.");
                }
            }

            return games;
        }

        internal List<string> GetLibraryFolders()
        {
            var dbs = new List<string>() { Steam.InstallationPath };
            var configPath = Path.Combine(Steam.InstallationPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(configPath))
            {
                return dbs;
            }

            var kv = new KeyValue();
            kv.ReadFileAsText(configPath);

            foreach (var child in kv.Children)
            {
                if (int.TryParse(child.Name, out int test))
                {
                    if (!string.IsNullOrEmpty(child.Value) && Directory.Exists(child.Value))
                    {
                        dbs.Add(child.Value);
                    }
                }
            }

            return dbs;
        }

        internal List<LocalSteamUser> GetSteamUsers()
        {
            var users = new List<LocalSteamUser>();
            if (File.Exists(Steam.LoginUsersPath))
            {
                var config = new KeyValue();

                try
                {
                    config.ReadFileAsText(Steam.LoginUsersPath);
                    foreach (var user in config.Children)
                    {
                        users.Add(new LocalSteamUser()
                        {
                            Id = ulong.Parse(user.Name),
                            AccountName = user["AccountName"].Value,
                            PersonaName = user["PersonaName"].Value,
                            Recent = user["mostrecent"].AsBoolean()
                        });
                    }
                }
                catch (Exception e) when (!Environment.IsDebugBuild)
                {
                    logger.Error(e, "Failed to get list of local users.");
                }
            }

            return users;
        }

        internal List<Game> GetLibraryGames(SteamLibrarySettings settings)
        {
            var userName = string.Empty;
            if (settings.IdSource == SteamIdSource.Name)
            {
                userName = settings.AccountName;
            }
            else
            {
                userName = settings.AccountId.ToString();
            }

            if (settings.IsPrivateAccount)
            {
                return GetLibraryGames(userName, settings.ApiKey);
            }
            else
            {
                return GetLibraryGames(userName);
            }
        }

        internal List<Game> GetLibraryGames(string userName, string apiKey)
        {
            var userNameUrl = @"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}";
            var libraryUrl = @"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&include_appinfo=1&include_played_free_games=1&format=json&steamid={1}";

            ulong userId = 0;
            if (!ulong.TryParse(userName, out userId))
            {
                var stringData = HttpDownloader.DownloadString(string.Format(userNameUrl, apiKey, userName));
                userId = ulong.Parse(JsonConvert.DeserializeObject<ResolveVanityResult>(stringData).response.steamid);
            }

            var stringLibrary = HttpDownloader.DownloadString(string.Format(libraryUrl, apiKey, userId));
            var library = JsonConvert.DeserializeObject<GetOwnedGamesResult>(stringLibrary);
            if (library.response.games == null)
            {
                throw new Exception("No games found on specified Steam account.");
            }

            var games = new List<Game>();
            foreach (var game in library.response.games)
            {
                // Ignore games without name, like 243870
                if (string.IsNullOrEmpty(game.name))
                {
                    continue;
                }

                var newGame = new Game()
                {
                    PluginId = Id,
                    Source = "Steam",
                    Name = game.name,
                    GameId = game.appid.ToString(),
                    Playtime = game.playtime_forever * 60,
                    CompletionStatus = game.playtime_forever > 0 ? CompletionStatus.Played : CompletionStatus.NotPlayed
                };

                games.Add(newGame);
            }

            return games;
        }

        internal List<Game> GetLibraryGames(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("Steam user name cannot be empty.");
            }

            var games = new List<Game>();
            var importedGames = servicesClient.GetSteamLibrary(userName);
            if (importedGames == null)
            {
                throw new Exception("No games found on specified Steam account.");
            }

            foreach (var game in importedGames)
            {
                // Ignore games without name, like 243870
                if (string.IsNullOrEmpty(game.name))
                {
                    continue;
                }

                var newGame = new Game()
                {
                    PluginId = Id,
                    GameId = game.appid.ToString(),
                    Source = "Steam",
                    Name = game.name,
                    Playtime = game.playtime_forever * 60,
                    CompletionStatus = game.playtime_forever > 0 ? CompletionStatus.Played : CompletionStatus.NotPlayed
                };

                games.Add(newGame);
            }

            return games;
        }

        public List<Game> GetCategorizedGames(ulong steamId)
        {
            var id = new SteamID(steamId);
            var result = new List<Game>();
            var vdf = Path.Combine(Steam.InstallationPath, "userdata", id.AccountID.ToString(), "7", "remote", "sharedconfig.vdf");
            var sharedconfig = new KeyValue();
            sharedconfig.ReadFileAsText(vdf);

            var apps = sharedconfig["Software"]["Valve"]["Steam"]["apps"];
            foreach (var app in apps.Children)
            {
                if (app.Children.Count == 0)
                {
                    continue;
                }

                var appData = new List<string>();
                foreach (var tag in app["tags"].Children)
                {
                    appData.Add(tag.Value);
                }

                string gameId = app.Name;
                if (app.Name.Contains('_'))
                {
                    // Mods are keyed differently, "<appId>_<modId>"
                    // Ex. 215_2287856061
                    string[] parts = app.Name.Split('_');
                    if (uint.TryParse(parts[0], out uint appId) && uint.TryParse(parts[1], out uint modId))
                    {
                        var gid = new GameID()
                        {
                            AppID = appId,
                            AppType = GameID.GameType.GameMod,
                            ModID = modId
                        };
                        gameId = gid;
                    }
                    else
                    {
                        // Malformed app id?
                        continue;
                    }
                }

                result.Add(new Game()
                {
                    PluginId = Id,
                    Source = "Steam",
                    GameId = gameId,
                    Categories = new ComparableList<string>(appData),
                    Hidden = app["hidden"].AsInteger() == 1
                });
            }

            return result;
        }

        public void ImportSteamCategories(ulong accountId)
        {
            var dialogs = playniteApi.Dialogs;
            var resources = playniteApi.Resources;
            var db = playniteApi.Database;

            if (dialogs.ShowMessage(
                resources.FindString("LOCSettingsSteamCatImportWarn"),
                resources.FindString("LOCSettingsSteamCatImportWarnTitle"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            if (accountId == 0)
            {
                dialogs.ShowMessage(
                    resources.FindString("LOCSettingsSteamCatImportErrorAccount"),
                    resources.FindString("LOCImportError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!db.IsOpen)
            {
                dialogs.ShowMessage(
                    resources.FindString("LOCSettingsSteamCatImportErrorDb"),
                    resources.FindString("LOCImportError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (db.BufferedUpdate())
                {
                    foreach (var game in GetCategorizedGames(accountId))
                    {
                        var dbGame = db.GetGames().FirstOrDefault(a => a.PluginId == game.PluginId && a.GameId == game.GameId);
                        if (dbGame == null)
                        {
                            continue;
                        }

                        dbGame.Categories = game.Categories;
                        dbGame.Hidden = game.Hidden;
                        db.UpdateGame(dbGame);
                    }
                }

                dialogs.ShowMessage(resources.FindString("LOCImportCompleted"));
            }
            catch (Exception exc) when (!Environment.IsDebugBuild)
            {
                logger.Error(exc, "Failed to import Steam categories.");
                dialogs.ShowMessage(
                    resources.FindString("LOCSettingsSteamCatImportError"),
                    resources.FindString("LOCImportError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region ILibraryPlugin

        public ILibraryClient Client { get; } = new SteamClient();

        public Guid Id { get; } = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB");

        public string Name { get; } = "Steam";

        public string LibraryIcon { get; private set; }

        public bool IsClientInstalled => Steam.IsInstalled;

        public void Dispose()
        {
            apiClient.Logout();
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            LibrarySettings.ShowCategoryImport = !firstRunSettings;
            return LibrarySettings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return new SteamLibrarySettingsView();
        }

        public IEnumerable<Game> GetGames()
        {
            var allGames = new List<Game>();
            var installedGames = new Dictionary<string, Game>();
            Exception importError = null;

            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    installedGames = GetInstalledGames();
                    logger.Debug($"Found {installedGames.Count} installed Steam games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed battle.net games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ImportUninstalledGames)
            {
                try
                {
                    var uninstalled = GetLibraryGames(LibrarySettings);
                    logger.Debug($"Found {uninstalled.Count} library Steam games.");

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
                    logger.Error(e, "Failed to import uninstalled Steam games.");
                    importError = e;
                }
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

        public IGameController GetGameController(Game game)
        {
            return new SteamGameController(game, this);
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return new SteamMetadataProvider(servicesClient, this, apiClient);
        }

        #endregion ILibraryPlugin
    }
}
