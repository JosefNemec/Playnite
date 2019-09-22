using Newtonsoft.Json.Linq;
using OriginLibrary.Models;
using OriginLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class OriginMetadataProvider : LibraryMetadataProvider
    {
        private readonly IPlayniteAPI api;

        public OriginMetadataProvider(IPlayniteAPI api)
        {
            this.api = api;
        }

        #region IMetadataProvider

        public override GameMetadata GetMetadata(Game game)
        {
            var storeMetadata = DownloadGameMetadata(game.GameId);
            var gameInfo = new GameInfo
            {
                Name = StringExtensions.NormalizeGameName(storeMetadata.StoreDetails.i18n.displayName),
                Description = storeMetadata.StoreDetails.i18n.longDescription,
                ReleaseDate = storeMetadata.StoreDetails.platforms.First(a => a.platform == "PCWIN").releaseDate,
                Links = new List<Link>()
                {
                    new Link("Store Page", @"https://www.origin.com/store" + storeMetadata.StoreDetails.offerPath),
                    new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + game.Name)
                }
            };

            if (!storeMetadata.StoreDetails.publisherFacetKey.IsNullOrEmpty())
            {
                gameInfo.Publishers = new List<string>() { storeMetadata.StoreDetails.publisherFacetKey };
            }

            if (!storeMetadata.StoreDetails.developerFacetKey.IsNullOrEmpty())
            {
                gameInfo.Developers = new List<string>() { storeMetadata.StoreDetails.developerFacetKey };
            }

            if (!storeMetadata.StoreDetails.genreFacetKey.IsNullOrEmpty())
            {
                gameInfo.Genres = new List<string>(storeMetadata.StoreDetails.genreFacetKey?.Split(','));
            }

            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo,
                Icon = storeMetadata.Icon,
                CoverImage = storeMetadata.CoverImage,
                BackgroundImage = storeMetadata.BackgroundImage
            };

            if (!string.IsNullOrEmpty(storeMetadata.StoreDetails.i18n.gameForumURL))
            {
                gameInfo.Links.Add(new Link("Forum", storeMetadata.StoreDetails.i18n.gameForumURL));
            }

            if (!string.IsNullOrEmpty(storeMetadata.StoreDetails.i18n.gameManualURL))
            {
                game.OtherActions = new ObservableCollection<GameAction>()
                {
                    new GameAction()
                    {
                        IsHandledByPlugin = false,
                        Type = GameActionType.URL,
                        Path = storeMetadata.StoreDetails.i18n.gameManualURL,
                        Name = "Manual"
                    }
                };
            }

            // There's not icon available on Origin servers so we will load one from EXE
            if (game.IsInstalled && string.IsNullOrEmpty(game.Icon))
            {
                var playAction = api.ExpandGameVariables(game, game.PlayAction);
                var executable = string.Empty;
                if (File.Exists(playAction.Path))
                {
                    executable = playAction.Path;
                }
                else if (!string.IsNullOrEmpty(playAction.WorkingDir))
                {
                    executable = Path.Combine(playAction.WorkingDir, playAction.Path);
                }

                if (string.IsNullOrEmpty(executable))
                {
                    return storeMetadata;
                }

                var exeIcon = IconExtension.ExtractIconFromExe(executable, true);
                if (exeIcon != null)
                {
                    var iconName = Guid.NewGuid() + ".png";
                    metadata.Icon = new MetadataFile(iconName, exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));
                }
            }

            return metadata;
        }

        #endregion IMetadataProvider

        public OriginGameMetadata DownloadGameMetadata(string id)
        {
            var data = new OriginGameMetadata()
            {
                StoreDetails = OriginApiClient.GetGameStoreData(id)
            };

            data.CoverImage = new MetadataFile(data.StoreDetails.imageServer + data.StoreDetails.i18n.packArtLarge);
            if (!string.IsNullOrEmpty(data.StoreDetails.offerPath))
            {
                data.StoreMetadata = OriginApiClient.GetStoreMetadata(data.StoreDetails.offerPath);
                var bkData = data.StoreMetadata?.gamehub.components.items?.FirstOrDefault(a => a.ContainsKey("origin-store-pdp-hero"));
                if (bkData != null)
                {
                    var bk = (bkData["origin-store-pdp-hero"] as JObject).ToObject<Dictionary<string, object>>();
                    if (bk.TryGetValue("background-image", out var backgroundUrl))
                    {
                        data.BackgroundImage = new MetadataFile(backgroundUrl.ToString());
                    }
                }
            }

            return data;
        }
    }
}
