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

namespace Playnite.DesktopApp.ViewModels
{
    public class GameEditViewModel : ObservableObject
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

        public object IconImageObject => ImageSourceManager.GetImage(EditingGame.Icon, false, new BitmapLoadProperties(256, 256));
        public object CoverImageObject => ImageSourceManager.GetImage(EditingGame.CoverImage, false, new BitmapLoadProperties(900, 900));
        public object BackgroundImageObject => ImageSourceManager.GetImage(EditingGame.BackgroundImage, false, new BitmapLoadProperties(1920, 1080));

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

        #region Field commands

        public RelayCommand<object> AddPlatformCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewPlatform();
            });
        }

        public RelayCommand<object> AddSeriesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewSeries();
            });
        }

        public RelayCommand<object> AddAgeRatingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewAreRating();
            });
        }

        public RelayCommand<object> AddRegionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewRegion();
            });
        }

        public RelayCommand<object> AddSourceCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewSource();
            });
        }

        public RelayCommand<object> AddCategoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewCategory();
            });
        }

        public RelayCommand<object> AddPublisherCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewPublisher();
            });
        }

        public RelayCommand<object> AddDeveloperCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewDeveloper();
            });
        }

        public RelayCommand<object> AddGenreCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewGenre();
            });
        }

        public RelayCommand<object> AddTagCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewTag();
            });
        }

        public RelayCommand<object> AddFeatureCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddNewFeature();
            });
        }

        #endregion Field commands

        #region Field checks

        private bool useNameChanges;
        public bool UseNameChanges
        {
            get
            {
                return useNameChanges;
            }

            set
            {
                useNameChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useSortingNameChanges;
        public bool UseSortingNameChanges
        {
            get
            {
                return useSortingNameChanges;
            }

            set
            {
                useSortingNameChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool usePlatformChanges;
        public bool UsePlatformChanges
        {
            get
            {
                return usePlatformChanges;
            }

            set
            {
                usePlatformChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useGenresChanges;
        public bool UseGenresChanges
        {
            get
            {
                return useGenresChanges;
            }

            set
            {
                useGenresChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useReleaseDateChanges;
        public bool UseReleaseDateChanges
        {
            get
            {
                return useReleaseDateChanges;
            }

            set
            {
                useReleaseDateChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useDeveloperChanges;
        public bool UseDeveloperChanges
        {
            get
            {
                return useDeveloperChanges;
            }

            set
            {
                useDeveloperChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool usePublisherChanges;
        public bool UsePublisherChanges
        {
            get
            {
                return usePublisherChanges;
            }

            set
            {
                usePublisherChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useCategoryChanges;
        public bool UseCategoryChanges
        {
            get
            {
                return useCategoryChanges;
            }

            set
            {
                useCategoryChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useTagChanges;
        public bool UseTagChanges
        {
            get
            {
                return useTagChanges;
            }

            set
            {
                useTagChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useFeatureChanges;
        public bool UseFeatureChanges
        {
            get
            {
                return useFeatureChanges;
            }

            set
            {
                useFeatureChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useDescriptionChanges;
        public bool UseDescriptionChanges
        {
            get
            {
                return useDescriptionChanges;
            }

            set
            {
                useDescriptionChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useIconChanges;
        public bool UseIconChanges
        {
            get
            {
                return useIconChanges;
            }

            set
            {
                useIconChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowMediaChangeNotif));
            }
        }

        private bool useImageChanges;
        public bool UseImageChanges
        {
            get
            {
                return useImageChanges;
            }

            set
            {
                useImageChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowMediaChangeNotif));
            }
        }

        private bool useBackgroundChanges;
        public bool UseBackgroundChanges
        {
            get
            {
                return useBackgroundChanges;
            }

            set
            {
                useBackgroundChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowMediaChangeNotif));
            }
        }

        private bool useInstallDirChanges;
        public bool UseInstallDirChanges
        {
            get
            {
                return useInstallDirChanges;
            }

            set
            {
                useInstallDirChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowInstallChangeNotif));
            }
        }

        private bool useIsoPathChanges;
        public bool UseIsoPathChanges
        {
            get
            {
                return useIsoPathChanges;
            }

            set
            {
                useIsoPathChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowInstallChangeNotif));
            }
        }

        private bool useInstallStateChanges;
        public bool UseInstallStateChanges
        {
            get
            {
                return useInstallStateChanges;
            }

            set
            {
                useInstallStateChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowInstallChangeNotif));
            }
        }

        private bool useLinksChanges;
        public bool UseLinksChanges
        {
            get
            {
                return useLinksChanges;
            }

            set
            {
                useLinksChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowLinksChangeNotif));
            }
        }

        private bool useLastActivityChanges;
        public bool UseLastActivityChanges
        {
            get
            {
                return useLastActivityChanges;
            }

            set
            {
                useLastActivityChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool usePlaytimeChanges;
        public bool UsePlaytimeChanges
        {
            get
            {
                return usePlaytimeChanges;
            }

            set
            {
                usePlaytimeChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useAddedChanges;
        public bool UseAddedChanges
        {
            get
            {
                return useAddedChanges;
            }

            set
            {
                useAddedChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool usePlayCountChanges;
        public bool UsePlayCountChanges
        {
            get
            {
                return usePlayCountChanges;
            }

            set
            {
                usePlayCountChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useSeriesChanges;
        public bool UseSeriesChanges
        {
            get
            {
                return useSeriesChanges;
            }

            set
            {
                useSeriesChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useVersionChanges;
        public bool UseVersionChanges
        {
            get
            {
                return useVersionChanges;
            }

            set
            {
                useVersionChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useAgeRatingChanges;
        public bool UseAgeRatingChanges
        {
            get
            {
                return useAgeRatingChanges;
            }

            set
            {
                useAgeRatingChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useRegionChanges;
        public bool UseRegionChanges
        {
            get
            {
                return useRegionChanges;
            }

            set
            {
                useRegionChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useSourceChanges;
        public bool UseSourceChanges
        {
            get
            {
                return useSourceChanges;
            }

            set
            {
                useSourceChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useCompletionStatusChanges;
        public bool UseCompletionStatusChanges
        {
            get
            {
                return useCompletionStatusChanges;
            }

            set
            {
                useCompletionStatusChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useUserScoreChanges;
        public bool UseUserScoreChanges
        {
            get
            {
                return useUserScoreChanges;
            }

            set
            {
                useUserScoreChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useCriticScoreChanges;
        public bool UseCriticScoreChanges
        {
            get
            {
                return useCriticScoreChanges;
            }

            set
            {
                useCriticScoreChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useCommunityScoreChanges;
        public bool UseCommunityScoreChanges
        {
            get
            {
                return useCommunityScoreChanges;
            }

            set
            {
                useCommunityScoreChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useHiddenChanges;
        public bool UseHiddenChanges
        {
            get
            {
                return useHiddenChanges;
            }

            set
            {
                useHiddenChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useFavoriteChanges;
        public bool UseFavoriteChanges
        {
            get
            {
                return useFavoriteChanges;
            }

            set
            {
                useFavoriteChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGeneralChangeNotif));
            }
        }

        private bool useScriptRuntimeChanges;
        public bool UseScriptRuntimeChanges
        {
            get
            {
                return useScriptRuntimeChanges;
            }

            set
            {
                useScriptRuntimeChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowScriptsChangeNotif));
            }
        }

        private bool usePreScriptChanges;
        public bool UsePreScriptChanges
        {
            get
            {
                return usePreScriptChanges;
            }

            set
            {
                usePreScriptChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowScriptsChangeNotif));
            }
        }

        private bool usePostScriptChanges;
        public bool UsePostScriptChanges
        {
            get
            {
                return usePostScriptChanges;
            }

            set
            {
                usePostScriptChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowScriptsChangeNotif));
            }
        }

        private bool usePreGlobalScriptChanges;
        public bool UsePreGlobalScriptChanges
        {
            get
            {
                return usePreGlobalScriptChanges;
            }

            set
            {
                usePreGlobalScriptChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowScriptsChangeNotif));
            }
        }

        private bool usePostGlobalScriptChanges;
        public bool UsePostGlobalScriptChanges
        {
            get
            {
                return usePostGlobalScriptChanges;
            }

            set
            {
                usePostGlobalScriptChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowScriptsChangeNotif));
            }
        }

        public bool ShowGeneralChangeNotif
        {
            get
            {
                return ShowCheckBoxes &&
                    (UseNameChanges ||
                    UseReleaseDateChanges ||
                    UseCategoryChanges ||
                    UseDescriptionChanges ||
                    UseDeveloperChanges ||
                    UseGenresChanges ||
                    UsePlatformChanges ||
                    UsePublisherChanges ||
                    UseSortingNameChanges ||
                    UseTagChanges ||
                    UseFeatureChanges ||
                    UseSeriesChanges ||
                    UseVersionChanges ||
                    UseAgeRatingChanges ||
                    UseRegionChanges ||
                    UseSourceChanges ||
                    UseUserScoreChanges ||
                    UseCriticScoreChanges ||
                    UseCommunityScoreChanges);
            }
        }

        public bool ShowMediaChangeNotif
        {
            get
            {
                return ShowCheckBoxes &&
                    (UseImageChanges ||
                    UseBackgroundChanges ||
                    UseIconChanges);
            }
        }

        public bool ShowAdvancedChangeNotif
        {
            get
            {
                return ShowCheckBoxes &&
                    (UseHiddenChanges ||
                    UseFavoriteChanges ||
                    UseLastActivityChanges ||
                    UsePlaytimeChanges ||
                    UseAddedChanges ||
                    UsePlayCountChanges ||
                    UseCompletionStatusChanges);
            }
        }

        public bool ShowLinksChangeNotif
        {
            get
            {
                return ShowCheckBoxes && UseLinksChanges;
            }
        }

        public bool ShowInstallChangeNotif
        {
            get
            {
                return ShowCheckBoxes &&
                    (UseInstallDirChanges ||
                    UseIsoPathChanges ||
                    UseInstallStateChanges);
            }
        }

        public bool ShowScriptsChangeNotif
        {
            get
            {
                return ShowCheckBoxes &&
                    (UseScriptRuntimeChanges ||
                    UsePreScriptChanges ||
                    UsePostScriptChanges ||
                    UsePreGlobalScriptChanges ||
                    UsePostGlobalScriptChanges);
            }
        }

        #endregion Field checks

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

        private bool showActions;
        public bool ShowActions
        {
            get
            {
                return showActions;
            }

            set
            {
                showActions = value;
                OnPropertyChanged();
            }
        }

        private bool showLinks;
        public bool ShowLinks
        {
            get
            {
                return showLinks;
            }

            set
            {
                showLinks = value;
                OnPropertyChanged();
            }
        }

        private bool showInstallation;
        public bool ShowInstallation
        {
            get
            {
                return showInstallation;
            }

            set
            {
                showInstallation = value;
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

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<object> SelectIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectIcon();
            });
        }

        public RelayCommand<object> UseExeIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                UseExeIcon();
            });
        }

        public RelayCommand<DragEventArgs> DropIconCommand
        {
            get => new RelayCommand<DragEventArgs>((args) =>
            {
                DropIcon(args);
            });
        }

        public RelayCommand<DragEventArgs> DropCoverCommand
        {
            get => new RelayCommand<DragEventArgs>((args) =>
            {
                DropCover(args);
            });
        }

        public RelayCommand<object> SelectCoverCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectCover();
            });
        }

        public RelayCommand<DragEventArgs> DropBackgroundCommand
        {
            get => new RelayCommand<DragEventArgs>((args) =>
            {
                DropBackground(args);
            });
        }

        public RelayCommand<object> SelectBackgroundCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectBackground();
            });
        }

        public RelayCommand<object> SetBackgroundUrlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SetBackgroundUrl();
            });
        }

        public RelayCommand<object> SetIconUrlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SetIconUrl();
            });
        }

        public RelayCommand<object> SetCoverUrlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SetCoverUrl();
            });
        }

        public RelayCommand<object> AddLinkCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddLink();
            });
        }

        public RelayCommand<Link> RemoveLinkCommand
        {
            get => new RelayCommand<Link>((link) =>
            {
                RemoveLink(link);
            });
        }

        public RelayCommand<object> SelectInstallDirCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectInstallDir();
            });
        }

        public RelayCommand<object> SelectGameImageCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGameImage();
            });
        }

        public RelayCommand<object> AddPlayActionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddPlayAction();
            });
        }

        public RelayCommand<object> DeletePlayActionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemovePlayAction();
            });
        }

        public RelayCommand<object> AddActionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddAction();
            });
        }

        public RelayCommand<GameAction> MoveUpActionCommand
        {
            get => new RelayCommand<GameAction>((action) =>
            {
                MoveActionUp(action);
            });
        }

        public RelayCommand<GameAction> MoveDownActionCommand
        {
            get => new RelayCommand<GameAction>((action) =>
            {
                MoveActionDown(action);
            });
        }

        public RelayCommand<GameAction> DeleteActionCommand
        {
            get => new RelayCommand<GameAction>((action) =>
            {
                RemoveAction(action);
            });
        }

        public RelayCommand<object> RemoveIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveIcon();
            });
        }

        public RelayCommand<object> RemoveImageCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveImage();
            });
        }

        public RelayCommand<object> RemoveBackgroundCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveBackground();
            });
        }

        public RelayCommand<object> OpenMetadataFolderCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenMetadataFolder();
            });
        }

        public RelayCommand<object> SelectGoogleIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGoogleIcon();
            });
        }

        public RelayCommand<object> SelectGoogleCoverCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGoogleCover();
            });
        }

        public RelayCommand<object> SelectGoogleBackgroundCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectGoogleBackground();
            });
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
            ShowLinks = true;
            ShowActions = true;
            ShowInstallation = true;
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
            ShowLinks = false;
            ShowActions = false;
            ShowInstallation = true;
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
        }

        private void EditingGame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Game.Name):
                    if (IsSingleGameEdit)
                    {
                        UseNameChanges = Game.Name != EditingGame.Name;
                    }
                    else
                    {
                        UseNameChanges = true;
                    }
                    break;
                case nameof(Game.SortingName):
                    if (IsSingleGameEdit)
                    {
                        UseSortingNameChanges = Game.SortingName != EditingGame.SortingName;
                    }
                    else
                    {
                        UseSortingNameChanges = true;
                    }
                    break;
                case nameof(Game.PlatformId):
                    if (IsSingleGameEdit)
                    {
                        UsePlatformChanges = Game.PlatformId != EditingGame.PlatformId;
                    }
                    else
                    {
                        UsePlatformChanges = true;
                    }
                    break;
                case nameof(Game.CoverImage):
                    OnPropertyChanged(nameof(CoverMetadata));
                    OnPropertyChanged(nameof(IsCoverTooLage));
                    OnPropertyChanged(nameof(CoverImageObject));
                    if (IsSingleGameEdit)
                    {
                        UseImageChanges = Game.CoverImage != EditingGame.CoverImage;
                    }
                    else
                    {
                        UseImageChanges = true;
                    }
                    break;
                case nameof(Game.BackgroundImage):
                    OnPropertyChanged(nameof(BackgroundMetadata));
                    OnPropertyChanged(nameof(IsBackgroundTooLage));
                    OnPropertyChanged(nameof(BackgroundImageObject));
                    if (IsSingleGameEdit)
                    {
                        UseBackgroundChanges = Game.BackgroundImage != EditingGame.BackgroundImage;
                    }
                    else
                    {
                        UseBackgroundChanges = true;
                    }

                    OnPropertyChanged(nameof(ShowBackgroundUrl));
                    break;
                case nameof(Game.Icon):
                    OnPropertyChanged(nameof(IconMetadata));
                    OnPropertyChanged(nameof(IsIconTooLage));
                    OnPropertyChanged(nameof(IconImageObject));
                    if (IsSingleGameEdit)
                    {
                        UseIconChanges = Game.Icon != EditingGame.Icon;
                    }
                    else
                    {
                        UseIconChanges = true;
                    }
                    break;
                case nameof(Game.Links):
                    if (IsSingleGameEdit)
                    {
                        UseLinksChanges = !Game.Links.IsEqualJson(EditingGame.Links);
                    }
                    else
                    {
                        UseLinksChanges = true;
                    }
                    break;
                case nameof(Game.InstallDirectory):
                    if (IsSingleGameEdit)
                    {
                        UseInstallDirChanges = Game.InstallDirectory != EditingGame.InstallDirectory;
                    }
                    else
                    {
                        UseInstallDirChanges = true;
                    }
                    break;
                case nameof(Game.GameImagePath):
                    if (IsSingleGameEdit)
                    {
                        UseIsoPathChanges = Game.GameImagePath != EditingGame.GameImagePath;
                    }
                    else
                    {
                        UseIsoPathChanges = true;
                    }
                    break;
                case nameof(Game.IsInstalled):
                    if (IsSingleGameEdit)
                    {
                        UseInstallStateChanges = Game.IsInstalled != EditingGame.IsInstalled;
                    }
                    else
                    {
                        UseInstallStateChanges = true;
                    }
                    break;
                case nameof(Game.Description):
                    if (IsSingleGameEdit)
                    {
                        UseDescriptionChanges = Game.Description != EditingGame.Description;
                    }
                    else
                    {
                        UseDescriptionChanges = true;
                    }
                    break;
                case nameof(Game.CategoryIds):
                    if (IsSingleGameEdit)
                    {
                        UseCategoryChanges = !Game.CategoryIds.IsListEqual(EditingGame.CategoryIds);
                    }
                    else
                    {
                        UseCategoryChanges = true;
                    }
                    break;
                case nameof(Game.TagIds):
                    if (IsSingleGameEdit)
                    {
                        UseTagChanges = !Game.TagIds.IsListEqual(EditingGame.TagIds);
                    }
                    else
                    {
                        UseTagChanges = true;
                    }
                    break;
                case nameof(Game.FeatureIds):
                    if (IsSingleGameEdit)
                    {
                        UseFeatureChanges = !Game.FeatureIds.IsListEqual(EditingGame.FeatureIds);
                    }
                    else
                    {
                        UseFeatureChanges = true;
                    }
                    break;
                case nameof(Game.GenreIds):
                    if (IsSingleGameEdit)
                    {
                        UseGenresChanges = !Game.GenreIds.IsListEqual(EditingGame.GenreIds);
                    }
                    else
                    {
                        UseGenresChanges = true;
                    }
                    break;
                case nameof(Game.ReleaseDate):
                    if (IsSingleGameEdit)
                    {
                        UseReleaseDateChanges = Game.ReleaseDate != EditingGame.ReleaseDate;
                    }
                    else
                    {
                        UseReleaseDateChanges = true;
                    }
                    break;
                case nameof(Game.DeveloperIds):
                    if (IsSingleGameEdit)
                    {
                        UseDeveloperChanges = !Game.DeveloperIds.IsListEqual(EditingGame.DeveloperIds);
                    }
                    else
                    {
                        UseDeveloperChanges = true;
                    }
                    break;
                case nameof(Game.PublisherIds):
                    if (IsSingleGameEdit)
                    {
                        UsePublisherChanges = !Game.PublisherIds.IsListEqual(EditingGame.PublisherIds);
                    }
                    else
                    {
                        UsePublisherChanges = true;
                    }
                    break;
                case nameof(Game.LastActivity):
                    if (IsSingleGameEdit)
                    {
                        UseLastActivityChanges = Game.LastActivity != EditingGame.LastActivity;
                    }
                    else
                    {
                        UseLastActivityChanges = true;
                    }
                    break;
                case nameof(Game.Playtime):
                    if (IsSingleGameEdit)
                    {
                        UsePlaytimeChanges = Game.Playtime != EditingGame.Playtime;
                    }
                    else
                    {
                        UsePlaytimeChanges = true;
                    }
                    break;
                case nameof(Game.Added):
                    if (IsSingleGameEdit)
                    {
                        UseAddedChanges = Game.Added != EditingGame.Added;
                    }
                    else
                    {
                        UseAddedChanges = true;
                    }
                    break;
                case nameof(Game.PlayCount):
                    if (IsSingleGameEdit)
                    {
                        UsePlayCountChanges = Game.PlayCount != EditingGame.PlayCount;
                    }
                    else
                    {
                        UsePlayCountChanges = true;
                    }
                    break;
                case nameof(Game.SeriesId):
                    if (IsSingleGameEdit)
                    {
                        UseSeriesChanges = Game.SeriesId != EditingGame.SeriesId;
                    }
                    else
                    {
                        UseSeriesChanges = true;
                    }
                    break;
                case nameof(Game.Version):
                    if (IsSingleGameEdit)
                    {
                        UseVersionChanges = Game.Version != EditingGame.Version;
                    }
                    else
                    {
                        UseVersionChanges = true;
                    }
                    break;
                case nameof(Game.AgeRatingId):
                    if (IsSingleGameEdit)
                    {
                        UseAgeRatingChanges = Game.AgeRatingId != EditingGame.AgeRatingId;
                    }
                    else
                    {
                        UseAgeRatingChanges = true;
                    }
                    break;
                case nameof(Game.RegionId):
                    if (IsSingleGameEdit)
                    {
                        UseRegionChanges = Game.RegionId != EditingGame.RegionId;
                    }
                    else
                    {
                        UseRegionChanges = true;
                    }
                    break;
                case nameof(Game.SourceId):
                    if (IsSingleGameEdit)
                    {
                        UseSourceChanges = Game.SourceId != EditingGame.SourceId;
                    }
                    else
                    {
                        UseSourceChanges = true;
                    }
                    break;
                case nameof(Game.CompletionStatus):
                    if (IsSingleGameEdit)
                    {
                        UseCompletionStatusChanges = Game.CompletionStatus != EditingGame.CompletionStatus;
                    }
                    else
                    {
                        UseCompletionStatusChanges = true;
                    }
                    break;
                case nameof(Game.UserScore):
                    if (IsSingleGameEdit)
                    {
                        UseUserScoreChanges = Game.UserScore != EditingGame.UserScore;
                    }
                    else
                    {
                        UseUserScoreChanges = true;
                    }
                    break;
                case nameof(Game.CriticScore):
                    if (IsSingleGameEdit)
                    {
                        UseCriticScoreChanges = Game.CriticScore != EditingGame.CriticScore;
                    }
                    else
                    {
                        UseCriticScoreChanges = true;
                    }
                    break;
                case nameof(Game.CommunityScore):
                    if (IsSingleGameEdit)
                    {
                        UseCommunityScoreChanges = Game.CommunityScore != EditingGame.CommunityScore;
                    }
                    else
                    {
                        UseCommunityScoreChanges = true;
                    }
                    break; ;
                case nameof(Game.Favorite):
                    if (IsSingleGameEdit)
                    {
                        UseFavoriteChanges = Game.Favorite != EditingGame.Favorite;
                    }
                    else
                    {
                        UseFavoriteChanges = true;
                    }
                    break; ;
                case nameof(Game.Hidden):
                    if (IsSingleGameEdit)
                    {
                        UseHiddenChanges = Game.Hidden != EditingGame.Hidden;
                    }
                    else
                    {
                        UseHiddenChanges = true;
                    }
                    break;
                case nameof(Game.PreScript):
                    if (IsSingleGameEdit)
                    {
                        UsePreScriptChanges = !string.Equals(Game.PreScript, EditingGame.PreScript, StringComparison.Ordinal);
                    }
                    else
                    {
                        UsePreScriptChanges = true;
                    }
                    break;
                case nameof(Game.PostScript):
                    if (IsSingleGameEdit)
                    {
                        UsePostScriptChanges = !string.Equals(Game.PostScript, EditingGame.PostScript, StringComparison.Ordinal);
                    }
                    else
                    {
                        UsePostScriptChanges = true;
                    }
                    break;
                case nameof(Game.ActionsScriptLanguage):
                    if (IsSingleGameEdit)
                    {
                        UseScriptRuntimeChanges = Game.ActionsScriptLanguage != EditingGame.ActionsScriptLanguage;
                    }
                    else
                    {
                        UseScriptRuntimeChanges = true;
                    }
                    break;
                case nameof(Game.UseGlobalPostScript):
                    if (IsSingleGameEdit)
                    {
                        UsePostGlobalScriptChanges = Game.UseGlobalPostScript != EditingGame.UseGlobalPostScript;
                    }
                    else
                    {
                        UsePostGlobalScriptChanges = true;
                    }
                    break;
                case nameof(Game.UseGlobalPreScript):
                    if (IsSingleGameEdit)
                    {
                        UsePreGlobalScriptChanges = Game.UseGlobalPreScript != EditingGame.UseGlobalPreScript;
                    }
                    else
                    {
                        UsePreGlobalScriptChanges = true;
                    }
                    break;
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result = false)
        {
            CleanupTempFiles();
            LibraryPluginMetadataDownloader?.Dispose();
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

            if (IsSingleGameEdit)
            {
                if (!Game.PlayAction.IsEqualJson(EditingGame.PlayAction))
                {
                    Game.PlayAction = EditingGame.PlayAction;
                }

                if (!Game.OtherActions.IsEqualJson(EditingGame.OtherActions))
                {
                    Game.OtherActions = EditingGame.OtherActions;
                }

                if (!Game.Links.IsEqualJson(EditingGame.Links))
                {
                    if ((ShowCheckBoxes && UseLinksChanges) || !ShowCheckBoxes)
                    {
                        Game.Links = EditingGame.Links;
                    }
                }
            }

            var changeDate = DateTime.Now;
            var gamesToUpdate = IsMultiGameEdit ? Games : new List<Game> { Game };
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

                if (UseInstallDirChanges)
                {
                    game.InstallDirectory = EditingGame.InstallDirectory;
                }

                if (UseIsoPathChanges)
                {
                    game.GameImagePath = EditingGame.GameImagePath;
                }

                if (UseInstallDirChanges)
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

            CloseView(true);
        }

        internal void CleanupTempFiles()
        {
            try
            {
                if (Path.GetDirectoryName(EditingGame.Icon) == PlaynitePaths.TempPath)
                {
                    FileSystem.DeleteFile(EditingGame.Icon);
                }

                if (Path.GetDirectoryName(EditingGame.CoverImage) == PlaynitePaths.TempPath)
                {
                    FileSystem.DeleteFile(EditingGame.CoverImage);
                }

                if (Path.GetDirectoryName(EditingGame.BackgroundImage) == PlaynitePaths.TempPath)
                {
                    FileSystem.DeleteFile(EditingGame.BackgroundImage);
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to cleanup temporary files.");
            }
        }

        public void PreviewGameData(GameMetadata metadata)
        {
            ShowCheckBoxes = true;

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

            if (!metadata.GameInfo.AgeRating.IsNullOrEmpty())
            {
                AddNewAreRating(metadata.GameInfo.AgeRating);
            }

            if (!metadata.GameInfo.Region.IsNullOrEmpty())
            {
                AddNewRegion(metadata.GameInfo.Region);
            }

            if (!metadata.GameInfo.Series.IsNullOrEmpty())
            {
                AddNewSeries(metadata.GameInfo.Series);
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

            if (metadata.CoverImage != null)
            {
                var newCover = ProcessMetadataFile(metadata.CoverImage);
                if (newCover != null)
                {
                    EditingGame.CoverImage = newCover;
                }
            }

            if (metadata.Icon != null)
            {
                var newIcon = ProcessMetadataFile(metadata.Icon);
                if (newIcon != null)
                {
                    EditingGame.Icon = newIcon;
                }
            }

            if (metadata.BackgroundImage != null)
            {
                var newBackground = ProcessMetadataFile(metadata.BackgroundImage);
                if (newBackground != null)
                {
                    EditingGame.BackgroundImage = newBackground;
                }
            }
        }

        private string ProcessMetadataFile(MetadataFile file)
        {
            if (file.HasContent)
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = Guid.NewGuid().ToString() + extension;
                var targetPath = Path.Combine(PlaynitePaths.TempPath, fileName);
                FileSystem.PrepareSaveFile(targetPath);
                File.WriteAllBytes(targetPath, file.Content);
                return targetPath;
            }
            else if (!file.OriginalUrl.IsNullOrEmpty())
            {
                if (file.OriginalUrl.IsHttpUrl())
                {
                    var extension = Path.GetExtension(new Uri(file.OriginalUrl).AbsolutePath);
                    var fileName = Guid.NewGuid().ToString() + extension;
                    var targetPath = Path.Combine(PlaynitePaths.TempPath, fileName);

                    try
                    {
                        HttpDownloader.DownloadFile(file.OriginalUrl, targetPath);
                        return targetPath;
                    }
                    catch (WebException e)
                    {
                        logger.Error(e, $"Failed to add {file.OriginalUrl} file to database.");
                        return null;
                    }
                }
                else
                {
                    return file.OriginalUrl;
                }
            }
            else
            {
                return null;
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

        public string PrepareImagePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (path.IsHttpUrl())
                {
                    path = ProcessMetadataFile(new MetadataFile(path));
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
            var path = PrepareImagePath(GetDroppedImage(args));
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
            var path = PrepareImagePath(dialogs.SelectImagefile());
            if (path != null)
            {
                EditingGame.CoverImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedCoverSize);
            }
        }

        public void DropCover(DragEventArgs args)
        {
            var path = PrepareImagePath(GetDroppedImage(args));
            if (!path.IsNullOrEmpty())
            {
                EditingGame.CoverImage = path;
                CheckImagePerformanceRestrains(path, GameDatabase.MaximumRecommendedCoverSize);
            }
        }

        public void SelectBackground()
        {
            var path = PrepareImagePath(dialogs.SelectImagefile());
            if (!path.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = path;
            }
        }

        public void DropBackground(DragEventArgs args)
        {
            var path = PrepareImagePath(GetDroppedImage(args));
            if (!path.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = path;
            }
        }

        public void SetBackgroundUrl()
        {
            var image = SelectUrlImage();
            if (!image.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = image;
            }
        }

        public void SetIconUrl()
        {
            var image = SelectUrlImage();
            if (!image.IsNullOrEmpty())
            {
                EditingGame.Icon = image;
            }
        }

        public void SetCoverUrl()
        {
            var image = SelectUrlImage();
            if (!image.IsNullOrEmpty())
            {
                EditingGame.CoverImage = image;
            }
        }

        public string SelectUrlImage()
        {
            var url = dialogs.SelectString(
                resources.GetString("LOCURLInputInfo"),
                resources.GetString("LOCURLInputInfoTitile"),
                string.Empty);
            if (url.Result && !url.SelectedString.IsNullOrEmpty())
            {
                try
                {
                    return PrepareImagePath(url.SelectedString);
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
            EditingGame.PlayAction = new GameAction()
            {
                Name = "Play",
                IsHandledByPlugin = false
            };
        }

        public void RemovePlayAction()
        {
            EditingGame.PlayAction = null;
        }

        public void AddAction()
        {
            if (EditingGame.OtherActions == null)
            {
                EditingGame.OtherActions = new ObservableCollection<GameAction>();
            }

            var newTask = new GameAction()
            {
                Name = "New Action",
                IsHandledByPlugin = false
            };

            if (EditingGame.PlayAction != null && EditingGame.PlayAction.Type == GameActionType.File)
            {
                newTask.WorkingDir = EditingGame.PlayAction.WorkingDir;
                newTask.Path = EditingGame.PlayAction.Path;
            }

            EditingGame.OtherActions.Add(newTask);
        }

        public void RemoveAction(GameAction action)
        {
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

        public void AddLink()
        {
            if (EditingGame.Links == null)
            {
                EditingGame.Links = new ObservableCollection<Link>();
            }

            EditingGame.Links.Add(new Link("NewLink", "NewUrl"));
        }

        public void RemoveLink(Link link)
        {
            EditingGame.Links.Remove(link);
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

        public async void DownloadPluginData(MetadataPlugin plugin)
        {
            ProgressVisible = true;

            await Task.Run(() =>
            {
                try
                {
                    var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(EditingGame, false));
                    if (provider != null)
                    {
                        try
                        {
                            var gameInfo = new GameInfo
                            {
                                Name = provider.GetName(),
                                Genres = provider.GetGenres(),
                                ReleaseDate = provider.GetReleaseDate(),
                                Developers = provider.GetDevelopers(),
                                Publishers = provider.GetPublishers(),
                                Tags = provider.GetTags(),
                                Features = provider.GetFeatures(),
                                Description = provider.GetDescription(),
                                Links = provider.GetLinks(),
                                CriticScore = provider.GetCriticScore(),
                                CommunityScore = provider.GetCommunityScore()
                            };

                            var metadata = new GameMetadata
                            {
                                GameInfo = gameInfo,
                                Icon = provider.GetIcon(),
                                CoverImage = provider.GetCoverImage(),
                                BackgroundImage = provider.GetBackgroundImage()
                            };

                            Application.Current.Dispatcher.Invoke(() => PreviewGameData(metadata));
                        }
                        finally
                        {
                            provider.Dispose();
                        }
                    }
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, string.Format("Failed to download metadata, {0}, {1}", Game.PluginId, Game.GameId));
                    dialogs.ShowMessage(
                        string.Format(resources.GetString("LOCMetadataDownloadError"), exc.Message),
                        resources.GetString("LOCDownloadError"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ProgressVisible = false;
                }
            });
        }

        public async void DownloadStoreData()
        {
            ProgressVisible = true;

            await Task.Run(() =>
            {
                try
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
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, string.Format("Failed to download metadata, {0}, {1}", Game.PluginId, Game.GameId));
                    dialogs.ShowMessage(
                        string.Format(resources.GetString("LOCMetadataDownloadError"), exc.Message),
                        resources.GetString("LOCDownloadError"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ProgressVisible = false;
                }
            });
        }

        public void OpenMetadataFolder()
        {
            Explorer.OpenDirectory(database.GetFileStoragePath(EditingGame.Id));
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
                    existing = collection.FirstOrDefault(a => a.Item.Name.Equals(newItem.Name, StringComparison.InvariantCultureIgnoreCase));
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
            return string.Equals(
                Regex.Replace(existingItem.Item.Name, @"[\s-]", ""),
                Regex.Replace(newName, @"[\s-]", ""), StringComparison.OrdinalIgnoreCase);
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
                    var ask = new MessageBoxWindow();
                    var result = ask.ShowCustom(
                        window.Window,
                        string.Format(resources.GetString("LOCGameImageSizeWarning"),
                        GameDatabase.MaximumRecommendedIconSize,
                        GameDatabase.MaximumRecommendedCoverSize,
                        GameDatabase.MaximumRecommendedBackgroundSize),
                        resources.GetString("LOCPerformanceWarningTitle"),
                        MessageBoxImage.Warning,
                        new List<object> { true, false },
                        new List<string>
                        {
                        resources.GetString("LOCOKLabel"),
                        resources.GetString("LOCDontShowAgainTitle")
                        });
                    if ((result as bool?) == false)
                    {
                        appSettings.ShowImagePerformanceWarning = false;
                    }
                }
            }
        }

        public void SelectGoogleIcon()
        {
            var image = SelectGoogleImage($"{EditingGame.Name} icon", 128, 128);
            if (!image.IsNullOrEmpty())
            {
                EditingGame.Icon = image;
            }
        }

        public void SelectGoogleCover()
        {
            var image = SelectGoogleImage($"{EditingGame.Name} cover");
            if (!image.IsNullOrEmpty())
            {
                EditingGame.CoverImage = image;
            }
        }

        public void SelectGoogleBackground()
        {
            var image = SelectGoogleImage($"{EditingGame.Name} wallpaper");
            if (!image.IsNullOrEmpty())
            {
                EditingGame.BackgroundImage = image;
            }
        }

        public string SelectGoogleImage(string searchTerm, double imageWidth = 0, double imageHeight = 0)
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
                    return PrepareImagePath(model.SelectedImage?.ImageUrl);
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
