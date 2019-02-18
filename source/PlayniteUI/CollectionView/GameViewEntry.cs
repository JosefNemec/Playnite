using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public class GameViewEntry : INotifyPropertyChanged
    {
        private readonly ILibraryPlugin plugin;
        private readonly Type colGroupType;
        private readonly Guid colGroupId;

        public Guid Id => Game.Id;
        public Guid PluginId => Game.PluginId;
        public string GameId => Game.GameId;
        public List<Tag> Tags => Game.Tags;
        public List<Genre> Genres => Game.Genres;
        public List<Company> Developers => Game.Developers;
        public List<Company> Publishers => Game.Publishers;
        public List<Category> Categories => Game.Categories;
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
        public long Playtime => Game.Playtime;
        public DateTime? Added => Game.Added;
        public DateTime? Modified => Game.Modified;
        public long PlayCount => Game.PlayCount;        
        public string Version => Game.Version;    
        public CompletionStatus CompletionStatus => Game.CompletionStatus;
        public int? UserScore => Game.UserScore;
        public int? CriticScore => Game.CriticScore;
        public int? CommunityScore => Game.CommunityScore;

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

        public object IconObject => GetImageObject(Game.Icon);
        public object CoverImageObject => GetImageObject(Game.CoverImage);
        public object BackgroundImageObject => GetImageObject(Game.BackgroundImage);
        public object DefaultIconObject => GetImageObject(DefaultIcon);
        public object DefaultCoverImageObject => GetImageObject(DefaultCoverImage);

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

        public string Provider
        {
            get;
        }

        public string DefaultIcon
        {
            get
            {                
                if (!string.IsNullOrEmpty(Platform?.Icon))
                {
                    return Platform.Icon;
                }
                else
                {
                    if (!string.IsNullOrEmpty(plugin?.LibraryIcon))
                    {
                        return plugin.LibraryIcon;
                    }

                    return @"resources:/Images/icon_dark.png";
                }
            }
        }

        public string DefaultCoverImage
        {
            get
            {
                if (!string.IsNullOrEmpty(Platform?.Cover))
                {
                    return Platform.Cover;
                }
                else
                {
                    return @"resources:/Images/custom_cover_background.png";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameViewEntry(Game game, ILibraryPlugin plugin)
        {
            this.plugin = plugin;
            Game = game;
            Game.PropertyChanged += Game_PropertyChanged;
            Provider = string.IsNullOrEmpty(plugin?.Name) ? "Playnite" : plugin.Name;
        }

        public GameViewEntry(Game game, ILibraryPlugin plugin, Type colGroupType, Guid colGroupId) : this(game, plugin)
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
            else if (colGroupType == typeof(Category))
            {
                Category = game.Categories?.FirstOrDefault(a => a.Id == colGroupId);
            }
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
            }

            if (propertyName == nameof(Game.CoverImage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoverImageObject)));
            }

            if (propertyName == nameof(Game.BackgroundImage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundImageObject)));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private object GetImageObject(string data)
        {
            return CustomImageStringToImageConverter.GetImageFromSource(data);
        }

        public override string ToString()
        {
            return Name;
        }

        public static explicit operator Game(GameViewEntry entry)
        {
            return entry.Game;
        }
    }
}
