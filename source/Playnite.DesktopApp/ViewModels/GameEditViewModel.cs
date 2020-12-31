using Playnite;
using Playnite.Database;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Common.Web;
using Playnite.Metadata;
using Playnite.SDK.Metadata;
using Playnite.Settings;
using Playnite.Plugins;
using Playnite.Common;
using System.Net;
using Playnite.Windows;
using System.Drawing.Imaging;
using Playnite.DesktopApp.Windows;
using Playnite.SDK.Plugins;
using Playnite.Metadata.Providers;
using System.Text.RegularExpressions;
using Playnite.Common.Media.Icons;
using System.Diagnostics;

namespace Playnite.DesktopApp.ViewModels
{
    public partial class GameEditViewModel : ObservableObject
    {
        public class MetadataDownloadOption
        {
            private IDialogsFactory dialogs;
            private IResourceProvider resources;
            private GameEditViewModel editModel;

            public object Downloader { get; set; }

            public string Name { get; set; }

            public RelayCommand<object> DownloadCommand
            {
                get => new RelayCommand<object>((a) =>
                {
                    if (Downloader is MetadataPlugin plugin)
                    {
                        editModel.DownloadPluginData(plugin);
                    }
                    else if (Downloader is LibraryMetadataProvider provider)
                    {
                        editModel.DownloadStoreData();
                    }
                });
            }

            public MetadataDownloadOption(GameEditViewModel model, IDialogsFactory dialogs, IResourceProvider resources)
            {
                this.dialogs = dialogs;
                this.resources = resources;
                editModel = model;
            }

            public override string ToString()
            {
                return Name ?? base.ToString();
            }
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private IPlayniteAPI playniteApi;
        private GameDatabase database;
        private ExtensionFactory extensions;
        private PlayniteSettings appSettings;

        public string IconMetadata
        {
            get => GetImageProperties(EditingGame.Icon)?.Item1;
        }

        public string CoverMetadata
        {
            get => GetImageProperties(EditingGame.CoverImage)?.Item1;
        }

        public string BackgroundMetadata
        {
            get => GetImageProperties(EditingGame.BackgroundImage)?.Item1;
        }

        public bool IsIconTooLage
        {
            get
            {
                var props = GetImageProperties(EditingGame.Icon);
                if (props != null)
                {
                    return Sizes.GetMegapixelsFromRes(props.Item2) > GameDatabase.MaximumRecommendedIconSize;
                }

                return false;
            }
        }

        public bool IsCoverTooLage
        {
            get
            {
                var props = GetImageProperties(EditingGame.CoverImage);
                if (props != null)
                {
                    return Sizes.GetMegapixelsFromRes(props.Item2) > GameDatabase.MaximumRecommendedCoverSize;
                }

                return false;
            }
        }

        public bool IsBackgroundTooLage
        {
            get
            {
                var props = GetImageProperties(EditingGame.BackgroundImage);
                if (props != null)
                {
                    return Sizes.GetMegapixelsFromRes(props.Item2) > GameDatabase.MaximumRecommendedBackgroundSize;
                }

                return false;
            }
        }

        public object IconImageObject => ImageSourceManager.GetImage(EditingGame.Icon, false, new BitmapLoadProperties(0, 256));
        public object CoverImageObject => ImageSourceManager.GetImage(EditingGame.CoverImage, false, new BitmapLoadProperties(0, 900));
        public object BackgroundImageObject => ImageSourceManager.GetImage(EditingGame.BackgroundImage, false, new BitmapLoadProperties(0, 1080));

        #region Database fields

        public SelectableDbItemList Genres { get; set; }

        public SelectableDbItemList Developers { get; set; }

        public SelectableDbItemList Publishers { get; set; }

        public SelectableDbItemList Tags { get; set; }

        public SelectableDbItemList Features { get; set; }

        public SelectableDbItemList Categories { get; set; }

        public ObservableCollection<GameSource> Sources { get; set; }

        public ObservableCollection<Region> Regions { get; set; }

        public ObservableCollection<Series> Series { get; set; }

        public ObservableCollection<AgeRating> AgeRatings { get; set; }

        public ObservableCollection<Platform> Platforms { get; set; }

        public List<Emulator> Emulators { get; set; }

        #endregion Database fields

        private Game editingGame;
        public Game EditingGame
        {
            get
            {
                return editingGame;
            }

            set
            {
                editingGame = value;
                OnPropertyChanged();
            }
        }

        public Game Game
        {
            get; set;
        }

        public IEnumerable<Game> Games
        {
            get; set;
        }

        private bool progressVisible = false;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                OnPropertyChanged();
            }
        }

        public bool ShowBackgroundUrl
        {
            get
            {
                return EditingGame?.BackgroundImage?.IsHttpUrl() == true;
            }
        }

        private bool showMetaDownload;
        public bool ShowMetaDownload
        {
            get
            {
                return showMetaDownload;
            }

            set
            {
                showMetaDownload = value;
                OnPropertyChanged();
            }
        }

        private bool showCheckBoxes = false;
        public bool ShowCheckBoxes
        {
            get
            {
                return showCheckBoxes;
            }

            set
            {
                showCheckBoxes = value;
                OnPropertyChanged();
            }
        }

        public bool IsSingleGameEdit
        {
            get;
        }

        public bool IsMultiGameEdit
        {
            get;
        }

        public bool IsOfficialMetadataAvailable => LibraryPluginMetadataDownloader != null;

        public List<MetadataDownloadOption> MetadataDownloadOptions
        {
            get; set;
        }

        public LibraryPlugin LibraryPlugin
        {
            get; set;
        }

        public LibraryMetadataProvider LibraryPluginMetadataDownloader
        {
            get; set;
        }

        public GameEditViewModel(
            Game game,
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            IPlayniteAPI playniteApi,
            PlayniteSettings appSettings)
        {
            Game = game.GetClone();
            IsSingleGameEdit = true;
            IsMultiGameEdit = false;
            EditingGame = game.GetClone();
            ShowCheckBoxes = false;
            ShowMetaDownload = true;
            Init(database, window, dialogs, resources, extensions, playniteApi, appSettings);
        }

        public GameEditViewModel(
            IEnumerable<Game> games,
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            IPlayniteAPI playniteApi,
            PlayniteSettings appSettings)
        {
            Games = games.Select(a => a.GetClone()).ToList();
            IsSingleGameEdit = false;
            IsMultiGameEdit = true;
            EditingGame = GameTools.GetMultiGameEditObject(Games);
            ShowCheckBoxes = true;
            ShowMetaDownload = false;
            Init(database, window, dialogs, resources, extensions, playniteApi, appSettings, EditingGame as MultiEditGame);
        }

        private void Init(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            IPlayniteAPI playniteApi,
            PlayniteSettings appSettings,
            MultiEditGame multiEditData = null)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.extensions = extensions;
            this.playniteApi = playniteApi;
            this.appSettings = appSettings;

            EditingGame.PropertyChanged += EditingGame_PropertyChanged;

            Genres = new SelectableDbItemList(database.Genres, EditingGame.GenreIds, multiEditData?.DistinctGenreIds);
            Genres.SelectionChanged += (s, e) => { EditingGame.GenreIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Developers = new SelectableDbItemList(database.Companies, EditingGame.DeveloperIds, multiEditData?.DistinctDeveloperIds);
            Developers.SelectionChanged += (s, e) => { EditingGame.DeveloperIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Publishers = new SelectableDbItemList(database.Companies, EditingGame.PublisherIds, multiEditData?.DistinctPublisherIds);
            Publishers.SelectionChanged += (s, e) => { EditingGame.PublisherIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Categories = new SelectableDbItemList(database.Categories, EditingGame.CategoryIds, multiEditData?.DistinctCategoryIds);
            Categories.SelectionChanged += (s, e) => { EditingGame.CategoryIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Tags = new SelectableDbItemList(database.Tags, EditingGame.TagIds, multiEditData?.DistinctTagIds);
            Tags.SelectionChanged += (s, e) => { EditingGame.TagIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Features = new SelectableDbItemList(database.Features, EditingGame.FeatureIds, multiEditData?.DistinctFeatureIds);
            Features.SelectionChanged += (s, e) => { EditingGame.FeatureIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Sources = database.Sources.OrderBy(a => a.Name).ToObservable();
            Sources.Insert(0, new GameSource() { Id = Guid.Empty, Name = string.Empty });

            Regions = database.Regions.OrderBy(a => a.Name).ToObservable();
            Regions.Insert(0, new Region() { Id = Guid.Empty, Name = string.Empty });

            Series = database.Series.OrderBy(a => a.Name).ToObservable();
            Series.Insert(0, new Series() { Id = Guid.Empty, Name = string.Empty });

            AgeRatings = database.AgeRatings.OrderBy(a => a.Name).ToObservable();
            AgeRatings.Insert(0, new AgeRating() { Id = Guid.Empty, Name = string.Empty });

            Platforms = database.Platforms.OrderBy(a => a.Name).ToObservable();
            Platforms.Insert(0, new Platform() { Id = Guid.Empty, Name = string.Empty });

            Emulators = database.Emulators.OrderBy(a => a.Name).ToList();

            if (EditingGame.Links != null)
            {
                EditingGame.Links.CollectionChanged += Links_CollectionChanged;
                foreach (var link in EditingGame.Links)
                {
                    if (link != null)
                    {
                        link.PropertyChanged += Link_PropertyChanged;
                    }
                }
            }

            if (EditingGame.OtherActions != null)
            {
                EditingGame.OtherActions.CollectionChanged += OtherActions_CollectionChanged;
                foreach (var action in EditingGame.OtherActions)
                {
                    action.PropertyChanged += OtherAction_PropertyChanged;
                }
            }

            if (EditingGame.PlayAction != null)
            {
                EditingGame.PlayAction.PropertyChanged += PlayAction_PropertyChanged;
            }

            if (IsSingleGameEdit)
            {
                MetadataDownloadOptions = new List<MetadataDownloadOption>();
                if (extensions?.MetadataPlugins != null)
                {
                    foreach (var plugin in extensions.MetadataPlugins)
                    {
                        MetadataDownloadOptions.Add(new MetadataDownloadOption(this, dialogs, resources)
                        {
                            Downloader = plugin,
                            Name = plugin.Name
                        });
                    }
                }

                MetadataDownloadOptions.Add(new MetadataDownloadOption(this, dialogs, resources)
                {
                    Downloader = new WikipediaMetadataPlugin(playniteApi),
                    Name = "Wikipedia"
                });

                LibraryPlugin = extensions?.LibraryPlugins?.FirstOrDefault(a => a.Id == Game?.PluginId);
                try
                {
                    LibraryPluginMetadataDownloader = LibraryPlugin?.GetMetadataDownloader();
                    if (LibraryPluginMetadataDownloader != null)
                    {
                        MetadataDownloadOptions.Add(new MetadataDownloadOption(this, dialogs, resources)
                        {
                            Downloader = LibraryPluginMetadataDownloader,
                            Name = resources.GetString("LOCMetaSourceStore")
                        });
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get library metadata downloader {LibraryPlugin?.GetType()}");
                }
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result = false)
        {
            CleanupTempFiles();
            try
            {
                LibraryPluginMetadataDownloader?.Dispose();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to dispose library metadata downloader {LibraryPluginMetadataDownloader.GetType()}");
            }

            window.Close(result);
        }

        public void ConfirmDialog()
        {
            List<Guid> consolidateIds(SelectableDbItemList selectionList, List<Guid> originalIds)
            {
                var selected = selectionList.GetSelectedIds();
                var indetermined = selectionList.Where(a => a.Selected == null).Select(a => a.Item.Id);
                if (indetermined.HasItems() && originalIds.HasItems())
                {
                    var fromOriginal = indetermined.Intersect(originalIds);
                    selected.AddRange(fromOriginal.ToList());
                }

                if (selected.HasItems())
                {
                    return selected;
                }
                else
                {
                    return null;
                }
            }

            if (UseNameChanges)
            {
                if (string.IsNullOrWhiteSpace(EditingGame.Name))
                {
                    dialogs.ShowMessage(
                        resources.GetString("LOCEmptyGameNameError"),
                        resources.GetString("LOCInvalidGameData"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (UseGenresChanges)
            {
                AddNewItemsToDb(Genres, EditingGame.GenreIds, database.Genres);
            }

            if (UseDeveloperChanges)
            {
                AddNewItemsToDb(Developers, EditingGame.DeveloperIds, database.Companies);
            }

            if (UsePublisherChanges)
            {
                AddNewItemsToDb(Publishers, EditingGame.PublisherIds, database.Companies);
            }

            if (UseCategoryChanges)
            {
                AddNewItemsToDb(Categories, EditingGame.CategoryIds, database.Categories);
            }

            if (UseTagChanges)
            {
                AddNewItemsToDb(Tags, EditingGame.TagIds, database.Tags);
            }

            if (UseFeatureChanges)
            {
                AddNewItemsToDb(Features, EditingGame.FeatureIds, database.Features);
            }

            if (UsePlatformChanges)
            {
                AddNewItemToDb(Platforms, EditingGame.PlatformId, database.Platforms);
            }

            if (UseSeriesChanges)
            {
                AddNewItemToDb(Series, EditingGame.SeriesId, database.Series);
            }

            if (UseAgeRatingChanges)
            {
                AddNewItemToDb(AgeRatings, EditingGame.AgeRatingId, database.AgeRatings);
            }

            if (UseRegionChanges)
            {
                AddNewItemToDb(Regions, EditingGame.RegionId, database.Regions);
            }

            if (UseSourceChanges)
            {
                AddNewItemToDb(Sources, EditingGame.SourceId, database.Sources);
            }

            var changeDate = DateTime.Now;
            var gamesToUpdate = IsMultiGameEdit ? Games : new List<Game> { Game };
            database.Games.BeginBufferUpdate();
            foreach (var game in gamesToUpdate)
            {
                if (UseNameChanges)
                {
                    game.Name = EditingGame.Name;
                }

                if (UseSortingNameChanges)
                {
                    game.SortingName = EditingGame.SortingName;
                }

                if (UseReleaseDateChanges)
                {
                    game.ReleaseDate = EditingGame.ReleaseDate;
                }

                if (UseGenresChanges)
                {
                    game.GenreIds = consolidateIds(Genres, game.GenreIds);
                }

                if (UseDeveloperChanges)
                {
                    game.DeveloperIds = consolidateIds(Developers, game.DeveloperIds);
                }

                if (UsePublisherChanges)
                {
                    game.PublisherIds = consolidateIds(Publishers, game.PublisherIds);
                }

                if (UseCategoryChanges)
                {
                    game.CategoryIds = consolidateIds(Categories, game.CategoryIds);
                }

                if (UseTagChanges)
                {
                    game.TagIds = consolidateIds(Tags, game.TagIds);
                }

                if (UseFeatureChanges)
                {
                    game.FeatureIds = consolidateIds(Features, game.FeatureIds);
                }

                if (UseDescriptionChanges)
                {
                    game.Description = EditingGame.Description;
                }

                if (UseNotesChanges)
                {
                    game.Notes = EditingGame.Notes;
                }

                if (UseManualChanges)
                {
                    game.Manual = EditingGame.Manual;
                }

                if (UseInstallDirChanges)
                {
                    game.InstallDirectory = EditingGame.InstallDirectory;
                }

                if (UseIsoPathChanges)
                {
                    game.GameImagePath = EditingGame.GameImagePath;
                }

                if (UseInstallStateChanges)
                {
                    game.IsInstalled = EditingGame.IsInstalled;
                }

                if (UsePlatformChanges)
                {
                    game.PlatformId = EditingGame.PlatformId;
                }

                if (UseLastActivityChanges)
                {
                    game.LastActivity = EditingGame.LastActivity;
                }

                if (UsePlaytimeChanges)
                {
                    game.Playtime = EditingGame.Playtime;
                }

                if (UseAddedChanges)
                {
                    game.Added = EditingGame.Added;
                }

                if (UsePlayCountChanges)
                {
                    game.PlayCount = EditingGame.PlayCount;
                }

                if (UseSeriesChanges)
                {
                    game.SeriesId = EditingGame.SeriesId;
                }

                if (UseVersionChanges)
                {
                    game.Version = EditingGame.Version;
                }

                if (UseAgeRatingChanges)
                {
                    game.AgeRatingId = EditingGame.AgeRatingId;
                }

                if (UseRegionChanges)
                {
                    game.RegionId = EditingGame.RegionId;
                }

                if (UseSourceChanges)
                {
                    game.SourceId = EditingGame.SourceId;
                }

                if (UseCompletionStatusChanges)
                {
                    game.CompletionStatus = EditingGame.CompletionStatus;
                }

                if (UseUserScoreChanges)
                {
                    game.UserScore = EditingGame.UserScore;
                }

                if (UseCriticScoreChanges)
                {
                    game.CriticScore = EditingGame.CriticScore;
                }

                if (UseCommunityScoreChanges)
                {
                    game.CommunityScore = EditingGame.CommunityScore;
                }

                if (UseFavoriteChanges)
                {
                    game.Favorite = EditingGame.Favorite;
                }

                if (UseHiddenChanges)
                {
                    game.Hidden = EditingGame.Hidden;
                }

                if (UseScriptRuntimeChanges)
                {
                    game.ActionsScriptLanguage = EditingGame.ActionsScriptLanguage;
                }

                if (UsePreScriptChanges)
                {
                    game.PreScript = EditingGame.PreScript;
                }

                if (UsePostScriptChanges)
                {
                    game.PostScript = EditingGame.PostScript;
                }

                if (UsePreGlobalScriptChanges)
                {
                    game.UseGlobalPreScript = EditingGame.UseGlobalPreScript;
                }

                if (UsePostGlobalScriptChanges)
                {
                    game.UseGlobalPostScript = EditingGame.UseGlobalPostScript;
                }

                if (UseGameStartedScriptChanges)
                {
                    game.GameStartedScript = EditingGame.GameStartedScript;
                }

                if (UseGameStartedGlobalScriptChanges)
                {
                    game.UseGlobalGameStartedScript = EditingGame.UseGlobalGameStartedScript;
                }

                if (UsePlayActionChanges)
                {
                    game.PlayAction = EditingGame.PlayAction;
                }

                if (UseOtherActionsChanges)
                {
                    game.OtherActions = EditingGame.OtherActions;
                }

                if (UseLinksChanges)
                {
                    game.Links = EditingGame.Links;
                }

                if (UseIconChanges)
                {
                    if (EditingGame.Icon.IsNullOrEmpty())
                    {
                        game.Icon = null;
                    }
                    else if (File.Exists(EditingGame.Icon))
                    {
                        game.Icon = database.AddFile(EditingGame.Icon, game.Id);
                    }
                }

                if (UseImageChanges)
                {
                    if (EditingGame.CoverImage.IsNullOrEmpty())
                    {
                        game.CoverImage = null;
                    }
                    else if (File.Exists(EditingGame.CoverImage))
                    {
                        game.CoverImage = database.AddFile(EditingGame.CoverImage, game.Id);
                    }
                }

                if (UseBackgroundChanges)
                {
                    if (EditingGame.BackgroundImage.IsNullOrEmpty())
                    {
                        game.BackgroundImage = null;
                    }
                    else if (EditingGame.BackgroundImage.IsHttpUrl())
                    {
                        game.BackgroundImage = EditingGame.BackgroundImage;
                    }
                    else if (File.Exists(EditingGame.BackgroundImage))
                    {
                        game.BackgroundImage = database.AddFile(EditingGame.BackgroundImage, game.Id);
                    }
                }

                game.Modified = changeDate;
                database.Games.Update(game);
            }

            database.Games.EndBufferUpdate();
            CloseView(true);
        }

        internal void CleanupTempFiles()
        {
            try
            {
                foreach (var icon in Directory.GetFiles(PlaynitePaths.TempPath, tempIconFileName + ".*"))
                {
                    File.Delete(icon);
                }

                foreach (var cover in Directory.GetFiles(PlaynitePaths.TempPath, tempCoverFileName + ".*"))
                {
                    File.Delete(cover);
                }

                foreach (var bk in Directory.GetFiles(PlaynitePaths.TempPath, tempBackgroundFileName + ".*"))
                {
                    File.Delete(bk);
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to cleanup temporary files.");
            }
        }

        private string SaveFileIconToTemp(string exePath)
        {
            var tempPath = Path.Combine(PlaynitePaths.TempPath, "tempico.ico");
            FileSystem.PrepareSaveFile(tempPath);
            if (IconExtractor.ExtractMainIconFromFile(exePath, tempPath))
            {
                return tempPath;
            }
            else
            {
                return string.Empty;
            }
        }

        private string SaveConvertedTgaToTemp(string tgaPath)
        {
            var tempPath = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + ".png");
            FileSystem.PrepareSaveFile(tempPath);
            File.WriteAllBytes(tempPath, BitmapExtensions.TgaToBitmap(tgaPath).ToPngArray());
            return tempPath;
        }

        public void UseExeIcon()
        {
            if (EditingGame.PlayAction == null || EditingGame.PlayAction.Type == GameActionType.URL)
            {
                dialogs.ShowMessage(resources.GetString("LOCExecIconMissingPlayAction"));
                return;
            }

            var path = Game.GetRawExecutablePath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                logger.Error($"Can't find executable for icon extraction, file {path}");
                return;
            }

            var icon = SaveFileIconToTemp(path);
            if (string.IsNullOrEmpty(icon))
            {
                return;
            }

            EditingGame.Icon = icon;
        }

        public string PrepareImagePath(string path, string tempFileName)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (path.IsHttpUrl())
                {
                    path = ProcessMetadataFile(new MetadataFile(path), tempFileName);
                }

                if (!path.IsNullOrEmpty() && path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    path = SaveConvertedTgaToTemp(path);
                }

                return path;
            }

            return null;
        }

        public string GetDroppedImage(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (files?.Length == 1)
                {
                    var path = files[0];
                    if (File.Exists(path) && new List<string> { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tga", ".exe" }.Contains(Path.GetExtension(path).ToLower()))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        public void DropIcon(DragEventArgs args)
        {
            var path = PrepareImagePath(GetDroppedImage(args), tempIconFileName);
            if (!path.IsNullOrEmpty())
            {
                if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    path = SaveFileIconToTemp(path);
                    if (string.IsNullOrEmpty(path))
                    {
                        return;
                    }
                }

                EditingGame.Icon = path;
                CheckImagePerformanceRestrains(path, 512);
            }
        }

        public void SelectIcon()
        {
            var path = dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    path = SaveFileIconToTemp(path);
                    if (string.IsNullOrEmpty(path))
                    {
                        return;
                    }
                }
                else if (path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    path = SaveConvertedTgaToTemp(path);
                }

                EditingGame.Icon = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedIconSize);
            }
        }

        public void SelectCover()
        {
            var path = PrepareImagePath(dialogs.SelectImagefile(), tempCoverFileName);
            if (path != null)
            {
                EditingGame.CoverImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedCoverSize);
            }
        }

        public void DropCover(DragEventArgs args)
        {
            var path = PrepareImagePath(GetDroppedImage(args), tempCoverFileName);
            if (!path.IsNullOrEmpty())
            {
                EditingGame.CoverImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedCoverSize);
            }
        }

        public void SelectBackground()
        {
            var path = PrepareImagePath(dialogs.SelectImagefile(), tempBackgroundFileName);
            if (!path.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = path;
            }
        }

        public void DropBackground(DragEventArgs args)
        {
            var path = PrepareImagePath(GetDroppedImage(args), tempBackgroundFileName);
            if (!path.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = path;
            }
        }

        public void SetBackgroundUrl()
        {
            var image = SelectUrlImage(tempBackgroundFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = image;
            }
        }

        public void SetIconUrl()
        {
            var image = SelectUrlImage(tempIconFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.Icon = image;
            }
        }

        public void SetCoverUrl()
        {
            var image = SelectUrlImage(tempCoverFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.CoverImage = image;
            }
        }

        public string SelectUrlImage(string tempFileName)
        {
            var url = dialogs.SelectString(
                resources.GetString("LOCURLInputInfo"),
                resources.GetString("LOCURLInputInfoTitile"),
                string.Empty);
            if (url.Result && !url.SelectedString.IsNullOrEmpty())
            {
                try
                {
                    return PrepareImagePath(url.SelectedString, tempFileName);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to download image from url {url.SelectedString}");
                }
            }

            return null;
        }

        public void AddPlayAction()
        {
            if (EditingGame.PlayAction != null)
            {
                EditingGame.PlayAction.PropertyChanged -= PlayAction_PropertyChanged;
            }

            EditingGame.PlayAction = new GameAction()
            {
                Name = "Play",
                IsHandledByPlugin = false
            };

            EditingGame.PlayAction.PropertyChanged += PlayAction_PropertyChanged;
        }

        public void RemovePlayAction()
        {
            if (EditingGame.PlayAction != null)
            {
                EditingGame.PlayAction.PropertyChanged -= PlayAction_PropertyChanged;
            }

            EditingGame.PlayAction = null;
        }

        public void AddAction()
        {
            if (EditingGame.OtherActions == null)
            {
                EditingGame.OtherActions = new ObservableCollection<GameAction>();
                EditingGame.OtherActions.CollectionChanged += OtherActions_CollectionChanged;
            }

            var newAction = new GameAction()
            {
                Name = "New Action",
                IsHandledByPlugin = false
            };

            newAction.PropertyChanged += OtherAction_PropertyChanged;
            if (EditingGame.PlayAction != null && EditingGame.PlayAction.Type == GameActionType.File)
            {
                newAction.WorkingDir = EditingGame.PlayAction.WorkingDir;
                newAction.Path = EditingGame.PlayAction.Path;
            }

            EditingGame.OtherActions.Add(newAction);
        }

        public void RemoveAction(GameAction action)
        {
            action.PropertyChanged -= OtherAction_PropertyChanged;
            EditingGame.OtherActions.Remove(action);
        }

        public void MoveActionUp(GameAction action)
        {
            var index = EditingGame.OtherActions.IndexOf(action);
            if (index != 0)
            {
                EditingGame.OtherActions.Move(index, index - 1);
            }
        }

        public void MoveActionDown(GameAction action)
        {
            var index = EditingGame.OtherActions.IndexOf(action);
            if (index != EditingGame.OtherActions.Count - 1)
            {
                EditingGame.OtherActions.Move(index, index + 1);
            }
        }

        private void Link_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UseLinksChanges = true;
        }

        private void Links_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UseLinksChanges = true;
        }

        public void AddLink()
        {
            if (EditingGame.Links == null)
            {
                EditingGame.Links = new ObservableCollection<Link>();
                EditingGame.Links.CollectionChanged += Links_CollectionChanged;
            }

            var newLink = new Link("NewLink", "NewUrl");
            newLink.PropertyChanged += Link_PropertyChanged;
            EditingGame.Links.Add(newLink);
        }

        public void RemoveLink(Link link)
        {
            link.PropertyChanged -= Link_PropertyChanged;
            EditingGame.Links.Remove(link);
        }

        public void MoveLinkUp(Link link)
        {
            var index = EditingGame.Links.IndexOf(link);
            if (index != 0)
            {
                EditingGame.Links.Move(index, index - 1);
            }
        }

        public void MoveLinkDown(Link link)
        {
            var index = EditingGame.Links.IndexOf(link);
            if (index != EditingGame.Links.Count - 1)
            {
                EditingGame.Links.Move(index, index + 1);
            }
        }

        public void SelectInstallDir()
        {
            var path = dialogs.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.InstallDirectory = path;
            }
        }

        public void SelectGameImage()
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.GameImagePath = path;
            }
        }

        public void RemoveIcon()
        {
            EditingGame.Icon = null;
        }

        public void RemoveImage()
        {
            EditingGame.CoverImage = null;
        }

        public void RemoveBackground()
        {
            EditingGame.BackgroundImage = null;
        }

        public void OpenMetadataFolder()
        {
            if (appSettings.DirectoryOpenCommand.IsNullOrWhiteSpace())
            {
                Process.Start(database.GetFileStoragePath(EditingGame.Id));
            }
            else
            {
                try
                {
                    ProcessStarter.ShellExecute(appSettings.DirectoryOpenCommand.Replace("{Dir}", database.GetFileStoragePath(EditingGame.Id)));
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to open directory using custom command.");
                    Process.Start(database.GetFileStoragePath(EditingGame.Id));
                }
            }
        }

        public void AddNewItemsToDb<TItem>(SelectableDbItemList sourceList, List<Guid> itemsToAdd, IItemCollection<TItem> targetCollection) where TItem : DatabaseObject
        {
            if (itemsToAdd?.Any() != true)
            {
                return;
            }

            var addedItems = sourceList.Where(a => itemsToAdd.Contains(a.Item.Id) == true && targetCollection[a.Item.Id] == null).ToList();
            if (addedItems.Any())
            {
                targetCollection.Add(addedItems.Select(a => (TItem)a.Item));
            }
        }

        public void AddNewItemToDb<TItem>(ObservableCollection<TItem> sourceList, Guid itemToAdd, IItemCollection<TItem> targetCollection) where TItem : DatabaseObject
        {
            if (itemToAdd == Guid.Empty || itemToAdd == null)
            {
                return;
            }

            var addedItem = sourceList.FirstOrDefault(a => a.Id == itemToAdd && targetCollection[a.Id] == null);
            if (addedItem != null)
            {
                targetCollection.Add(addedItem);
            }
        }

        public TItem CreateNewItem<TItem>(string itemName = null) where TItem : DatabaseObject
        {
            if (itemName.IsNullOrEmpty())
            {
                var res = dialogs.SelectString(
                    resources.GetString("LOCEnterName"),
                    resources.GetString("LOCAddNewItem"),
                    "");
                if (res.Result)
                {
                    return typeof(TItem).CrateInstance<TItem>(res.SelectedString);
                }
            }
            else
            {
                return typeof(TItem).CrateInstance<TItem>(itemName);
            }

            return null;
        }

        public TItem CreateNewItemInCollection<TItem>(ObservableCollection<TItem> collection, string itemName = null) where TItem : DatabaseObject
        {
            var newItem = CreateNewItem<TItem>(itemName);
            if (newItem == null)
            {
                return null;
            }
            else
            {
                var existing = collection.FirstOrDefault(a => a.Name.Equals(newItem.Name, StringComparison.InvariantCultureIgnoreCase));
                if (existing != null)
                {
                    return existing;
                }
                else
                {
                    collection.Add(newItem);
                    return newItem;
                }
            }
        }

        public TItem CreateNewItemInCollection<TItem>(
            SelectableDbItemList collection,
            string itemName = null,
            Func<SelectableItem<DatabaseObject>, string, bool> existingComparer = null) where TItem : DatabaseObject
        {
            var newItem = CreateNewItem<TItem>(itemName);
            if (newItem == null)
            {
                return null;
            }
            else
            {
                SelectableItem<DatabaseObject> existing = null;
                if (existingComparer != null)
                {
                    existing = collection.FirstOrDefault(a => existingComparer(a, newItem.Name));
                }
                else
                {
                    existing = collection.FirstOrDefault(a => a.Item.Name?.Equals(newItem.Name, StringComparison.InvariantCultureIgnoreCase) == true);
                }

                if (existing != null)
                {
                    existing.Selected = true;
                    return (TItem)existing.Item;
                }
                else
                {
                    collection.Add(newItem, true);
                    return newItem;
                }
            }
        }

        public void AddNewPlatform(string platform = null)
        {
            EditingGame.PlatformId = CreateNewItemInCollection(Platforms, platform)?.Id ?? EditingGame.PlatformId;
        }

        public void AddNewSeries(string series = null)
        {
            EditingGame.SeriesId = CreateNewItemInCollection(Series, series)?.Id ?? EditingGame.SeriesId;
        }

        public void AddNewAreRating(string ageRating = null)
        {
            EditingGame.AgeRatingId = CreateNewItemInCollection(AgeRatings, ageRating)?.Id ?? EditingGame.AgeRatingId;
        }

        public void AddNewRegion(string region = null)
        {
            EditingGame.RegionId = CreateNewItemInCollection(Regions, region)?.Id ?? EditingGame.RegionId;
        }

        public void AddNewSource(string source = null)
        {
            EditingGame.SourceId = CreateNewItemInCollection(Sources, source)?.Id ?? EditingGame.SourceId;
        }

        public void AddNewCategory()
        {
            CreateNewItemInCollection<Category>(Categories);
        }

        public Company AddNewPublisher(string publisher = null)
        {
            var newItem = CreateNewItemInCollection<Company>(Publishers, publisher);
            if (newItem != null)
            {
                if (!Developers.Any(a => a.Item.Name.Equals(newItem.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Developers.Add(newItem);
                }
            }

            return newItem;
        }

        public void AddNewPublishers(List<string> publishers)
        {
            var added = new List<Company>();
            publishers?.ForEach(a => added.Add(AddNewPublisher(a)));
            if (added.Any())
            {
                Publishers.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        public Company AddNewDeveloper(string developer = null)
        {
            var newItem = CreateNewItemInCollection<Company>(Developers, developer);
            if (newItem != null)
            {
                if (!Publishers.Any(a => a.Item.Name.Equals(newItem.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Publishers.Add(newItem);
                }
            }

            return newItem;
        }

        public void AddNewDevelopers(List<string> developers)
        {
            var added = new List<Company>();
            developers?.ForEach(a => added.Add(AddNewDeveloper(a)));
            if (added.Any())
            {
                Developers.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        public Genre AddNewGenre(string genre = null)
        {
            return CreateNewItemInCollection<Genre>(Genres, genre, LooseDbNameComparer);
        }

        public void AddNewGenres(List<string> genres)
        {
            var added = new List<Genre>();
            genres?.ForEach(a => added.Add(AddNewGenre(a)));
            if (added.Any())
            {
                Genres.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        public Tag AddNewTag(string tag = null)
        {
            return CreateNewItemInCollection<Tag>(Tags, tag, LooseDbNameComparer);
        }

        public void AddNewTags(List<string> tags)
        {
            var added = new List<Tag>();
            tags?.ForEach(a => added.Add(AddNewTag(a)));
            if (added.Any())
            {
                Tags.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        public GameFeature AddNewFeature(string feature = null)
        {
            return CreateNewItemInCollection<GameFeature>(Features, feature, LooseDbNameComparer);
        }

        public void AddNewFeatures(List<string> features)
        {
            var added = new List<GameFeature>();
            features?.ForEach(a => added.Add(AddNewFeature(a)));
            if (added.Any())
            {
                Features.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        private bool LooseDbNameComparer(SelectableItem<DatabaseObject> existingItem, string newName)
        {
            return GameFieldComparer.StringEquals(existingItem.Item.Name, newName);
        }

        private Tuple<string, ImageProperties> GetImageProperties(string image)
        {
            try
            {
                var imagePath = ImageSourceManager.GetImagePath(image);
                if (!imagePath.IsNullOrEmpty())
                {
                    var props = Images.GetImageProperties(imagePath);
                    return new Tuple<string, ImageProperties>($"{props?.Width}x{props.Height}px", props);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to get metadata from image  {image}");
                return null;
            }
        }

        private void CheckImagePerformanceRestrains(string imagePath, double maxMegapixels)
        {
            if (!appSettings.ShowImagePerformanceWarning)
            {
                return;
            }

            if (imagePath != null)
            {
                var imageProps = Images.GetImageProperties(imagePath);
                if (imageProps != null && Sizes.GetMegapixelsFromRes(imageProps) > maxMegapixels)
                {
                    var okResponse = new MessageBoxOption("LOCOKLabel", true, true);
                    var dontShowResponse = new MessageBoxOption("LOCDontShowAgainTitle");
                    var ask = new MessageBoxWindow();
                    var result = ask.ShowCustom(
                        window.Window,
                        string.Format(resources.GetString("LOCGameImageSizeWarning"),
                        GameDatabase.MaximumRecommendedIconSize,
                        GameDatabase.MaximumRecommendedCoverSize,
                        GameDatabase.MaximumRecommendedBackgroundSize),
                        resources.GetString("LOCPerformanceWarningTitle"),
                        MessageBoxImage.Warning,
                        new List<MessageBoxOption> { okResponse, dontShowResponse });
                    if (result == dontShowResponse)
                    {
                        appSettings.ShowImagePerformanceWarning = false;
                    }
                }
            }
        }
    }
}
