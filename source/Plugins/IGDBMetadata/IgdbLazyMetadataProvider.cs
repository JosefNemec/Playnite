using IGDBMetadata.Models;
using Playnite.Common;
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
using static IGDBMetadata.IgdbMetadataPlugin;
using IgdbServerModels = PlayniteServices.Models.IGDB;

namespace IGDBMetadata
{
    public class IgdbLazyMetadataProvider : OnDemandMetadataProvider
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly MetadataRequestOptions options;
        private readonly IgdbMetadataPlugin plugin;
        internal IgdbServerModels.ExpandedGame IgdbData { get; private set; }

        private List<MetadataField> availableFields;
        public override List<MetadataField> AvailableFields
        {
            get
            {
                if (availableFields == null)
                {
                    availableFields = GetAvailableFields();
                }

                return availableFields;
            }
        }

        public IgdbLazyMetadataProvider(MetadataRequestOptions options, IgdbMetadataPlugin plugin)
        {
            this.options = options;
            this.plugin = plugin;
        }

        private IgdbImageOption GetBackgroundManually(List<IgdbServerModels.GameImage> possibleBackgrounds)
        {
            var selection = new List<ImageFileOption>();
            foreach (var artwork in possibleBackgrounds)
            {
                selection.Add(new IgdbImageOption
                {
                    Path = IgdbMetadataPlugin.GetImageUrl(artwork, ImageSizes.screenshot_med),
                    Image = artwork
                });
            }
            return plugin.PlayniteApi.Dialogs.ChooseImageFile(
                selection,
                string.Format(plugin.PlayniteApi.Resources.GetString("LOCIgdbSelectBackgroundTitle"), IgdbData.name)) as IgdbImageOption;
        }

        public override MetadataFile GetBackgroundImage()
        {
            if (AvailableFields.Contains(MetadataField.BackgroundImage))
            {
                List<IgdbServerModels.GameImage> possibleBackgrounds = null;
                if (IgdbData.artworks.HasItems())
                {
                    possibleBackgrounds = IgdbData.artworks;
                }
                else if (plugin.Settings.UseScreenshotsIfNecessary && IgdbData.screenshots.HasItems())
                {
                    possibleBackgrounds = IgdbData.screenshots;
                }

                if (possibleBackgrounds.HasItems())
                {
                    IgdbServerModels.GameImage selected = null;
                    if (possibleBackgrounds.Count == 1)
                    {
                        selected = possibleBackgrounds[0];
                    }
                    else
                    {
                        if (options.IsBackgroundDownload)
                        {
                            if (plugin.Settings.ImageSelectionPriority == MultiImagePriority.First)
                            {
                                selected = possibleBackgrounds[0];
                            }
                            else if (plugin.Settings.ImageSelectionPriority == MultiImagePriority.Random ||
                                (plugin.Settings.ImageSelectionPriority == MultiImagePriority.Select && plugin.PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen))
                            {
                                var index = GlobalRandom.Next(0, possibleBackgrounds.Count - 1);
                                selected = possibleBackgrounds[index];
                            }
                            else if (plugin.Settings.ImageSelectionPriority == MultiImagePriority.Select && plugin.PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                            {
                                selected = GetBackgroundManually(possibleBackgrounds)?.Image;
                            }
                        }
                        else
                        {
                            selected = GetBackgroundManually(possibleBackgrounds)?.Image;
                        }
                    }

                    if (selected != null && !selected.url.IsNullOrEmpty())
                    {
                        if (selected.height > 1080)
                        {
                            return new MetadataFile(GetImageUrl(selected, ImageSizes.p1080));
                        }
                        else
                        {
                            return new MetadataFile(GetImageUrl(selected, ImageSizes.original));
                        }
                    }
                }
            }

            return base.GetBackgroundImage();
        }

        public override int? GetCommunityScore()
        {
            if (AvailableFields.Contains(MetadataField.CommunityScore))
            {
                return Convert.ToInt32(IgdbData.rating);
            }

            return base.GetCommunityScore();
        }

        public override MetadataFile GetCoverImage()
        {
            if (AvailableFields.Contains(MetadataField.CoverImage) && !IgdbData.cover_v3.url.IsNullOrEmpty())
            {
                if (IgdbData.cover_v3.height > 1080)
                {
                    return new MetadataFile(GetImageUrl(IgdbData.cover_v3, ImageSizes.p1080));
                }
                else
                {
                    return new MetadataFile(GetImageUrl(IgdbData.cover_v3, ImageSizes.original));
                }
            }

            return base.GetCoverImage();
        }

        public override int? GetCriticScore()
        {
            if (AvailableFields.Contains(MetadataField.CriticScore))
            {
                return Convert.ToInt32(IgdbData.aggregated_rating);
            }

            return base.GetCriticScore();
        }

        public override string GetDescription()
        {
            if (AvailableFields.Contains(MetadataField.Description))
            {
                return IgdbData.summary.Replace("\n", "\n<br>");
            }

            return base.GetDescription();
        }

        public override List<string> GetDevelopers()
        {
            if (AvailableFields.Contains(MetadataField.Developers))
            {
                return IgdbData.developers;
            }

            return base.GetDevelopers();
        }

        public override List<string> GetGenres()
        {
            if (AvailableFields.Contains(MetadataField.Genres))
            {
                return IgdbData.genres;
            }

            return base.GetGenres();
        }

        public override string GetName()
        {
            if (AvailableFields.Contains(MetadataField.Name))
            {
                return IgdbData.name;
            }

            return base.GetName();
        }

        public override List<string> GetPublishers()
        {
            if (AvailableFields.Contains(MetadataField.Publishers))
            {
                return IgdbData.publishers;
            }

            return base.GetPublishers();
        }

        public override DateTime? GetReleaseDate()
        {
            if (AvailableFields.Contains(MetadataField.ReleaseDate))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(IgdbData.first_release_date).DateTime;
            }

            return base.GetReleaseDate();
        }

        public override List<string> GetFeatures()
        {
            if (AvailableFields.Contains(MetadataField.Features))
            {
                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                var features = IgdbData.game_modes.Select(a => cultInfo.ToTitleCase(a)).ToList();
                if (IgdbData.player_perspectives.HasItems() &&
                    IgdbData.player_perspectives.FirstOrDefault(a => a.name == "Virtual Reality") != null)
                {
                    features.Add("VR");
                }

                return features;
            }
            return base.GetFeatures();
        }

        public override List<Link> GetLinks()
        {
            if (AvailableFields.Contains(MetadataField.Links))
            {
                return IgdbData.websites.Where(a => !a.url.IsNullOrEmpty()).Select(a => new Link(a.category.ToString(), a.url)).ToList();
            }

            return base.GetLinks();
        }

        private List<MetadataField> GetAvailableFields()
        {
            if (IgdbData == null)
            {
                GetIgdbMetadata();
            }

            if (IgdbData.id == 0)
            {
                return new List<MetadataField>();
            }
            else
            {
                var fields = new List<MetadataField> { MetadataField.Name };
                if (!IgdbData.summary.IsNullOrEmpty())
                {
                    fields.Add(MetadataField.Description);
                }

                if (IgdbData.cover_v3 != null)
                {
                    fields.Add(MetadataField.CoverImage);
                }

                if (IgdbData.artworks.HasItems())
                {
                    fields.Add(MetadataField.BackgroundImage);
                }
                else if (IgdbData.screenshots.HasItems() && plugin.Settings.UseScreenshotsIfNecessary)
                {
                    fields.Add(MetadataField.BackgroundImage);
                }

                if (IgdbData.first_release_date != 0)
                {
                    fields.Add(MetadataField.ReleaseDate);
                }

                if (IgdbData.developers.HasItems())
                {
                    fields.Add(MetadataField.Developers);
                }

                if (IgdbData.publishers.HasItems())
                {
                    fields.Add(MetadataField.Publishers);
                }

                if (IgdbData.genres.HasItems())
                {
                    fields.Add(MetadataField.Genres);
                }

                if (IgdbData.websites.HasItems())
                {
                    fields.Add(MetadataField.Links);
                }

                if (IgdbData.game_modes.HasItems())
                {
                    fields.Add(MetadataField.Features);
                }

                if (IgdbData.aggregated_rating != 0)
                {
                    fields.Add(MetadataField.CriticScore);
                }

                if (IgdbData.rating != 0)
                {
                    fields.Add(MetadataField.CommunityScore);
                }

                return fields;
            }
        }

        private void GetIgdbMetadata()
        {
            if (IgdbData != null)
            {
                return;
            }

            if (!options.IsBackgroundDownload)
            {
                var item = plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, (a) =>
                {
                    if (a.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var gameId = GetGameInfoFromUrl(a);
                            var data = plugin.Client.GetIGDBGameParsed(ulong.Parse(gameId));
                            return new List<GenericItemOption> { new SearchResult(gameId, data.name) };
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, $"Failed to get game data from {a}");
                            return new List<GenericItemOption>();
                        }
                    }
                    else
                    {
                        var res = plugin.GetSearchResults(a);
                        return res.Select(b => b as GenericItemOption).ToList();
                    }
                }, options.GameData.Name);

                if (item != null)
                {
                    var searchItem = item as SearchResult;
                    IgdbData = plugin.Client.GetIGDBGameParsed(ulong.Parse(searchItem.Id));
                }
                else
                {
                    IgdbData = new IgdbServerModels.ExpandedGame() { id = 0 };
                }
            }
            else
            {
                var metadata = plugin.Client.GetMetadata(options.GameData);
                if (metadata.id > 0)
                {
                    IgdbData = metadata;
                }
                else
                {
                    IgdbData = new IgdbServerModels.ExpandedGame() { id = 0 };
                }
            }
        }
    }
}
