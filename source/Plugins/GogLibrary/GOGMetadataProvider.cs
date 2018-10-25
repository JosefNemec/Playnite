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

        public GogMetadataProvider()
        {
        }

        #region IMetadataProvider

        public GameMetadata GetMetadata(Game game)
        {
            var gameData = new Game("GOGGame")
            {
                GameId = game.GameId
            };

            var data = UpdateGameWithMetadata(gameData);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        #endregion IMetadataProvider

        internal GogGameMetadata DownloadGameMetadata(string id, string storeUrl = null)
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
                else if (!string.IsNullOrEmpty(storeUrl))
                {
                    metadata.StoreDetails = apiClient.GetGameStoreData(storeUrl);
                }

                var icon = HttpDownloader.DownloadData("http:" + gameDetail.images.icon);
                var iconName = Path.GetFileName(new Uri(gameDetail.images.icon).AbsolutePath);
                var image = HttpDownloader.DownloadData("http:" + gameDetail.images.logo2x);
                var imageName = Path.GetFileName(new Uri(gameDetail.images.logo2x).AbsolutePath);
                metadata.Icon = new MetadataFile(iconName, icon);
                metadata.Image = new MetadataFile(imageName, image);
                metadata.BackgroundImage = "http:" + gameDetail.images.background;
            }

            return metadata;
        }

        internal GogGameMetadata UpdateGameWithMetadata(Game game)
        {
            var currentUrl = string.Empty;
            var metadata = DownloadGameMetadata(game.GameId, currentUrl);
            if (metadata.GameDetails == null)
            {
                logger.Warn($"Could not gather metadata for game {game.GameId}");
                return metadata;
            }

            game.Name = StringExtensions.NormalizeGameName(metadata.GameDetails.title);
            game.Description = metadata.GameDetails.description.full;
            game.Links = new ObservableCollection<Link>()
            {
                new Link("Wiki", @"http://pcgamingwiki.com/w/index.php?search=" + metadata.GameDetails.title)
            };

            if (!string.IsNullOrEmpty(metadata.GameDetails.links.forum))
            {
                game.Links.Add(new Link("Forum", metadata.GameDetails.links.forum));
            };

            if (string.IsNullOrEmpty(currentUrl) && !string.IsNullOrEmpty(metadata.GameDetails.links.product_card))
            {
                game.Links.Add(new Link("Store", metadata.GameDetails.links.product_card));
            };

            if (metadata.StoreDetails != null)
            {
                game.Genres = new ComparableList<string>(metadata.StoreDetails.genres?.Select(a => a.name));
                game.Tags = new ComparableList<string>(metadata.StoreDetails.tags?.Select(a => a.name));
                game.Developers = new ComparableList<string>(metadata.StoreDetails.developers.Select(a => a.name));
                game.Publishers = new ComparableList<string>() { metadata.StoreDetails.publisher };

                var cultInfo = new CultureInfo("en-US", false).TextInfo;

                if (game.ReleaseDate == null && metadata.StoreDetails.globalReleaseDate != null)
                {
                    game.ReleaseDate = metadata.StoreDetails.globalReleaseDate;
                }
            }

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            return metadata;
        }
    }
}
