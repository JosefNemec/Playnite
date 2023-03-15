using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.DesktopApp.Markup;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Xml.Linq;
using BooleanToVisibilityConverter = Playnite.Converters.BooleanToVisibilityConverter;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ElemPlayTime", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLastPlayed", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemAdded", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemRecentActivity", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCompletionStatus", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLibrary", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemPlatform", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemGenres", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemDevelopers", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemPublishers", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemReleaseDate", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCategories", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemTags", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemFeatures", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLinks", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemDescription", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemNotes", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemAgeRating", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemSeries", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemRegion", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemSource", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemVersion", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCommunityScore", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCriticScore", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemUserScore", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_TextPlayTime", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextLastActivity", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextRecentActivity", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ButtonCompletionStatus", Type = typeof(Button))]
    [TemplatePart(Name = "PART_TextNotes", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_TextCommunityScore", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextCriticScore", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextUserScore", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ButtonLibrary", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonReleaseDate", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonSource", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonVersion", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ItemsPlatforms", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsAgeRatings", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsSeries", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsRegions", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsGenres", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsDevelopers", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsPublishers", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsCategories", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsTags", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsFeatures", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsLinks", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ButtonPlayAction", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonContextAction", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonMoreActions", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonEditGame", Type = typeof(Button))]
    [TemplatePart(Name = "PART_HtmlDescription", Type = typeof(HtmlTextView))]
    [TemplatePart(Name = "PART_ImageCover", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ImageIcon", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ImageBackground", Type = typeof(FadeImage))]
    [TemplatePart(Name = "PART_TextDisplayName", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ScrollViewHost", Type = typeof(ScrollViewer))]
    public abstract class GameOverview : Control
    {
        internal readonly DesktopView viewType;
        internal readonly DesktopAppViewModel mainModel;

        private FrameworkElement ElemPlayTime;
        private FrameworkElement ElemInstallSize;
        private FrameworkElement ElemInstallDirectory;
        private FrameworkElement ElemLastPlayed;
        private FrameworkElement ElemAdded;
        private FrameworkElement ElemRecentActivity;
        private FrameworkElement ElemCompletionStatus;
        private FrameworkElement ElemLibrary;
        private FrameworkElement ElemPlatform;
        private FrameworkElement ElemGenres;
        private FrameworkElement ElemDevelopers;
        private FrameworkElement ElemPublishers;
        private FrameworkElement ElemReleaseDate;
        private FrameworkElement ElemCategories;
        private FrameworkElement ElemTags;
        private FrameworkElement ElemFeatures;
        private FrameworkElement ElemLinks;
        private FrameworkElement ElemDescription;
        private FrameworkElement ElemNotes;
        private FrameworkElement ElemAgeRating;
        private FrameworkElement ElemSeries;
        private FrameworkElement ElemRegion;
        private FrameworkElement ElemSource;
        private FrameworkElement ElemVersion;
        private FrameworkElement ElemCommunityScore;
        private FrameworkElement ElemCriticScore;
        private FrameworkElement ElemUserScore;

        private TextBlock TextDisplayName;
        private TextBlock TextPlayTime;
        private TextBlock TextLastActivity;
        private TextBlock TextAdded;
        private TextBlock TextInstallSize;
        private TextBlock TextCommunityScore;
        private TextBlock TextCriticScore;
        private TextBlock TextUserScore;
        private TextBox TextNotes;
        private Button ButtonLibrary;
        private Button ButtonReleaseDate;
        private Button ButtonVersion;
        private Button ButtonSource;
        private Button ButtonCompletionStatus;
        private Button ButtonInstallDirectory;
        private ItemsControl ItemsGenres;
        private ItemsControl ItemsDevelopers;
        private ItemsControl ItemsPublishers;
        private ItemsControl ItemsCategories;
        private ItemsControl ItemsTags;
        private ItemsControl ItemsFeatures;
        private ItemsControl ItemsLinks;
        private ItemsControl ItemsAgeRatings;
        private ItemsControl ItemsSeries;
        private ItemsControl ItemsRegions;
        private ItemsControl ItemsPlatforms;

        private Button ButtonPlayAction;
        private Button ButtonContextAction;
        private Button ButtonMoreActions;
        private Button ButtonEditGame;
        private HtmlTextView HtmlDescription;
        private Image ImageCover;
        private Image ImageIcon;
        private FadeImage ImageBackground;
        private ScrollViewer ScrollViewHost;

        static GameOverview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameOverview), new FrameworkPropertyMetadata(typeof(GameOverview)));
        }

        public GameOverview(DesktopView viewType) : this(viewType, DesktopApplication.Current?.MainModel)
        {
        }

        public GameOverview(DesktopView viewType, DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
                DataContext = this.mainModel.SelectedGameDetails;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }

            this.viewType = viewType;
            Loaded += GameOverview_Loaded;
            Unloaded += GameOverview_Unloaded;
            DataContextChanged += GameOverview_DataContextChanged;
        }

        private void GameOverview_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewHost?.ScrollToTop();
        }

        private void GameOverview_Loaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            mainModel.AppSettings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
        }

        private void GameOverview_Unloaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.PropertyChanged -= AppSettings_PropertyChanged;
            mainModel.AppSettings.ViewSettings.PropertyChanged -= ViewSettings_PropertyChanged;
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewSettings.GamesViewType))
            {
                if (ImageBackground != null)
                {
                    SetBackgroundBinding();
                }
            }
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteSettings.DetailsVisibility))
            {
                if (ImageBackground != null)
                {
                    SetBackgroundBinding();
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ButtonPlayAction = Template.FindName("PART_ButtonPlayAction", this) as Button;
            if (ButtonPlayAction != null)
            {
                BindingTools.SetBinding(ButtonPlayAction,
                    Button.CommandProperty,
                    nameof(GameDetailsViewModel.PlayCommand));
                BindingTools.SetBinding(ButtonPlayAction,
                    Button.ContentProperty,
                    nameof(GameDetailsViewModel.ContextActionDescription));
                BindingTools.SetBinding(ButtonPlayAction,
                    Button.VisibilityProperty,
                    nameof(GameDetailsViewModel.IsPlayAvailable),
                    converter: new BooleanToVisibilityConverter());
            }

            ButtonContextAction = Template.FindName("PART_ButtonContextAction", this) as Button;
            if (ButtonContextAction != null)
            {
                BindingTools.SetBinding(ButtonContextAction,
                    Button.CommandProperty,
                    nameof(GameDetailsViewModel.ContextActionCommand));
                BindingTools.SetBinding(ButtonContextAction,
                    Button.ContentProperty,
                    nameof(GameDetailsViewModel.ContextActionDescription));
                BindingTools.SetBinding(ButtonContextAction,
                    Button.VisibilityProperty,
                    nameof(GameDetailsViewModel.IsContextAvailable),
                    converter: new BooleanToVisibilityConverter());
            }

            ButtonMoreActions = Template.FindName("PART_ButtonMoreActions", this) as Button;
            if (ButtonMoreActions != null)
            {
                LeftClickContextMenuBehavior.SetEnabled(ButtonMoreActions, true);

                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    ButtonMoreActions.ContextMenu = new GameMenu(mainModel)
                    {
                        ShowStartSection = false,
                        Placement = PlacementMode.Relative
                    };
                    BindingTools.SetBinding(ButtonMoreActions.ContextMenu,
                        Button.DataContextProperty,
                        mainModel,
                        nameof(DesktopAppViewModel.SelectedGame));
                }
            }

            ButtonEditGame = Template.FindName("PART_ButtonEditGame", this) as Button;
            if (ButtonEditGame != null)
            {
                BindingTools.SetBinding(ButtonEditGame,
                    Button.CommandProperty,
                    nameof(GameDetailsViewModel.EditGameCommand));
                AutomationProperties.SetName(ButtonEditGame, LOC.EditGame.GetLocalized());
            }

            HtmlDescription = Template.FindName("PART_HtmlDescription", this) as HtmlTextView;
            if (HtmlDescription != null)
            {
                BindingTools.SetBinding(HtmlDescription,
                    HtmlTextView.HtmlTextProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.Description)));
                BindingTools.SetBinding(HtmlDescription,
                    HtmlTextView.VisibilityProperty,
                    nameof(GameDetailsViewModel.DescriptionVisibility));
                HtmlDescription.TemplatePath = ThemeFile.GetFilePath("DescriptionView.html");
                BindingTools.SetBinding(HtmlDescription,
                    HtmlTextView.PartialLoadEnabledProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.PartialDescriptionLoading),
                    mode: BindingMode.OneWay);
            }

            TextNotes = Template.FindName("PART_TextNotes", this) as TextBox;
            if (TextNotes != null)
            {
                BindingTools.SetBinding(TextNotes,
                    TextBox.TextProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.Notes)));
                BindingTools.SetBinding(TextNotes,
                    TextBox.VisibilityProperty,
                    nameof(GameDetailsViewModel.NotesVisibility));
            }

            ImageCover = Template.FindName("PART_ImageCover", this) as Image;
            if (ImageCover != null)
            {
                BindingTools.SetBinding(ImageCover,
                    Image.SourceProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.CoverImageObject)),
                    converter: new NullToDependencyPropertyUnsetConverter(),
                    mode: BindingMode.OneWay);
                BindingTools.SetBinding(ImageCover,
                    Image.VisibilityProperty,
                    nameof(GameDetailsViewModel.CoverVisibility),
                    mode: BindingMode.OneWay);
            }

            ImageIcon = Template.FindName("PART_ImageIcon", this) as Image;
            if (ImageIcon != null)
            {
                var sourceBinding = new PriorityBinding();
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(GetGameBindingPath(nameof(GamesCollectionViewEntry.IconObject))),
                    Converter = new NullToDependencyPropertyUnsetConverter(),
                    Mode = BindingMode.OneWay
                });
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(GetGameBindingPath(nameof(GamesCollectionViewEntry.DefaultIconObject))),
                    Converter = new NullToDependencyPropertyUnsetConverter(),
                    Mode = BindingMode.OneWay
                });

                BindingOperations.SetBinding(ImageIcon, Image.SourceProperty, sourceBinding);
                BindingTools.SetBinding(ImageIcon,
                    Image.VisibilityProperty,
                    nameof(GameDetailsViewModel.IconVisibility),
                    mode: BindingMode.OneWay);
            }

            ImageBackground = Template.FindName("PART_ImageBackground", this) as FadeImage;
            if (ImageBackground != null)
            {
                SetBackgroundBinding();
                BindingTools.SetBinding(ImageBackground,
                    Image.VisibilityProperty,
                    nameof(GameDetailsViewModel.BackgroundVisibility),
                    mode: BindingMode.OneWay);
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.AnimationEnabledProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.BackgroundImageAnimation),
                    mode: BindingMode.OneWay);
            }

            SetElemVisibility(ref ElemPlayTime, "PART_ElemPlayTime", nameof(GameDetailsViewModel.PlayTimeVisibility));
            SetElemVisibility(ref ElemInstallSize, "PART_ElemInstallSize", nameof(GameDetailsViewModel.InstallSizeVisibility));
            SetElemVisibility(ref ElemInstallDirectory, "PART_ElemInstallDirectory", nameof(GameDetailsViewModel.InstallDirectoryVisibility));
            SetElemVisibility(ref ElemLastPlayed, "PART_ElemLastPlayed", nameof(GameDetailsViewModel.LastPlayedVisibility));
            SetElemVisibility(ref ElemAdded, "PART_ElemAdded", nameof(GameDetailsViewModel.AddedVisibility));
            SetElemVisibility(ref ElemRecentActivity, "PART_ElemRecentActivity", nameof(GameDetailsViewModel.RecentActivityVisibility));
            SetElemVisibility(ref ElemCompletionStatus, "PART_ElemCompletionStatus", nameof(GameDetailsViewModel.CompletionStatusVisibility));
            SetElemVisibility(ref ElemLibrary, "PART_ElemLibrary", nameof(GameDetailsViewModel.SourceLibraryVisibility));
            SetElemVisibility(ref ElemPlatform, "PART_ElemPlatform", nameof(GameDetailsViewModel.PlatformVisibility));
            SetElemVisibility(ref ElemGenres, "PART_ElemGenres", nameof(GameDetailsViewModel.GenreVisibility));
            SetElemVisibility(ref ElemDevelopers, "PART_ElemDevelopers", nameof(GameDetailsViewModel.DeveloperVisibility));
            SetElemVisibility(ref ElemPublishers, "PART_ElemPublishers", nameof(GameDetailsViewModel.PublisherVisibility));
            SetElemVisibility(ref ElemReleaseDate, "PART_ElemReleaseDate", nameof(GameDetailsViewModel.ReleaseDateVisibility));
            SetElemVisibility(ref ElemTags, "PART_ElemTags", nameof(GameDetailsViewModel.TagVisibility));
            SetElemVisibility(ref ElemFeatures, "PART_ElemFeatures", nameof(GameDetailsViewModel.FeatureVisibility));
            SetElemVisibility(ref ElemCategories, "PART_ElemCategories", nameof(GameDetailsViewModel.CategoryVisibility));
            SetElemVisibility(ref ElemLinks, "PART_ElemLinks", nameof(GameDetailsViewModel.LinkVisibility));
            SetElemVisibility(ref ElemDescription, "PART_ElemDescription", nameof(GameDetailsViewModel.DescriptionVisibility));
            SetElemVisibility(ref ElemAgeRating, "PART_ElemAgeRating", nameof(GameDetailsViewModel.AgeRatingVisibility));
            SetElemVisibility(ref ElemSeries, "PART_ElemSeries", nameof(GameDetailsViewModel.SeriesVisibility));
            SetElemVisibility(ref ElemRegion, "PART_ElemRegion", nameof(GameDetailsViewModel.RegionVisibility));
            SetElemVisibility(ref ElemSource, "PART_ElemSource", nameof(GameDetailsViewModel.SourceVisibility));
            SetElemVisibility(ref ElemVersion, "PART_ElemVersion", nameof(GameDetailsViewModel.VersionVisibility));
            SetElemVisibility(ref ElemCommunityScore, "PART_ElemCommunityScore", nameof(GameDetailsViewModel.CommunityScoreVisibility));
            SetElemVisibility(ref ElemCriticScore, "PART_ElemCriticScore", nameof(GameDetailsViewModel.CriticScoreVisibility));
            SetElemVisibility(ref ElemUserScore, "PART_ElemUserScore", nameof(GameDetailsViewModel.UserScoreVisibility));
            SetElemVisibility(ref ElemNotes, "PART_ElemNotes", nameof(GameDetailsViewModel.NotesVisibility));

            SetGameItemButtonBinding(ref ButtonLibrary, "PART_ButtonLibrary",
                nameof(GameDetailsViewModel.SetLibraryFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.PluginId)),
                GetGameBindingPath($"{nameof(GamesCollectionViewEntry.LibraryPlugin)}.{nameof(GamesCollectionViewEntry.LibraryPlugin.Name)}"),
                nameof(GameDetailsViewModel.SourceLibraryVisibility));

            SetGameItemButtonBinding(ref ButtonReleaseDate, "PART_ButtonReleaseDate",
                nameof(GameDetailsViewModel.SetReleaseDateFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.ReleaseDate)),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.ReleaseDate)),
                nameof(GameDetailsViewModel.ReleaseDateVisibility),
                new ReleaseDateToStringConverter(),
                mainModel.AppSettings.DateTimeFormatReleaseDate);

            SetGameItemButtonBinding(ref ButtonVersion, "PART_ButtonVersion",
                nameof(GameDetailsViewModel.SetVersionFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.Version)),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.Version)),
                nameof(GameDetailsViewModel.VersionVisibility));

            SetGameItemButtonBinding(ref ButtonSource, "PART_ButtonSource",
                nameof(GameDetailsViewModel.SetSourceFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.Source)),
                GetGameBindingPath($"{nameof(GamesCollectionViewEntry.Source)}.{nameof(GamesCollectionViewEntry.Source.Name)}"),
                nameof(GameDetailsViewModel.SourceVisibility));

            SetGameItemTextBinding(ref TextPlayTime, "PART_TextPlayTime",
                nameof(GameDetailsViewModel.Game.Playtime),
                nameof(GameDetailsViewModel.PlayTimeVisibility),
                new PlayTimeToStringConverter(),
                mainModel.AppSettings.PlaytimeUseDaysFormat);

            SetGameItemTextBinding(ref TextInstallSize, "PART_TextInstallSize",
                nameof(GameDetailsViewModel.Game.InstallSize),
                nameof(GameDetailsViewModel.InstallSizeVisibility),
                new NullableUlongBytesSizeToStringConverter());

            SetGameItemButtonBinding(ref ButtonInstallDirectory, "PART_ButtonInstallDirectory",
                nameof(GameDetailsViewModel.OpenGameLocationCommand),
                GetGameBindingPath(nameof(GameDetailsViewModel.Game)),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.InstallDirectory)),
                nameof(GameDetailsViewModel.InstallDirectoryVisibility));

            SetGameItemTextBinding(ref TextLastActivity, "PART_TextLastActivity",
                nameof(GameDetailsViewModel.Game.LastActivity),
                nameof(GameDetailsViewModel.LastPlayedVisibility),
                new DateTimeToLastPlayedConverter(),
                mainModel.AppSettings.DateTimeFormatLastPlayed);

            SetGameItemTextBinding(ref TextAdded, "PART_TextAdded",
                nameof(GameDetailsViewModel.Game.Added),
                nameof(GameDetailsViewModel.AddedVisibility),
                new NullableDateToStringConverter(),
                mainModel.AppSettings.DateTimeFormatAdded);

            SetGameItemTextBinding(ref TextLastActivity, "PART_TextRecentActivity",
                nameof(GameDetailsViewModel.Game.RecentActivity),
                nameof(GameDetailsViewModel.RecentActivityVisibility),
                new NullableDateToStringConverter(),
                mainModel.AppSettings.DateTimeFormatRecentActivity);

            SetGameItemButtonBinding(ref ButtonCompletionStatus, "PART_ButtonCompletionStatus",
                nameof(GameDetailsViewModel.SetCompletionStatusFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.CompletionStatus)),
                GetGameBindingPath($"{nameof(GamesCollectionViewEntry.CompletionStatus)}.{nameof(GamesCollectionViewEntry.CompletionStatus.Name)}"),
                nameof(GameDetailsViewModel.CompletionStatusVisibility));

            SetGameItemTextBinding(ref TextCommunityScore, "PART_TextCommunityScore",
                nameof(GameDetailsViewModel.Game.CommunityScore),
                nameof(GameDetailsViewModel.CommunityScoreVisibility));
            if (TextCommunityScore != null)
            {
                BindingTools.SetBinding(TextCommunityScore,
                    TextBlock.TagProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.CommunityScoreRating)));
            }

            SetGameItemTextBinding(ref TextCriticScore, "PART_TextCriticScore",
                nameof(GameDetailsViewModel.Game.CriticScore),
                nameof(GameDetailsViewModel.CriticScoreVisibility));
            if (TextCriticScore != null)
            {
                BindingTools.SetBinding(TextCriticScore,
                    TextBlock.TagProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.CriticScoreRating)));
            }

            SetGameItemTextBinding(ref TextUserScore, "PART_TextUserScore",
                nameof(GameDetailsViewModel.Game.UserScore),
                nameof(GameDetailsViewModel.UserScoreVisibility));
            if (TextUserScore != null)
            {
                BindingTools.SetBinding(TextUserScore,
                    TextBlock.TagProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.UserScoreRating)));
            }

            SetGameItemTextBinding(ref TextDisplayName, "PART_TextDisplayName",
                nameof(GameDetailsViewModel.Game.DisplayName),
                nameof(GameDetailsViewModel.NameVisibility));

            SetItemsControlBinding(ref ItemsGenres, "PART_ItemsGenres",
                nameof(GameDetailsViewModel.SetGenreFilterCommand),
                nameof(GamesCollectionViewEntry.Genres),
                nameof(GameDetailsViewModel.GenreVisibility));

            SetItemsControlBinding(ref ItemsDevelopers, "PART_ItemsDevelopers",
                nameof(GameDetailsViewModel.SetDeveloperFilterCommand),
                nameof(GamesCollectionViewEntry.Developers),
                nameof(GameDetailsViewModel.DeveloperVisibility));

            SetItemsControlBinding(ref ItemsPublishers, "PART_ItemsPublishers",
                nameof(GameDetailsViewModel.SetPublisherFilterCommand),
                nameof(GamesCollectionViewEntry.Publishers),
                nameof(GameDetailsViewModel.PublisherVisibility));

            SetItemsControlBinding(ref ItemsCategories, "PART_ItemsCategories",
                nameof(GameDetailsViewModel.SetCategoryFilterCommand),
                nameof(GamesCollectionViewEntry.Categories),
                nameof(GameDetailsViewModel.CategoryVisibility));

            SetItemsControlBinding(ref ItemsTags, "PART_ItemsTags",
                nameof(GameDetailsViewModel.SetTagFilterCommand),
                nameof(GamesCollectionViewEntry.Tags),
                nameof(GameDetailsViewModel.TagVisibility));

            SetItemsControlBinding(ref ItemsFeatures, "PART_ItemsFeatures",
                nameof(GameDetailsViewModel.SetFeatureFilterCommand),
                nameof(GamesCollectionViewEntry.Features),
                nameof(GameDetailsViewModel.FeatureVisibility));

            SetItemsControlBinding(ref ItemsLinks, "PART_ItemsLinks",
                nameof(GameDetailsViewModel.OpenLinkCommand),
                nameof(GamesCollectionViewEntry.Links),
                nameof(GameDetailsViewModel.LinkVisibility),
                nameof(Link.Url));

            SetItemsControlBinding(ref ItemsPlatforms, "PART_ItemsPlatforms",
                nameof(GameDetailsViewModel.SetPlatformFilterCommand),
                nameof(GamesCollectionViewEntry.Platforms),
                nameof(GameDetailsViewModel.PlatformVisibility));

            SetItemsControlBinding(ref ItemsAgeRatings, "PART_ItemsAgeRatings",
                nameof(GameDetailsViewModel.SetAgeRatingFilterCommand),
                nameof(GamesCollectionViewEntry.AgeRatings),
                nameof(GameDetailsViewModel.AgeRatingVisibility));

            SetItemsControlBinding(ref ItemsSeries, "PART_ItemsSeries",
                nameof(GameDetailsViewModel.SetSeriesFilterCommand),
                nameof(GamesCollectionViewEntry.Series),
                nameof(GameDetailsViewModel.SeriesVisibility));

            SetItemsControlBinding(ref ItemsRegions, "PART_ItemsRegions",
                nameof(GameDetailsViewModel.SetRegionFilterCommand),
                nameof(GamesCollectionViewEntry.Regions),
                nameof(GameDetailsViewModel.RegionVisibility));

            ControlTemplateTools.InitializePluginControls(
                mainModel.Extensions,
                Template,
                this,
                ApplicationMode.Desktop,
                mainModel,
                $"{nameof(DesktopAppViewModel.SelectedGameDetails)}.{nameof(GameDetailsViewModel.Game)}.{nameof(GameDetailsViewModel.Game.Game)}");

            ScrollViewHost = Template.FindName("PART_ScrollViewHost", this) as ScrollViewer;
        }

        private void SetBackgroundBinding()
        {
            if (mainModel.AppSettings.DetailsVisibility.BackgroundImage &&
                mainModel.AppSettings.ViewSettings.GamesViewType == viewType)
            {
                ImageBackground.SourceUpdateDelay = 150;
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.SourceProperty,
                    mainModel,
                    $"{nameof(mainModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.DisplayBackgroundImageObject)}");
            }
            else
            {
                ImageBackground.Source = null;
            }
        }

        private void SetItemsControlBinding(ref ItemsControl elem, string partId, string command, string listSource, string visibility, string tooltip = null)
        {
            elem = Template.FindName(partId, this) as ItemsControl;
            if (elem != null)
            {
                elem.ItemTemplate = GetFieldItemTemplate(command, tooltip);
                BindingTools.SetBinding(elem,
                    ItemsControl.ItemsSourceProperty,
                    GetGameBindingPath(listSource));
                BindingTools.SetBinding(elem,
                    TextBlock.VisibilityProperty,
                    visibility);
            }
        }

        private void SetElemVisibility(ref FrameworkElement elem, string partId, string binding)
        {
            elem = Template.FindName(partId, this) as FrameworkElement;
            if (elem != null)
            {
                BindingTools.SetBinding(elem,
                    FrameworkElement.VisibilityProperty,
                    binding);
            }
        }

        private void SetGameItemTextBinding(ref TextBlock text, string partId, string textContent, string visibility, IValueConverter converter = null, object converterParameter = null)
        {
            text = Template.FindName(partId, this) as TextBlock;
            if (text != null)
            {
                BindingTools.SetBinding(text,
                    TextBlock.TextProperty,
                    GetGameBindingPath(textContent),
                    converter: converter,
                    converterParameter: converterParameter);
                BindingTools.SetBinding(text,
                    TextBlock.VisibilityProperty,
                    visibility);
            }
        }

        private void SetGameItemButtonBinding(ref Button button, string partId, string command, string commandParameter, string content, string visibility, IValueConverter contentConverter = null, object contentConverterParameter = null)
        {
            button = Template.FindName(partId, this) as Button;
            if (button != null)
            {
                BindingTools.SetBinding(button,
                    Button.CommandProperty,
                    command);
                BindingTools.SetBinding(button,
                    Button.CommandParameterProperty,
                    commandParameter);
                BindingTools.SetBinding(button,
                    Button.ContentProperty,
                    content,
                    converter: contentConverter,
                    converterParameter: contentConverterParameter);
                BindingTools.SetBinding(button,
                    Button.VisibilityProperty,
                    visibility);
            }
        }

        private string GetGameBindingPath(string path)
        {
            return $"{nameof(GameDetailsViewModel.Game)}.{path}";
        }

        private DataTemplate GetFieldItemTemplate(string command, string tooltip = null)
        {
            XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            var buttonElem = new XElement(pns + nameof(Button),
                new XAttribute("Command", $"{{Binding DataContext.{command}, RelativeSource={{RelativeSource AncestorType=ItemsControl}}}}"),
                new XAttribute("CommandParameter", "{Binding}"),
                new XAttribute("Content", "{Binding Name}"),
                new XAttribute("Style", "{StaticResource PropertyItemButton}"));

            if (!tooltip.IsNullOrEmpty())
            {
                buttonElem.Add(new XAttribute("ToolTip", $"{{Binding {tooltip}}}"));
            }

            var templateDoc = new XDocument(
                new XElement(pns + nameof(DataTemplate), buttonElem)
            );

            return Xaml.FromString<DataTemplate>(templateDoc.ToString());
        }
    }
}
