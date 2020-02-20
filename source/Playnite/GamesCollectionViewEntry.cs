using Playnite.Converters;
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
        private PlayniteSettings settings;
        private PlayniteApplication application;
        private readonly Type colGroupType;
        private readonly Guid colGroupId;
        private BitmapLoadProperties detailsListIconProperties;
        private BitmapLoadProperties gridViewCoverProperties;
        private BitmapLoadProperties backgroundImageProperties;

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
        public DateTime? ReleaseDate => Game.ReleaseDate;
        public int? ReleaseYear => Game.ReleaseYear;
        public DateTime? LastActivity => Game.LastActivity;
        public ObservableCollection<Link> Links => Game.Links;
        public string Icon => Game.Icon;
        public string CoverImage => Game.CoverImage;
        public string BackgroundImage => Game.BackgroundImage;
        public bool Hidden => Game.Hidden;
        public bool Favorite => Game.Favorite;
        public string InstallDirectory => Game.InstallDirectory;
        public ObservableCollection<GameAction> OtherActions => Game.OtherActions;
        public GameAction PlayAction => Game.PlayAction;
        public string DisplayName => Game.Name;
        public string Description => Game.Description;
        public bool IsInstalled => Game.IsInstalled;
        public bool IsInstalling => Game.IsInstalling;
        public bool IsUnistalling => Game.IsUninstalling;
        public bool IsLaunching => Game.IsLaunching;
        public bool IsRunning => Game.IsRunning;
        public bool IsCustomGame => Game.IsCustomGame;
        public long Playtime => Game.Playtime;
        public DateTime? Added => Game.Added;
        public DateTime? Modified => Game.Modified;
        public long PlayCount => Game.PlayCount;
        public string Version => Game.Version;
        public CompletionStatus CompletionStatus => Game.CompletionStatus;
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
        public PlaytimeCategory PlaytimeCategory => Game.PlaytimeCategory;
        public InstallationStatus InstallationState => Game.InstallationStatus;

        public List<Guid> CategoryIds => Game.CategoryIds;
        public List<Guid> GenreIds => Game.GenreIds;
        public List<Guid> DeveloperIds => Game.DeveloperIds;
        public List<Guid> PublisherIds => Game.PublisherIds;
        public List<Guid> TagIds => Game.TagIds;
        public Guid SeriesId => Game.SeriesId;
        public Guid AgeRatingId => Game.AgeRatingId;
        public Guid RegionId => Game.RegionId;
        public Guid SourceId => Game.SourceId;
        public Guid PlatformId => Game.PlatformId;
        public List<Guid> FeatureIds => Game.FeatureIds;

        public object IconObject => GetImageObject(Game.Icon, false);
        public object CoverImageObject => GetImageObject(Game.CoverImage, false);
        public object DefaultIconObject => GetDefaultIcon(false);
        public object DefaultCoverImageObject => GetDefaultCoverImage(false);

        public object IconObjectCached => GetImageObject(Game.Icon, true);
        public object CoverImageObjectCached => GetImageObject(Game.CoverImage, true);
        public object DefaultIconObjectCached => GetDefaultIcon(true);
        public object DefaultCoverImageObjectCached => GetDefaultCoverImage(true);

        public string DisplayBackgroundImage => GetBackgroundImage();
        public object DisplayBackgroundImageObject => GetBackgroundImageObject(backgroundImageProperties);

        public object DetailsListIconObjectCached => GetImageObject(Game.Icon, true, detailsListIconProperties);
        public object GridViewCoverObjectCached => GetImageObject(Game.CoverImage, true, gridViewCoverProperties);
        public object DefaultDetailsListIconObjectCached => GetDefaultIcon(true, detailsListIconProperties);
        public object DefaultGridViewCoverObjectCached => GetDefaultCoverImage(true, gridViewCoverProperties);

        public Series Series
        {
            get => Game.SeriesId == Guid.Empty ? Series.Empty : Game.Series;
        }

        public Platform Platform
        {
            get => Game.PlatformId == Guid.Empty ? Platform.Empty : Game.Platform;
        }

        public Region Region
        {
            get => Game.RegionId == Guid.Empty ? Region.Empty : Game.Region;
        }

        public GameSource Source
        {
            get => Game.SourceId == Guid.Empty ? GameSource.Empty : Game.Source;
        }

        public AgeRating AgeRating
        {
            get => Game.AgeRatingId == Guid.Empty ? AgeRating.Empty : Game.AgeRating;
        }

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

        public GamesCollectionViewEntry(Game game, LibraryPlugin plugin, PlayniteSettings settings)
        {
            this.settings = settings;
            settings.PropertyChanged += Settings_PropertyChanged;

            // Use optimized rendering only for Desktop mode where we know pixel perfect data
            application = PlayniteApplication.Current;
            if (application?.Mode == ApplicationMode.Desktop)
            {
                detailsListIconProperties = new BitmapLoadProperties(
                    0,
                    Convert.ToInt32(settings.DetailsViewListIconSize),
                    application.DpiScale);
                gridViewCoverProperties = new BitmapLoadProperties(
                    Convert.ToInt32(settings.GridItemWidth),
                    0,
                    application.DpiScale);
            }

            if (application != null)
            {
                backgroundImageProperties = new BitmapLoadProperties(
                    application.CurrentScreen.WorkingArea.Width,
                    0,
                    application.DpiScale);
            }

            LibraryPlugin = plugin;
            Game = game;
            Game.PropertyChanged += Game_PropertyChanged;
            Library = string.IsNullOrEmpty(plugin?.Name) ? "Playnite" : plugin.Name;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PlayniteApplication.Current?.Mode == ApplicationMode.Desktop)
            {
                if (e.PropertyName == nameof(PlayniteSettings.DetailsViewListIconSize))
                {
                    detailsListIconProperties = new BitmapLoadProperties(
                        0,
                        Convert.ToInt32(settings.DetailsViewListIconSize),
                        PlayniteApplication.Current.DpiScale);
                    OnPropertyChanged(nameof(DetailsListIconObjectCached));
                    OnPropertyChanged(nameof(DefaultDetailsListIconObjectCached));
                }

                if (e.PropertyName == nameof(PlayniteSettings.GridItemWidth) ||
                    e.PropertyName == nameof(PlayniteSettings.CoverAspectRatio) ||
                    e.PropertyName == nameof(PlayniteSettings.CoverArtStretch))
                {
                    gridViewCoverProperties = new BitmapLoadProperties(
                        Convert.ToInt32(settings.GridItemWidth),
                        0,
                        PlayniteApplication.Current.DpiScale);
                    OnPropertyChanged(nameof(GridViewCoverObjectCached));
                    OnPropertyChanged(nameof(DefaultGridViewCoverObjectCached));
                }
            }
        }

        public GamesCollectionViewEntry(Game game, LibraryPlugin plugin, Type colGroupType, Guid colGroupId, PlayniteSettings settings) : this(game, plugin, settings)
        {
            this.colGroupType = colGroupType;
            this.colGroupId = colGroupId;

            if (colGroupType == typeof(Genre))
            {
                Genre = game.Genres?.FirstOrDefault(a => a.Id == colGroupId);
            }
            else if (colGroupType == typeof(Developer))
            {
                Developer = game.Developers?.FirstOrDefault(a => a.Id == colGroupId);
            }
            else if (colGroupType == typeof(Publisher))
            {
                Publisher = game.Publishers?.FirstOrDefault(a => a.Id == colGroupId);
            }
            else if (colGroupType == typeof(Tag))
            {
                Tag = game.Tags?.FirstOrDefault(a => a.Id == colGroupId);
            }
            else if (colGroupType == typeof(GameFeature))
            {
                Feature = game.Features?.FirstOrDefault(a => a.Id == colGroupId);
            }
            else if (colGroupType == typeof(Category))
            {
                Category = game.Categories?.FirstOrDefault(a => a.Id == colGroupId);
            }
        }

        public void Dispose()
        {
            Game.PropertyChanged -= Game_PropertyChanged;
            settings.PropertyChanged -= Settings_PropertyChanged;
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
            if (settings.DefaultIconSource == DefaultIconSourceOptions.None)
            {
                return null;
            }
            else if (settings.DefaultIconSource == DefaultIconSourceOptions.Library && LibraryPlugin?.LibraryIcon.IsNullOrEmpty() == false)
            {
                return ImageSourceManager.GetImage(LibraryPlugin.LibraryIcon, cached);
            }
            else if (settings.DefaultIconSource == DefaultIconSourceOptions.Platform && Platform?.Icon.IsNullOrEmpty() == false)
            {
                return ImageSourceManager.GetImage(Platform.Icon, cached);
            }
            else
            {
                return ImageSourceManager.GetResourceImage("DefaultGameIcon", cached, loadProperties);
            }
        }

        public object GetDefaultCoverImage(bool cached, BitmapLoadProperties loadProperties = null)
        {
            if (settings.DefaultCoverSource == DefaultCoverSourceOptions.None)
            {
                return null;
            }
            else if (settings.DefaultCoverSource == DefaultCoverSourceOptions.Platform && Platform?.Cover.IsNullOrEmpty() == false)
            {
                return ImageSourceManager.GetImage(Platform.Cover, cached);
            }
            else
            {
                return ImageSourceManager.GetResourceImage("DefaultGameCover", cached, loadProperties);
            }
        }

        public string GetBackgroundImage()
        {
            if (!Game.BackgroundImage.IsNullOrEmpty())
            {
                return Game.BackgroundImage;
            }
            else if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.None)
            {
                return null;
            }
            else if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.Cover && !CoverImage.IsNullOrEmpty())
            {
                return CoverImage;
            }
            else if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.Platform && Platform?.Background.IsNullOrEmpty() == false)
            {
                return Platform.Background;
            }
            else if (settings.DefaultBackgroundSource == DefaultBackgroundSourceOptions.Library && LibraryPlugin?.LibraryBackground.IsNullOrEmpty() == false)
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
                    return new BitmapLoadProperties(loadProperties.MaxDecodePixelWidth, 0, loadProperties.DpiScale)
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
