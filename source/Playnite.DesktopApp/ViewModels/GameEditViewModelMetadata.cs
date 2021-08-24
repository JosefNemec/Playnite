using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Database;
using Playnite.DesktopApp.Windows;
using Playnite.SDK;
using Playnite.SDK.Metadata;
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
    public partial class GameEditViewModel
    {
        private const string tempIconFileName = "temp_preview_icon";
        private const string tempCoverFileName = "temp_preview_cover";
        private const string tempBackgroundFileName = "temp_preview_background";

        private List<GameField> GetDisplayDiffFields(Game testGame, GameMetadata metadata)
        {
            var diffFields = new List<GameField>();
            var newInfo = metadata.GameInfo;

            void checkListChanged<T>(List<T> source, List<string> other, GameField field) where T : DatabaseObject
            {
                if (other.HasNonEmptyItems())
                {
                    if (source.HasItems() && !source.Select(a => a.Name).IsListEqual(other, new GameFieldComparer()))
                    {
                        diffFields.Add(field);
                    }
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

            if (!newInfo.Name.IsNullOrEmpty())
            {
                if (!testGame.Name.IsNullOrEmpty() && !string.Equals(testGame.Name, newInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    diffFields.Add(GameField.Name);
                }
            }

            if (!newInfo.Description.IsNullOrEmpty())
            {
                if (!testGame.Description.IsNullOrEmpty() && !string.Equals(testGame.Description, newInfo.Description, StringComparison.Ordinal))
                {
                    diffFields.Add(GameField.Description);
                }
            }

            checkListChanged(testGame.AgeRatings, newInfo.AgeRatings, GameField.AgeRatings);
            checkListChanged(testGame.Regions, newInfo.Regions, GameField.Regions);
            checkListChanged(testGame.Series, newInfo.Series, GameField.Series);
            checkListChanged(testGame.Platforms, newInfo.Platforms, GameField.Platforms);
            checkListChanged(testGame.Developers, newInfo.Developers, GameField.Developers);
            checkListChanged(testGame.Publishers, newInfo.Publishers, GameField.Publishers);
            checkListChanged(testGame.Genres, newInfo.Genres, GameField.Genres);
            checkListChanged(testGame.Tags, newInfo.Tags, GameField.Tags);
            checkListChanged(testGame.Features, newInfo.Features, GameField.Features);

            if (newInfo.ReleaseDate != null)
            {
                if (testGame.ReleaseDate != null && testGame.ReleaseDate != newInfo.ReleaseDate)
                {
                    diffFields.Add(GameField.ReleaseDate);
                }
            }

            if (newInfo.Links.HasItems())
            {
                if (testGame.Links.HasItems() && !testGame.Links.IsListEqualExact(newInfo.Links))
                {
                    diffFields.Add(GameField.Links);
                }
            }

            if (newInfo.CriticScore != null)
            {
                if (testGame.CriticScore != null && testGame.CriticScore != newInfo.CriticScore)
                {
                    diffFields.Add(GameField.CriticScore);
                }
            }

            if (newInfo.CommunityScore != null)
            {
                if (testGame.CommunityScore != null && testGame.CommunityScore != newInfo.CommunityScore)
                {
                    diffFields.Add(GameField.CommunityScore);
                }
            }

            if (newInfo.Icon != null && !testGame.Icon.IsNullOrEmpty())
            {
                var newIcon = ProcessMetadataFile(newInfo.Icon, tempIconFileName);
                if (newIcon != null)
                {
                    var currentPath = ImageSourceManager.GetImagePath(EditingGame.Icon);
                    if (currentPath.IsNullOrEmpty() ||
                        !File.Exists(currentPath) ||
                        !FileSystem.AreFileContentsEqual(newIcon, currentPath))
                    {
                        newInfo.Icon = new MetadataFile(newIcon);
                        diffFields.Add(GameField.Icon);
                    }
                    else
                    {
                        newInfo.Icon = null;
                    }
                }
                else
                {
                    newInfo.Icon = null;
                }
            }

            if (newInfo.CoverImage != null && !testGame.CoverImage.IsNullOrEmpty())
            {
                var newCover = ProcessMetadataFile(newInfo.CoverImage, tempCoverFileName);
                if (newCover != null)
                {
                    var currentPath = ImageSourceManager.GetImagePath(EditingGame.CoverImage);
                    if (currentPath.IsNullOrEmpty() ||
                        !File.Exists(currentPath) ||
                        !FileSystem.AreFileContentsEqual(newCover, currentPath))
                    {
                        newInfo.CoverImage = new MetadataFile(newCover);
                        diffFields.Add(GameField.CoverImage);
                    }
                    else
                    {
                        newInfo.CoverImage = null;
                    }
                }
                else
                {
                    newInfo.CoverImage = null;
                }
            }

            if (newInfo.BackgroundImage != null && !testGame.BackgroundImage.IsNullOrEmpty())
            {
                var newBack = ProcessMetadataFile(newInfo.BackgroundImage, tempBackgroundFileName);
                if (newBack != null)
                {
                    var currentPath = ImageSourceManager.GetImagePath(EditingGame.BackgroundImage);
                    if (currentPath.IsNullOrEmpty() ||
                        !File.Exists(currentPath) ||
                        !FileSystem.AreFileContentsEqual(newBack, currentPath))
                    {
                        newInfo.BackgroundImage = new MetadataFile(newBack);
                        diffFields.Add(GameField.BackgroundImage);
                    }
                    else
                    {
                        newInfo.BackgroundImage = null;
                    }
                }
                else
                {
                    newInfo.BackgroundImage = null;
                }
            }

            return diffFields;
        }

        public void PreviewGameData(GameMetadata metadata)
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

        private void LoadNewMetadata(GameMetadata metadata)
        {
            if (!string.IsNullOrEmpty(metadata.GameInfo.Name))
            {
                EditingGame.Name = metadata.GameInfo.Name;
            }

            if (metadata.GameInfo.Developers?.HasNonEmptyItems() == true)
            {
                AddNewDevelopers(metadata.GameInfo.Developers);
            }

            if (metadata.GameInfo.Publishers?.HasNonEmptyItems() == true)
            {
                AddNewPublishers(metadata.GameInfo.Publishers);
            }

            if (metadata.GameInfo.Genres?.HasNonEmptyItems() == true)
            {
                AddNewGenres(metadata.GameInfo.Genres);
            }

            if (metadata.GameInfo.Tags?.HasNonEmptyItems() == true)
            {
                AddNewTags(metadata.GameInfo.Tags);
            }

            if (metadata.GameInfo.Features?.HasNonEmptyItems() == true)
            {
                AddNewFeatures(metadata.GameInfo.Features);
            }

            if (!metadata.GameInfo.AgeRatings?.HasNonEmptyItems() == true)
            {
                AddNewAgeRatings(metadata.GameInfo.AgeRatings);
            }

            if (!metadata.GameInfo.Regions?.HasNonEmptyItems() == true)
            {
                AddNewRegions(metadata.GameInfo.Regions);
            }

            if (!metadata.GameInfo.Series?.HasNonEmptyItems() == true)
            {
                AddNewSeries(metadata.GameInfo.Series);
            }

            if (!metadata.GameInfo.Platforms?.HasNonEmptyItems() == true)
            {
                AddNewPlatforms(metadata.GameInfo.Platforms);
            }

            if (metadata.GameInfo.ReleaseDate != null)
            {
                EditingGame.ReleaseDate = metadata.GameInfo.ReleaseDate;
            }

            if (!metadata.GameInfo.Description.IsNullOrEmpty())
            {
                EditingGame.Description = metadata.GameInfo.Description;
            }

            if (metadata.GameInfo.Links.HasItems())
            {
                EditingGame.Links = metadata.GameInfo.Links.ToObservable();
            }

            if (metadata.GameInfo.CriticScore != null)
            {
                EditingGame.CriticScore = metadata.GameInfo.CriticScore;
            }

            if (metadata.GameInfo.CommunityScore != null)
            {
                EditingGame.CommunityScore = metadata.GameInfo.CommunityScore;
            }

            if (metadata.GameInfo.CoverImage != null)
            {
                var newCover = ProcessMetadataFile(metadata.GameInfo.CoverImage, tempCoverFileName);
                if (newCover != null)
                {
                    EditingGame.CoverImage = newCover;
                }
            }

            if (metadata.GameInfo.Icon != null)
            {
                var newIcon = ProcessMetadataFile(metadata.GameInfo.Icon, tempIconFileName);
                if (newIcon != null)
                {
                    EditingGame.Icon = newIcon;
                }
            }

            if (metadata.GameInfo.BackgroundImage != null)
            {
                var newBackground = ProcessMetadataFile(metadata.GameInfo.BackgroundImage, tempBackgroundFileName);
                if (newBackground != null)
                {
                    EditingGame.BackgroundImage = newBackground;
                }
            }
        }

        private string ProcessMetadataFile(MetadataFile file, string tempFileName)
        {
            if (file.HasContent)
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = tempFileName + extension;
                var targetPath = Path.Combine(PlaynitePaths.TempPath, fileName);
                FileSystem.PrepareSaveFile(targetPath);
                File.WriteAllBytes(targetPath, file.Content);
                return targetPath;
            }
            else if (!file.Path.IsNullOrEmpty())
            {
                if (file.Path.IsHttpUrl())
                {
                    var extension = Path.GetExtension(new Uri(file.Path).AbsolutePath);
                    var fileName = tempFileName + extension;
                    var targetPath = Path.Combine(PlaynitePaths.TempPath, fileName);
                    var progRes = dialogs.ActivateGlobalProgress((a) =>
                        HttpDownloader.DownloadFile(file.Path, targetPath, a.CancelToken),
                        new GlobalProgressOptions("LOCDownloadingMediaLabel", true));
                    if (progRes.Result == true && !progRes.Canceled)
                    {
                        return targetPath;
                    }
                    else
                    {
                        logger.Error(progRes.Error, $"Failed to download {file.Path}.");
                        return null;
                    }
                }
                else
                {
                    return file.Path;
                }
            }
            else
            {
                return null;
            }
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
                        var gameInfo = new GameInfo
                        {
                            Name = provider.GetName(fieldArgs),
                            Genres = provider.GetGenres(fieldArgs)?.Where(a => a != null).ToList(),
                            ReleaseDate = provider.GetReleaseDate(fieldArgs),
                            Developers = provider.GetDevelopers(fieldArgs)?.Where(a => a != null).ToList(),
                            Publishers = provider.GetPublishers(fieldArgs)?.Where(a => a != null).ToList(),
                            Tags = provider.GetTags(fieldArgs)?.Where(a => a != null).ToList(),
                            Features = provider.GetFeatures(fieldArgs)?.Where(a => a != null).ToList(),
                            Description = provider.GetDescription(fieldArgs),
                            Links = provider.GetLinks(fieldArgs)?.Where(a => a != null).ToList(),
                            CriticScore = provider.GetCriticScore(fieldArgs),
                            CommunityScore = provider.GetCommunityScore(fieldArgs),
                            AgeRatings = provider.GetAgeRatings(fieldArgs)?.Where(a => a != null).ToList(),
                            Series = provider.GetSeries(fieldArgs)?.Where(a => a != null).ToList(),
                            Regions = provider.GetRegions(fieldArgs)?.Where(a => a != null).ToList(),
                            Platforms = provider.GetPlatforms(fieldArgs)?.Where(a => a != null).ToList(),
                            Icon = provider.GetIcon(fieldArgs),
                            CoverImage = provider.GetCoverImage(fieldArgs),
                            BackgroundImage = provider.GetBackgroundImage(fieldArgs)
                        };

                        var metadata = new GameMetadata(gameInfo);
                        Application.Current.Dispatcher.Invoke(() => PreviewGameData(metadata));
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
                        Application.Current.Dispatcher.Invoke(() => PreviewGameData(metadata));
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

                    return PrepareImagePath(url, tempFileName);
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
