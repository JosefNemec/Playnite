using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public partial class GameEditViewModel
    {
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
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
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
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
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
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
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
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
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
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
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
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
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

        private bool useGameStartedScriptChanges;
        public bool UseGameStartedScriptChanges
        {
            get
            {
                return useGameStartedScriptChanges;
            }

            set
            {
                useGameStartedScriptChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowScriptsChangeNotif));
            }
        }

        private bool useGameStartedGlobalScriptChanges;
        public bool UseGameStartedGlobalScriptChanges
        {
            get
            {
                return useGameStartedGlobalScriptChanges;
            }

            set
            {
                useGameStartedGlobalScriptChanges = value;
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

        private bool usePlayActionChanges;
        public bool UsePlayActionChanges
        {
            get
            {
                return usePlayActionChanges;
            }

            set
            {
                usePlayActionChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowActionsChangeNotif));
            }
        }

        private bool useOtherActionsChanges;
        public bool UseOtherActionsChanges
        {
            get
            {
                return useOtherActionsChanges;
            }

            set
            {
                useOtherActionsChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowActionsChangeNotif));
            }
        }

        private bool useNotesChanges;
        public bool UseNotesChanges
        {
            get
            {
                return useNotesChanges;
            }

            set
            {
                useNotesChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
            }
        }

        private bool useManualChanges;
        public bool UseManualChanges
        {
            get
            {
                return useManualChanges;
            }

            set
            {
                useManualChanges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowAdvancedChangeNotif));
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
                    UseCompletionStatusChanges ||
                    UseNotesChanges ||
                    UseManualChanges);
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
                    UsePostGlobalScriptChanges ||
                    UseGameStartedGlobalScriptChanges ||
                    UseGameStartedScriptChanges);
            }
        }

        public bool ShowActionsChangeNotif
        {
            get
            {
                return ShowCheckBoxes &&
                    (UseOtherActionsChanges ||
                    UsePlayActionChanges);
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
                case nameof(Game.Notes):
                    if (IsSingleGameEdit)
                    {
                        UseNotesChanges = Game.Notes != EditingGame.Notes;
                    }
                    else
                    {
                        UseNotesChanges = true;
                    }
                    break;
                case nameof(Game.Manual):
                    if (IsSingleGameEdit)
                    {
                        UseManualChanges = Game.Manual != EditingGame.Manual;
                    }
                    else
                    {
                        UseManualChanges = true;
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
                case nameof(Game.GameStartedScript):
                    if (IsSingleGameEdit)
                    {
                        UseGameStartedScriptChanges = !string.Equals(Game.GameStartedScript, EditingGame.GameStartedScript, StringComparison.Ordinal);
                    }
                    else
                    {
                        UseGameStartedScriptChanges = true;
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
                case nameof(Game.UseGlobalGameStartedScript):
                    if (IsSingleGameEdit)
                    {
                        UseGameStartedGlobalScriptChanges = Game.UseGlobalGameStartedScript != EditingGame.UseGlobalGameStartedScript;
                    }
                    else
                    {
                        UseGameStartedGlobalScriptChanges = true;
                    }
                    break;
                case nameof(Game.PlayAction):
                    UsePlayActionChanges = true;
                    break;
                case nameof(Game.OtherActions):
                    UseOtherActionsChanges = true;
                    break;
            }
        }

        private void OtherActions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UseOtherActionsChanges = true;
        }

        private void OtherAction_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UseOtherActionsChanges = true;
        }

        private void PlayAction_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UsePlayActionChanges = true;
        }
    }
}
