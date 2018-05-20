﻿using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite.Providers.Steam;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using System.ComponentModel;
using Playnite.Providers.BattleNet;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace PlayniteUI.ViewModels
{
    public class GameEditViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;
        private Settings appSettings;

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
                OnPropertyChanged("UseNameChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseSortingNameChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UsePlatformChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseGenresChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseReleaseDateChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseDeveloperChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UsePublisherChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseCategoryChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseTagChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseDescriptionChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseIconChanges");
                OnPropertyChanged("ShowMediaChangeNotif");
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
                OnPropertyChanged("UseImageChanges");
                OnPropertyChanged("ShowMediaChangeNotif");
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
                OnPropertyChanged("UseBackgroundChanges");
                OnPropertyChanged("ShowMediaChangeNotif");
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
                OnPropertyChanged("UseInstallDirChanges");
                OnPropertyChanged("ShowInstallChangeNotif");
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
                OnPropertyChanged("UseIsoPathChanges");
                OnPropertyChanged("ShowInstallChangeNotif");
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
                OnPropertyChanged("UseLinksChanges");
                OnPropertyChanged("ShowLinksChangeNotif");
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
                OnPropertyChanged("UseLastActivityChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UsePlaytimeChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseAddedChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UsePlayCountChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseSeriesChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseVersionChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseAgeRatingChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseRegionChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseSourceChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseCompletionStatusChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseUserScoreChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseCriticScoreChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseCommunityScoreChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseHiddenChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("UseFavoriteChanges");
                OnPropertyChanged("ShowGeneralChangeNotif");
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
                OnPropertyChanged("EditingGame");
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
                OnPropertyChanged("Game");
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
                OnPropertyChanged("Games");
            }
        }

        private bool progressVisible = false;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                OnPropertyChanged("ProgressVisible");
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
                OnPropertyChanged("ShowActions");
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
                OnPropertyChanged("ShowLinks");
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
                OnPropertyChanged("ShowInstallation");
            }
        }

        public bool ShowBackgroundUrl
        {
            get
            {
                return EditingGame == null || EditingGame.BackgroundImage == null ? false : EditingGame.BackgroundImage.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
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
                OnPropertyChanged("ShowMetaDownload");
            }
        }

        public List<Platform> Platforms
        {
            get
            {
                var platforms = database.PlatformsCollection.FindAll().OrderBy(a => a.Name).ToList();
                platforms.Insert(0, new Platform(string.Empty));
                return platforms;
            }
        }

        public List<Emulator> Emulators
        {
            get
            {
                if (EditingGame.PlatformId == null || EditingGame.PlatformId == null)
                {
                    return database.EmulatorsCollection.FindAll().OrderBy(a => a.Name).ToList();
                }
                else
                {
                    return database.EmulatorsCollection.FindAll()
                        .Where(a => a.Profiles != null && a.Profiles.FirstOrDefault(b => b.Platforms?.Contains(EditingGame.PlatformId) == true) != null)
                        .OrderBy(a => a.Name).ToList();
                }
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
                OnPropertyChanged("ShowCheckBoxes");
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

        public RelayCommand<GameTask> MoveUpActionCommand
        {
            get => new RelayCommand<GameTask>((action) =>
            {
                MoveActionUp(action);
            });
        }

        public RelayCommand<GameTask> MoveDownActionCommand
        {
            get => new RelayCommand<GameTask>((action) =>
            {
                MoveActionDown(action);
            });
        }

        public RelayCommand<GameTask> DeleteActionCommand
        {
            get => new RelayCommand<GameTask>((action) =>
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

        public GameEditViewModel(Game game, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            Game = game;
            EditingGame = game.CloneJson();
            ShowCheckBoxes = false;
            ShowMetaDownload = true;
            ShowLinks = true;
            ShowActions = true;
            ShowInstallation = Game.Provider == Provider.Custom;
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;
        }

        public GameEditViewModel(IEnumerable<Game> games, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            Games = games;
            var previewGame = GameHandler.GetMultiGameEditObject(games);
            EditingGame = previewGame;
            ShowCheckBoxes = true;
            ShowMetaDownload = false;
            ShowLinks = false;
            ShowActions = false;
            ShowInstallation = false;
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;
        }

        public GameEditViewModel(Game game, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources, Settings appSettings)
            : this(game, database, window, dialogs, resources)
        {
            this.appSettings = appSettings;
        }

        public GameEditViewModel(IEnumerable<Game> games, GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources, Settings appSettings)
            : this(games, database, window, dialogs, resources)
        {
            this.appSettings = appSettings;
        }

        private void EditingGame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PlatformId")
            {
                OnPropertyChanged("Emulators");
            }

            switch (e.PropertyName)
            {
                case "Name":
                    if (Games == null)
                    {
                        UseNameChanges = Game.Name != EditingGame.Name;
                    }
                    else
                    {
                        UseNameChanges = true;
                    }
                    break;
                case "SortingName":
                    if (Games == null)
                    {
                        UseSortingNameChanges = Game.SortingName != EditingGame.SortingName;
                    }
                    else
                    {
                        UseSortingNameChanges = true;
                    }
                    break;
                case "PlatformId":
                    if (Games == null)
                    {
                        UsePlatformChanges = Game.PlatformId != EditingGame.PlatformId;
                    }
                    else
                    {
                        UsePlatformChanges = true;
                    }
                    break;
                case "Image":
                    if (Games == null)
                    {
                        UseImageChanges = Game.Image != EditingGame.Image;
                    }
                    else
                    {
                        UseImageChanges = true;
                    }
                    break;
                case "BackgroundImage":
                    if (Games == null)
                    {
                        UseBackgroundChanges = Game.BackgroundImage != EditingGame.BackgroundImage;
                    }
                    else
                    {
                        UseBackgroundChanges = true;
                    }

                    OnPropertyChanged("ShowBackgroundUrl");
                    break;
                case "Icon":
                    if (Games == null)
                    {
                        UseIconChanges = Game.Icon != EditingGame.Icon;
                    }
                    else
                    {
                        UseIconChanges = true;
                    }
                    break;
                case "Links":
                    if (Games == null)
                    {
                        UseLinksChanges = !Game.Links.IsEqualJson(EditingGame.Links);
                    }
                    else
                    {
                        UseLinksChanges = true;
                    }
                    break;
                case "InstallDirectory":
                    if (Games == null)
                    {
                        UseInstallDirChanges = Game.InstallDirectory != EditingGame.InstallDirectory;
                    }
                    else
                    {
                        UseInstallDirChanges = true;
                    }
                    break;
                case "IsoPath":
                    if (Games == null)
                    {
                        UseIsoPathChanges = Game.IsoPath != EditingGame.IsoPath;
                    }
                    else
                    {
                        UseIsoPathChanges = true;
                    }
                    break;
                case "Description":
                    if (Games == null)
                    {
                        UseDescriptionChanges = Game.Description != EditingGame.Description;
                    }
                    else
                    {
                        UseDescriptionChanges = true;
                    }
                    break;
                case "Categories":
                    if (Games == null)
                    {
                        UseCategoryChanges = !Game.Categories.IsListEqual(EditingGame.Categories);
                    }
                    else
                    {
                        UseCategoryChanges = true;
                    }
                    break;
                case "Tags":
                    if (Games == null)
                    {
                        UseTagChanges = !Game.Tags.IsListEqual(EditingGame.Tags);
                    }
                    else
                    {
                        UseTagChanges = true;
                    }
                    break;
                case "Genres":
                    if (Games == null)
                    {
                        UseGenresChanges = !Game.Genres.IsListEqual(EditingGame.Genres);
                    }
                    else
                    {
                        UseGenresChanges = true;
                    }
                    break;
                case "ReleaseDate":
                    if (Games == null)
                    {
                        UseReleaseDateChanges = Game.ReleaseDate != EditingGame.ReleaseDate;
                    }
                    else
                    {
                        UseReleaseDateChanges = true;
                    }
                    break;
                case "Developers":
                    if (Games == null)
                    {
                        UseDeveloperChanges = !Game.Developers.IsListEqual(EditingGame.Developers);
                    }
                    else
                    {
                        UseDeveloperChanges = true;
                    }
                    break;
                case "Publishers":
                    if (Games == null)
                    {
                        UsePublisherChanges = !Game.Publishers.IsListEqual(EditingGame.Publishers);
                    }
                    else
                    {
                        UsePublisherChanges = true;
                    }
                    break;
                case "LastActivity":
                    if (Games == null)
                    {
                        UseLastActivityChanges = Game.LastActivity != EditingGame.LastActivity;
                    }
                    else
                    {
                        UseLastActivityChanges = true;
                    }
                    break;
                case "Playtime":
                    if (Games == null)
                    {
                        UsePlaytimeChanges = Game.Playtime != EditingGame.Playtime;
                    }
                    else
                    {
                        UsePlaytimeChanges = true;
                    }
                    break;
                case "Added":
                    if (Games == null)
                    {
                        UseAddedChanges = Game.Added != EditingGame.Added;
                    }
                    else
                    {
                        UseAddedChanges = true;
                    }
                    break;
                case "PlayCount":
                    if (Games == null)
                    {
                        UsePlayCountChanges = Game.PlayCount != EditingGame.PlayCount;
                    }
                    else
                    {
                        UsePlayCountChanges = true;
                    }
                    break;
                case "Series":
                    if (Games == null)
                    {
                        UseSeriesChanges = Game.Series != EditingGame.Series;
                    }
                    else
                    {
                        UseSeriesChanges = true;
                    }
                    break;
                case "Version":
                    if (Games == null)
                    {
                        UseVersionChanges = Game.Version != EditingGame.Version;
                    }
                    else
                    {
                        UseVersionChanges = true;
                    }
                    break;
                case "AgeRating":
                    if (Games == null)
                    {
                        UseAgeRatingChanges = Game.AgeRating != EditingGame.AgeRating;
                    }
                    else
                    {
                        UseAgeRatingChanges = true;
                    }
                    break;
                case "Region":
                    if (Games == null)
                    {
                        UseRegionChanges = Game.Region != EditingGame.Region;
                    }
                    else
                    {
                        UseRegionChanges = true;
                    }
                    break;
                case "Source":
                    if (Games == null)
                    {
                        UseSourceChanges = Game.Source != EditingGame.Source;
                    }
                    else
                    {
                        UseSourceChanges = true;
                    }
                    break;
                case "CompletionStatus":
                    if (Games == null)
                    {
                        UseCompletionStatusChanges = Game.CompletionStatus != EditingGame.CompletionStatus;
                    }
                    else
                    {
                        UseCompletionStatusChanges = true;
                    }
                    break;
                case "UserScore":
                    if (Games == null)
                    {
                        UseUserScoreChanges = Game.UserScore != EditingGame.UserScore;
                    }
                    else
                    {
                        UseUserScoreChanges = true;
                    }
                    break;
                case "CriticScore":
                    if (Games == null)
                    {
                        UseCriticScoreChanges = Game.CriticScore != EditingGame.CriticScore;
                    }
                    else
                    {
                        UseCriticScoreChanges = true;
                    }
                    break;
                case "CommunityScore":
                    if (Games == null)
                    {
                        UseCommunityScoreChanges = Game.CommunityScore != EditingGame.CommunityScore;
                    }
                    else
                    {
                        UseCommunityScoreChanges = true;
                    }
                    break; ;
                case "Favorite":
                    if (Games == null)
                    {
                        UseFavoriteChanges = Game.Favorite != EditingGame.Favorite;
                    }
                    else
                    {
                        UseFavoriteChanges = true;
                    }
                    break; ;
                case "Hidden":
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
                if (Game.IsoPath != EditingGame.IsoPath)
                {
                    Game.IsoPath = EditingGame.IsoPath;
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
                            if (!string.IsNullOrEmpty(game.Icon))
                            {
                                database.DeleteImageSafe(game.Icon, game);
                                game.Icon = null;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Game.Icon))
                        {
                            database.DeleteImageSafe(Game.Icon, Game);
                            Game.Icon = null;
                        }
                    }
                }
                else if (File.Exists(EditingGame.Icon))
                {
                    var iconPath = EditingGame.Icon;
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(iconPath);
                    var iconId = "images/custom/" + fileName;
                    iconId = database.AddFileNoDuplicate(iconId, fileName, File.ReadAllBytes(iconPath));

                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            if (!string.IsNullOrEmpty(game.Icon) && game.Icon != iconId)
                            {
                                database.DeleteImageSafe(game.Icon, game);
                            }

                            game.Icon = iconId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Game.Icon) && Game.Icon != iconId)
                        {
                            database.DeleteImageSafe(Game.Icon, Game);
                        }

                        Game.Icon = iconId;
                    }

                    if (Path.GetDirectoryName(iconPath) == Paths.TempPath)
                    {
                        File.Delete(iconPath);
                    }
                }
            }

            if (UseImageChanges)
            {
                if (string.IsNullOrEmpty(EditingGame.Image))
                {
                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            if (!string.IsNullOrEmpty(game.Image))
                            {
                                database.DeleteImageSafe(game.Image, game);
                                game.Image = null;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Game.Image))
                        {
                            database.DeleteImageSafe(Game.Image, Game);
                            Game.Image = null;
                        }
                    }
                }
                else if (File.Exists(EditingGame.Image))
                {
                    var imagePath = EditingGame.Image;
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagePath);
                    var imageId = "images/custom/" + fileName;
                    imageId = database.AddFileNoDuplicate(imageId, fileName, File.ReadAllBytes(imagePath));

                    if (Games != null)
                    {
                        foreach (var game in Games)
                        {
                            if (!string.IsNullOrEmpty(game.Image) && game.Image != imageId)
                            {
                                database.DeleteImageSafe(game.Image, game);
                            }

                            game.Image = imageId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Game.Image) && Game.Image != imageId)
                        {
                            database.DeleteImageSafe(Game.Image, Game);
                        }

                        Game.Image = imageId;
                    }

                    if (Path.GetDirectoryName(imagePath) == Paths.TempPath)
                    {
                        File.Delete(imagePath);
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
                                database.DeleteImageSafe(game.BackgroundImage, game);
                                game.BackgroundImage = null;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Game.BackgroundImage))
                        {
                            database.DeleteImageSafe(Game.BackgroundImage, Game);
                            Game.BackgroundImage = null;
                        }
                    }
                }
                else
                {
                    if (EditingGame.BackgroundImage.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (Games != null)
                        {
                            foreach (var game in Games)
                            {
                                if (!string.IsNullOrEmpty(game.BackgroundImage) && !game.BackgroundImage.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    database.DeleteImageSafe(game.BackgroundImage, game);
                                }

                                game.BackgroundImage = EditingGame.BackgroundImage;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Game.BackgroundImage) && !Game.BackgroundImage.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                            {
                                database.DeleteImageSafe(Game.BackgroundImage, Game);
                            }

                            Game.BackgroundImage = EditingGame.BackgroundImage;
                        }
                    }
                    else if (File.Exists(EditingGame.BackgroundImage))
                    {
                        var imagePath = EditingGame.BackgroundImage;
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagePath);
                        var imageId = "images/custom/" + fileName;
                        imageId = database.AddFileNoDuplicate(imageId, fileName, File.ReadAllBytes(imagePath));

                        if (Games != null)
                        {
                            foreach (var game in Games)
                            {
                                if (!string.IsNullOrEmpty(game.BackgroundImage) &&
                                    !game.BackgroundImage.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) &&
                                    game.BackgroundImage != imageId)
                                {
                                    database.DeleteImageSafe(game.BackgroundImage, game);
                                }

                                game.BackgroundImage = imageId;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Game.BackgroundImage) &&
                                !Game.BackgroundImage.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) &&
                                Game.BackgroundImage != imageId)
                            {
                                database.DeleteImageSafe(Game.BackgroundImage, Game);
                            }

                            Game.BackgroundImage = imageId;
                        }

                        if (Path.GetDirectoryName(imagePath) == Paths.TempPath)
                        {
                            File.Delete(imagePath);
                        }
                    }
                }
            }

            if (Games == null)
            {
                if (!Game.PlayTask.IsEqualJson(EditingGame.PlayTask))
                {
                    Game.PlayTask = EditingGame.PlayTask;
                }

                if (!Game.OtherTasks.IsEqualJson(EditingGame.OtherTasks))
                {
                    Game.OtherTasks = EditingGame.OtherTasks;
                }

                if (!Game.Links.IsEqualJson(EditingGame.Links))
                {
                    if ((ShowCheckBoxes && UseLinksChanges) || !ShowCheckBoxes)
                    {
                        Game.Links = EditingGame.Links;
                    }
                }

                if (Game.State.Installed != EditingGame.State.Installed)
                {
                    // GameState is not observable so we need to change the whole object to send notify messages
                    Game.State = new GameState(Game.State) { Installed = EditingGame.State.Installed };
                }
            }

            if (Games != null)
            {
                var date = DateTime.Today;
                foreach (var game in Games)
                {
                    game.Modified = date;
                    database.UpdateGameInDatabase(game);
                }
            }
            else
            {
                game.Modified = DateTime.Today;
                database.UpdateGameInDatabase(Game);
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

            if (!string.IsNullOrEmpty(game.Image))
            {
                if (game.Image.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    var extension = Path.GetExtension(game.Image);
                    var tempPath = Path.Combine(Paths.TempPath, "tempimage" + extension);
                    FileSystem.PrepareSaveFile(tempPath);

                    try
                    {
                        Web.DownloadFile(game.Image, tempPath);
                        EditingGame.Image = tempPath;
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to download web image to preview game data.");
                    }
                }
                else
                {
                    EditingGame.Image = game.Image;
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

            var tempPath = Path.Combine(Paths.TempPath, "tempico.png");
            if (ico != null)
            {
                FileSystem.PrepareSaveFile(tempPath);
                ico.ToBitmap().Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return tempPath;
        }

        public void SelectIcon()
        {
            var path = dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith("exe", StringComparison.CurrentCultureIgnoreCase))
                {
                    path = SaveFileIconToTemp(path);
                    if (string.IsNullOrEmpty(path))
                    {
                        return;
                    }
                }

                EditingGame.Icon = path;
            }
        }

        public void UseExeIcon()
        {
            if (EditingGame.PlayTask == null || EditingGame.PlayTask.Type == GameTaskType.URL)
            {
                dialogs.ShowMessage(resources.FindString("LOCExecIconMissingPlayAction"));
                return;
            }

            var path = EditingGame.ResolveVariables(EditingGame.PlayTask.Path);
            if (!File.Exists(path))
            {
                path = Path.Combine(
                    EditingGame.ResolveVariables(EditingGame.PlayTask.WorkingDir),
                    path);
            }

            if (!File.Exists(path))
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
                EditingGame.Image = path;
            }
        }

        public void SelectBackground()
        {
            var path = dialogs.SelectImagefile();
            if (!string.IsNullOrEmpty(path))
            {
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
                EditingGame.BackgroundImage = url.SelectedString;
            }
        }

        public void AddPlayAction()
        {
            EditingGame.PlayTask = new GameTask()
            {
                Name = "Play",
                IsBuiltIn = false
            };
        }

        public void RemovePlayAction()
        {
            EditingGame.PlayTask = null;
        }

        public void AddAction()
        {
            if (EditingGame.OtherTasks == null)
            {
                EditingGame.OtherTasks = new ObservableCollection<GameTask>();
            }

            var newTask = new GameTask()
            {
                Name = "New Action",
                IsBuiltIn = false
            };

            if (EditingGame.PlayTask != null && EditingGame.PlayTask.Type == GameTaskType.File)
            {
                newTask.WorkingDir = EditingGame.PlayTask.WorkingDir;
                newTask.Path = EditingGame.PlayTask.Path;
            }

            EditingGame.OtherTasks.Add(newTask);
        }

        public void RemoveAction(GameTask action)
        {
            EditingGame.OtherTasks.Remove(action);
        }

        public void MoveActionUp(GameTask action)
        {
            var index = EditingGame.OtherTasks.IndexOf(action);
            if (index != 0)
            {
                EditingGame.OtherTasks.Move(index, index - 1);
            }
        }

        public void MoveActionDown(GameTask action)
        {
            var index = EditingGame.OtherTasks.IndexOf(action);
            if (index != EditingGame.OtherTasks.Count - 1)
            {
                EditingGame.OtherTasks.Move(index, index + 1);
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
                EditingGame.IsoPath = path;
            }
        }

        public void RemoveIcon()
        {
            EditingGame.Icon = null;
        }

        public void RemoveImage()
        {
            EditingGame.Image = null;
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
                    tempGame.Image = string.Empty;

                    switch (tempGame.Provider)
                    {
                        case Provider.Steam:
                            metadata = (new SteamLibrary()).UpdateGameWithMetadata(tempGame, appSettings.SteamSettings);
                            break;
                        case Provider.GOG:
                            metadata = (new GogLibrary()).UpdateGameWithMetadata(tempGame);
                            break;
                        case Provider.Origin:
                            metadata = (new OriginLibrary()).UpdateGameWithMetadata(tempGame);
                            break;
                        case Provider.BattleNet:
                            metadata = (new BattleNetLibrary()).UpdateGameWithMetadata(tempGame);
                            break;
                        case Provider.Uplay:
                            return;
                        case Provider.Custom:
                        default:
                            return;
                    }
                    
                    if (metadata.Image != null)
                    {
                        var path = Path.Combine(Paths.TempPath, metadata.Image.Name);
                        FileSystem.PrepareSaveFile(path);
                        File.WriteAllBytes(path, metadata.Image.Data);
                        tempGame.Image = path;
                    }

                    if (metadata.Icon != null)
                    {
                        var path = Path.Combine(Paths.TempPath, metadata.Icon.Name);
                        FileSystem.PrepareSaveFile(path);
                        File.WriteAllBytes(path, metadata.Icon.Data);
                        tempGame.Icon = path;
                    }

                    ShowCheckBoxes = true;
                    PreviewGameData(tempGame);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to download metadata, {0}, {1}", game.Provider, game.ProviderId);
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
    }
}
