using IGDBMetadata.Models;
using IGDBMetadata.Services;
using Playnite.Common;
using Playnite.Common.Web;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IGDBMetadata
{
    public class IgdbMetadataPlugin : MetadataPlugin
    {
        public class IgdbImageOption : ImageFileOption
        {
            public PlayniteServices.Models.IGDB.GameImage Image { get; set; }
        }

        public readonly IgdbServiceClient Client;
        private static readonly ILogger logger = LogManager.GetLogger();
        public override string Name { get; } = "IGDB";
        public override Guid Id { get; } = Guid.Parse("000001DB-DBD1-46C6-B5D0-B1BA559D10E4");
        internal readonly IgdbMetadataSettings Settings;
        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.Description,
            MetadataField.CoverImage,
            MetadataField.BackgroundImage,
            MetadataField.ReleaseDate,
            MetadataField.Developers,
            MetadataField.Publishers,
            MetadataField.Genres,
            MetadataField.Links,
            MetadataField.Features,
            MetadataField.CriticScore,
            MetadataField.CommunityScore
        };

        public IgdbMetadataPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
            Client = new IgdbServiceClient(playniteAPI.ApplicationInfo.ApplicationVersion);
            Settings = new IgdbMetadataSettings(this);
        }

        public IgdbMetadataPlugin(IPlayniteAPI playniteAPI, IgdbServiceClient client) : base(playniteAPI)
        {
            Client = client;
            Settings = new IgdbMetadataSettings(this);
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new IgdbLazyMetadataProvider(options, this);
        }

        internal static string GetImageUrl(PlayniteServices.Models.IGDB.GameImage image, string imageSize)
        {
            var url = image.url;
            if (!url.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
            {
                url = "https:" + url;
            }

            url = Regex.Replace(url, @"\/t_[^\/]+", "/t_" + imageSize);
            return url;
        }

        public List<SearchResult> GetSearchResults(string gameName)
        {
            var results = new List<SearchResult>();
            foreach (var game in Client.GetIGDBGames(gameName))
            {
                DateTime? releaseDate = null;
                string description = null;
                if (game.first_release_date != 0)
                {
                    releaseDate = DateTimeOffset.FromUnixTimeMilliseconds(game.first_release_date).DateTime;
                    description = $"({releaseDate.Value.Year})";
                }

                results.Add(new SearchResult(
                    game.id.ToString(),
                    game.name.RemoveTrademarks(),
                    releaseDate,
                    game.alternative_names?.Any() == true ? game.alternative_names.Select(name => name.name.RemoveTrademarks()).ToList() : null,
                    description));
            }

            return results;
        }

        internal static string GetGameInfoFromUrl(string url)
        {
            var data = HttpDownloader.DownloadString(url);
            var regex = Regex.Match(data, @"games\/(\d+)\/rates");
            if (regex.Success)
            {
                return regex.Groups[1].Value;
            }
            else
            {
                logger.Error($"Failed to get game id from {url}");
                return string.Empty;
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new IgdbMetadataSettingsView();
        }
    }
}
