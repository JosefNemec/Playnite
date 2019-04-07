using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordLibrary.Models;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.Web;

namespace DiscordLibrary
{
    public class DiscordMetadataProvider : ILibraryMetadataProvider
    {
        private readonly DiscordLibrary library;
        private readonly ILogger logger = LogManager.GetLogger();
        private readonly Dictionary<long, Application> applications;

        public DiscordMetadataProvider(DiscordLibrary library)
        {
            this.library = library;
            //throws WebException
            var applicationsData = HttpDownloader.DownloadString("https://discordapp.com/api/v6/applications");
            applications = Serialization.FromJson<List<Application>>(applicationsData).ToDictionary(x => x.id, x => x);
        }

        #region IMetadataProvider

        public GameMetadata GetMetadata(Game game)
        {
            var metadata = new GameMetadata();

            // Get application
            if (!long.TryParse(game.GameId, out var applicationId))
            {
                return null;
            }
            Application application = applications[applicationId];

            // Get storeListing
            long skuId = application.primary_sku_id;
            var skuData = HttpDownloader.DownloadString($"https://discordapp.com/api/v6/store/published-listings/skus/{skuId}");
            if(!Serialization.TryFromJson<StoreListing>(skuData, out var storeListing))
            {
                return null;
            }

            metadata.GameData = new Game()
            {
                Name = application.name,
                ReleaseDate = storeListing.sku.release_date,
                CoverImage = $"https://cdn.discordapp.com/app-assets/{applicationId}/store/{storeListing.thumbnail.id}.png?size=2048",
                Links = new ObservableCollection<Link>() { new Link("Discord", $"https://discordapp.com/store/skus/{skuId}/{storeListing.sku.slug}") },

                // TODO: Format Markdown
                //Description = storeListing.description,
            };

            var iconUrl = $"https://cdn.discordapp.com/app-icons/{applicationId}/{application.icon}.png";
            metadata.Icon = new MetadataFile("icon.png", HttpDownloader.DownloadData(iconUrl));

            metadata.Image = new MetadataFile("cover.png", HttpDownloader.DownloadData(metadata.GameData.CoverImage));

            metadata.BackgroundImage = $"https://cdn.discordapp.com/app-assets/{applicationId}/store/{storeListing.hero_background.id}.png?size=2048";

            if (application.developers != null)
            {
                metadata.GameData.Developers = new ComparableList<string>(application.developers.Select(d => d.name));
            }

            if (application.publishers != null)
            {
                metadata.GameData.Publishers = new ComparableList<string>(application.publishers.Select(d => d.name));
            }

            foreach (var thirdPartySku in application.third_party_skus)
            {
                switch(thirdPartySku.distributor)
                {
                    case "battlenet":
                        //TODO: add link
                        //Sometimes null id and sku
                        break;
                    case "discord":
                        //TODO: add link
                        break;
                    case "epic":
                        //TODO: add link
                        break;
                    case "glyph":
                        //TODO: add link
                        break;
                    case "gog":
                        //TODO: add link
                        break;
                    case "origin":
                        //TODO: add link
                        break;
                    case "steam":
                        metadata.GameData.Links.Add(new Link("Steam", $"https://store.steampowered.com/app/{thirdPartySku.id}"));
                        break;
                    case "twitch":
                        //TODO: add link
                        break;
                    case "uplay":
                        //TODO: add link
                        break;
                }
            }

            return metadata;
        }

        #endregion IMetadataProvider
    }
}
