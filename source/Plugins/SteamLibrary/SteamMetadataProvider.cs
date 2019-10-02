using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using SteamLibrary.Models;
using SteamLibrary.Services;
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
using Playnite.Common.Web;

namespace SteamLibrary
{
    public class SteamMetadataProvider : LibraryMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger();
        private SteamServicesClient playniteServices;
        private SteamLibrary library;
        private SteamApiClient apiClient;

        private readonly string[] backgroundUrls = new string[]
        {
            @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/page.bg.jpg",
            @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/page_bg_generated.jpg"
        };

        public SteamMetadataProvider(SteamServicesClient playniteServices, SteamLibrary library, SteamApiClient apiClient)
        {
            this.library = library;
            this.playniteServices = playniteServices;
            this.apiClient = apiClient;
        }

        #region IMetadataProvider

        public override GameMetadata GetMetadata(Game game)
        {
            var gameData = new Game("SteamGame")
            {
                GameId = game.GameId
            };

            var gameId = game.ToSteamGameID();
            if (gameId.IsMod)
            {
                var data = library.GetInstalledModFromFolder(game.InstallDirectory, ModInfo.GetModTypeOfGameID(gameId));
                return new GameMetadata(data, null, null, null);
            }
            else
            {
                return GetGameMetadata(gameId);
            }
        }

        #endregion IMetadataProvider

        internal KeyValue GetAppInfo(uint appId)
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

        internal StoreAppDetailsResult.AppDetails GetStoreData(uint appId)
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
                            logger.Error($"Reached download timeout for Steam store game {appId}");
                            return null;
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

        internal SteamGameMetadata DownloadGameMetadata(uint appId, BackgroundSource backgroundSource)
        {
            var metadata = new SteamGameMetadata();
            var productInfo = GetAppInfo(appId);
            metadata.ProductDetails = productInfo;

            try
            {
                metadata.StoreDetails = GetStoreData(appId);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to download Steam store metadata {appId}");
            }

            // Icon
            if (productInfo != null)
            {
                var iconRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.ico";
                var icon = productInfo["common"]["clienticon"];
                var iconUrl = string.Empty;
                if (!string.IsNullOrEmpty(icon.Value))
                {
                    iconUrl = string.Format(iconRoot, appId, icon.Value);
                }
                else
                {
                    var newIcon = productInfo["common"]["icon"];
                    if (!string.IsNullOrEmpty(newIcon.Value))
                    {
                        iconRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
                        iconUrl = string.Format(iconRoot, appId, newIcon.Value);
                    }
                }

                // There might be no icon assigned to game
                if (!string.IsNullOrEmpty(iconUrl))
                {
                    metadata.Icon = new MetadataFile(iconUrl);
                }
            }

            // Image
            var imageRoot = @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg";
            var imageUrl = string.Format(imageRoot, appId);
            if (HttpDownloader.GetResponseCode(imageUrl) == HttpStatusCode.OK)
            {
                metadata.CoverImage = new MetadataFile(imageUrl);
            }
            else
            {
                if (productInfo != null)
                {
                    imageRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
                    var image = productInfo["common"]["logo"];
                    if (!string.IsNullOrEmpty(image.Value))
                    {
                        imageUrl = string.Format(imageRoot, appId, image.Value);
                        metadata.CoverImage = new MetadataFile(imageUrl);
                    }
                }
            }

            // Background Image
            switch (backgroundSource)
            {
                case BackgroundSource.Image:
                    metadata.BackgroundImage = new MetadataFile(GetGameBackground(appId));
                    break;
                case BackgroundSource.StoreScreenshot:
                    if (metadata.StoreDetails != null)
                    {
                        metadata.BackgroundImage = new MetadataFile(Regex.Replace(metadata.StoreDetails.screenshots.First().path_full, "\\?.*$", ""));
                    }
                    break;
                case BackgroundSource.StoreBackground:
                    metadata.BackgroundImage = new MetadataFile(string.Format(@"https://steamcdn-a.akamaihd.net/steam/apps/{0}/page_bg_generated_v6b.jpg", appId));
                    break;
                default:
                    break;
            }

            return metadata;
        }

        internal string ParseDescription(string description)
        {
            return description.Replace("%CDN_HOST_MEDIA_SSL%", "steamcdn-a.akamaihd.net");
        }

        internal GameMetadata GetGameMetadata(GameID gameId)
        {
            var appId = gameId.AppID;
            var downloadedMetadata = DownloadGameMetadata(appId, library.LibrarySettings.BackgroundSource);
            var gameInfo = new GameInfo
            {
                Name = downloadedMetadata.ProductDetails?["common"]["name"]?.Value ?? downloadedMetadata.GameInfo.Name,
                Links = new List<Link>()
                {
                    new Link("{DynamicResource LOCSteamLinksCommunityHub}", $"https://steamcommunity.com/app/{appId}"),
                    new Link("{DynamicResource LOCSteamLinksDiscussions}", $"https://steamcommunity.com/app/{appId}/discussions/"),
                    new Link("{DynamicResource LOCSteamLinksNews}", $"https://store.steampowered.com/news/?appids={appId}"),
                    new Link("{DynamicResource LOCSteamLinksStorePage}", $"https://store.steampowered.com/app/{appId}"),
                    new Link("{DynamicResource LOCSteamLinksPCGW}", $"https://pcgamingwiki.com/api/appid.php?appid={appId}")
                }
            };

            downloadedMetadata.GameInfo = gameInfo;

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo,
                Icon = downloadedMetadata.Icon,
                CoverImage = downloadedMetadata.CoverImage,
                BackgroundImage = downloadedMetadata.BackgroundImage
            };

            if (downloadedMetadata.StoreDetails?.categories?.FirstOrDefault(a => a.id == 22) != null)
            {
                gameInfo.Links.Add(new Link("{LOCSteamLinksAchievements}", Steam.GetAchievementsUrl(appId)));
            }

            if (downloadedMetadata.StoreDetails?.categories?.FirstOrDefault(a => a.id == 30) != null)
            {
                gameInfo.Links.Add(new Link("{DynamicResource LOCSteamLinksWorkshop}", Steam.GetWorkshopUrl(appId)));
            }

            if (downloadedMetadata.StoreDetails != null)
            {
                gameInfo.Description = ParseDescription(downloadedMetadata.StoreDetails.detailed_description);
                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                gameInfo.ReleaseDate = downloadedMetadata.StoreDetails.release_date.date;
                gameInfo.CriticScore = downloadedMetadata.StoreDetails.metacritic?.score;

                if (downloadedMetadata.StoreDetails.publishers.HasNonEmptyItems())
                {
                    gameInfo.Publishers = new List<string>(downloadedMetadata.StoreDetails.publishers);
                }

                if (downloadedMetadata.StoreDetails.developers.HasNonEmptyItems())
                {
                    gameInfo.Developers = new List<string>(downloadedMetadata.StoreDetails.developers);
                }

                if (downloadedMetadata.StoreDetails.categories.HasItems())
                {
                    gameInfo.Tags = new List<string>(downloadedMetadata.StoreDetails.categories.Select(a => cultInfo.ToTitleCase(a.description)));
                }

                if (downloadedMetadata.StoreDetails.genres.HasItems())
                {
                    gameInfo.Genres = new List<string>(downloadedMetadata.StoreDetails.genres.Select(a => a.description));
                }
            }

            if (downloadedMetadata.ProductDetails != null)
            {
                var tasks = new List<GameAction>();
                var launchList = downloadedMetadata.ProductDetails["config"]["launch"].Children;
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
                            WorkingDir = ExpandableVariables.InstallationDirectory
                        };

                        tasks.Add(newTask);
                    }
                }

                var manual = downloadedMetadata.ProductDetails["extended"]["gamemanualurl"];
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

                gameInfo.OtherActions = tasks;
            }

            return downloadedMetadata;
        }

        private string GetGameBackground(uint appId)
        {
            foreach (var url in backgroundUrls)
            {
                var bk = string.Format(url, appId);
                if (HttpDownloader.GetResponseCode(bk) == HttpStatusCode.OK)
                {
                    return bk;
                }
            }

            return null;
        }
    }
}
