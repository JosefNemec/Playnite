using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using SteamLibrary.Models;
using SteamLibrary.Services;
using Playnite.Web;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SteamLibrary
{
    public class SteamMetadataProvider : IMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger();
        private SteamServicesClient playniteServices;
        private SteamLibrarySettings settings;
        private SteamApiClient apiClient;

        public SteamMetadataProvider(SteamServicesClient playniteServices, SteamLibrarySettings settings, SteamApiClient apiClient)
        {
            this.settings = settings;
            this.playniteServices = playniteServices;
            this.apiClient = apiClient;
        }

        #region IMetadataProvider

        public GameMetadata GetMetadata(string metadataId)
        {
            var gameData = new Game("SteamGame")
            {                
                GameId = metadataId
            };

            var data = UpdateGameWithMetadata(gameData);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        public GameMetadata GetMetadata(Game game)
        {
            return GetMetadata(game.GameId);
        }

        public ICollection<MetadataSearchResult> SearchMetadata(Game game)
        {
            throw new NotImplementedException();
        }

        #endregion IMetadataProvider

        internal KeyValue GetAppInfo(int appId)
        {
            KeyValue data = null;
            var stringData = string.Empty;

            // First try to get cached data
            try
            {
                stringData = playniteServices.GetSteamAppInfoData(appId);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get Steam appinfo cache data {appId}.");
            }

            // If no cache then download on client and push to cache
            if (string.IsNullOrEmpty(stringData))
            {
                data = apiClient.GetProductInfo(appId).GetAwaiter().GetResult();
                logger.Debug($"Steam appinfo got from live server {appId}");

                try
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

        internal StoreAppDetailsResult.AppDetails GetStoreData(int appId)
        {
            var stringData = string.Empty;

            // First try to get cached data
            try
            {
                stringData = playniteServices.GetSteamStoreData(appId);
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
                            playniteServices.PostSteamStoreData(appId, stringData);
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

        internal SteamGameMetadata DownloadGameMetadata(int id, bool screenAsBackground)
        {
            var metadata = new SteamGameMetadata();
            var productInfo = GetAppInfo(id);
            metadata.ProductDetails = productInfo;
            metadata.StoreDetails = GetStoreData(id);

            // Icon
            if (productInfo != null)
            {
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
                    var iconData = HttpDownloader.DownloadData(iconUrl);
                    metadata.Icon = new MetadataFile(

                        string.Format("images/steam/{0}/{1}", id.ToString(), iconName),
                        iconName,
                        iconData
                    );
                }
            }

            // Image
            var imageRoot = @"http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg";
            var imageUrl = string.Format(imageRoot, id);
            byte[] imageData = null;

            try
            {
                imageData = HttpDownloader.DownloadData(imageUrl);
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    if (productInfo != null)
                    {
                        imageRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
                        var image = productInfo["common"]["logo"];
                        if (image.Name != null)
                        {
                            imageUrl = string.Format(imageRoot, id, image.Value);
                            imageData = HttpDownloader.DownloadData(imageUrl);
                        }
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
                metadata.Image = new MetadataFile(
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

        internal SteamGameMetadata UpdateGameWithMetadata(Game game)
        {
            var metadata = DownloadGameMetadata(int.Parse(game.GameId), settings.PreferScreenshotForBackground);
            game.Name = metadata.ProductDetails?["common"]["name"]?.Value ?? game.Name;
            game.Links = new ObservableCollection<Link>()
            {
                new Link("Forum", @"https://steamcommunity.com/app/" + game.GameId),
                new Link("News", @"http://store.steampowered.com/news/?appids=" + game.GameId),
                new Link("Store", @"http://store.steampowered.com/app/" + game.GameId),
                new Link("Wiki", @"http://pcgamingwiki.com/api/appid.php?appid=" + game.GameId)
            };

            if (metadata.StoreDetails?.categories?.FirstOrDefault(a => a.id == 30) != null)
            {
                game.Links.Add(new Link("Workshop", Steam.GetWorkshopUrl(int.Parse(game.GameId))));
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

            if (metadata.ProductDetails != null)
            {
                var tasks = new ObservableCollection<GameAction>();
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
                        var newTask = new GameAction()
                        {
                            Name = task["description"].Value,
                            Arguments = task["arguments"].Value ?? string.Empty,
                            Path = task["executable"].Value,
                            IsHandledByPlugin = false,
                            WorkingDir = "{InstallDir}"
                        };

                        tasks.Add(newTask);
                    }
                }

                var manual = metadata.ProductDetails["extended"]["gamemanualurl"];
                if (manual.Name != null)
                {
                    tasks.Add((new GameAction()
                    {
                        Name = "Manual",
                        Type = GameActionType.URL,
                        Path = manual.Value,
                        IsHandledByPlugin = false
                    }));
                }

                game.OtherActions = tasks;
            }

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            return metadata;
        }
    }
}
