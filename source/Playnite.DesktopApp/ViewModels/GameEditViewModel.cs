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
using Playnite.Settings;
using Playnite.Plugins;
using Playnite.Common;
using System.Net;
using Playnite.Windows;
using System.Drawing.Imaging;
using Playnite.DesktopApp.Windows;
using Playnite.SDK.Plugins;
using System.Text.RegularExpressions;
using Playnite.Common.Media.Icons;
using System.Diagnostics;
using Playnite.SDK.Exceptions;
using Playnite.Scripting.PowerShell;
using System.Threading;

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

            public RelayCommand DownloadCommand
            {
                get => new RelayCommand(() =>
                {
                    if (Downloader == null)
                    {
                        return;
                    }

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
        private GameDatabase database;
        private ExtensionFactory extensions;
        private PlayniteSettings appSettings;
        private bool ignoreClosingEvent = false;
        private readonly MultiEditGame originalMultiGameObj;

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

        public SelectableDbItemList Regions { get; set; }

        public SelectableDbItemList Series { get; set; }

        public SelectableDbItemList AgeRatings { get; set; }

        public SelectableDbItemList Platforms { get; set; }

        public List<Emulator> Emulators { get; set; }

        public ObservableCollection<CompletionStatus> CompletionStatuses { get; set; }

        #endregion Database fields

        public bool IsHdrSupported => HdrUtilities.IsHdrSupported();

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

        public bool ShowIncludeLibraryPluginAction
        {
            get => IsMultiGameEdit || !EditingGame.IsCustomGame;
        }

        public bool ShowOverrideInstallStateOption
        {
            get => IsMultiGameEdit || !EditingGame.IsCustomGame;
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
            PlayniteSettings appSettings)
        {
            Game = game.GetClone();
            IsSingleGameEdit = true;
            IsMultiGameEdit = false;
            EditingGame = game.GetClone();
            ShowCheckBoxes = false;
            ShowMetaDownload = true;
            Init(database, window, dialogs, resources, extensions, appSettings);
        }

        public GameEditViewModel(
            IEnumerable<Game> games,
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            PlayniteSettings appSettings)
        {
            Games = games.Select(a => a.GetClone()).ToList();
            IsSingleGameEdit = false;
            IsMultiGameEdit = true;
            EditingGame = GameTools.GetMultiGameEditObject(Games);
            originalMultiGameObj = EditingGame.GetClone<Game, MultiEditGame>();
            ShowCheckBoxes = true;
            ShowMetaDownload = false;
            Init(database, window, dialogs, resources, extensions, appSettings, EditingGame as MultiEditGame);
        }

        private void Init(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            PlayniteSettings appSettings,
            MultiEditGame multiEditData = null)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.extensions = extensions;
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

            Platforms = new SelectableDbItemList(database.Platforms, EditingGame.PlatformIds, multiEditData?.DistinctPlatformIds);
            Platforms.SelectionChanged += (s, e) => { EditingGame.PlatformIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Series = new SelectableDbItemList(database.Series, EditingGame.SeriesIds, multiEditData?.DistinctSeriesIds);
            Series.SelectionChanged += (s, e) => { EditingGame.SeriesIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            AgeRatings = new SelectableDbItemList(database.AgeRatings, EditingGame.AgeRatingIds, multiEditData?.DistinctAgeRatingIds);
            AgeRatings.SelectionChanged += (s, e) => { EditingGame.AgeRatingIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Regions = new SelectableDbItemList(database.Regions, EditingGame.RegionIds, multiEditData?.DistinctRegionIds);
            Regions.SelectionChanged += (s, e) => { EditingGame.RegionIds = ((SelectableDbItemList)s).GetSelectedIds(); };

            Sources = database.Sources.OrderBy(a => a.Name).ToObservable();
            Sources.Insert(0, new GameSource() { Id = Guid.Empty, Name = string.Empty });

            CompletionStatuses = database.CompletionStatuses.OrderBy(a => a.Name).ToObservable();
            CompletionStatuses.Insert(0, new CompletionStatus() { Id = Guid.Empty, Name = string.Empty });

            Emulators = database.Emulators.OrderBy(a => a.Name).ToList();
            Emulators.Insert(0, new Emulator(resources.GetString(LOC.EmulatorSelectOnStart)) { Id = Guid.Empty });

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

            if (EditingGame.Roms != null)
            {
                EditingGame.Roms.CollectionChanged += Roms_CollectionChanged;
                foreach (var rom in EditingGame.Roms)
                {
                    if (rom != null)
                    {
                        rom.PropertyChanged += Rom_PropertyChanged;
                    }
                }
            }

            if (EditingGame.GameActions != null)
            {
                EditingGame.GameActions.CollectionChanged += OtherActions_CollectionChanged;
                foreach (var action in EditingGame.GameActions)
                {
                    action.PropertyChanged += GameAction_PropertyChanged;
                }
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

                if (!MetadataDownloadOptions.HasItems())
                {
                    MetadataDownloadOptions.Add(new MetadataDownloadOption(this, dialogs, resources)
                    {
                        Name = LOC.NoMetadataSource.GetLocalized()
                    });
                }
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        // Required for cases where a window is closed using ALT-F4 or via X button
        private void WindowClosing(CancelEventArgs e)
        {
            if (ignoreClosingEvent)
            {
                return;
            }

            var res = CheckUnsavedChanges();
            if (res == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            else if (res == MessageBoxResult.Yes)
            {
                ConfirmDialog(true);
                return;
            }
        }

        private MessageBoxResult CheckUnsavedChanges()
        {
            var compareObj = IsMultiGameEdit ? originalMultiGameObj : Game;
            if (!EditingGame.IsEqualJson(compareObj))
            {
                return dialogs.ShowMessage(LOC.UnsavedChangesAskMessage, "", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            }

            return MessageBoxResult.None;
        }

        public void CancelDialog()
        {
            var res = CheckUnsavedChanges();
            if (res == MessageBoxResult.Cancel)
            {
                return;
            }
            else if (res == MessageBoxResult.Yes)
            {
                ConfirmDialog(false);
                return;
            }

            ignoreClosingEvent = true;
            CloseView(false, false);
        }

        public void CloseView(bool result, bool alreadyClosing)
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

            if (!alreadyClosing)
            {
                window.Close(result);
            }
        }

        public void ConfirmDialog(bool alreadyClosing)
        {
            try
            {
                // This absolutely sucks, but it has to be done to fix issues like #3013.
                // The only other solution would be changing binding update trigger on ALL elements to PropertyChanged
                // which is probbaly even worse.
                // Basically, when a window is closed via default command action on Enter, current editing control
                // doesn't loose focus before closing a window (specifically before executing default command)
                // and therefore binding for that control is not updated.
                if (window?.Window != null)
                {
                    System.Windows.Input.FocusManager.SetFocusedElement(System.Windows.Input.FocusManager.GetFocusScope(window.Window), null);
                    System.Windows.Input.Keyboard.ClearFocus();
                }

                // What sucks even more is that this can't be handled generally in a view via something like OnClosing event,
                // because these events are executed after default command is executed.
                // This is therefore an issue on other views as well, not just game edit window.
                // TODO: Implement custom handling for default commands and solve this somehow globally.
            }
            catch
            {
                // This can obviously fail in some cases like when running via unit test runner.
            }

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

            if (EditingGame.GameActions.HasItems())
            {
                foreach (var action in EditingGame.GameActions)
                {
                    if (action.TrackingMode == TrackingMode.Directory && action.TrackingPath.IsNullOrWhiteSpace())
                    {
                        dialogs.ShowErrorMessage(
                            resources.GetString(LOC.EmptyTrackingFolderError),
                            resources.GetString(LOC.InvalidGameData));
                        return;
                    }
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
                AddNewItemsToDb(Platforms, EditingGame.PlatformIds, database.Platforms);
            }

            if (UseSeriesChanges)
            {
                AddNewItemsToDb(Series, EditingGame.SeriesIds, database.Series);
            }

            if (UseAgeRatingChanges)
            {
                AddNewItemsToDb(AgeRatings, EditingGame.AgeRatingIds, database.AgeRatings);
            }

            if (UseRegionChanges)
            {
                AddNewItemsToDb(Regions, EditingGame.RegionIds, database.Regions);
            }

            if (UseSourceChanges)
            {
                AddNewItemToDb(Sources, EditingGame.SourceId, database.Sources);
            }

            if (UseCompletionStatusChanges)
            {
                AddNewItemToDb(CompletionStatuses, EditingGame.CompletionStatusId, database.CompletionStatuses);
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

                if (UseInstallStateChanges)
                {
                    game.IsInstalled = EditingGame.IsInstalled;
                }

                if (UsePlatformChanges)
                {
                    game.PlatformIds = consolidateIds(Platforms, game.PlatformIds);
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

                if (UseInstallSizeChanges)
                {
                    if (game.InstallSize != EditingGame.InstallSize)
                    {
                        game.LastSizeScanDate = DateTime.Now;
                    }

                    game.InstallSize = EditingGame.InstallSize;
                }

                if (UseSeriesChanges)
                {
                    game.SeriesIds = consolidateIds(Series, game.SeriesIds);
                }

                if (UseVersionChanges)
                {
                    game.Version = EditingGame.Version;
                }

                if (UseAgeRatingChanges)
                {
                    game.AgeRatingIds = consolidateIds(AgeRatings, game.AgeRatingIds);
                }

                if (UseRegionChanges)
                {
                    game.RegionIds = consolidateIds(Regions, game.RegionIds);
                }

                if (UseSourceChanges)
                {
                    game.SourceId = EditingGame.SourceId;
                }

                if (UseCompletionStatusChanges)
                {
                    game.CompletionStatusId = EditingGame.CompletionStatusId;
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

                if (UseHdrChanges)
                {
                    game.EnableSystemHdr = EditingGame.EnableSystemHdr;
                }

                if (UseHiddenChanges)
                {
                    game.Hidden = EditingGame.Hidden;
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

                if (UseGameActionsChanges)
                {
                    // Get clone here, because original collection has some WPF bound collection view source that causes #2443
                    // This happens even after unloading game edit view, which makes no sense.
                    game.GameActions = EditingGame.GameActions?.GetClone();
                }

                if (UseLinksChanges)
                {
                    game.Links = EditingGame.Links?.GetClone();
                }

                if (UseRomsChanges)
                {
                    game.Roms = EditingGame.Roms?.GetClone();
                }

                if (UseIconChanges)
                {
                    if (EditingGame.Icon.IsNullOrEmpty())
                    {
                        game.Icon = null;
                    }
                    else if (File.Exists(EditingGame.Icon))
                    {
                        game.Icon = database.AddFile(EditingGame.Icon, game.Id, true, CancellationToken.None);
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
                        game.CoverImage = database.AddFile(EditingGame.CoverImage, game.Id, true, CancellationToken.None);
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
                        game.BackgroundImage = database.AddFile(EditingGame.BackgroundImage, game.Id, true, CancellationToken.None);
                    }
                }

                if (UseIncludeLibraryPluginAction)
                {
                    game.IncludeLibraryPluginAction = EditingGame.IncludeLibraryPluginAction;
                }

                if (UseOverrideInstallState)
                {
                    game.OverrideInstallState = EditingGame.OverrideInstallState;
                }

                game.Modified = changeDate;
                database.Games.Update(game);
            }

            database.Games.EndBufferUpdate();
            ignoreClosingEvent = true;
            CloseView(true, alreadyClosing);
        }

        internal void CleanupTempFiles()
        {
            try
            {
                if (!Directory.Exists(PlaynitePaths.TempPath))
                {
                    return;
                }

                foreach (var icon in Directory.GetFiles(PlaynitePaths.TempPath, tempEditingIconFileName + ".*"))
                {
                    File.Delete(icon);
                }

                foreach (var icon in Directory.GetFiles(PlaynitePaths.TempPath, tempDownloadIconFileName + ".*"))
                {
                    File.Delete(icon);
                }

                foreach (var cover in Directory.GetFiles(PlaynitePaths.TempPath, tempEditingCoverFileName + ".*"))
                {
                    File.Delete(cover);
                }

                foreach (var cover in Directory.GetFiles(PlaynitePaths.TempPath, tempDownloadCoverFileName + ".*"))
                {
                    File.Delete(cover);
                }

                foreach (var bk in Directory.GetFiles(PlaynitePaths.TempPath, tempEditingBackgroundFileName + ".*"))
                {
                    File.Delete(bk);
                }

                foreach (var bk in Directory.GetFiles(PlaynitePaths.TempPath, tempDownloadBackgroundFileName + ".*"))
                {
                    File.Delete(bk);
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to cleanup temporary files.");
            }
        }

        public string GetDroppedImage(DragEventArgs args, List<string> compatibleExtensions)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (files?.Length == 1)
                {
                    var path = files[0];
                    var imageExtension = Path.GetExtension(path).ToLower();
                    if (!compatibleExtensions.Contains(imageExtension))
                    {
                        dialogs.ShowErrorMessage(string.Format(
                            resources.GetString("LOCIncompatibleDragAndDropExtensionError"), imageExtension),
                            resources.GetString("LOCIncompatibleDragAndDropExtensionErrorTitle"));
                    }
                    else if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        public void DropIcon(DragEventArgs args)
        {
            var compatibleExtensions = new List<string> { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".ico", ".tga", ".exe", ".tif", ".webp" };
            var path = ProcessMetadataFile(GetDroppedImage(args, compatibleExtensions), tempEditingIconFileName);
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.Icon = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedIconSize);
            }
        }

        public void SelectIcon()
        {
            var path = ProcessMetadataFile(dialogs.SelectIconFile(), tempEditingIconFileName);
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.Icon = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedIconSize);
            }
        }

        public void SelectCover()
        {
            var path = ProcessMetadataFile(dialogs.SelectImagefile(), tempEditingCoverFileName);
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.CoverImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedCoverSize);
            }
        }

        public void DropCover(DragEventArgs args)
        {
            var compatibleExtensions = new List<string> { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tga", ".tif", ".webp" };
            var path = ProcessMetadataFile(GetDroppedImage(args, compatibleExtensions), tempEditingCoverFileName);
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.CoverImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedCoverSize);
            }
        }

        public void SelectBackground()
        {
            var path = ProcessMetadataFile(dialogs.SelectImagefile(), tempEditingBackgroundFileName);
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.BackgroundImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedBackgroundSize);
            }
        }

        public void DropBackground(DragEventArgs args)
        {
            var compatibleExtensions = new List<string> { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tga", ".tif", ".webp" };
            var path = ProcessMetadataFile(GetDroppedImage(args, compatibleExtensions), tempEditingBackgroundFileName);
            if (!string.IsNullOrEmpty(path))
            {
                EditingGame.BackgroundImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedBackgroundSize);
            }
        }

        public void SetBackgroundUrl()
        {
            var image = SelectUrlImage(tempEditingBackgroundFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = image;
            }
        }

        public void SetIconUrl()
        {
            var image = SelectUrlImage(tempEditingIconFileName);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.Icon = image;
            }
        }

        public void SetCoverUrl()
        {
            var image = SelectUrlImage(tempEditingCoverFileName);
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
                    return ProcessMetadataFile(url.SelectedString, tempFileName);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to download image from url {url.SelectedString}");
                }
            }

            return null;
        }

        public void AddAction()
        {
            if (EditingGame.GameActions == null)
            {
                EditingGame.GameActions = new ObservableCollection<GameAction>();
                EditingGame.GameActions.CollectionChanged += OtherActions_CollectionChanged;
            }

            var newAction = new GameAction()
            {
                Name = "New Action",
                IsPlayAction = true
            };

            newAction.PropertyChanged += GameAction_PropertyChanged;
            EditingGame.GameActions.Add(newAction);
        }

        public void RemoveAction(GameAction action)
        {
            action.PropertyChanged -= GameAction_PropertyChanged;
            EditingGame.GameActions.Remove(action);
        }

        public void MoveActionUp(GameAction action)
        {
            var index = EditingGame.GameActions.IndexOf(action);
            if (index != 0)
            {
                EditingGame.GameActions.Move(index, index - 1);
            }
        }

        public void MoveActionDown(GameAction action)
        {
            var index = EditingGame.GameActions.IndexOf(action);
            if (index != EditingGame.GameActions.Count - 1)
            {
                EditingGame.GameActions.Move(index, index + 1);
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
            if (link != null)
            {
                link.PropertyChanged -= Link_PropertyChanged;
            }

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

        private void Rom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UseRomsChanges = true;
        }

        private void Roms_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UseRomsChanges = true;
        }

        public void AddRom()
        {
            if (EditingGame.Roms == null)
            {
                EditingGame.Roms = new ObservableCollection<GameRom>();
                EditingGame.Roms.CollectionChanged += Roms_CollectionChanged;
            }

            var newRom = new GameRom("NewRom", "NewPath");
            newRom.PropertyChanged += Rom_PropertyChanged;
            EditingGame.Roms.Add(newRom);
        }

        public void RemoveRom(GameRom rom)
        {
            rom.PropertyChanged -= Rom_PropertyChanged;
            EditingGame.Roms.Remove(rom);
        }

        public void MoveRomUp(GameRom rom)
        {
            var index = EditingGame.Roms.IndexOf(rom);
            if (index != 0)
            {
                EditingGame.Roms.Move(index, index - 1);
            }
        }

        public void MoveRomDown(GameRom rom)
        {
            var index = EditingGame.Roms.IndexOf(rom);
            if (index != EditingGame.Roms.Count - 1)
            {
                EditingGame.Roms.Move(index, index + 1);
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

        public string SelectGameImage()
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            return null;
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
                var existing = collection.FirstOrDefault(a => a.Name?.Equals(newItem.Name, StringComparison.InvariantCultureIgnoreCase) == true);
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

        public Platform AddNewPlatform(string item = null)
        {
            return CreateNewItemInCollection<Platform>(Platforms, item, LooseDbNameComparer);
        }

        public void AddNewPlatforms(List<string> items)
        {
            var added = new List<Platform>();
            items?.ForEach(a => added.Add(AddNewPlatform(a)));
            if (added.Any())
            {
                Platforms.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        public Series AddNewSeries(string item = null)
        {
            return CreateNewItemInCollection<Series>(Series, item, LooseDbNameComparer);
        }

        public void AddNewSeries(List<string> items)
        {
            var added = new List<Series>();
            items?.ForEach(a => added.Add(AddNewSeries(a)));
            if (added.Any())
            {
                Series.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        public AgeRating AddNewAgeRating(string item = null)
        {
            return CreateNewItemInCollection<AgeRating>(AgeRatings, item, LooseDbNameComparer);
        }

        public void AddNewAgeRatings(List<string> items)
        {
            var added = new List<AgeRating>();
            items?.ForEach(a => added.Add(AddNewAgeRating(a)));
            if (added.Any())
            {
                AgeRatings.SetSelection(added.Select(a => a.Id).ToList());
            }
        }

        public Region AddNewRegion(string item = null)
        {
            return CreateNewItemInCollection<Region>(Regions, item, LooseDbNameComparer);
        }

        public void AddNewRegions(List<string> items)
        {
            var added = new List<Region>();
            items?.ForEach(a => added.Add(AddNewRegion(a)));
            if (added.Any())
            {
                Regions.SetSelection(added.Select(a => a.Id).ToList());
            }
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
                if (!Developers.Any(a => LooseDbNameComparer(a, newItem.Name) == true))
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
                if (!Publishers.Any(a => LooseDbNameComparer(a, newItem.Name) == true))
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

        public Genre AddNewGenre(string item = null)
        {
            return CreateNewItemInCollection<Genre>(Genres, item, LooseDbNameComparer);
        }

        public void AddNewGenres(List<string> items)
        {
            var added = new List<Genre>();
            items?.ForEach(a => added.Add(AddNewGenre(a)));
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

        public void AddNewCompletionStatus(string status = null)
        {
            EditingGame.CompletionStatusId = CreateNewItemInCollection(CompletionStatuses, status)?.Id ?? EditingGame.CompletionStatusId;
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
            return GameFieldComparer.StringEquals(
                existingItem.Item.Name ?? string.Empty,
                newName ?? string.Empty);
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

        public void TestScript(string script)
        {
            try
            {
                var expandedScript = EditingGame.ExpandVariables(script);
                var startingArgs = new SDK.Events.OnGameStartingEventArgs
                {
                    Game = EditingGame,
                    SelectedRomFile = EditingGame.Roms?.FirstOrDefault()?.Path,
                    SourceAction = EditingGame.GameActions?.FirstOrDefault()
                };

                using (var runtime = new PowerShellRuntime($"test script runtime"))
                {
                    PlayniteApplication.Current.GamesEditor.ExecuteScriptAction(runtime, expandedScript, EditingGame, true, false, GameScriptType.None,
                        new Dictionary<string, object>
                        {
                            {  "StartingArgs", startingArgs },
                            {  "SourceAction", startingArgs.SourceAction },
                            {  "SelectedRomFile", startingArgs.SelectedRomFile }
                        });
                }
            }
            catch (Exception exc)
            {
                var message = exc.Message;
                if (exc is ScriptRuntimeException err)
                {
                    message = err.Message + "\n\n" + err.ScriptStackTrace;
                }

                Dialogs.ShowMessage(
                    message,
                    resources.GetString("LOCScriptError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void CalculateInstallSizeWithDialog()
        {
            PlayniteApplication.Current.GamesEditor.UpdateGameSizeWithDialog(EditingGame, false, false);
        }
    }
}
