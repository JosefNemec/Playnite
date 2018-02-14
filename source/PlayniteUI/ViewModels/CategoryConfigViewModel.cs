using Playnite;
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.ViewModels
{
    public class CategoryConfigViewModel : ObservableObject
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

        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories
        {
            get
            {
                return categories;
            }

            set
            {
                categories = value;
                OnPropertyChanged("Categories");
            }
        }

        private bool enableThreeState;
        public bool EnableThreeState
        {
            get
            {
                return enableThreeState;
            }

            set
            {
                enableThreeState = value;
                OnPropertyChanged("EnableThreeState");
            }
        }

        private IGame game;
        private IEnumerable<IGame> games;
        private bool autoUpdate;
        private IWindowFactory window;
        private GameDatabase database;

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<string> AddCategoryCommand
        {
            get => new RelayCommand<string>((category) =>
            {
                AddCategory(category);
            });
        }


        public RelayCommand<object> SetCategoriesCommand
        {
            get => new RelayCommand<object>((category) =>
            {
                SetCategories();
            });
        }

        public CategoryConfigViewModel(IWindowFactory window, GameDatabase database, IEnumerable<IGame> games, bool autoUpdate)
        {
            this.window = window;
            this.database = database;
            this.games = games;
            this.autoUpdate = autoUpdate;
            EnableThreeState = true;
            Categories = GetAllCategories();
            SetCategoryStates();
        }

        public CategoryConfigViewModel(IWindowFactory window, GameDatabase database, IGame game, bool autoUpdate)
        {
            this.window = window;
            this.database = database;
            this.game = game;
            this.autoUpdate = autoUpdate;
            EnableThreeState = false;
            Categories = GetAllCategories();
            SetCategoryStates();
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            window.Close(false);
        }

        public void AddCategory(string category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                Categories.Add(new Category(category, true));
            }
        }

        public void SetCategories()
        {
            if (games != null)
            {
                var categories = new List<string>();
                categories = Categories.Where(a => a.Enabled == true).Select(a => a.Name).ToList();

                using (database.BufferedUpdate())
                {
                    foreach (var game in games)
                    {
                        var tempCat = game.Categories;

                        if (tempCat != null)
                        {
                            foreach (var cat in Categories.Where(a => a.Enabled == null))
                            {
                                if (tempCat.Contains(cat.Name, StringComparer.OrdinalIgnoreCase))
                                {
                                    categories.Add(cat.Name);
                                }
                            }
                        }

                        if (categories.Count > 0)
                        {
                            game.Categories = new ComparableList<string>(categories.OrderBy(a => a));
                        }
                        else
                        {
                            game.Categories = null;
                        }

                        if (autoUpdate)
                        {
                            database.UpdateGameInDatabase(game);
                        }
                    }
                }
            }
            else if (game != null)
            {
                var categories = Categories.Where(a => a.Enabled == true).Select(a => a.Name).OrderBy(a => a).ToList();

                if (categories.Count > 0)
                {
                    game.Categories = new ComparableList<string>(categories);
                }
                else
                {
                    game.Categories = null;
                }

                if (autoUpdate)
                {
                    database.UpdateGameInDatabase(game);
                }
            }

            window.Close(true);
        }

        private void SetCategoryStates()
        {
            var catCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (games != null)
            {
                EnableThreeState = true;
                foreach (var game in games)
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
                        if (catCount[cat.Name] == games.Count())
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
                if (game.Categories != null)
                {
                    // Also offer categories that are held by current game instance, but are not in DB yet
                    foreach (var cat in game.Categories.Except(Categories.Select(a => a.Name)))
                    {
                        Categories.Add(new Category(cat, true));
                    }

                    foreach (var cat in game.Categories)
                    {
                        var existingCat = Categories.FirstOrDefault(a => string.Equals(a.Name, cat, StringComparison.OrdinalIgnoreCase));
                        if (existingCat != null)
                        {
                            existingCat.Enabled = true;
                        }
                    }
                }
            }

            Categories = new ObservableCollection<Category>(Categories.OrderBy(a => a.Name));
        }

        private ObservableCollection<Category> GetAllCategories()
        {
            var categories = new ObservableCollection<Category>();

            foreach (var game in database.GamesCollection.Find(a => a.Categories != null))
            {
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
