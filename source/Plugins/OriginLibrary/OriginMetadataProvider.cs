using OriginLibrary.Models;
using OriginLibrary.Services;
using Playnite;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class OriginMetadataProvider : ILibraryMetadataProvider
    {
        private readonly IPlayniteAPI api;

        public OriginMetadataProvider(IPlayniteAPI api)
        {
            this.api = api;
        }

        #region IMetadataProvider

        public GameMetadata GetMetadata(Game game)
        {
            var gameData = game.CloneJson();
            var data = UpdateGameWithMetadata(gameData);
            return new GameMetadata(gameData, data.Icon, data.Image, data.BackgroundImage);
        }

        #endregion IMetadataProvider

        public OriginGameMetadata DownloadGameMetadata(string id)
        {
            var data = new OriginGameMetadata()
            {
                StoreDetails = OriginApiClient.GetGameStoreData(id)
            };

            var imageUrl = data.StoreDetails.imageServer + data.StoreDetails.i18n.packArtLarge;
            var imageData = HttpDownloader.DownloadData(imageUrl);
            var imageName = Guid.NewGuid() + Path.GetExtension(new Uri(imageUrl).AbsolutePath);

            data.Image = new MetadataFile(
                string.Format("images/origin/{0}/{1}", id.Replace(":", ""), imageName),
                imageName,
                imageData
            );

            return data;
        }

        public OriginGameMetadata UpdateGameWithMetadata(Game game)
        {
            var metadata = DownloadGameMetadata(game.GameId);
            game.Name = StringExtensions.NormalizeGameName(metadata.StoreDetails.i18n.displayName);
            game.Links = new ObservableCollection<Link>()
            {
                new Link("Store", @"https://www.origin.com/store" + metadata.StoreDetails.offerPath),
                new Link("Wiki", @"http://pcgamingwiki.com/w/index.php?search=" + game.Name)
            };

            if (!string.IsNullOrEmpty(metadata.StoreDetails.i18n.gameForumURL))
            {
                game.Links.Add(new Link("Forum", metadata.StoreDetails.i18n.gameForumURL));
            }

            game.Description = metadata.StoreDetails.i18n.longDescription;
            game.Developers = new ComparableList<string>() { metadata.StoreDetails.developerFacetKey };
            game.Publishers = new ComparableList<string>() { metadata.StoreDetails.publisherFacetKey };
            game.Genres = new ComparableList<string>(metadata.StoreDetails.genreFacetKey?.Split(','));
            game.ReleaseDate = metadata.StoreDetails.platforms.First(a => a.platform == "PCWIN").releaseDate;

            if (!string.IsNullOrEmpty(metadata.StoreDetails.i18n.gameManualURL))
            {
                game.OtherActions = new ObservableCollection<GameAction>()
                {
                    new GameAction()
                    {
                        IsHandledByPlugin = false,
                        Type = GameActionType.URL,
                        Path = metadata.StoreDetails.i18n.gameManualURL,
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
                    return metadata;
                }

                var exeIcon = IconExtension.ExtractIconFromExe(executable, true);
                if (exeIcon != null)
                {
                    var iconName = Guid.NewGuid() + ".png";

                    metadata.Icon = new MetadataFile(
                        string.Format("images/origin/{0}/{1}", game.GameId.Replace(":", ""), iconName),
                        iconName,
                        exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png)
                    );
                }
            }

            return metadata;
        }
    }
}
