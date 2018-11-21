using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
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
        private ObservableCollection<SelectableItem<Category>> categories;
        public ObservableCollection<SelectableItem<Category>> Categories
        {
            get
            {
                return categories;
            }

            set
            {
                categories = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private Game game;
        private IEnumerable<Game> games;
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

        public CategoryConfigViewModel(IWindowFactory window, GameDatabase database, IEnumerable<Game> games, bool autoUpdate)
        {
            this.window = window;
            this.database = database;
            this.games = games;
            this.autoUpdate = autoUpdate;
            EnableThreeState = true;
            Categories = GetAllCategories();
            SetCategoryStates();
        }

        public CategoryConfigViewModel(IWindowFactory window, GameDatabase database, Game game, bool autoUpdate)
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
            if (!string.IsNullOrEmpty(category) || Categories.FirstOrDefault(a => a.Item.Name.Equals(category, StringComparison.CurrentCultureIgnoreCase)) != null)
            {
                return;
            }
            
            var newCat = database.Categories.Add(category);
            Categories.Add(new SelectableItem<Category>(newCat) { Selected = true });
        }

        public void SetCategories()
        {
            if (games != null)
            {
                using (database.BufferedUpdate())
                {
                    foreach (var game in games)
                    {
                        var categories = Categories.Where(a => a.Selected == true).Select(a => a.Item.Id).ToComparable();
                        if (game.CategoryIds != null)
                        {
                            foreach (var cat in Categories.Where(a => a.Selected == null))
                            {
                                if (game.CategoryIds.Contains(cat.Item.Id) && !categories.Contains(cat.Item.Id))
                                {
                                    categories.Add(cat.Item.Id);
                                }
                            }
                        }

                        if (categories.HasItems())
                        {
                            game.CategoryIds = categories;
                        }
                        else
                        {
                            game.CategoryIds = null;
                        }

                        if (autoUpdate)
                        {
                            database.Games.Update(game);
                        }
                    }
                }
            }
            else if (game != null)
            {
                var categories = Categories.Where(a => a.Selected == true).Select(a => a.Item.Id);
                if (categories.HasItems())
                {
                    game.CategoryIds = categories.ToComparable();
                }
                else
                {
                    game.CategoryIds = null;
                }

                if (autoUpdate)
                {
                    database.Games.Update(game);
                }
            }

            window.Close(true);
        }

        private void SetCategoryStates()
        {
            if (games != null)
            {
                var catCount = new Dictionary<Guid, int>();
                EnableThreeState = true;
                foreach (var game in games)
                {
                    if (!game.CategoryIds.HasItems())
                    {
                        continue;
                    }

                    foreach (var cat in game.CategoryIds)
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
                    if (catCount.ContainsKey(cat.Item.Id))
                    {
                        if (catCount[cat.Item.Id] == games.Count())
                        {
                            cat.Selected = true;
                        }
                        else
                        {

                            cat.Selected = null;
                        }
                    }
                }
            }
            else
            {
                if (game.CategoryIds.HasItems())
                {
                    // Also offer categories that are held by current game instance, but are not in DB yet
                    // TODO
                    //foreach (var cat in game.Categories.Except(Categories.Select(a => a.Name)))
                    //{
                    //    Categories.Add(new Category(cat, true));
                    //}

                    foreach (var cat in game.CategoryIds)
                    {
                        var existingCat = Categories.FirstOrDefault(a => a.Item.Id == cat);
                        if (existingCat != null)
                        {
                            existingCat.Selected = true;
                        }
                    }
                }
            }
        }

        private ObservableCollection<SelectableItem<Category>> GetAllCategories()
        {
            return database.Categories.Select(a => new SelectableItem<Category>(a)).ToObservable();
        }
    }
}
