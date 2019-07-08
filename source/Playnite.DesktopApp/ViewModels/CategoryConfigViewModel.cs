using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Commands;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public class CategoryConfigViewModel : ObservableObject
    {
        private string newTextCat;
        public string NewTextCat
        {
            get
            {
                return newTextCat;
            }

            set
            {
                newTextCat = value;
                OnPropertyChanged();
            }
        }

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
        private IWindowFactory window;
        private GameDatabase database;

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<object> AddCategoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddCategory();
            });
        }


        public RelayCommand<object> SetCategoriesCommand
        {
            get => new RelayCommand<object>((category) =>
            {
                SetCategories();
            });
        }

        public CategoryConfigViewModel(IWindowFactory window, GameDatabase database, IEnumerable<Game> games)
        {
            this.window = window;
            this.database = database;
            this.games = games;
            EnableThreeState = true;
            Categories = GetAllCategories();
            SetCategoryStates();
        }

        public CategoryConfigViewModel(IWindowFactory window, GameDatabase database, Game game)
        {
            this.window = window;
            this.database = database;
            this.game = game;
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

        public void AddCategory()
        {
            if (NewTextCat.IsNullOrEmpty())
            {
                return;
            }

            var existing = Categories.FirstOrDefault(a => a.Item.Name.Equals(NewTextCat, StringComparison.CurrentCultureIgnoreCase));
            if (existing != null)
            {
                existing.Selected = true;
            }
            else
            {
                var newCat = new Category(NewTextCat);
                Categories.Add(new SelectableItem<Category>(newCat) { Selected = true });
            }

            NewTextCat = string.Empty;
        }

        public void SetCategories()
        {            
            using (database.BufferedUpdate())
            {
                var newCategoeries = Categories.Where(a => (a.Selected == true || a.Selected == null) && database.Categories[a.Item.Id] == null);
                if (newCategoeries.Any())
                {
                    database.Categories.Add(newCategoeries.Select(a => a.Item).ToList());
                }

                if (games != null)
                {
                    var toBeAdded = Categories.Where(a => a.Selected == true || a.Selected == null);
                    var selected = toBeAdded.Where(a => a.Selected == true);
                    foreach (var game in games)
                    {
                        if (toBeAdded.Any())
                        {
                            var toAssign = new List<Guid>();
                            if (selected.Any())
                            {
                                toAssign.AddRange(selected.Select(a => a.Item.Id));
                            }

                            var under = toBeAdded.Where(a => a.Selected == null);
                            if (under.Any() && game.CategoryIds?.Any() == true)
                            {
                                var approp = game.CategoryIds.Intersect(under.Select(a => a.Item.Id));
                                if (approp.Any())
                                {
                                    toAssign.AddRange(approp);
                                }
                            }

                            game.CategoryIds = toAssign;
                        }
                        else
                        {
                            game.CategoryIds = null;
                        }
                    }

                    database.Games.Update(games);
                }
                else if (game != null)
                {
                    var selected = Categories.Where(a => a.Selected == true);
                    if (selected.Any())
                    {
                        game.CategoryIds = selected.Select(a => a.Item.Id).ToList();
                    }
                    else
                    {
                        game.CategoryIds = null;
                    }

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
            return database.Categories.Select(a => new SelectableItem<Category>(a)).OrderBy(a => a.Item.Name).ToObservable();
        }
    }
}
