using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Database;
using Playnite.DesktopApp.Windows;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public class ComparableMetadatGameData
    {
        public string Name { get; set; }
        public List<Genre> Genres { get; set; }
        public ReleaseDate? ReleaseDate { get; set; }
        public List<Company> Developers { get; set; }
        public List<Company> Publishers { get; set; }
        public List<Tag> Tags { get; set; }
        public List<GameFeature> Features { get; set; }
        public string Description { get; set; }
        public List<Link> Links { get; set; }
        public int? CriticScore { get; set; }
        public int? CommunityScore { get; set; }
        public List<AgeRating> AgeRatings { get; set; }
        public List<Series> Series { get; set; }
        public List<Region> Regions { get; set; }
        public List<Platform> Platforms { get; set; }
        public MetadataFile Icon { get; set; }
        public MetadataFile CoverImage { get; set; }
        public MetadataFile BackgroundImage { get; set; }
        public ulong? InstallSize { get; set; }
    }

    public partial class GameEditViewModel
    {
        private const string tempEditingIconFileName = "temp_edit_preview_icon";
        private const string tempEditingCoverFileName = "temp_edit_preview_cover";
        private const string tempEditingBackgroundFileName = "temp_edit_preview_background";

        private const string tempDownloadIconFileName = "temp_download_preview_icon";
        private const string tempDownloadCoverFileName = "temp_download_preview_cover";
        private const string tempDownloadBackgroundFileName = "temp_download_preview_background";

        private List<GameField> GetDisplayDiffFields(Game oldGame, ComparableMetadatGameData newGame)
        {
            var diffFields = new List<GameField>();
            void checkListChanged<T>(SelectableDbItemList oldData, IEnumerable<T> newData, GameField field) where T : DatabaseObject
            {
                var oldFields = oldData.Where(a => a.Selected == true).Select(a => a.Item.Name).ToList();
                var newFields = newData?.Select(a => a.Name).ToList() ?? new List<string>();
                if (!oldFields.HasItems() && newFields.HasItems())
                {
                    return;
                }

                if (newFields.HasItems() && oldFields.HasItems() && !oldFields.IsListEqual(newFields, GameFieldComparer.Instance))
                {
                    diffFields.Add(field);
                }
            }

            if (!newGame.Name.IsNullOrWhiteSpace())
            {
                if (!oldGame.Name.IsNullOrWhiteSpace() && !string.Equals(oldGame.Name, newGame.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    diffFields.Add(GameField.Name);
                }
            }

            if (!newGame.Description.IsNullOrWhiteSpace())
            {
                if (!oldGame.Description.IsNullOrWhiteSpace() && !string.Equals(oldGame.Description, newGame.Description, StringComparison.InvariantCultureIgnoreCase))
                {
                    diffFields.Add(GameField.Description);
                }
            }

            checkListChanged(AgeRatings, newGame.AgeRatings, GameField.AgeRatings);
            checkListChanged(Regions, newGame.Regions, GameField.Regions);
            checkListChanged(Series, newGame.Series, GameField.Series);
            checkListChanged(Platforms, newGame.Platforms, GameField.Platforms);
            checkListChanged(Developers, newGame.Developers, GameField.Developers);
            checkListChanged(Publishers, newGame.Publishers, GameField.Publishers);
            checkListChanged(Genres, newGame.Genres, GameField.Genres);
            checkListChanged(Tags, newGame.Tags, GameField.Tags);
            checkListChanged(Features, newGame.Features, GameField.Features);

            if (newGame.ReleaseDate != null)
            {
                if (oldGame.ReleaseDate != null && oldGame.ReleaseDate != newGame.ReleaseDate)
                {
                    diffFields.Add(GameField.ReleaseDate);
                }
            }

            if (newGame.Links.HasItems())
            {
                if (oldGame.Links.HasItems() && !oldGame.Links.IsListEqualExact(newGame.Links))
                {
                    diffFields.Add(GameField.Links);
                }
            }

            if (newGame.CriticScore != null)
            {
                if (oldGame.CriticScore != null && oldGame.CriticScore != newGame.CriticScore)
                {
                    diffFields.Add(GameField.CriticScore);
                }
            }

            if (newGame.CommunityScore != null)
            {
                if (oldGame.CommunityScore != null && oldGame.CommunityScore != newGame.CommunityScore)
                {
                    diffFields.Add(GameField.CommunityScore);
                }
            }

            if (newGame.Icon != null && !oldGame.Icon.IsNullOrEmpty())
            {
                var newIcon = ProcessMetadataFile(newGame.Icon, tempDownloadIconFileName);
                if (newIcon != null)
                {
                    var currentPath = ImageSourceManager.GetImagePath(EditingGame.Icon);
                    if (currentPath.IsNullOrEmpty() ||
                        !File.Exists(currentPath) ||
                        !FileSystem.AreFileContentsEqual(newIcon, currentPath))
                    {
                        newGame.Icon = new MetadataFile(newIcon);
                        diffFields.Add(GameField.Icon);
                    }
                    else
                    {
                        newGame.Icon = null;
                    }
                }
                else
                {
                    newGame.Icon = null;
                }
            }

            if (newGame.CoverImage != null && !oldGame.CoverImage.IsNullOrEmpty())
            {
                var newCover = ProcessMetadataFile(newGame.CoverImage, tempDownloadCoverFileName);
                if (newCover != null)
                {
                    var currentPath = ImageSourceManager.GetImagePath(EditingGame.CoverImage);
                    if (currentPath.IsNullOrEmpty() ||
                        !File.Exists(currentPath) ||
                        !FileSystem.AreFileContentsEqual(newCover, currentPath))
                    {
                        newGame.CoverImage = new MetadataFile(newCover);
                        diffFields.Add(GameField.CoverImage);
                    }
                    else
                    {
                        newGame.CoverImage = null;
                    }
                }
                else
                {
                    newGame.CoverImage = null;
                }
            }

            if (newGame.BackgroundImage != null && !oldGame.BackgroundImage.IsNullOrEmpty())
            {
                var newBack = ProcessMetadataFile(newGame.BackgroundImage, tempDownloadBackgroundFileName);
                if (newBack != null)
                {
                    var currentPath = ImageSourceManager.GetImagePath(EditingGame.BackgroundImage);
                    if (currentPath.IsNullOrEmpty() ||
                        !File.Exists(currentPath) ||
                        !FileSystem.AreFileContentsEqual(newBack, currentPath))
                    {
                        newGame.BackgroundImage = new MetadataFile(newBack);
                        diffFields.Add(GameField.BackgroundImage);
                    }
                    else
                    {
                        newGame.BackgroundImage = null;
                    }
                }
                else
                {
                    newGame.BackgroundImage = null;
                }
            }

            if (!oldGame.IsInstalled && newGame.InstallSize != null)
            {
                if (oldGame.InstallSize != null && oldGame.InstallSize != newGame.InstallSize)
                {
                    diffFields.Add(GameField.InstallSize);
                }
            }

            return diffFields;
        }

        public void PreviewGameData(ComparableMetadatGameData metadata)
        {
            var diffItems = GetDisplayDiffFields(EditingGame, metadata);
            if (diffItems.HasItems())
            {
                var comp = new MetadataComparisonViewModel(
                    new MetadataComparisonWindowFactory(),
                    dialogs,
                    resources,
                    EditingGame,
                    metadata,
                    diffItems);

                if (comp.OpenView() == true)
                {
                    LoadNewMetadata(comp.ResultMetadata);
                }
            }
            else
            {
                LoadNewMetadata(metadata);
            }
        }

        public void AddNewAndSetFieldList<T>(List<T> items, SelectableDbItemList selectList) where T : DatabaseObject
        {
            foreach (var item in items)
            {
                if (selectList.FirstOrDefault(a => a.Item.Id == item.Id) == null)
                {
                    selectList.Add(item);
                }
            }

            selectList.SetSelection(items.Select(a => a.Id));
        }

        private void LoadNewMetadata(ComparableMetadatGameData newData)
        {
            ShowCheckBoxes = true;
            if (!newData.Name.IsNullOrWhiteSpace() && !string.Equals(newData.Name, EditingGame.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                EditingGame.Name = newData.Name;
            }

            if (newData.Developers.HasItems())
            {
                AddNewAndSetFieldList(newData.Developers, Developers);
            }

            if (newData.Publishers.HasItems())
            {
                AddNewAndSetFieldList(newData.Publishers, Publishers);
            }

            if (newData.Genres.HasItems())
            {
                AddNewAndSetFieldList(newData.Genres, Genres);
            }

            if (newData.Tags.HasItems())
            {
                AddNewAndSetFieldList(newData.Tags, Tags);
            }

            if (newData.Features.HasItems())
            {
                AddNewAndSetFieldList(newData.Features, Features);
            }

            if (newData.AgeRatings.HasItems())
            {
                AddNewAndSetFieldList(newData.AgeRatings, AgeRatings);
            }

            if (newData.Regions.HasItems())
            {
                AddNewAndSetFieldList(newData.Regions, Regions);
            }

            if (newData.Series.HasItems())
            {
                AddNewAndSetFieldList(newData.Series, Series);
            }

            if (newData.Platforms.HasItems())
            {
                AddNewAndSetFieldList(newData.Platforms, Platforms);
            }

            if (newData.ReleaseDate != null)
            {
                EditingGame.ReleaseDate = newData.ReleaseDate;
            }

            if (!newData.Description.IsNullOrWhiteSpace() && !string.Equals(newData.Description, EditingGame.Description, StringComparison.InvariantCultureIgnoreCase))
            {
                EditingGame.Description = newData.Description;
            }

            if (newData.Links.HasItems())
            {
                EditingGame.Links = newData.Links.ToObservable();
            }

            if (newData.CriticScore != null)
            {
                EditingGame.CriticScore = newData.CriticScore;
            }

            if (newData.CommunityScore != null)
            {
                EditingGame.CommunityScore = newData.CommunityScore;
            }

            if (newData.CoverImage != null)
            {
                var newCover = ProcessMetadataFile(newData.CoverImage, tempEditingCoverFileName);
                if (newCover != null)
                {
                    EditingGame.CoverImage = newCover;
                }
            }

            if (newData.Icon != null)
            {
                var newIcon = ProcessMetadataFile(newData.Icon, tempEditingIconFileName);
                if (newIcon != null)
                {
                    EditingGame.Icon = newIcon;
                }
            }

            if (newData.BackgroundImage != null)
            {
                var newBackground = ProcessMetadataFile(newData.BackgroundImage, tempEditingBackgroundFileName);
                if (newBackground != null)
                {
                    EditingGame.BackgroundImage = newBackground;
                }
            }

            if (!EditingGame.IsInstalled && newData.InstallSize != null)
            {
                EditingGame.InstallSize = newData.InstallSize;
            }
        }

        private string ProcessMetadataFile(string file, string tempFileName)
        {
            return ProcessMetadataFile(new MetadataFile(file), tempFileName);
        }

        private string ProcessMetadataFile(MetadataFile file, string tempFileName)
        {
            string localFile = null;
            var progRes = dialogs.ActivateGlobalProgress((a) =>
                {
                    localFile = file.GetLocalFile(a.CancelToken);
                    // GetLocalFile creates generic Guid based name if a file is http link
                    if (localFile?.StartsWith(PlaynitePaths.TempPath) == true)
                    {
                        var resFile = Path.Combine(PlaynitePaths.TempPath, tempFileName + Path.GetExtension(localFile));
                        if (!localFile.Equals(resFile, StringComparison.OrdinalIgnoreCase))
                        {
                            FileSystem.DeleteFile(resFile);
                            File.Move(localFile, resFile);
                            localFile = resFile;
                        }
                    }
                },
                new GlobalProgressOptions("LOCDownloadingMediaLabel", true));
            if (progRes.Result == true && !progRes.Canceled)
            {
                if (localFile.IsNullOrEmpty())
                {
                    return null;
                }
                else
                {
                    localFile = Images.ConvertToCompatibleFormat(localFile, Path.Combine(PlaynitePaths.TempPath, tempFileName));
                    return localFile;
                }
            }
            else
            {
                logger.Error(progRes.Error, $"Failed to download {file.Path}.");
                return null;
            }
        }

        public IEnumerable<T> GetOrGenerateNewFieldItem<T>(HashSet<MetadataProperty> properties, SelectableDbItemList selectList) where T : DatabaseObject
        {
            foreach (var property in properties)
            {
                if (property is MetadataNameProperty nameProp)
                {
                    if (nameProp.Name.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    var existingItem = selectList.FirstOrDefault(a => GameFieldComparer.StringEquals(a.Item.Name, nameProp.Name));
                    if (existingItem != null)
                    {
                        yield return (T)existingItem.Item;
                    }
                    else
                    {
                        yield return typeof(T).CrateInstance<T>(nameProp.Name);
                    }
                }
                else if (property is MetadataIdProperty idProp)
                {
                    var existingItem = selectList.FirstOrDefault(a => a.Item.Id == idProp.Id);
                    if (existingItem != null)
                    {
                        yield return  (T)existingItem.Item;
                    }
                }
                else if (property is MetadataSpecProperty specProp)
                {
                    if (typeof(T) == typeof(Platform))
                    {
                        var platSpec = Emulation.Platforms.FirstOrDefault(a => a.Id == specProp.Id || a.Name == specProp.Id);
                        if (platSpec != null)
                        {
                            var exPlat = selectList.FirstOrDefault(a => ((Platform)a.Item).SpecificationId == platSpec.Id);
                            if (exPlat != null)
                            {
                                yield return (T)exPlat.Item;
                            }
                            else
                            {
                                yield return (T)(new Platform(platSpec.Name) { SpecificationId = platSpec.Id } as DatabaseObject);
                            }
                        }
                    }
                    else if (typeof(T) == typeof(Region))
                    {
                        var regionSpec = Emulation.Regions.FirstOrDefault(a => a.Id == specProp.Id || a.Name == specProp.Id);
                        if (regionSpec != null)
                        {
                            var exRegion = selectList.FirstOrDefault(a => ((Region)a.Item).SpecificationId == regionSpec.Id);
                            if (exRegion != null)
                            {
                                yield return (T)exRegion.Item;
                            }
                            else
                            {
                                yield return (T)(new Region(regionSpec.Name) { SpecificationId = regionSpec.Id } as DatabaseObject);
                            }
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"{property.GetType()} property type is not supported in {typeof(T)} collection.");
                    }
                }
                else
                {
                    throw new NotSupportedException($"{property.GetType()} property type is not supported in {typeof(T)} collection.");
                }
            }
        }

        ComparableMetadatGameData ConvertGameInfo(GameMetadata game)
        {
            var result = new ComparableMetadatGameData
            {
                Name = game.Name,
                Description = game.Description,
                ReleaseDate = game.ReleaseDate,
                CommunityScore = game.CommunityScore,
                CriticScore = game.CriticScore,
                Links = game.Links,
                InstallSize = game.InstallSize
            };

            if (game.Genres.HasItems())
            {
                result.Genres = GetOrGenerateNewFieldItem<Genre>(game.Genres, Genres).ToList();
            }

            if (game.Developers.HasItems())
            {
                result.Developers = GetOrGenerateNewFieldItem<Company>(game.Developers, Developers).ToList();
            }

            if (game.Publishers.HasItems())
            {
                var publishers = GetOrGenerateNewFieldItem<Company>(game.Publishers, Publishers).ToList();
                for (int i = 0; i < publishers.Count; i++)
                {
                    var p = publishers[i];
                    var existingDev = result.Developers?.FirstOrDefault(d => GameFieldComparer.StringEquals(p.Name, d.Name));
                    if (existingDev != null && p.Id != existingDev.Id)
                    {
                        publishers[i] = existingDev;
                    }
                }

                result.Publishers = publishers;
            }

            if (game.Tags.HasItems())
            {
                result.Tags = GetOrGenerateNewFieldItem<Tag>(game.Tags, Tags).ToList();
            }

            if (game.Features.HasItems())
            {
                result.Features = GetOrGenerateNewFieldItem<GameFeature>(game.Features, Features).ToList();
            }

            if (game.AgeRatings.HasItems())
            {
                result.AgeRatings = GetOrGenerateNewFieldItem<AgeRating>(game.AgeRatings, AgeRatings).ToList();
            }

            if (game.Series.HasItems())
            {
                result.Series = GetOrGenerateNewFieldItem<Series>(game.Series, Series).ToList();
            }

            if (game.Regions.HasItems())
            {
                result.Regions = GetOrGenerateNewFieldItem<Region>(game.Regions, Regions).ToList();
            }

            if (game.Platforms.HasItems())
            {
                result.Platforms = GetOrGenerateNewFieldItem<Platform>(game.Platforms, Platforms).ToList();
            }

            if (game.CoverImage != null)
            {
                result.CoverImage = new MetadataFile(ProcessMetadataFile(game.CoverImage, tempDownloadCoverFileName));
            }

            if (game.Icon != null)
            {
                result.Icon = new MetadataFile(ProcessMetadataFile(game.Icon, tempDownloadIconFileName));
            }

            if (game.BackgroundImage != null)
            {
                result.BackgroundImage = new MetadataFile(ProcessMetadataFile(game.BackgroundImage, tempDownloadBackgroundFileName));
            }

            return result;
        }

        public void DownloadPluginData(MetadataPlugin plugin)
        {
            var res = dialogs.ActivateGlobalProgress((args) =>
            {
                var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(EditingGame, false));
                if (provider != null)
                {
                    try
                    {
                        var fieldArgs = new GetMetadataFieldArgs { CancelToken = args.CancelToken };
                        var metadata = new GameMetadata
                        {
                            Name = provider.GetName(fieldArgs),
                            Genres = provider.GetGenres(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            ReleaseDate = provider.GetReleaseDate(fieldArgs),
                            Developers = provider.GetDevelopers(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Publishers = provider.GetPublishers(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Tags = provider.GetTags(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Features = provider.GetFeatures(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Description = provider.GetDescription(fieldArgs),
                            Links = provider.GetLinks(fieldArgs)?.Where(a => a != null).ToList(),
                            CriticScore = provider.GetCriticScore(fieldArgs),
                            CommunityScore = provider.GetCommunityScore(fieldArgs),
                            AgeRatings = provider.GetAgeRatings(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Series = provider.GetSeries(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Regions = provider.GetRegions(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Platforms = provider.GetPlatforms(fieldArgs)?.Where(a => a != null).ToHashSet(),
                            Icon = provider.GetIcon(fieldArgs),
                            CoverImage = provider.GetCoverImage(fieldArgs),
                            BackgroundImage = provider.GetBackgroundImage(fieldArgs),
                            InstallSize = provider.GetInstallSize(fieldArgs)
                        };

                        Application.Current.Dispatcher.Invoke(() => PreviewGameData(ConvertGameInfo(metadata)));
                    }
                    finally
                    {
                        provider.Dispose();
                    }
                }
            }, new GlobalProgressOptions(LOC.DownloadingLabel)
            {
                IsIndeterminate = true,
                Cancelable = true
            });

            if (res.Error != null)
            {
                logger.Error(res.Error, string.Format("Failed to download metadata, {0}, {1}", Game.PluginId, Game.GameId));
                dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCMetadataDownloadError"), res.Error.Message),
                    resources.GetString("LOCDownloadError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DownloadStoreData()
        {
            var res = dialogs.ActivateGlobalProgress((args) =>
            {
                if (extensions.Plugins.TryGetValue(Game.PluginId, out var plugin))
                {
                    if (LibraryPluginMetadataDownloader == null)
                    {
                        dialogs.ShowErrorMessage(
                            resources.GetString("LOCErrorNoMetadataDownloader"),
                            resources.GetString("LOCGameError"));
                        return;
                    }

                    var metadata = LibraryPluginMetadataDownloader.GetMetadata(EditingGame);
                    if (metadata != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => PreviewGameData(ConvertGameInfo(metadata)));
                    }
                }
                else
                {
                    dialogs.ShowErrorMessage(
                        resources.GetString("LOCErrorLibraryPluginNotFound"),
                        resources.GetString("LOCGameError"));
                    return;
                }
            }, new GlobalProgressOptions(LOC.DownloadingLabel)
            {
                IsIndeterminate = true,
                Cancelable = true
            });

            if (res.Error != null)
            {
                logger.Error(res.Error, string.Format("Failed to download metadata, {0}, {1}", Game.PluginId, Game.GameId));
                dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCMetadataDownloadError"), res.Error.Message),
                    resources.GetString("LOCDownloadError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ReplaceImageSearchVariables(string input)
        {
            input = input.Replace("{Name}", editingGame.Name, StringComparison.OrdinalIgnoreCase);
            return input.Replace("{Platform}", editingGame.Platforms?.FirstOrDefault()?.Name, StringComparison.OrdinalIgnoreCase);
        }

        public void SelectGoogleIcon()
        {
            var searchTerm = appSettings.WebImageSarchIconTerm.IsNullOrWhiteSpace() ? $"{EditingGame.Name} icon" : appSettings.WebImageSarchIconTerm;
            searchTerm = ReplaceImageSearchVariables(searchTerm);
            var image = SelectGoogleImage(searchTerm, tempEditingIconFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.Icon = image;
            }
        }

        public void SelectGoogleCover()
        {
            var searchTerm = appSettings.WebImageSarchCoverTerm.IsNullOrWhiteSpace() ? $"{EditingGame.Name} cover" : appSettings.WebImageSarchCoverTerm;
            searchTerm = ReplaceImageSearchVariables(searchTerm);
            var image = SelectGoogleImage(searchTerm, tempEditingCoverFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.CoverImage = image;
            }
        }

        public void SelectGoogleBackground()
        {
            var searchTerm = appSettings.WebImageSarchBackgroundTerm.IsNullOrWhiteSpace() ? $"{EditingGame.Name} wallpaper" : appSettings.WebImageSarchBackgroundTerm;
            searchTerm = ReplaceImageSearchVariables(searchTerm);
            var image = SelectGoogleImage(searchTerm, tempEditingBackgroundFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = image;
            }
        }

        public string SelectGoogleImage(string searchTerm, string tempFileName, double imageWidth = 0, double imageHeight = 0)
        {
            var model = new GoogleImageDownloadViewModel(
                new GoogleImageDownloadWindowFactory(),
                resources,
                searchTerm,
                imageWidth,
                imageHeight);
            if (model.OpenView() == true)
            {
                try
                {
                    var url = model.SelectedImage?.ImageUrl;
                    if (url.IsNullOrEmpty())
                    {
                        return null;
                    }

                    var response = HttpDownloader.GetResponseCode(url);
                    if (response != HttpStatusCode.OK)
                    {
                        logger.Warn("Original Google image request failed: " + response.ToString());
                        url = model.SelectedImage.ThumbUrl;
                    }

                    return ProcessMetadataFile(url, tempFileName);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to use google image {model.SelectedImage?.ImageUrl}.");
                }
            }

            return null;
        }
    }
}
