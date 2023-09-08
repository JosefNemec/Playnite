using Playnite.Converters;
using Playnite.Database;
using Playnite.Extensions.Markup;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Playnite
{
    public class GamesCollectionViewEntry : INotifyPropertyChanged, IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly PlayniteSettings settings;

        public static BitmapLoadProperties DetailsListIconProperties { get; private set; }
        public static BitmapLoadProperties GridViewCoverProperties { get; private set; }
        public static BitmapLoadProperties BackgroundImageProperties { get; private set; }
        public static BitmapLoadProperties FullscreenListCoverProperties { get; private set; }

        public LibraryPlugin LibraryPlugin { get; }
        public Guid Id => Game.Id;
        public Guid PluginId => Game.PluginId;
        public string GameId => Game.GameId;
        public ComparableDbItemList<Tag> Tags => new ComparableDbItemList<Tag>(Game.Tags);
        public ComparableDbItemList<GameFeature> Features => new ComparableDbItemList<GameFeature>(Game.Features);
        public ComparableDbItemList<Genre> Genres => new ComparableDbItemList<Genre>(Game.Genres);
        public ComparableDbItemList<Company> Developers => new ComparableDbItemList<Company>(Game.Developers);
        public ComparableDbItemList<Company> Publishers => new ComparableDbItemList<Company>(Game.Publishers);
        public ComparableDbItemList<Category> Categories => new ComparableDbItemList<Category>(Game.Categories);
        public ComparableDbItemList<AgeRating> AgeRatings => new ComparableDbItemList<AgeRating>(Game.AgeRatings);
        public ComparableDbItemList<Series> Series => new ComparableDbItemList<Series>(Game.Series);
        public ComparableDbItemList<Region> Regions => new ComparableDbItemList<Region>(Game.Regions);
        public ComparableDbItemList<Platform> Platforms => new ComparableDbItemList<Platform>(Game.Platforms);
        public ReleaseDate? ReleaseDate => Game.ReleaseDate;
        public int? ReleaseYear => Game.ReleaseYear;
        public DateTime? LastActivity => Game.LastActivity;
        public ObservableCollection<Link> Links => Game.Links;
        public string Icon => Game.Icon;
        public string CoverImage => Game.CoverImage;
        public string BackgroundImage => Game.BackgroundImage;
        public bool Hidden => Game.Hidden;
        public bool Favorite => Game.Favorite;
        public string InstallDirectory => Game.InstallDirectory;
        public ObservableCollection<GameAction> GameActions => Game.GameActions;
        public string DisplayName => Game.Name;
        public string Description => Game.Description;
        public string Notes => Game.Notes;
        public bool IsInstalled => Game.IsInstalled;
        public bool IsInstalling => Game.IsInstalling;
        public bool IsUninstalling => Game.IsUninstalling;
        public bool IsLaunching => Game.IsLaunching;
        public bool IsRunning => Game.IsRunning;
        public bool IsCustomGame => Game.IsCustomGame;
        public ulong Playtime => Game.Playtime;
        public DateTime? Added => Game.Added;
        public DateTime? Modified => Game.Modified;
        public ulong PlayCount => Game.PlayCount;
        public ulong? InstallSize => Game.InstallSize;
        public string Version => Game.Version;
        public int? UserScore => Game.UserScore;
        public int? CriticScore => Game.CriticScore;
        public int? CommunityScore => Game.CommunityScore;
        public ScoreGroup UserScoreGroup => Game.UserScoreGroup;
        public ScoreGroup CriticScoreGroup => Game.CriticScoreGroup;
        public ScoreGroup CommunityScoreGroup => Game.CommunityScoreGroup;
        public ScoreRating UserScoreRating => Game.UserScoreRating;
        public ScoreRating CriticScoreRating => Game.CriticScoreRating;
        public ScoreRating CommunityScoreRating => Game.CommunityScoreRating;
        public PastTimeSegment LastActivitySegment => Game.LastActivitySegment;
        public PastTimeSegment AddedSegment => Game.AddedSegment;
        public PastTimeSegment ModifiedSegment => Game.ModifiedSegment;
        public PastTimeSegment RecentActivitySegment => Game.RecentActivitySegment;
        public PlaytimeCategory PlaytimeCategory => Game.PlaytimeCategory;
        public InstallationStatus InstallationState => Game.InstallationStatus;
        public char NameGroup => Game.GetNameGroup();
        public DateTime? RecentActivity => Game.RecentActivity;
        public string InstallDriveGroup => Game.GetInstallDriveGroup();
        public InstallSizeGroup InstallSizeGroup => Game.GetInstallSizeGroup();
        public bool OverrideInstallState => Game.OverrideInstallState;

        public List<Guid> CategoryIds => Game.CategoryIds;
        public List<Guid> GenreIds => Game.GenreIds;
        public List<Guid> DeveloperIds => Game.DeveloperIds;
        public List<Guid> PublisherIds => Game.PublisherIds;
        public List<Guid> TagIds => Game.TagIds;
        public List<Guid> SeriesIds => Game.SeriesIds;
        public List<Guid> AgeRatingIds => Game.AgeRatingIds;
        public List<Guid> RegionIds => Game.RegionIds;
        public Guid SourceId => Game.SourceId;
        public List<Guid> PlatformIds => Game.PlatformIds;
        public List<Guid> FeatureIds => Game.FeatureIds;
        public Guid CompletionStatusId => Game.CompletionStatusId;
        public ObservableCollection<GameRom> Roms => Game.Roms;
        public string RomList => Game.Roms.HasItems() ? string.Join(", ", Game.Roms.Select(a => a.Path)) : string.Empty;

        public object LibraryIcon => GetImageObject(LibraryPlugin?.LibraryIcon, true);
        public object IconObject => GetImageObject(Game.Icon, false);
        public object CoverImageObject => GetImageObject(Game.CoverImage, false);
        public object DefaultIconObject => GetDefaultIcon(false);
        public object DefaultCoverImageObject => GetDefaultCoverImage(false);

        public object IconObjectCached => GetImageObject(Game.Icon, true);
        public object CoverImageObjectCached => GetImageObject(Game.CoverImage, true);
        public object DefaultIconObjectCached => GetDefaultIcon(true);
        public object DefaultCoverImageObjectCached => GetDefaultCoverImage(true);

        public string DisplayBackgroundImage => GetBackgroundImage();
        public object DisplayBackgroundImageObject => GetBackgroundImageObject(BackgroundImageProperties);

        public object DetailsListIconObjectCached => GetImageObject(Game.Icon, true, DetailsListIconProperties);
        public object GridViewCoverObjectCached => GetImageObject(Game.CoverImage, true, GridViewCoverProperties);
        public object DefaultDetailsListIconObjectCached => GetDefaultIcon(true, DetailsListIconProperties);
        public object DefaultGridViewCoverObjectCached => GetDefaultCoverImage(true, GridViewCoverProperties);

        public object FullscreenListItemCoverObject => GetImageObject(
            Game.CoverImage,
            settings.Fullscreen.ImageScalerMode != ImageLoadScaling.None,
            FullscreenListCoverProperties);
        public object DefaultFullscreenListItemCoverObject => GetDefaultCoverImage(true, FullscreenListCoverProperties);

        public Series Serie
        {
            get; private set;
        } = Playnite.SDK.Models.Series.Empty;

        public Platform Platform
        {
            get; private set;
        } = Platform.Empty;

        public Region Region
        {
            get; private set;
        } = Region.Empty;

        public GameSource Source
        {
            get => Game.Source ?? GameSource.Empty;
        }

        public CompletionStatus CompletionStatus
        {
            get => Game.CompletionStatus ?? CompletionStatus.Empty;
        }

        public AgeRating AgeRating
        {
            get; private set;
        } = AgeRating.Empty;

        public Category Category
        {
            get; private set;
        } = Category.Empty;

        public Genre Genre
        {
            get; private set;
        } = Genre.Empty;

        public Company Developer
        {
            get; private set;
        } = Company.Empty;

        public Company Publisher
        {
            get; private set;
        } = Company.Empty;

        public Tag Tag
        {
            get; private set;
        } = Tag.Empty;

        public GameFeature Feature
        {
            get; private set;
        } = GameFeature.Empty;

        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(Game.SortingName) ? Game.Name : Game.SortingName;
            }
        }

        public Game Game
        {
            get;
        }

        public string Library
        {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GamesCollectionViewEntry(Game game, LibraryPlugin plugin, PlayniteSettings settings, bool readOnly = false)
        {
            this.settings = settings;
            LibraryPlugin = plugin;
            Library = string.IsNullOrEmpty(plugin?.Name) ? "Playnite" : plugin.Name;
            Game = game;
            if (!readOnly)
            {
                Game.PropertyChanged += Game_PropertyChanged;
            }
        }

        public static void InitItemViewProperties(PlayniteApplication app, PlayniteSettings settings)
        {
            logger.Debug("Reloading collection item view properties.");
            // Use optimized rendering only for Desktop mode where we know pixel perfect data
            if (app.Mode == ApplicationMode.Desktop)
            {
                DetailsListIconProperties = new BitmapLoadProperties(
                    0,
                    Convert.ToInt32(settings.DetailsViewListIconSize),
                    app.DpiScale);
                GridViewCoverProperties = new BitmapLoadProperties(
                    Convert.ToInt32(settings.GridItemWidth),
                    0,
                    app.DpiScale,
                    settings.ImageScalerMode);
            }
            else
            {
                FullscreenListCoverProperties = GetFullscreenItemRenderSettings(app, settings);
            }

            BackgroundImageProperties = new BitmapLoadProperties(
                app.CurrentScreen.WorkingArea.Width,
                0,
                app.DpiScale,
                settings.ImageScalerMode);
        }

        private static BitmapLoadProperties GetFullscreenItemRenderSettings(PlayniteApplication app, PlayniteSettings settings)
        {
            if (app == null)
            {
                return null;
            }

            var dpi = app.DpiScale;
            var properties = new BitmapLoadProperties(0, 0, null, settings.Fullscreen.ImageScalerMode);
            if (settings.Fullscreen.HorizontalLayout)
            {
                properties.MaxDecodePixelWidth = app.CurrentScreen.Bounds.Width / (settings.Fullscreen.Columns == 0 ? 1 : settings.Fullscreen.Columns);
                properties.MaxDecodePixelWidth = (int)Math.Round(properties.MaxDecodePixelWidth / dpi.DpiScaleX);
            }
            else
            {
                properties.MaxDecodePixelHeight = app.CurrentScreen.Bounds.Height / (settings.Fullscreen.Rows == 0 ? 1 : settings.Fullscreen.Rows);
                properties.MaxDecodePixelHeight = (int)Math.Round(properties.MaxDecodePixelHeight / dpi.DpiScaleY);
            }

            return properties;
        }

        public static GamesCollectionViewEntry GetAdvancedGroupedEntry(
            Game game,
            LibraryPlugin plugin,
            Type colGroupType,
            Guid groupObjId,
            IGameDatabase database,
            PlayniteSettings settings)
        {
            if (colGroupType == typeof(Genre))
            {
                var obj = database.Genres.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Genre = obj };
                }
            }
            else if (colGroupType == typeof(Developer))
            {
                var obj = database.Companies.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Developer = obj };
                }
            }
            else if (colGroupType == typeof(Publisher))
            {
                var obj = database.Companies.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Publisher = obj };
                }
            }
            else if (colGroupType == typeof(Tag))
            {
                var obj = database.Tags.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Tag = obj };
                }
            }
            else if (colGroupType == typeof(GameFeature))
            {
                var obj = database.Features.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Feature = obj };
                }
            }
            else if (colGroupType == typeof(Category))
            {
                var obj = database.Categories.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Category = obj };
                }
            }
            else if (colGroupType == typeof(Platform))
            {
                var obj = database.Platforms.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Platform = obj };
                }
            }
            else if (colGroupType == typeof(AgeRating))
            {
                var obj = database.AgeRatings.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { AgeRating = obj };
                }
            }
            else if (colGroupType == typeof(Series))
            {
                var obj = database.Series.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Serie = obj };
                }
            }
            else if (colGroupType == typeof(Region))
            {
                var obj = database.Regions.Get(groupObjId);
                if (obj != null)
                {
                    return new GamesCollectionViewEntry(game, plugin, settings) { Region = obj };
                }
            }

            return null;
        }

        public void Dispose()
        {
            Game.PropertyChanged -= Game_PropertyChanged;
        }

        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(Game.SortingName) || propertyName == nameof(Game.Name))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Game.Name)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameGroup)));
            }

            if (propertyName == nameof(Game.Icon))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconObject)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconObjectCached)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DetailsListIconObjectCached)));
            }

            if (propertyName == nameof(Game.CoverImage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoverImageObject)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoverImageObjectCached)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GridViewCoverObjectCached)));
            }

            if (propertyName == nameof(Game.BackgroundImage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayBackgroundImage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayBackgroundImageObject)));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private object GetImageObject(string data, bool cached, BitmapLoadProperties loadProperties = null)
        {
            return ImageSourceManager.GetImage(data, cached, loadProperties);
        }

        public object GetDefaultIcon(bool cached, BitmapLoadProperties loadProperties = null)
        {
            var icon = GetDefaultIconFile(Game, settings, GameDatabase.Instance, LibraryPlugin);
            if (icon.IsNullOrEmpty())
            {
                return ImageSourceManager.GetResourceImage("DefaultGameIcon", cached, loadProperties);
            }
            else
            {
                return ImageSourceManager.GetImage(icon, cached);
            }
        }

        public static string GetDefaultIconFile(Game game, PlayniteSettings settings, IGameDatabaseMain database, LibraryPlugin plugin)
        {
            if (settings.DefaultIconSource == DefaultIconSourceOptions.None)
            {
                return null;
            }
            else if (settings.DefaultIconSource == DefaultIconSourceOptions.Library && plugin?.LibraryIcon.IsNullOrEmpty() == false)
            {
                return plugin.LibraryIcon;
            }
            else if (settings.DefaultIconSource == DefaultIconSourceOptions.Platform)
            {
                var plat = game.Platforms?.FirstOrDefault(a => !a.Icon.IsNullOrEmpty());
                if (plat != null)
                {
                    return database?.GetFullFilePath(plat.Icon);
                }
            }

            return null;
        }

        public object GetDefaultCoverImage(bool cached, BitmapLoadProperties loadProperties = null)
        {
            if (settings.DefaultCoverSource == DefaultCoverSourceOptions.None)
            {
                return null;
            }

            if (settings.DefaultCoverSource == DefaultCoverSourceOptions.Platform)
            {
                var plat = Game.Platforms?.FirstOrDefault(a => !a.Cover.IsNullOrEmpty());
                if (plat != null)
                {
                    return ImageSourceManager.GetImage(plat.Cover, cached);
                }
            }

            return ImageSourceManager.GetResourceImage("DefaultGameCover", cached, loadProperties);
        }

        public string GetBackgroundImage()
        {
            if (!Game.BackgroundImage.IsNullOrEmpty())
            {
                return Game.BackgroundImage;
            }

            if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.None)
            {
                return null;
            }

            if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.Cover && !CoverImage.IsNullOrEmpty())
            {
                return CoverImage;
            }

            if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.Platform)
            {
                var plat = Game.Platforms?.FirstOrDefault(a => !a.Background.IsNullOrEmpty());
                if (plat != null)
                {
                    return plat.Background;
                }
            }

            if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.Library && LibraryPlugin?.LibraryBackground.IsNullOrEmpty() == false)
            {
                return LibraryPlugin.LibraryBackground;
            }

            return null;
        }

        public object GetBackgroundImageObject(BitmapLoadProperties loadProperties = null)
        {
            var imagePath = GetBackgroundImage();
            if (imagePath.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                if (loadProperties == null)
                {
                    return new BitmapLoadProperties(0, 0) { Source = imagePath };
                }
                else
                {
                    return new BitmapLoadProperties(loadProperties.MaxDecodePixelWidth, 0, loadProperties.DpiScale, loadProperties.Scaling)
                    {
                        Source = imagePath
                    };
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public static explicit operator Game(GamesCollectionViewEntry entry)
        {
            return entry.Game;
        }
    }
}
