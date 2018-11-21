using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using GogLibrary.Models;
using Playnite.Web;
using System.IO;
using Playnite;
using System.Collections.ObjectModel;
using System.Globalization;
using GogLibrary.Services;

namespace GogLibrary
{
    public class GogMetadataProvider : ILibraryMetadataProvider
    {
        private GogApiClient apiClient = new GogApiClient();
        private ILogger logger = LogManager.GetLogger();

        public GameMetadata GetMetadata(Game game)
        {
            var storeData = DownloadGameMetadata(game.GameId);
            if (storeData.GameDetails == null)
            {
                logger.Warn($"Could not gather metadata for game {game.GameId}");
                return null;
            }

            var gameInfo = new GameInfo
            {
                Name = StringExtensions.NormalizeGameName(storeData.GameDetails.title),
                Description = storeData.GameDetails.description.full,
                Links = new List<Link>()
            };

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo,
                Icon = storeData.Icon,
                CoverImage = storeData.CoverImage,
                BackgroundImage = storeData.BackgroundImage
            };

            gameInfo.Links.Add(new Link("Wiki", @"http://pcgamingwiki.com/w/index.php?search=" + storeData.GameDetails.title));
            if (!string.IsNullOrEmpty(storeData.GameDetails.links.forum))
            {
                gameInfo.Links.Add(new Link("Forum", storeData.GameDetails.links.forum));
            };

            if (!string.IsNullOrEmpty(storeData.GameDetails.links.product_card))
            {
                gameInfo.Links.Add(new Link("Store", storeData.GameDetails.links.product_card));
            };

            if (storeData.StoreDetails != null)
            {
                gameInfo.Genres = storeData.StoreDetails.genres?.Select(a => a.name).ToList();
                gameInfo.Tags = storeData.StoreDetails.tags?.Select(a => a.name).ToList();
                gameInfo.Developers = storeData.StoreDetails.developers.Select(a => a.name).ToList();
                gameInfo.Publishers = new List<string>() { storeData.StoreDetails.publisher };
                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                if (gameInfo.ReleaseDate == null && storeData.StoreDetails.globalReleaseDate != null)
                {
                    gameInfo.ReleaseDate = storeData.StoreDetails.globalReleaseDate;
                }
            }

            return metadata;
        }

        internal GogGameMetadata DownloadGameMetadata(string id)
        {
            var metadata = new GogGameMetadata();
            var gameDetail = apiClient.GetGameDetails(id);
            metadata.GameDetails = gameDetail;

            if (gameDetail != null)
            {
                if (gameDetail.links.product_card != @"https://www.gog.com/" && !string.IsNullOrEmpty(gameDetail.links.product_card))
                {
                    metadata.StoreDetails = apiClient.GetGameStoreData(gameDetail.links.product_card);
                }

                var icon = HttpDownloader.DownloadData("http:" + gameDetail.images.icon);
                var iconName = Path.GetFileName(new Uri(gameDetail.images.icon).AbsolutePath);
                metadata.Icon = new MetadataFile(iconName, icon);

                if (metadata.StoreDetails != null)
                {
                    var imageUrl = metadata.StoreDetails.image + "_product_card_v2_mobile_slider_639.jpg";
                    var image = HttpDownloader.DownloadData(imageUrl);
                    var imageName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                    metadata.CoverImage = new MetadataFile(imageName, image);
                }
                else
                {
                    var image = HttpDownloader.DownloadData("http:" + gameDetail.images.logo2x);
                    var imageName = Path.GetFileName(new Uri(gameDetail.images.logo2x).AbsolutePath);
                    metadata.CoverImage = new MetadataFile(imageName, image);
                }

                if (metadata.StoreDetails != null)
                {
                    var url = metadata.StoreDetails.galaxyBackgroundImage ?? metadata.StoreDetails.backgroundImage;
                    metadata.BackgroundImage = new MetadataFile(url.Replace(".jpg", "_bg_crop_1920x655.jpg"));
                }
                else
                {
                    metadata.BackgroundImage = new MetadataFile("http:" + gameDetail.images.background);
                }
            }

            return metadata;
        }
    }
}
