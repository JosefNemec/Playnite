using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using GogLibrary.Models;
using System.IO;
using Playnite;
using System.Collections.ObjectModel;
using System.Globalization;
using GogLibrary.Services;

namespace GogLibrary
{
    public class GogMetadataProvider : LibraryMetadataProvider
    {
        private GogApiClient apiClient = new GogApiClient();
        private ILogger logger = LogManager.GetLogger();
        private GogLibrary library;

        public GogMetadataProvider(GogLibrary library)
        {
            this.library = library;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var resources = library.PlayniteApi.Resources;
            var storeData = DownloadGameMetadata(game);
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

            if (!string.IsNullOrEmpty(storeData.GameDetails.links.forum))
            {
                gameInfo.Links.Add(new Link(resources.GetString("LOCCommonLinksForum"), storeData.GameDetails.links.forum));
            };

            if (!string.IsNullOrEmpty(storeData.GameDetails.links.product_card))
            {
                gameInfo.Links.Add(new Link(resources.GetString("LOCCommonLinksStorePage"), storeData.GameDetails.links.product_card));
            };

            gameInfo.Links.Add(new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + storeData.GameDetails.title));

            if (storeData.StoreDetails != null)
            {
                gameInfo.Genres = storeData.StoreDetails.genres?.Select(a => a.name).ToList();
                gameInfo.Features = storeData.StoreDetails.features?.Select(a => a.name).ToList();
                gameInfo.Developers = storeData.StoreDetails.developers.Select(a => a.name).ToList();
                gameInfo.Publishers = new List<string>() { storeData.StoreDetails.publisher };
                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                if (gameInfo.ReleaseDate == null && storeData.StoreDetails.globalReleaseDate != null)
                {
                    gameInfo.ReleaseDate = storeData.StoreDetails.globalReleaseDate;
                }

                if (gameInfo.Features?.Contains("Overlay") == true)
                {
                    gameInfo.Features.Remove("Overlay");
                }
            }

            return metadata;
        }

        internal GogGameMetadata DownloadGameMetadata(Game game)
        {
            var metadata = new GogGameMetadata();
            var gameDetail = apiClient.GetGameDetails(game.GameId);
            if (gameDetail == null)
            {
                logger.Warn($"Product page for game {game.GameId} not found, using fallback search.");
                var search = apiClient.GetStoreSearch(game.Name);
                var match = search?.FirstOrDefault(a => a.title.Equals(game.Name, StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                {
                    gameDetail = apiClient.GetGameDetails(match.id.ToString());
                }
            }

            metadata.GameDetails = gameDetail;

            if (gameDetail != null)
            {
                if (gameDetail.links.product_card != @"https://www.gog.com/" && !string.IsNullOrEmpty(gameDetail.links.product_card))
                {
                    metadata.StoreDetails = apiClient.GetGameStoreData(gameDetail.links.product_card);
                }

                metadata.Icon = new MetadataFile("http:" + gameDetail.images.icon);
                if (metadata.StoreDetails != null)
                {
                    var imageUrl = metadata.StoreDetails.image + "_product_card_v2_mobile_slider_639.jpg";
                    metadata.CoverImage = new MetadataFile(imageUrl);
                }
                else
                {
                    metadata.CoverImage = new MetadataFile("http:" + gameDetail.images.logo2x);
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
