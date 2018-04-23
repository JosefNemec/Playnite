using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using Playnite.Models;
using Playnite.Providers.Steam;
using SteamKit2;
using Playnite.Services;
using Playnite.Database;
using System.Windows;
using System.Globalization;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace Playnite.Providers.Steam
{
    public class SteamLibrary : ISteamLibrary
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        private ServicesClient playniteServices;

        public SteamLibrary()
        {
        }

        public SteamLibrary(ServicesClient playniteServices)
        {
            this.playniteServices = playniteServices;
        }

        private string GetGameWorkshopUrl(int id)
        {
            return $"http://steamcommunity.com/app/{id}/workshop/";
        }

        public Game GetInstalledGameFromFile(string path)
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

            var game = new Game()
            {
                Provider = Provider.Steam,
                Source = "Steam",
                ProviderId = kv["appID"].Value,
                Name = name,
                InstallDirectory = Path.Combine((new FileInfo(path)).Directory.FullName, "common", kv["installDir"].Value),
                PlayTask = new GameTask()
                {
                    Name = "Play",
                    Type = GameTaskType.URL,
                    Path = @"steam://run/" + kv["appID"].Value,
                    IsPrimary = true,
                    IsBuiltIn = true
                }
            };

            return game;
        }

        public List<Game> GetInstalledGamesFromFolder(string path)
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
                    logger.Error(exc, "Failed to get information about installed game from {0}: ", file);
                }
            }

            return games;
        }

        public List<Game> GetInstalledGames()
        {
            var games = new List<Game>();

            foreach (var folder in GetLibraryFolders())
            {
                var libFolder = Path.Combine(folder, "steamapps");
                if (Directory.Exists(libFolder))
                {
                    games.AddRange(GetInstalledGamesFromFolder(libFolder));
                }
                else
                {
                    logger.Warn($"Steam library {libFolder} not found.");
                }                
            }

            return games;
        }

        public List<string> GetLibraryFolders()
        {
            var dbs = new List<string>() { SteamSettings.InstallationPath };
            var configPath = Path.Combine(SteamSettings.InstallationPath, "steamapps", "libraryfolders.vdf");
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

        public List<Game> GetLibraryGames(SteamSettings settings)
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

            if (settings.PrivateAccount)
            {
                return GetLibraryGames(userName, settings.APIKey);
            }
            else
            {
                return GetLibraryGames(userName);
            }
        }

        public List<Game> GetLibraryGames(string userName, string apiKey)
        {
            var userNameUrl = @"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}";
            var libraryUrl = @"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&include_appinfo=1&format=json&steamid={1}";

            ulong userId = 0;
            if (!ulong.TryParse(userName, out userId))
            {
                var stringData = Web.DownloadString(string.Format(userNameUrl, apiKey, userName));
                userId = ulong.Parse(JsonConvert.DeserializeObject<ResolveVanityResult>(stringData).response.steamid);
            }

            var stringLibrary = Web.DownloadString(string.Format(libraryUrl, apiKey, userId));
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

                games.Add(new Game()
                {
                    Provider = Provider.Steam,
                    Source = "Steam",
                    ProviderId = game.appid.ToString(),
                    Name = game.name,
                    Playtime = game.playtime_forever * 60,
                    CompletionStatus = game.playtime_forever > 0 ? CompletionStatus.Played : CompletionStatus.NotPlayed
                });
            }

            return games;
        }

        public List<Game> GetLibraryGames(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("Steam user name cannot be empty.");
            }

            var games = new List<Game>();
            var importedGames = (new ServicesClient()).GetSteamLibrary(userName);
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

                games.Add(new Game()
                {
                    Provider = Provider.Steam,
                    Source = "Steam",
                    ProviderId = game.appid.ToString(),
                    Name = game.name,
                    Playtime = game.playtime_forever * 60,
                    CompletionStatus = game.playtime_forever > 0 ? CompletionStatus.Played : CompletionStatus.NotPlayed
                });
            }

            return games;
        }

        public StoreAppDetailsResult.AppDetails GetStoreData(int appId)
        {
            var stringData = string.Empty;

            // First try to get cached data
            try
            {
                stringData = playniteServices?.GetSteamStoreData(appId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get Steam store cache data.");
            }

            // If no cache then download on client and push to cache
            if (string.IsNullOrEmpty(stringData))
            {
                // Steam may return 429 if we put too many request
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        stringData = WebApiClient.GetRawStoreAppDetail(appId);
                        logger.Debug($"Steam store data got from live server {appId}");

                        try
                        {
                            playniteServices?.PostSteamStoreData(appId, stringData);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, $"Failed to post steam store data to cache {appId}");
                        }

                        break;
                    }
                    catch (WebException e)
                    {
                        if (i + 1 == 10)
                        {
                            throw;
                        }

                        if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(2500);
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            else
            {
                logger.Debug($"Steam store data got from cache {appId}");
            }

            if (!string.IsNullOrEmpty(stringData))
            {
                var response = WebApiClient.ParseStoreData(appId, stringData);
                if (response.success != true)
                {
                    return null;
                }

                return response.data;
            }

            return null;
        }

        public KeyValue GetAppInfo(int appId)
        {
            KeyValue data = null;
            var stringData = string.Empty;

            // First try to get cached data
            try
            {
                stringData = playniteServices?.GetSteamAppInfoData(appId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get Steam appinfo cache data.");
            }

            // If no cache then download on client and push to cache
            if (string.IsNullOrEmpty(stringData))
            {
                data = SteamApiClient.GetProductInfo(appId).GetAwaiter().GetResult();                
                logger.Debug($"Steam appinfo got from live server {appId}");

                try
                {
                    if (playniteServices != null)
                    {
                        using (var str = new MemoryStream())
                        {
                            data.SaveToStream(str, false);
                            using (var reader = new StreamReader(str, Encoding.UTF8))
                            {
                                str.Seek(0, SeekOrigin.Begin);
                                stringData = reader.ReadToEnd();
                            }
                        }

                        playniteServices.PostSteamAppInfoData(appId, stringData);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to post steam appinfo data to cache {appId}");
                }
            }
            else
            {
                logger.Debug($"Steam appinfo data got from cache {appId}");
            }

            if (data != null)
            {
                return data;
            }
            else if (!string.IsNullOrEmpty(stringData))
            {
                return KeyValue.LoadFromString(stringData);
            }

            return null;
        }

        public SteamGameMetadata DownloadGameMetadata(int id, bool screenAsBackground)
        {
            var metadata = new SteamGameMetadata();
            var productInfo = GetAppInfo(id);
            metadata.ProductDetails = productInfo;
            metadata.StoreDetails = GetStoreData(id);

            // Icon
            var iconRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.ico";
            var icon = productInfo["common"]["clienticon"];
            var iconUrl = string.Empty;
            if (icon.Name != null)
            {
                iconUrl = string.Format(iconRoot, id, icon.Value);
            }
            else
            {
                var newIcon = productInfo["common"]["icon"];
                if (newIcon.Name != null)
                {
                    iconRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
                    iconUrl = string.Format(iconRoot, id, newIcon.Value);
                }
            }

            // There might be no icon assigned to game
            if (!string.IsNullOrEmpty(iconUrl))
            {
                var iconName = Path.GetFileName(new Uri(iconUrl).AbsolutePath);
                var iconData = Web.DownloadData(iconUrl);
                metadata.Icon = new FileDefinition(

                    string.Format("images/steam/{0}/{1}", id.ToString(), iconName),
                    iconName,
                    iconData
                );
            }


            // Image
            var imageRoot = @"http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg";
            var imageUrl = string.Format(imageRoot, id);
            byte[] imageData = null;

            try
            {
                imageData = Web.DownloadData(imageUrl);
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    imageRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
                    var image = productInfo["common"]["logo"];
                    if (image.Name != null)
                    {
                        imageUrl = string.Format(imageRoot, id, image.Value);
                        imageData = Web.DownloadData(imageUrl);
                    }
                }
                else
                {
                    throw;
                }
            }

            if (imageData != null)
            {
                var imageName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                metadata.Image = new FileDefinition(
                    string.Format("images/steam/{0}/{1}", id.ToString(), imageName),
                    imageName,
                    imageData
                );
            }

            // Background Image
            if (screenAsBackground)
            {
                if (metadata.StoreDetails?.screenshots?.Any() == true)
                {
                    metadata.BackgroundImage = Regex.Replace(metadata.StoreDetails.screenshots.First().path_full, "\\?.*$", "");
                }
            }
            else
            {
                metadata.BackgroundImage = string.Format(@"https://steamcdn-a.akamaihd.net/steam/apps/{0}/page_bg_generated_v6b.jpg", id);
            }

            return metadata;
        }

        public SteamGameMetadata UpdateGameWithMetadata(Game game, SteamSettings settings)
        {
            var metadata = DownloadGameMetadata(int.Parse(game.ProviderId), settings.PreferScreenshotForBackground);
            game.Name = metadata.ProductDetails["common"]["name"].Value ?? game.Name;
            game.Links = new ObservableCollection<Link>()
            {
                new Link("Forum", @"https://steamcommunity.com/app/" + game.ProviderId),
                new Link("News", @"http://store.steampowered.com/news/?appids=" + game.ProviderId),
                new Link("Store", @"http://store.steampowered.com/app/" + game.ProviderId),
                new Link("Wiki", @"http://pcgamingwiki.com/api/appid.php?appid=" + game.ProviderId)
            };

            if (metadata.StoreDetails?.categories?.FirstOrDefault(a => a.id == 30) != null)
            {
                game.Links.Add(new Link("Workshop", GetGameWorkshopUrl(int.Parse(game.ProviderId))));
            }

            if (metadata.StoreDetails != null)
            {
                game.Description = metadata.StoreDetails.detailed_description;
                game.Genres = new ComparableList<string>(metadata.StoreDetails.genres?.Select(a => a.description));
                game.Developers = new ComparableList<string>(metadata.StoreDetails.developers);
                game.Publishers = new ComparableList<string>(metadata.StoreDetails.publishers);
                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                game.Tags = new ComparableList<string>(metadata.StoreDetails.categories?.Select(a => cultInfo.ToTitleCase(a.description)));
                game.ReleaseDate = metadata.StoreDetails.release_date.date;
                game.CriticScore = metadata.StoreDetails.metacritic?.score;
            }

            var tasks = new ObservableCollection<GameTask>();
            var launchList = metadata.ProductDetails["config"]["launch"].Children;
            foreach (var task in launchList.Skip(1))
            {
                var properties = task["config"];
                if (properties.Name != null)
                {
                    if (properties["oslist"].Name != null)
                    {
                        if (properties["oslist"].Value != "windows")
                        {
                            continue;
                        }
                    }
                }

                // Ignore action without name  - shoudn't be visible to end user
                if (task["description"].Name != null)
                {
                    var newTask = new GameTask()
                    {
                        Name = task["description"].Value,
                        Arguments = task["arguments"].Value ?? string.Empty,
                        Path = task["executable"].Value,
                        IsBuiltIn = true,
                        WorkingDir = "{InstallDir}"
                    };

                    tasks.Add(newTask);
                }
            }

            var manual = metadata.ProductDetails["extended"]["gamemanualurl"];
            if (manual.Name != null)
            {
                tasks.Add((new GameTask()
                {
                    Name = "Manual",
                    Type = GameTaskType.URL,
                    Path = manual.Value,
                    IsBuiltIn = true
                }));
            }

            game.OtherTasks = tasks;

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            return metadata;
        }

        public List<Game> GetCategorizedGames(ulong steamId)
        {
            var id = new SteamID(steamId);
            var result = new List<Game>();
            var vdf = Path.Combine(SteamSettings.InstallationPath, "userdata", id.AccountID.ToString(), "7", "remote", "sharedconfig.vdf");
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
                    Provider = Provider.Steam,
                    Source = "Steam",
                    ProviderId = app.Name,
                    Categories = new ComparableList<string>(appData)
                });
            }

            return result;
        }

        public List<LocalSteamUser> GetSteamUsers()
        {
            var users = new List<LocalSteamUser>();
            if (File.Exists(SteamSettings.LoginUsersPath))
            {
                try
                {
                    var config = new KeyValue();
                    config.ReadFileAsText(SteamSettings.LoginUsersPath);
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
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to get list of Steam users.");                    
                }
            }

            return users;
        }

        public GameState GetAppState(int id)
        {
            var state = new GameState();
            var rootString = @"Software\Valve\Steam\Apps\" + id.ToString();
            var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            var appKey = root.OpenSubKey(rootString);
            if (appKey != null)
            {
                if (appKey.GetValue("Installed")?.ToString() == "1")
                {
                    state.Installed = true;
                }

                if (appKey.GetValue("Launching")?.ToString() == "1")
                {
                    state.Launching = true;
                }

                if (appKey.GetValue("Running")?.ToString() == "1")
                {
                    state.Running = true;
                }

                if (appKey.GetValue("Updating")?.ToString() == "1")
                {
                    state.Installing = true;
                }
            }

            return state;
        }
    }

    public class LocalSteamUser
    {
        public ulong Id
        {
            get; set;
        }

        public string AccountName
        {
            get; set;
        }

        public string PersonaName
        {
            get; set;
        }

        public bool Recent
        {
            get; set;
        }
    }

    public class SteamGameMetadata : GameMetadata
    {
        public KeyValue ProductDetails
        {
            get;set;
        }

        public StoreAppDetailsResult.AppDetails StoreDetails
        {
            get; set;
        }
    }
}
