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

        internal SteamLibrarySettings LibrarySettings
        {
            get => (SteamLibrarySettings)Settings;
        }

        public SteamLibrary(IPlayniteAPI api)
        {
            Initialize(api);
            var configPath = Path.Combine(api.GetPluginUserDataPath(this), "config.json");
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
            Settings = new SteamLibrarySettings(this, playniteApi)
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
                State = new GameState() { Installed = true }
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
                    games.Add(game);
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
                    games.Add(game);
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

            var game = new Game()
            {
                PluginId = Id,
                Source = "Steam",
                GameId = modInfo.GameId.ToString(),
                Name = modInfo.Name,
                InstallDirectory = path,
                PlayAction = CreatePlayTask(modInfo.GameId),
                State = new GameState() { Installed = true },
                Developers = new ComparableList<string>() { modInfo.Developer },
                Links = modInfo.Links,
                Tags = modInfo.Categories,
                Icon = modInfo.IconPath
            };

            return game;
        }

        internal Dictionary<string, Game> GetInstalledGames()
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

            return games;
        }

        internal List<string> GetLibraryFolders()
        {
            var dbs = new List<string>() { Steam.InstallationPath };
            var configPath = Path.Combine(Steam.InstallationPath, "steamapps", "libraryfolders.vdf");
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
            var libraryUrl = @"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&include_appinfo=1&format=json&steamid={1}";

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
                    CompletionStatus = game.playtime_forever > 0 ? CompletionStatus.Played : CompletionStatus.NotPlayed,
                    State = new GameState() { Installed = false }
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
                if (app["tags"].Children.Count == 0)
                {
                    continue;
                }

                var appData = new List<string>();
                foreach (var tag in app["tags"].Children)
                {
                    appData.Add(tag.Value);
                }

                result.Add(new Game()
                {
                    PluginId = Id,
                    Source = "Steam",
                    GameId = app.Name,
                    Categories = new ComparableList<string>(appData)
                });
            }

            return result;
        }

        public void ImportSteamCategories(ulong accountId)
        {
            var dialogs = playniteApi.Dialogs;
            var resources = playniteApi.Resources;

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

            if (!playniteApi.Database.IsOpen)
            {
                dialogs.ShowMessage(
                    resources.FindString("LOCSettingsSteamCatImportErrorDb"),
                    resources.FindString("LOCImportError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var games = GetCategorizedGames(accountId);
                playniteApi.Database.ImportCategories(games);
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

        public ISettings Settings { get; private set; }

        public string LibraryIcon { get; private set; }

        public void Dispose()
        {
            apiClient.Logout();
        }

        public UserControl SettingsView
        {
            get => new SteamLibrarySettingsView();
        }

        public IEnumerable<Game> GetGames()
        {
            var allGames = new List<Game>();
            var installedGames = GetInstalledGames();

            if (LibrarySettings.ImportInstalledGames)
            {
                allGames.AddRange(installedGames.Values.ToList());
            }

            if (LibrarySettings.ImportUninstalledGames)
            {
                var uninstalled = GetLibraryGames(LibrarySettings);
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

            return allGames;
        }

        public IGameController GetGameController(Game game)
        {
            return new SteamGameController(game);
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return new SteamMetadataProvider(servicesClient, this, apiClient);
        }

        #endregion ILibraryPlugin
    }
}
