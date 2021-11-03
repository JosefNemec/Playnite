using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Database;
using Playnite.DesktopApp.Windows;
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
    }

    public partial class GameEditViewModel
    {
        private const string tempIconFileName = "temp_preview_icon";
        private const string tempCoverFileName = "temp_preview_cover";
        private const string tempBackgroundFileName = "temp_preview_background";

        private List<GameField> GetDisplayDiffFields(Game oldGame, ComparableMetadatGameData newGame)
        {
            var diffFields = new List<GameField>();
            void checkListChanged<T>(List<T> oldData, List<T> newData, GameField field) where T : DatabaseObject
            {
                if (!oldData.HasItems() && newData.HasItems())
                {
                    diffFields.Add(field);
                    return;
                }

                if (newData.HasItems() && oldData.HasItems() && !oldData.Select(a => a.Id).IsListEqual(newData.Select(b => b.Id)))
                {
                    diffFields.Add(field);
                }
            }

            //void checkItemChanged<T>(T source, string other, GameField field) where T : DatabaseObject
            //{
            //    if (!other.IsNullOrEmpty())
            //    {
            //        if (source != null && !string.Equals(source.Name, other, StringComparison.OrdinalIgnoreCase))
            //        {
            //            diffFields.Add(field);
            //        }
            //    }
            //}

            if (!newGame.Name.IsNullOrEmpty())
            {
                if (!oldGame.Name.IsNullOrEmpty() && !string.Equals(oldGame.Name, newGame.Name, StringComparison.OrdinalIgnoreCase))
                {
                    diffFields.Add(GameField.Name);
                }
            }

            if (!newGame.Description.IsNullOrEmpty())
            {
                if (!oldGame.Description.IsNullOrEmpty() && !string.Equals(oldGame.Description, newGame.Description, StringComparison.Ordinal))
                {
                    diffFields.Add(GameField.Description);
                }
            }

            checkListChanged(oldGame.AgeRatings, newGame.AgeRatings, GameField.AgeRatings);
            checkListChanged(oldGame.Regions, newGame.Regions, GameField.Regions);
            checkListChanged(oldGame.Series, newGame.Series, GameField.Series);
            checkListChanged(oldGame.Platforms, newGame.Platforms, GameField.Platforms);
            checkListChanged(oldGame.Developers, newGame.Developers, GameField.Developers);
            checkListChanged(oldGame.Publishers, newGame.Publishers, GameField.Publishers);
            checkListChanged(oldGame.Genres, newGame.Genres, GameField.Genres);
            checkListChanged(oldGame.Tags, newGame.Tags, GameField.Tags);
            checkListChanged(oldGame.Features, newGame.Features, GameField.Features);

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
                var newIcon = ProcessMetadataFile(newGame.Icon, tempIconFileName);
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
                var newCover = ProcessMetadataFile(newGame.CoverImage, tempCoverFileName);
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
                var newBack = ProcessMetadataFile(newGame.BackgroundImage, tempBackgroundFileName);
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

            return diffFields;
        }

        public void PreviewGameData(ComparableMetadatGameData metadata)
        {
            ShowCheckBoxes = true;

            var diffItems = GetDisplayDiffFields(EditingGame, metadata);
            if (diffItems.HasItems())
            {
                var comp = new MetadataComparisonViewModel(
                    database,
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

        public void AddNewAndSetItems<T>(List<T> items, IItemCollection<T> dbCollection, SelectableDbItemList selectList) where T : DatabaseObject
        {
            foreach (var item in items)
            {
                if (dbCollection[item.Id] == null)
                {
                    selectList.Add(item);
                }
            }

            selectList.SetSelection(items.Select(a => a.Id));
        }

        private void LoadNewMetadata(ComparableMetadatGameData newData)
        {
            if (!string.IsNullOrEmpty(newData.Name))
            {
                EditingGame.Name = newData.Name;
            }

            if (newData.Developers.HasItems())
            {
                AddNewAndSetItems(newData.Developers, database.Companies, Developers);
            }

            if (newData.Publishers.HasItems())
            {
                AddNewAndSetItems(newData.Publishers, database.Companies, Publishers);
            }

            if (newData.Genres.HasItems())
            {
                AddNewAndSetItems(newData.Genres, database.Genres, Genres);
            }

            if (newData.Tags.HasItems())
            {
                AddNewAndSetItems(newData.Tags, database.Tags, Tags);
            }

            if (newData.Features.HasItems())
            {
                AddNewAndSetItems(newData.Features, database.Features, Features);
            }

            if (newData.AgeRatings.HasItems())
            {
                AddNewAndSetItems(newData.AgeRatings, database.AgeRatings, AgeRatings);
            }

            if (newData.Regions.HasItems())
            {
                AddNewAndSetItems(newData.Regions, database.Regions, Regions);
            }

            if (newData.Series.HasItems())
            {
                AddNewAndSetItems(newData.Series, database.Series, Series);
            }

            if (newData.Platforms.HasItems())
            {
                AddNewAndSetItems(newData.Platforms, database.Platforms, Platforms);
            }

            if (newData.ReleaseDate != null)
            {
                EditingGame.ReleaseDate = newData.ReleaseDate;
            }

            if (!newData.Description.IsNullOrEmpty())
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
                var newCover = ProcessMetadataFile(newData.CoverImage, tempCoverFileName);
                if (newCover != null)
                {
                    EditingGame.CoverImage = newCover;
                }
            }

            if (newData.Icon != null)
            {
                var newIcon = ProcessMetadataFile(newData.Icon, tempIconFileName);
                if (newIcon != null)
                {
                    EditingGame.Icon = newIcon;
                }
            }

            if (newData.BackgroundImage != null)
            {
                var newBackground = ProcessMetadataFile(newData.BackgroundImage, tempBackgroundFileName);
                if (newBackground != null)
                {
                    EditingGame.BackgroundImage = newBackground;
                }
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

        ComparableMetadatGameData ConvertGameInfo(GameMetadata game)
        {
            var result = new ComparableMetadatGameData
            {
                Name = game.Name,
                Description = game.Description,
                ReleaseDate = game.ReleaseDate,
                CommunityScore = game.CommunityScore,
                CriticScore = game.CriticScore,
                Links = game.Links
            };

            if (game.Genres.HasItems())
            {
                result.Genres = (database.Genres as GenresCollection).GetOrGenerate(game.Genres).Where(a => a != null).ToList();
            }

            if (game.Developers.HasItems())
            {
                result.Developers = (database.Companies as CompaniesCollection).GetOrGenerate(game.Developers).Where(a => a != null).ToList();
            }

            if (game.Publishers.HasItems())
            {
                result.Publishers = (database.Companies as CompaniesCollection).GetOrGenerate(game.Publishers).Where(a => a != null).ToList();
            }

            if (game.Tags.HasItems())
            {
                result.Tags = (database.Tags as TagsCollection).GetOrGenerate(game.Tags).Where(a => a != null).ToList();
            }

            if (game.Features.HasItems())
            {
                result.Features = (database.Features as FeaturesCollection).GetOrGenerate(game.Features).Where(a => a != null).ToList();
            }

            if (game.AgeRatings.HasItems())
            {
                result.AgeRatings = (database.AgeRatings as AgeRatingsCollection).GetOrGenerate(game.AgeRatings).Where(a => a != null).ToList();
            }

            if (game.Series.HasItems())
            {
                result.Series = (database.Series as SeriesCollection).GetOrGenerate(game.Series).Where(a => a != null).ToList();
            }

            if (game.Regions.HasItems())
            {
                result.Regions = (database.Regions as RegionsCollection).GetOrGenerate(game.Regions).Where(a => a != null).ToList();
            }

            if (game.Platforms.HasItems())
            {
                result.Platforms = (database.Platforms as PlatformsCollection).GetOrGenerate(game.Platforms).Where(a => a != null).ToList();
            }

            if (game.CoverImage != null)
            {
                result.CoverImage = new MetadataFile(ProcessMetadataFile(game.CoverImage, tempCoverFileName));
            }

            if (game.Icon != null)
            {
                result.Icon = new MetadataFile(ProcessMetadataFile(game.Icon, tempIconFileName));
            }

            if (game.BackgroundImage != null)
            {
                result.BackgroundImage = new MetadataFile(ProcessMetadataFile(game.BackgroundImage, tempBackgroundFileName));
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
                            BackgroundImage = provider.GetBackgroundImage(fieldArgs)
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

        public void SelectGoogleIcon()
        {
            var image = SelectGoogleImage($"{EditingGame.Name} icon", tempIconFileName, 128, 128);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.Icon = image;
            }
        }

        public void SelectGoogleCover()
        {
            var image = SelectGoogleImage($"{EditingGame.Name} cover", tempCoverFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.CoverImage = image;
            }
        }

        public void SelectGoogleBackground()
        {
            var image = SelectGoogleImage($"{EditingGame.Name} wallpaper", tempBackgroundFileName);
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
