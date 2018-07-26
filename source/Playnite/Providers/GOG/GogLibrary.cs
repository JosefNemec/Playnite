//using Newtonsoft.Json;
//using NLog;
//using Playnite.Metadata;
//using Playnite.SDK.Models;
//using Playnite.Web;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Windows;

//namespace Playnite.Providers.GOG
//{

//        

//        public GogGameMetadata DownloadGameMetadata(string id, string storeUrl = null)
//        {
//            var metadata = new GogGameMetadata();
//            var gameDetail = WebApiClient.GetGameDetails(id);
//            metadata.GameDetails = gameDetail;

//            if (gameDetail != null)
//            {
//                if (gameDetail.links.product_card != @"https://www.gog.com/" && !string.IsNullOrEmpty(gameDetail.links.product_card))
//                {
//                    metadata.StoreDetails = WebApiClient.GetGameStoreData(gameDetail.links.product_card);
//                }
//                else if (!string.IsNullOrEmpty(storeUrl))
//                {
//                    metadata.StoreDetails = WebApiClient.GetGameStoreData(storeUrl);
//                }

//                var icon = HttpDownloader.DownloadData("http:" + gameDetail.images.icon);
//                var iconName = Path.GetFileName(new Uri(gameDetail.images.icon).AbsolutePath);
//                var image = HttpDownloader.DownloadData("http:" + gameDetail.images.logo2x);
//                var imageName = Path.GetFileName(new Uri(gameDetail.images.logo2x).AbsolutePath);

//                metadata.Icon = new MetadataFile(
//                    string.Format("images/gog/{0}/{1}", id, iconName),
//                    iconName,
//                    icon
//                );

//                metadata.Image = new MetadataFile(
//                    string.Format("images/gog/{0}/{1}", id, imageName),
//                    imageName,
//                    image
//                );

//                metadata.BackgroundImage = "http:" + gameDetail.images.background;
//            }

//            return metadata;
//        }

//        public GogGameMetadata UpdateGameWithMetadata(Game game)
//        {
//            var currentUrl = string.Empty;
//            var metadata = DownloadGameMetadata(game.GameId, currentUrl);
//            if(metadata.GameDetails == null)
//            {
//                logger.Warn($"Could not gather metadata for game {game.GameId}");
//                return metadata;
//            }

//            game.Name = StringExtensions.NormalizeGameName(metadata.GameDetails.title);
//            game.Description = metadata.GameDetails.description.full;
//            game.Links = new ObservableCollection<Link>()
//            {
//                new Link("Wiki", @"http://pcgamingwiki.com/w/index.php?search=" + metadata.GameDetails.title)
//            };
            
//            if (!string.IsNullOrEmpty(metadata.GameDetails.links.forum))
//            {
//                game.Links.Add(new Link("Forum", metadata.GameDetails.links.forum));
//            };

//            if (string.IsNullOrEmpty(currentUrl) && !string.IsNullOrEmpty(metadata.GameDetails.links.product_card))
//            {
//                game.Links.Add(new Link("Store", metadata.GameDetails.links.product_card));
//            };

//            if (metadata.StoreDetails != null)
//            {
//                game.Genres = new ComparableList<string>(metadata.StoreDetails.genres.Select(a => a.name));
//                game.Developers = new ComparableList<string>() { metadata.StoreDetails.developer.name };
//                game.Publishers = new ComparableList<string>() { metadata.StoreDetails.publisher.name };
//                game.CommunityScore = metadata.StoreDetails.rating != 0 ? metadata.StoreDetails.rating * 2 : (int?)null;

//                var cultInfo = new CultureInfo("en-US", false).TextInfo;
//                game.Tags = new ComparableList<string>(metadata.StoreDetails.features?.Select(a => cultInfo.ToTitleCase(a.title)));

//                if (game.ReleaseDate == null && metadata.StoreDetails.releaseDate != null)
//                {
//                    game.ReleaseDate = DateTimeOffset.FromUnixTimeSeconds(metadata.StoreDetails.releaseDate.Value).DateTime;
//                }
//            }

//            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
//            {
//                game.BackgroundImage = metadata.BackgroundImage;
//            }

//            return metadata;
//        }
//    }

//    public class GogGameMetadata : GameMetadata
//    {
//        public ProductApiDetail GameDetails
//        {
//            get;set;
//        }

//        public StorePageResult.ProductDetails StoreDetails
//        {
//            get;set;
//        }
//    }
//}
