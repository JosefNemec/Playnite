using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Playnite.Database;
using Playnite.Models;

namespace PlayniteUI.Windows
{
    /// <summary>
    /// Interaction logic for CategoryConfigWindow.xaml
    /// </summary>
    public partial class CategoryConfigWindow : Window
    {
        public class Category
        {
            public bool? Enabled
            {
                get; set;
            }

            public string Name
            {
                get; set;
            }

            public Category()
            {

            }

            public Category(string name, bool enabled)
            {
                Name = name;
                Enabled = enabled;
            }
        }

        public bool AutoUpdateGame
        {
            get; set;
        }

        private IGame game;
        public IGame Game
        {
            get
            {
                return game;
            }

            set
            {
                game = value;
                DataChanged(game);
            }
        }

        private IEnumerable<IGame> games;
        public IEnumerable<IGame> Games
        {
            get
            {
                return games;
            }

            set
            {
                games = value;
                DataChanged(games);
            }
        }

        private ObservableCollection<Category> Categories
        {
            get; set;
        }


        public CategoryConfigWindow()
        {
            InitializeComponent();
            AutoUpdateGame = false;
        }

        private void ButtonAddCat_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextNewCat.Text))
            {
                Categories.Add(new Category(TextNewCat.Text, true));
                TextNewCat.Text = string.Empty;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (Games != null)
            {
                foreach (var game in Games)
                {
                    var tempCat = game.Categories;
                    var categories = new List<string>();

                    categories = Categories.Where(a => a.Enabled == true).Select(a => a.Name).ToList();

                    foreach (var cat in Categories.Where(a => a.Enabled == null))
                    {
                        if (tempCat.Contains(cat.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            categories.Add(cat.Name);
                        }
                    }

                    if (categories.Count > 0)
                    {
                        game.Categories = categories.OrderBy(a => a).ToList();
                    }
                    else
                    {
                        game.Categories = null;
                    }

                    if (AutoUpdateGame)
                    {
                        GameDatabase.Instance.UpdateGameInDatabase(game);
                    }
                }
            }
            else if (Game != null)
            {
                var categories = Categories.Where(a => a.Enabled == true).Select(a => a.Name).OrderBy(a => a).ToList();

                if (categories.Count > 0)
                {
                    game.Categories = categories;
                }
                else
                {
                    game.Categories = null;
                }

                if (AutoUpdateGame)
                {
                    GameDatabase.Instance.UpdateGameInDatabase(Game);
                }
            }

            DialogResult = true;
            Close();
        }

        private void DataChanged(object data)
        {
            Categories = GetAllCategories();
            var catCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (data is IEnumerable<IGame>)
            {
                foreach (var game in Games)
                {
                    if (game.Categories == null)
                    {
                        continue;
                    }

                    foreach (var cat in game.Categories)
                    {
                        if (catCount.ContainsKey(cat))
                        {
                            catCount[cat] += 1;
                        }
                        else
                        {
                            catCount.Add(cat, 1);
                        }
                    }
                }

                foreach (var cat in Categories)
                {
                    if (catCount.ContainsKey(cat.Name))
                    {
                        if (catCount[cat.Name] == Games.Count())
                        {
                            cat.Enabled = true;
                        }
                        else
                        {

                            cat.Enabled = null;
                        }
                    }
                }
            }
            else
            {
                if (Game.Categories != null)
                {
                    foreach (var cat in Game.Categories)
                    {
                        var existingCat = Categories.FirstOrDefault(a => string.Equals(a.Name, cat, StringComparison.OrdinalIgnoreCase));

                        if (existingCat == null)
                        {
                            existingCat.Enabled = false;
                        }
                        else
                        {
                            existingCat.Enabled = true;
                        }
                    }
                }
            }

            Categories = new ObservableCollection<Category>(Categories.OrderBy(a => a.Name));
            ListCategories.ItemsSource = Categories;
        }

        private ObservableCollection<Category> GetAllCategories()
        {
            var categories = new ObservableCollection<Category>();

            foreach (var game in GameDatabase.Instance.Games)
            {
                if (game.Categories == null)
                {
                    continue;
                }

                foreach (var cat in game.Categories)
                {
                    var existingCat = categories.FirstOrDefault(a => string.Equals(a.Name, cat, StringComparison.OrdinalIgnoreCase));

                    if (existingCat == null)
                    {
                        categories.Add(new Category(cat, false));
                    }
                }
            }

            return categories;
        }
    }
}
