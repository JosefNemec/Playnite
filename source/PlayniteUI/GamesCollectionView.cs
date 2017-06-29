using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public class CategoryView : IComparable
    {
        public string Category
        {
            get; set;
        }

        public CategoryView(string category)
        {
            Category = category;
        }

        public int CompareTo(object obj)
        {
            var cat = (obj as CategoryView).Category;

            if (string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(cat))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(Category))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(cat))
            {
                return -1;
            }
            if (Category.Equals(cat))
            {
                return 0;
            }

            return string.Compare(Category, cat, true);
        }

        public override bool Equals(object obj)
        {
            var cat = ((CategoryView)obj).Category;

            if (string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(cat))
            {
                return true;
            }
            if (string.IsNullOrEmpty(Category))
            {
                return false;
            }
            if (string.IsNullOrEmpty(cat))
            {
                return false;
            }
            if (Category.Equals(cat))
            {
                return true;
            }

            return string.Compare(Category, cat, true) == 0;
        }

        public override int GetHashCode()
        {
            if (Category == null)
            {
                return 0;
            }
            else
            {
                return Category.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Category;
        }
    }

    public class GameViewEntry : INotifyPropertyChanged
    {
        public CategoryView Category
        {
            get; set;
        }

        public IGame Game
        {
            get; set;
        }

        public string Name => Game.Name;
        public Provider Provider => Game.Provider;
        public List<string> Categories => Game.Categories;
        public List<string> Genres => Game.Genres;
        public DateTime? ReleaseDate => Game.ReleaseDate;
        public DateTime? LastActivity => Game.LastActivity;
        public List<string> Developers => Game.Developers;
        public List<string> Publishers => Game.Publishers;
        public string Icon => Game.Icon;
        public string DefaultIcon => Game.DefaultIcon;
        public string DefaultImage => Game.DefaultImage;
        public string Image => Game.Image;
        public bool IsInstalled => Game.IsInstalled;
        public bool Hidden => Game.Hidden;
        public bool Favorite => Game.Favorite;
        public string InstallDirectory => Game.InstallDirectory;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameViewEntry(IGame game, string category)
        {
            Category = new CategoryView(category);
            Game = game;
            Game.PropertyChanged += Game_PropertyChanged;            
        }

        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Name, Category);
        }

        public static explicit operator Game(GameViewEntry entry)
        {
            return entry.Game as Game;
        }
    }

    public class GamesCollectionView
    {
        public ObservableCollection<GameViewEntry> Items
        {
            get; set;
        }

        public GamesCollectionView(ObservableCollection<IGame> items)
        {
            items.CollectionChanged += Items_CollectionChanged;

            Items = new ObservableCollection<GameViewEntry>(items.SelectMany(x =>
            {
                if (x.Categories == null || x.Categories.Count == 0)
                {
                    return new List<GameViewEntry>()
                    {
                        new GameViewEntry(x, null)
                    };
                }
                else
                {
                    return x.Categories.Select(c =>
                    {
                        return new GameViewEntry(x, c);
                    });
                }
            }));
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }
    }
}
