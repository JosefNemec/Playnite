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
        private ILibraryPlugin plugin;

        public Guid Id => Game.Id;
        public Guid PluginId => Game.PluginId;
        public string GameId => Game.GameId;
        public List<string> Categories => Game.Categories;
        public List<string> Tags => Game.Tags;
        public List<string> Genres => Game.Genres;
        public DateTime? ReleaseDate => Game.ReleaseDate;
        public DateTime? LastActivity => Game.LastActivity;
        public List<string> Developers => Game.Developers;
        public List<string> Publishers => Game.Publishers;
        public ObservableCollection<Link> Links => Game.Links;
        public string Icon => Game.Icon;
        public string CoverImage => Game.CoverImage;
        public string BackgroundImage => Game.BackgroundImage;
        public bool IsInstalled => Game.IsInstalled;
        public bool Hidden => Game.Hidden;
        public bool Favorite => Game.Favorite;
        public string InstallDirectory => Game.InstallDirectory;
        public Guid PlatformId => Game.PlatformId;
        public ObservableCollection<GameAction> OtherActions => Game.OtherActions;
        public GameAction PlayAction => Game.PlayAction;
        public string DisplayName => Game.Name;
        public string Description => Game.Description;
        public bool IsInstalling => Game.IsInstalling;
        public bool IsUnistalling => Game.IsUninstalling;
        public bool IsLaunching => Game.IsLaunching;
        public bool IsRunning => Game.IsRunning;
        public GameState State => Game.State;
        public long Playtime => Game.Playtime;
        public DateTime? Added => Game.Added;
        public DateTime? Modified => Game.Modified;
        public long PlayCount => Game.PlayCount;
        public string Series => Game.Series;
        public string Version => Game.Version;
        public string AgeRating => Game.AgeRating;
        public string Region => Game.Region;
        public string Source => Game.Source;
        public CompletionStatus CompletionStatus => Game.CompletionStatus;
        public int? UserScore => Game.UserScore;
        public int? CriticScore => Game.CriticScore;
        public int? CommunityScore => Game.CommunityScore;

        public object IconObject => GetImageObject(Game.Icon);
        public object CoverImageObject => GetImageObject(Game.CoverImage);
        public object BackgroundImageObject => GetImageObject(Game.BackgroundImage);
        public object DefaultIconObject => GetImageObject(DefaultIcon);
        public object DefaultCoverImageObject => GetImageObject(DefaultCoverImage);


        public string Name
        {
            get
            {
                return (string.IsNullOrEmpty(Game.SortingName)) ? Game.Name : Game.SortingName;
            }
        }

        public CategoryView Category
        {
            get; set;
        }

        private PlatformView platform;
        public PlatformView Platform
        {
            get => platform;
            set
            {
                platform = value;
                OnPropertyChanged("PlatformId");
            }
        }

        public Game Game
        {
            get; set;
        }

        public string Provider
        {
            get
            {
                if (string.IsNullOrEmpty(plugin?.Name))
                {
                    return "";
                }
                else
                {
                    return plugin?.Name;
                }
            }
        }

        public string DefaultIcon
        {
            get
            {
                if (!string.IsNullOrEmpty(Platform?.Platform?.Icon))
                {
                    return Platform.Platform.Icon;
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
                if (!string.IsNullOrEmpty(Platform?.Platform?.Cover))
                {
                    return Platform.Platform.Cover;
                }
                else
                {
                    return @"resources:/Images/custom_cover_background.png";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameViewEntry(Game game, string category, Platform platform, ILibraryPlugin plugin)
        {
            this.plugin = plugin;
            Category = new CategoryView(category);
            Game = game;
            Game.PropertyChanged += Game_PropertyChanged;
            Platform = new PlatformView(PlatformId, platform);
        }

        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "PlatformId")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Platform"));
            }

            if (propertyName == "SortingName" || propertyName == "Name")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
            }

            if (propertyName == "Icon")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IconObject"));
            }

            if (propertyName == "CoverImage")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CoverImageObject"));
            }

            if (propertyName == "BackgroundImage")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BackgroundImageObject"));
            }
        }

        private object GetImageObject(string data)
        {
            return CustomImageStringToImageConverter.GetImageFromSource(data);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Name, Category);
        }

        public static explicit operator Game(GameViewEntry entry)
        {
            return entry.Game;
        }
    }
}
