using Playnite;
using Playnite.Database;
using PlayniteUI.Commands;
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
using Playnite.Web;
using Playnite.Metadata;
using Playnite.SDK.Metadata;
using Playnite.Settings;
using Playnite.Plugins;
using Playnite.Common;
using System.Net;

namespace PlayniteUI.ViewModels
{
    public class GameEditViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;
        private ExtensionFactory extensions;

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
                    UseLastActivityChanges ||
                    UsePlaytimeChanges ||
                    UseAddedChanges ||
                    UsePlayCountChanges ||
                    UseSeriesChanges ||
                    UseVersionChanges ||
                    UseAgeRatingChanges ||
                    UseRegionChanges ||
                    UseSourceChanges ||
                    UseCompletionStatusChanges ||
                    UseUserScoreChanges ||
                    UseCriticScoreChanges ||
                    UseCommunityScoreChanges ||
                    UseHiddenChanges ||
                    UseFavoriteChanges);
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
                return ShowCheckBoxes && (UseInstallDirChanges || UseIsoPathChanges);
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

        private Game game;
        public Game Game
        {
            get
            {
                return game;
            }

            set
            {
                game = value;
                OnPropertyChanged();
            }
        }

        private IEnumerable<Game> games;
        public IEnumerable<Game> Games
        {
            get
            {
                return games;
            }

            set
            {
                games = value;
                OnPropertyChanged();
            }
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
                return EditingGame == null || EditingGame.BackgroundImage == null ? false : EditingGame.BackgroundImage.IsHttpUrl();
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

        public List<Platform> Platforms
        {
            get
            {
                var platforms = database.Platforms.OrderBy(a => a.Name).ToList();
                platforms.Insert(0, new Platform(string.Empty));
                return platforms;
            }
        }

        public List<Emulator> Emulators
        {
            get
            {
                return database.Emulators.OrderBy(a => a.Name).ToList();        
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

        private bool isSingleGameEdit;
        public bool IsSingleGameEdit
        {
            get
            {
                return isSingleGameEdit;
            }

            set
            {
                isSingleGameEdit = value;
                OnPropertyChanged();
            }
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

        public RelayCommand<object> SelectCoverCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectCover();
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

        public RelayCommand<object> SelectCategoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new CategoryConfigViewModel(CategoryConfigWindowFactory.Instance, database, EditingGame, false);
                SelectCategories(model);
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

        public RelayCommand<object> DownloadIGDBMetadataCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new MetadataLookupViewModel(MetadataProvider.IGDB, MetadataLookupWindowFactory.Instance, dialogs, resources);
                DoMetadataLookup(model);
            });
        }

        public RelayCommand<object> DownloadWikiMetadataCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new MetadataLookupViewModel(MetadataProvider.Wiki, MetadataLookupWindowFactory.Instance, dialogs, resources);
                DoMetadataLookup(model);
            });
        }

        public RelayCommand<object> DownloadStoreCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                DownloadStoreData();
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

        public GameEditViewModel(Game game, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources, ExtensionFactory extensions)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.extensions = extensions;

            Game = game;
            IsSingleGameEdit = true;
            EditingGame = game.CloneJson();
            ShowCheckBoxes = false;
            ShowMetaDownload = true;
            ShowLinks = true;
            ShowActions = true;
            ShowInstallation = true;
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;
        }

        public GameEditViewModel(IEnumerable<Game> games, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources, ExtensionFactory extensions)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.extensions = extensions;

            Games = games;
            IsSingleGameEdit = false;
            var previewGame = GameTools.GetMultiGameEditObject(games);
            EditingGame = previewGame;
            ShowCheckBoxes = true;
            ShowMetaDownload = false;
            ShowLinks = false;
            ShowActions = false;
            ShowInstallation = false;
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;
        }

        private void EditingGame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Game.Name):
                    if (Games == null)
                    {
                        UseNameChanges = Game.Name != EditingGame.Name;
                    }
                    else
                    {
                        UseNameChanges = true;
                    }
                    break;
                case nameof(Game.SortingName):
                    if (Games == null)
                    {
                        UseSortingNameChanges = Game.SortingName != EditingGame.SortingName;
                    }
                    else
                    {
                        UseSortingNameChanges = true;
                    }
                    break;
                case nameof(Game.PlatformId):
                    if (Games == null)
                    {
                        UsePlatformChanges = Game.PlatformId != EditingGame.PlatformId;
                    }
                    else
                    {
                        UsePlatformChanges = true;
                    }
                    break;
                case nameof(Game.CoverImage):
                    if (Games == null)
                    {
                        UseImageChanges = Game.CoverImage != EditingGame.CoverImage;
                    }
                    else
                    {
                        UseImageChanges = true;
                    }
                    break;
                case nameof(Game.BackgroundImage):
                    if (Games == null)
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
                    if (Games == null)
                    {
                        UseIconChanges = Game.Icon != EditingGame.Icon;
                    }
                    else
                    {
                        UseIconChanges = true;
                    }
                    break;
                case nameof(Game.Links):
                    if (Games == null)
                    {
                        UseLinksChanges = !Game.Links.IsEqualJson(EditingGame.Links);
                    }
                    else
                    {
                        UseLinksChanges = true;
                    }
                    break;
                case nameof(Game.InstallDirectory):
                    if (Games == null)
                    {
                        UseInstallDirChanges = Game.InstallDirectory != EditingGame.InstallDirectory;
                    }
                    else
                    {
                        UseInstallDirChanges = true;
                    }
                    break;
                case nameof(Game.GameImagePath):
                    if (Games == null)
                    {
                        UseIsoPathChanges = Game.GameImagePath != EditingGame.GameImagePath;
                    }
                    else
                    {
                        UseIsoPathChanges = true;
                    }
                    break;
                case nameof(Game.Description):
                    if (Games == null)
                    {
                        UseDescriptionChanges = Game.Description != EditingGame.Description;
                    }
                    else
                    {
                        UseDescriptionChanges = true;
                    }
                    break;
                case nameof(Game.Categories):
                    if (Games == null)
                    {
                        UseCategoryChanges = !Game.Categories.IsListEqual(EditingGame.Categories);
                    }
                    else
                    {
                        UseCategoryChanges = true;
                    }
                    break;
                case nameof(Game.Tags):
                    if (Games == null)
                    {
                        UseTagChanges = !Game.Tags.IsListEqual(EditingGame.Tags);
                    }
                    else
                    {
                        UseTagChanges = true;
                    }
                    break;
                case nameof(Game.Genres):
                    if (Games == null)
                    {
                        UseGenresChanges = !Game.Genres.IsListEqual(EditingGame.Genres);
                    }
                    else
                    {
                        UseGenresChanges = true;
                    }
                    break;
                case nameof(Game.ReleaseDate):
                    if (Games == null)
                    {
                        UseReleaseDateChanges = Game.ReleaseDate != EditingGame.ReleaseDate;
                    }
                    else
                    {
                        UseReleaseDateChanges = true;
                    }
                    break;
                case nameof(Game.Developers):
                    if (Games == null)
                    {
                        UseDeveloperChanges = !Game.Developers.IsListEqual(EditingGame.Developers);
                    }
                    else
                    {
                        UseDeveloperChanges = true;
                    }
                    break;
                case nameof(Game.Publishers):
                    if (Games == null)
                    {
                        UsePublisherChanges = !Game.Publishers.IsListEqual(EditingGame.Publishers);
                    }
                    else
                    {
                        UsePublisherChanges = true;
                    }
                    break;
                case nameof(Game.LastActivity):
                    if (Games == null)
                    {
                        UseLastActivityChanges = Game.LastActivity != EditingGame.LastActivity;
                    }
                    else
                    {
                        UseLastActivityChanges = true;
                    }
                    break;
                case nameof(Game.Playtime):
                    if (Games == null)
                    {
                        UsePlaytimeChanges = Game.Playtime != EditingGame.Playtime;
                    }
                    else
                    {
                        UsePlaytimeChanges = true;
                    }
                    break;
                case nameof(Game.Added):
                    if (Games == null)
                    {
                        UseAddedChanges = Game.Added != EditingGame.Added;
                    }
                    else
                    {
                        UseAddedChanges = true;
                    }
                    break;
                case nameof(Game.PlayCount):
                    if (Games == null)
                    {
                        UsePlayCountChanges = Game.PlayCount != EditingGame.PlayCount;
                    }
                    else
                    {
                        UsePlayCountChanges = true;
                    }
                    break;
                case nameof(Game.Series):
                    if (Games == null)
                    {
                        UseSeriesChanges = Game.Series != EditingGame.Series;
                    }
                    else
                    {
                        UseSeriesChanges = true;
                    }
                    break;
                case nameof(Game.Version):
                    if (Games == null)
                    {
                        UseVersionChanges = Game.Version != EditingGame.Version;
                    }
                    else
                    {
                        UseVersionChanges = true;
                    }
                    break;
                case nameof(Game.AgeRating):
                    if (Games == null)
                    {
                        UseAgeRatingChanges = Game.AgeRating != EditingGame.AgeRating;
                    }
                    else
                    {
                        UseAgeRatingChanges = true;
                    }
                    break;
                case nameof(Game.Region):
                    if (Games == null)
                    {
                        UseRegionChanges = Game.Region != EditingGame.Region;
                    }
                    else
                    {
                        UseRegionChanges = true;
                    }
                    break;
                case nameof(Game.Source):
                    if (Games == null)
                    {
                        UseSourceChanges = Game.Source != EditingGame.Source;
                    }
                    else
                    {
                        UseSourceChanges = true;
                    }
                    break;
                case nameof(Game.CompletionStatus):
                    if (Games == null)
                    {
                        UseCompletionStatusChanges = Game.CompletionStatus != EditingGame.CompletionStatus;
                    }
                    else
                    {
                        UseCompletionStatusChanges = true;
                    }
                    break;
                case nameof(Game.UserScore):
                    if (Games == null)
                    {
                        UseUserScoreChanges = Game.UserScore != EditingGame.UserScore;
                    }
                    else
                    {
                        UseUserScoreChanges = true;
                    }
                    break;
                case nameof(Game.CriticScore):
                    if (Games == null)
                    {
                        UseCriticScoreChanges = Game.CriticScore != EditingGame.CriticScore;
                    }
                    else
                    {
                        UseCriticScoreChanges = true;
                    }
                    break;
                case nameof(Game.CommunityScore):
                    if (Games == null)
                    {
                        UseCommunityScoreChanges = Game.CommunityScore != EditingGame.CommunityScore;
                    }
                    else
                    {
                        UseCommunityScoreChanges = true;
                    }
                    break; ;
                case nameof(Game.Favorite):
                    if (Games == null)
                    {
                        UseFavoriteChanges = Game.Favorite != EditingGame.Favorite;
                    }
                    else
                    {
                        UseFavoriteChanges = true;
                    }
                    break; ;
                case nameof(Game.Hidden):
                    if (Games == null)
                    {
                        UseHiddenChanges = Game.Hidden != EditingGame.Hidden;
                    }
                    else
                    {
                        UseHiddenChanges = true;
                    }
                    break;
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
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

            window.Close(false);
        }

        public void ConfirmDialog()
        {
            if (Games == null)
            {
                if (string.IsNullOrWhiteSpace(EditingGame.Name))
                {
                    dialogs.ShowMessage(
                        resources.FindString("LOCEmptyGameNameError"),
                        resources.FindString("LOCInvalidGameData"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (UseNameChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Name = EditingGame.Name;
                    }
                }
                else
                {
                    Game.Name = EditingGame.Name;
                }
            }

            if (UseSortingNameChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.SortingName = EditingGame.SortingName;
                    }
                }
                else
                {
                    Game.SortingName = EditingGame.SortingName;
                }
            }

            if (UseGenresChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Genres = EditingGame.Genres;
                    }
                }
                else
                {
                    Game.Genres = EditingGame.Genres;
                }
            }

            if (UseReleaseDateChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.ReleaseDate = EditingGame.ReleaseDate;
                    }
                }
                else
                {
                    Game.ReleaseDate = EditingGame.ReleaseDate;
                }
            }

            if (UseDeveloperChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Developers = EditingGame.Developers;
                    }
                }
                else
                {
                    Game.Developers = EditingGame.Developers;
                }
            }

            if (UsePublisherChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Publishers = EditingGame.Publishers;
                    }
                }
                else
                {
                    Game.Publishers = EditingGame.Publishers;
                }
            }

            if (UseCategoryChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Categories = EditingGame.Categories;
                    }
                }
                else
                {
                    Game.Categories = EditingGame.Categories;
                }
            }

            if (UseTagChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Tags = EditingGame.Tags;
                    }
                }
                else
                {
                    Game.Tags = EditingGame.Tags;
                }
            }

            if (UseDescriptionChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Description = EditingGame.Description;
                    }
                }
                else
                {
                    Game.Description = EditingGame.Description;
                }
            }

            if (Games == null)
            {
                if (Game.InstallDirectory != EditingGame.InstallDirectory)
                {
                    Game.InstallDirectory = EditingGame.InstallDirectory;
                }
            }

            if (Games == null)
            {
                if (Game.GameImagePath != EditingGame.GameImagePath)
                {
                    Game.GameImagePath = EditingGame.GameImagePath;
                }
            }

            if (UsePlatformChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.PlatformId = EditingGame.PlatformId;
                    }
                }
                else
                {
                    Game.PlatformId = EditingGame.PlatformId;
                }
            }

            if (UseLastActivityChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.LastActivity = EditingGame.LastActivity;
                    }
                }
                else
                {
                    Game.LastActivity = EditingGame.LastActivity;
                }
            }

            if (UsePlaytimeChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Playtime = EditingGame.Playtime;
                    }
                }
                else
                {
                    Game.Playtime = EditingGame.Playtime;
                }
            }

            if (UseAddedChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Added = EditingGame.Added;
                    }
                }
                else
                {
                    Game.Added = EditingGame.Added;
                }
            }

            if (UsePlayCountChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.PlayCount = EditingGame.PlayCount;
                    }
                }
                else
                {
                    Game.PlayCount = EditingGame.PlayCount;
                }
            }

            if (UseSeriesChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Series = EditingGame.Series;
                    }
                }
                else
                {
                    Game.Series = EditingGame.Series;
                }
            }

            if (UseVersionChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Version = EditingGame.Version;
                    }
                }
                else
                {
                    Game.Version = EditingGame.Version;
                }
            }

            if (UseAgeRatingChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.AgeRating = EditingGame.AgeRating;
                    }
                }
                else
                {
                    Game.AgeRating = EditingGame.AgeRating;
                }
            }

            if (UseRegionChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Region = EditingGame.Region;
                    }
                }
                else
                {
                    Game.Region = EditingGame.Region;
                }
            }

            if (UseSourceChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Source = EditingGame.Source;
                    }
                }
                else
                {
                    Game.Source = EditingGame.Source;
                }
            }

            if (UseCompletionStatusChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.CompletionStatus = EditingGame.CompletionStatus;
                    }
                }
                else
                {
                    Game.CompletionStatus = EditingGame.CompletionStatus;
                }
            }

            if (UseUserScoreChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.UserScore = EditingGame.UserScore;
                    }
                }
                else
                {
                    Game.UserScore = EditingGame.UserScore;
                }
            }

            if (UseCriticScoreChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.CriticScore = EditingGame.CriticScore;
                    }
                }
                else
                {
                    Game.CriticScore = EditingGame.CriticScore;
                }
            }

            if (UseCommunityScoreChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.CommunityScore = EditingGame.CommunityScore;
                    }
                }
                else
                {
                    Game.CommunityScore = EditingGame.CommunityScore;
                }
            }

            if (UseFavoriteChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Favorite = EditingGame.Favorite;
                    }
                }
                else
                {
                    Game.Favorite = EditingGame.Favorite;
                }
            }

            if (UseHiddenChanges)
            {
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Hidden = EditingGame.Hidden;
                    }
                }
                else
                {
                    Game.Hidden = EditingGame.Hidden;
                }
            }

            if (UseIconChanges)
            {
                if (string.IsNullOrEmpty(EditingGame.Icon))
                {
                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            RemoveGameMedia(game.Icon);
                            game.Icon = null;
                        }
                    }
                    else
                    {
                        RemoveGameMedia(Game.Icon);
                        Game.Icon = null;
                    }
                }
                else if (File.Exists(EditingGame.Icon))
                {
                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            RemoveGameMedia(game.Icon);
                            game.Icon = database.AddFile(EditingGame.Icon, game.Id);
                        }
                    }
                    else
                    {
                        RemoveGameMedia(Game.Icon);
                        Game.Icon = database.AddFile(EditingGame.Icon, game.Id); ;
                    }

                    if (Path.GetDirectoryName(EditingGame.Icon) == PlaynitePaths.TempPath)
                    {
                        FileSystem.DeleteFile(EditingGame.Icon);
                    }
                }
            }

            if (UseImageChanges)
            {
                if (string.IsNullOrEmpty(EditingGame.CoverImage))
                {
                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            RemoveGameMedia(game.CoverImage);
                            game.CoverImage = null;
                        }
                    }
                    else
                    {
                        RemoveGameMedia(Game.CoverImage);
                        Game.CoverImage = null;
                    }
                }
                else if (File.Exists(EditingGame.CoverImage))
                {
                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            RemoveGameMedia(game.CoverImage);
                            game.CoverImage = database.AddFile(EditingGame.CoverImage, game.Id);
                        }
                    }
                    else
                    {
                        RemoveGameMedia(Game.CoverImage);
                        Game.CoverImage = database.AddFile(EditingGame.CoverImage, Game.Id);
                    }

                    if (Path.GetDirectoryName(EditingGame.CoverImage) == PlaynitePaths.TempPath)
                    {
                        FileSystem.DeleteFile(EditingGame.CoverImage);
                    }
                }
            }

            if (UseBackgroundChanges)
            {
                if (string.IsNullOrEmpty(EditingGame.BackgroundImage))
                {
                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            if (!string.IsNullOrEmpty(game.BackgroundImage))
                            {
                                RemoveGameMedia(game.BackgroundImage);
                                game.BackgroundImage = null;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Game.BackgroundImage))
                        {
                            RemoveGameMedia(Game.BackgroundImage);
                            Game.BackgroundImage = null;
                        }
                    }
                }
                else
                {
                    if (EditingGame.BackgroundImage.IsHttpUrl())
                    {
                        if (Games != null)
                        {
                            foreach (var game in Games)
                            {
                                RemoveGameMedia(game.BackgroundImage);
                                game.BackgroundImage = EditingGame.BackgroundImage;
                            }
                        }
                        else
                        {
                            RemoveGameMedia(Game.BackgroundImage);
                            Game.BackgroundImage = EditingGame.BackgroundImage;
                        }
                    }
                    else if (File.Exists(EditingGame.BackgroundImage))
                    {
                        if (Games != null)
                        {
                            foreach (var game in Games)
                            {
                                RemoveGameMedia(game.BackgroundImage);
                                game.BackgroundImage = database.AddFile(EditingGame.BackgroundImage, game.Id);
                            }
                        }
                        else
                        {
                            RemoveGameMedia(Game.BackgroundImage);
                            Game.BackgroundImage = database.AddFile(EditingGame.BackgroundImage, Game.Id);
                        }

                        if (Path.GetDirectoryName(EditingGame.BackgroundImage) == PlaynitePaths.TempPath)
                        {
                            FileSystem.DeleteFile(EditingGame.BackgroundImage);
                        }
                    }
                }
            }

            if (Games == null)
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

                if (Game.IsInstalled != EditingGame.IsInstalled)
                {
                    Game.IsInstalled = EditingGame.IsInstalled;
                }
            }

            if (Games != null)
            {
                var date = DateTime.Today;
                foreach (var game in Games)
                {
                    game.Modified = date;
                    database.Games.Update(game);
                }
            }
            else
            {
                game.Modified = DateTime.Today;
                database.Games.Update(Game);
            }

            window.Close(true);
        }

        public void PreviewGameData(Game game)
        {
            var listConverter = new ListToStringConverter();
            var dateConverter = new NullableDateToStringConverter();

            if (!string.IsNullOrEmpty(game.Name))
            {
                EditingGame.Name = game.Name;
            }

            if (game.Developers != null && game.Developers.Count != 0)
            {
                EditingGame.Developers = game.Developers;
            }

            if (game.Publishers != null && game.Publishers.Count != 0)
            {
                EditingGame.Publishers = game.Publishers;
            }

            if (game.Genres != null && game.Genres.Count != 0)
            {
                EditingGame.Genres = game.Genres;
            }

            if (game.Tags != null && game.Tags.Count != 0)
            {
                EditingGame.Tags = game.Tags;
            }

            if (game.ReleaseDate != null)
            {
                EditingGame.ReleaseDate = game.ReleaseDate;
            }

            if (!string.IsNullOrEmpty(game.Description))
            {
                EditingGame.Description = game.Description;
            }

            if (game.Links != null)
            {
                EditingGame.Links = game.Links;
            }

            if (!string.IsNullOrEmpty(game.BackgroundImage))
            {
                EditingGame.BackgroundImage = game.BackgroundImage;
            }

            if (game.CriticScore != null)
            {
                EditingGame.CriticScore = game.CriticScore;
            }

            if (game.CommunityScore != null)
            {
                EditingGame.CommunityScore = game.CommunityScore;
            }

            if (!string.IsNullOrEmpty(game.AgeRating))
            {
                EditingGame.AgeRating = game.AgeRating;
            }

            if (!string.IsNullOrEmpty(game.Region))
            {
                EditingGame.Region = game.Region;
            }

            if (!string.IsNullOrEmpty(game.Series))
            {
                EditingGame.Series = game.Series;
            }

            if (!string.IsNullOrEmpty(game.CoverImage))
            {
                if (game.CoverImage.IsHttpUrl())
                {
                    var extension = Path.GetExtension(game.CoverImage);
                    var tempPath = Path.Combine(PlaynitePaths.TempPath, "tempimage" + extension);
                    FileSystem.PrepareSaveFile(tempPath);

                    try
                    {
                        HttpDownloader.DownloadFile(game.CoverImage, tempPath);
                        EditingGame.CoverImage = tempPath;
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to download web image to preview game data.");
                    }
                }
                else
                {
                    EditingGame.CoverImage = game.CoverImage;
                }
            }

            if (!string.IsNullOrEmpty(game.BackgroundImage))
            {
                EditingGame.BackgroundImage = game.BackgroundImage;
            }

            if (!string.IsNullOrEmpty(game.Icon))
            {
                EditingGame.Icon = game.Icon;
            }
        }

        private string SaveFileIconToTemp(string exePath)
        {
            var ico = IconExtension.ExtractIconFromExe(exePath, true);
            if (ico == null)
            {
                return string.Empty;
            }

            var tempPath = Path.Combine(PlaynitePaths.TempPath, "tempico.png");
            if (ico != null)
            {
                FileSystem.PrepareSaveFile(tempPath);
                ico.ToBitmap().Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return tempPath;
        }

        private string SaveConvertedTgaToTemp(string tgaPath)
        {
            var tempPath = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + ".png");
            File.WriteAllBytes(tempPath, BitmapExtensions.TgaToBitmap(tgaPath).ToPngArray());
            return tempPath;
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
            }
        }

        public void UseExeIcon()
        {
            if (EditingGame.PlayAction == null || EditingGame.PlayAction.Type == GameActionType.URL)
            {
                dialogs.ShowMessage(resources.FindString("LOCExecIconMissingPlayAction"));
                return;
            }

            var path = game.GetRawExecutablePath();
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

        public void SelectCover()
        {
            var path = dialogs.SelectImagefile();
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    path = SaveConvertedTgaToTemp(path);
                }

                EditingGame.CoverImage = path;
            }
        }

        public void SelectBackground()
        {
            var path = dialogs.SelectImagefile();
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    path = SaveConvertedTgaToTemp(path);
                }

                EditingGame.BackgroundImage = path;
            }
        }

        public void SetBackgroundUrl()
        {
            var url = dialogs.SelectString(
                resources.FindString("LOCURLInputInfo"),
                resources.FindString("LOCURLInputInfoTitile"),
                string.Empty);

            if (url.Result)
            {
                if (url.SelectedString.IsHttpUrl())
                {
                    if (HttpDownloader.GetResponseCode(url.SelectedString) == HttpStatusCode.OK)
                    {
                        EditingGame.BackgroundImage = url.SelectedString;
                        return;
                    }
                }

                dialogs.ShowErrorMessage(resources.FindString("LOCInvalidURL"), string.Empty);
            }
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

        public void SelectCategories(CategoryConfigViewModel model)
        {
            model.OpenView();
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

        public void DoMetadataLookup(MetadataLookupViewModel model)
        {
            if (string.IsNullOrEmpty(EditingGame.Name))
            {
                dialogs.ShowMessage(resources.FindString("LOCEmptyGameNameMetaSearchError"), "", MessageBoxButton.OK);
                return;
            }
            
            model.SearchTerm = EditingGame.Name;
            if (model.OpenView() == true)
            {
                ShowCheckBoxes = true;
                PreviewGameData(model.MetadataData);
            }
        }       

        public async void DownloadStoreData()
        {
            ProgressVisible = true;

            await Task.Run(() =>
            {
                try
                {
                    GameMetadata metadata;
                    var tempGame = game.CloneJson();
                    tempGame.CoverImage = string.Empty;

                    if (extensions.LibraryPlugins.TryGetValue(tempGame.PluginId, out var plugin))
                    {                        
                        var downloader = plugin.Plugin.GetMetadataDownloader();
                        try
                        {
                            metadata = downloader.GetMetadata(tempGame);
                            metadata?.GameData?.CopyProperties(tempGame, false);
                        }
                        finally
                        {
                            // TODO move to proper disposable
                            if (downloader.HasMethod("Dispose"))
                            {
                                (downloader as dynamic)?.Dispose();
                            }
                        }
                    }
                    else
                    {
                        dialogs.ShowErrorMessage(
                            resources.FindString("LOCErrorLibraryPluginNotFound"),
                            resources.FindString("LOCGameError"));
                        return;
                    }
                    
                    if (metadata.Image != null)
                    {
                        var path = Path.Combine(PlaynitePaths.TempPath, metadata.Image.FileName);
                        FileSystem.PrepareSaveFile(path);
                        File.WriteAllBytes(path, metadata.Image.Content);
                        tempGame.CoverImage = path;
                    }

                    if (metadata.Icon != null)
                    {
                        var path = Path.Combine(PlaynitePaths.TempPath, metadata.Icon.FileName);
                        FileSystem.PrepareSaveFile(path);
                        File.WriteAllBytes(path, metadata.Icon.Content);
                        tempGame.Icon = path;
                    }

                    ShowCheckBoxes = true;
                    PreviewGameData(tempGame);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, string.Format("Failed to download metadata, {0}, {1}", game.PluginId, game.GameId));
                    dialogs.ShowMessage(
                        string.Format(resources.FindString("LOCMetadataDownloadError"), exc.Message),
                        resources.FindString("LOCDownloadError"),
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

        private void RemoveGameMedia(string mediaId)
        {
            if (string.IsNullOrEmpty(mediaId))
            {
                return;
            }

            if (mediaId.IsHttpUrl())
            {
                try
                {
                    HttpFileCache.ClearCache(mediaId);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to clean file from cache {mediaId}");
                }
            }
            else
            {
                database.RemoveFile(mediaId);
            }
        }
    }
}
