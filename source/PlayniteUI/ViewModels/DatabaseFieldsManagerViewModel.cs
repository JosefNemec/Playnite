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
    public class DatabaseFieldsManagerViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;

        public ObservableCollection<Category> EditingCategories
        {
            get;
        }

        public ObservableCollection<Genre> EditingGenres
        {
            get;
        }

        public ObservableCollection<Company> EditingCompanies
        {
            get;
        }

        public ObservableCollection<Tag> EditingTags
        {
            get;
        }

        public ObservableCollection<Platform> EditingPlatforms
        {
            get;
        }

        public ObservableCollection<Series> EditingSeries
        {
            get;
        }

        public ObservableCollection<AgeRating> EditingAgeRatings
        {
            get;
        }

        public ObservableCollection<Region> EditingRegions
        {
            get;
        }

        public ObservableCollection<GameSource> EditingSources
        {
            get;
        }

        public RelayCommand<object> AddCategoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddCategory();
            });
        }

        public RelayCommand<IList<object>> RemoveCategoryCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {

            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameCategoryCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameCategory(a.First() as Category);
            }, (a) => a?.Count == 1);
        }













        public RelayCommand<object> SaveCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SaveChanges();
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public DatabaseFieldsManagerViewModel(GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            EditingCategories = database.Categories.GetClone().ToObservable();
            EditingAgeRatings = database.AgeRatings.GetClone().ToObservable();
            EditingCompanies = database.Companies.GetClone().ToObservable();
            EditingGenres = database.Genres.GetClone().ToObservable();
            EditingPlatforms = database.Platforms.GetClone().ToObservable();
            EditingRegions = database.Regions.GetClone().ToObservable();
            EditingSeries = database.Series.GetClone().ToObservable();
            EditingSources = database.Sources.GetClone().ToObservable();
            EditingTags = database.Tags.GetClone().ToObservable();
        }

        public void OpenView()
        {
            window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            window.Close();
        }

        internal void SaveChanges()
        {
            window.Close(true);
        }

        public void AddCategory()
        {
            EditingCategories.Add(new Category("test"));
        }

        public void RenameCategory(Category category)
        {

        }
    }
}
