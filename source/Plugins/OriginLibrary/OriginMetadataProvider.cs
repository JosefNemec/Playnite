using Newtonsoft.Json.Linq;
using OriginLibrary.Models;
using OriginLibrary.Services;
using Playnite.Common.Media.Icons;
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
        private readonly OriginLibrary library;

        public OriginMetadataProvider(OriginLibrary library)
        {
            this.library = library;
        }

        #region IMetadataProvider

        public override GameMetadata GetMetadata(Game game)
        {
            var resources = library.PlayniteApi.Resources;
            var storeMetadata = DownloadGameMetadata(game.GameId);
            var gameInfo = new GameInfo
            {
                Name = StringExtensions.NormalizeGameName(storeMetadata.StoreDetails.i18n.displayName),
                Description = storeMetadata.StoreDetails.i18n.longDescription,
                ReleaseDate = storeMetadata.StoreDetails.platforms.First(a => a.platform == "PCWIN").releaseDate,
                Links = new List<Link>()
                {
                    new Link(resources.GetString("LOCCommonLinksStorePage"), @"https://www.origin.com/store" + storeMetadata.StoreDetails.offerPath),
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
                gameInfo.Links.Add(new Link(resources.GetString("LOCCommonLinksForum"), storeMetadata.StoreDetails.i18n.gameForumURL));
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
                var playAction = library.PlayniteApi.ExpandGameVariables(game, game.PlayAction);
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

                using (var ms = new MemoryStream())
                {
                    if (IconExtractor.ExtractMainIconFromFile(executable, ms))
                    {
                        var iconName = Guid.NewGuid() + ".ico";
                        metadata.Icon = new MetadataFile(iconName, ms.ToArray());
                    }
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
