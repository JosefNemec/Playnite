using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public class DatabaseFieldsManagerViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;

        #region Genres

        public ObservableCollection<Genre> EditingGenres
        {
            get;
        }

        public RelayCommand<object> AddGenreCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingGenres);
            });
        }

        public RelayCommand<IList<object>> RemoveGenreCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingGenres, a.Cast<Genre>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameGenreCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as Genre);
            }, (a) => a?.Count == 1);
        }

        #endregion Genres

        #region Companies

        public ObservableCollection<Company> EditingCompanies
        {
            get;
        }

        public RelayCommand<object> AddCompanyCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingCompanies);
            });
        }

        public RelayCommand<IList<object>> RemoveCompanyCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingCompanies, a.Cast<Company>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameCompanyCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as Company);
            }, (a) => a?.Count == 1);
        }

        #endregion Companies

        #region Tags

        public ObservableCollection<Tag> EditingTags
        {
            get;
        }

        public RelayCommand<object> AddTagCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingTags);
            });
        }

        public RelayCommand<IList<object>> RemoveTagCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingTags, a.Cast<Tag>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameTagCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as Tag);
            }, (a) => a?.Count == 1);
        }

        #endregion Tags

        #region Platforms

        public ObservableCollection<Platform> EditingPlatforms
        {
            get;
        }

        public RelayCommand<object> AddPlatformCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingPlatforms);
            });
        }

        public RelayCommand<IList<object>> RemovePlatformCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingPlatforms, a.Cast<Platform>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenamePlatformCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as Platform);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<IList<object>> SelectPlatformIconCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                SelectPlatformIcon(a.First() as Platform);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<IList<object>> SelectPlatformCoverCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                SelectPlatformCover(a.First() as Platform);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<IList<object>> RemovePlatformIconCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemovePlatformIcon(a.First() as Platform);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<IList<object>> RemovePlatformCoverCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemovePlatformCover(a.First() as Platform);
            }, (a) => a?.Count == 1);
        }

        #endregion Platforms

        #region Series

        public ObservableCollection<Series> EditingSeries
        {
            get;
        }

        public RelayCommand<object> AddSeriesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingSeries);
            });
        }

        public RelayCommand<IList<object>> RemoveSeriesCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingSeries, a.Cast<Series>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameSeriesCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as Series);
            }, (a) => a?.Count == 1);
        }

        #endregion Series

        #region AgeRatings

        public ObservableCollection<AgeRating> EditingAgeRatings
        {
            get;
        }

        public RelayCommand<object> AddAgeRatingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingAgeRatings);
            });
        }

        public RelayCommand<IList<object>> RemoveAgeRatingCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingAgeRatings, a.Cast<AgeRating>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameAgeRatingCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as AgeRating);
            }, (a) => a?.Count == 1);
        }

        #endregion AgeRatings

        #region Regions

        public ObservableCollection<Region> EditingRegions
        {
            get;
        }

        public RelayCommand<object> AddRegionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingRegions);
            });
        }

        public RelayCommand<IList<object>> RemoveRegionCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingRegions, a.Cast<Region>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameRegionCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as Region);
            }, (a) => a?.Count == 1);
        }

        #endregion Regions

        #region Sources

        public ObservableCollection<GameSource> EditingSources
        {
            get;
        }

        public RelayCommand<object> AddSourceCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingSources);
            });
        }

        public RelayCommand<IList<object>> RemoveSourceCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingSources, a.Cast<GameSource>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameSourceCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as GameSource);
            }, (a) => a?.Count == 1);
        }

        #endregion Sources

        #region Categories

        public ObservableCollection<Category> EditingCategories
        {
            get;
        }

        public RelayCommand<object> AddCategoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingCategories);
            });
        }

        public RelayCommand<IList<object>> RemoveCategoryCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingCategories, a.Cast<Category>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameCategoryCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(a.First() as Category);
            }, (a) => a?.Count == 1);
        }

        #endregion Categories


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
            EditingCategories = database.Categories.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingAgeRatings = database.AgeRatings.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingCompanies = database.Companies.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingGenres = database.Genres.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingPlatforms = database.Platforms.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingRegions = database.Regions.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingSeries = database.Series.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingSources = database.Sources.GetClone().OrderBy(a => a.Name).ToObservable();
            EditingTags = database.Tags.GetClone().OrderBy(a => a.Name).ToObservable();
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
            using (database.BufferedUpdate())
            {
                UpdateDbCollection(database.Categories, EditingCategories);
                UpdateDbCollection(database.AgeRatings, EditingAgeRatings);
                UpdateDbCollection(database.Companies, EditingCompanies);
                UpdateDbCollection(database.Genres, EditingGenres);
                UpdateDbCollection(database.Regions, EditingRegions);
                UpdateDbCollection(database.Series, EditingSeries);
                UpdateDbCollection(database.Sources, EditingSources);
                UpdateDbCollection(database.Tags, EditingTags);
                UpdatePlatformsCollection();
            }

            window.Close(true);
        }

        private void UpdateDbCollection<TItem>(IItemCollection<TItem> dbCollection, IList<TItem> updatedCollection) where TItem : DatabaseObject
        {            
            // Remove deleted items
            var removedItems = dbCollection.Where(a => updatedCollection.FirstOrDefault(b => b.Id == a.Id) == null);
            if (removedItems.Any())
            {
                dbCollection.Remove(removedItems.ToList());
            }

            // Add new items
            var addedItems = updatedCollection.Where(a => dbCollection[a.Id] == null);
            if (addedItems.Any())
            {
                dbCollection.Add(addedItems.ToList());
            }

            // Update modified items
            foreach (var item in updatedCollection)
            {
                var dbItem = dbCollection[item.Id];
                if (dbItem != null && !item.IsEqualJson(dbItem))
                {
                    dbCollection.Update(item);
                }
            }
        }

        private void UpdatePlatformsCollection()
        {
            string addNewFile(string path, Guid parent)
            {
                var newPath = database.AddFile(path, parent);
                if (Paths.AreEqual(Path.GetDirectoryName(path), PlaynitePaths.TempPath))
                {
                    File.Delete(path);
                }

                return newPath;
            }

            // Update modified platforms in database
            foreach (var platform in EditingPlatforms.Where(a => database.Platforms[a.Id] != null).ToList())
            {
                var dbPlatform = database.Platforms.Get(platform.Id);
                if (platform.IsEqualJson(dbPlatform))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(platform.Icon) && File.Exists(platform.Icon))
                {
                    platform.Icon = addNewFile(platform.Icon, dbPlatform.Id);
                }

                if (!string.IsNullOrEmpty(platform.Cover) && File.Exists(platform.Cover))
                {
                    platform.Cover = addNewFile(platform.Cover, dbPlatform.Id);
                }
                
                database.Platforms.Update(platform);
            }

            // Remove deleted platforms from database
            var removedItems = database.Platforms.Where(a => EditingPlatforms.FirstOrDefault(b => b.Id == a.Id) == null).ToList();
            database.Platforms.Remove(removedItems);

            // Add new platforms to database
            foreach (var addedPlatform in EditingPlatforms.Where(a => database.Platforms[a.Id] == null).ToList())
            {
                if (!string.IsNullOrEmpty(addedPlatform.Icon))
                {
                    addedPlatform.Icon = addNewFile(addedPlatform.Icon, addedPlatform.Id);
                }

                if (!string.IsNullOrEmpty(addedPlatform.Cover))
                {
                    addedPlatform.Cover = addNewFile(addedPlatform.Cover, addedPlatform.Id);
                }

                database.Platforms.Add(addedPlatform);
            }
        }

        public void AddItem<TItem>(IList<TItem> collection) where TItem : DatabaseObject
        {
            var res = dialogs.SelectString(
                resources.GetString("LOCEnterName"),
                resources.GetString("LOCAddNewItem"),
                "");
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                collection.Add(typeof(TItem).CrateInstance<TItem>(res.SelectedString));
            }
        }

        public void RenameItem<TItem>(TItem item) where TItem : DatabaseObject
        {
            var res = dialogs.SelectString(
                resources.GetString("LOCEnterNewName"),
                resources.GetString("LOCRenameItem"),
                item.Name);
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                item.Name = res.SelectedString;
            }
        }

        public void RemoveItem<TItem>(IList<TItem> collection, IList<TItem> items) where TItem : DatabaseObject
        {
            foreach (var item in items)
            {
                collection.Remove(item);
            }
        }

        public void SelectPlatformIcon(Platform platform)
        {
            var path = dialogs.SelectIconFile();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (path.EndsWith("exe", StringComparison.OrdinalIgnoreCase))
            {
                var ico = System.Drawing.IconExtension.ExtractIconFromExe(path, true);
                if (ico == null)
                {
                    return;
                }

                path = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + ".png");
                FileSystem.PrepareSaveFile(path);
                ico.ToBitmap().Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }

            platform.Icon = path;
        }

        public void SelectPlatformCover(Platform platform)
        {
            var path = dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(path))
            {
                platform.Cover = path;
            }
        }

        public void RemovePlatformIcon(Platform platform)
        {
            platform.Icon = null;
        }

        public void RemovePlatformCover(Platform platform)
        {
            platform.Cover = null;
        }
    }
}
