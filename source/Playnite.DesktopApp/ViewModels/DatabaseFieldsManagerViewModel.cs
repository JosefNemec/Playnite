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
using System.Windows;
using Playnite.Common.Media.Icons;

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
                RenameItem(EditingGenres, a.First() as Genre);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedGenresCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingGenres, g => g.GenreIds);
            }, (a) => EditingGenres.Count > 0);
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
                RenameItem(EditingCompanies, a.First() as Company);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedCompaniesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingCompanies, g =>
                {
                    var ids = new List<Guid>();
                    if (g.DeveloperIds.HasItems())
                    {
                        g.DeveloperIds.ForEach(d => ids.AddMissing(d));
                    }

                    if (g.PublisherIds.HasItems())
                    {
                        g.PublisherIds.ForEach(d => ids.AddMissing(d));
                    }

                    return ids;
                });
            }, (a) => EditingCompanies.Count > 0);
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
                RenameItem(EditingTags, a.First() as Tag);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedTagsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingTags, g => g.TagIds);
            }, (a) => EditingTags.Count > 0);
        }

        #endregion Tags

        #region Features

        public ObservableCollection<GameFeature> EditingFeatures
        {
            get;
        }

        public RelayCommand<object> AddFeatureCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddItem(EditingFeatures);
            });
        }

        public RelayCommand<IList<object>> RemoveFeatureCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemoveItem(EditingFeatures, a.Cast<GameFeature>().ToList());
            }, (a) => a?.Count > 0);
        }

        public RelayCommand<IList<object>> RenameFeatureCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RenameItem(EditingFeatures, a.First() as GameFeature);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedFeaturesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingFeatures, g => g.FeatureIds);
            }, (a) => EditingFeatures.Count > 0);
        }

        #endregion Features

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
                RenameItem(EditingPlatforms, a.First() as Platform);
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

        public RelayCommand<IList<object>> SelectPlatformBackgroundCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                SelectPlatformBackground(a.First() as Platform);
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

        public RelayCommand<IList<object>> RemovePlatformBackgroundCommand
        {
            get => new RelayCommand<IList<object>>((a) =>
            {
                RemovePlatformBackground(a.First() as Platform);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedPlatformsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingPlatforms, g => g.PlatformId);
            }, (a) => EditingPlatforms.Count > 0);
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
                RenameItem(EditingSeries, a.First() as Series);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedSeriesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingSeries, g => g.SeriesId);
            }, (a) => EditingSeries.Count > 0);
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
                RenameItem(EditingAgeRatings, a.First() as AgeRating);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedAgeRatingsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingAgeRatings, g => g.AgeRatingId);
            }, (a) => EditingAgeRatings.Count > 0);
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
                RenameItem(EditingRegions, a.First() as Region);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedRegionsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingRegions, g => g.RegionId);
            }, (a) => EditingRegions.Count > 0);
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
                RenameItem(EditingSources, a.First() as GameSource);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedSourcesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingSources, g => g.SourceId);
            }, (a) => EditingSources.Count > 0);
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
                RenameItem(EditingCategories, a.First() as Category);
            }, (a) => a?.Count == 1);
        }

        public RelayCommand<object> RemoveUnusedCategoriesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveUnusedItems(EditingCategories, g => g.CategoryIds);
            }, (a) => EditingCategories.Count > 0);
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
            EditingFeatures = database.Features.GetClone().OrderBy(a => a.Name).ToObservable();
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
                UpdateDbCollection(database.Features, EditingFeatures);
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

                if (!string.IsNullOrEmpty(platform.Background) && File.Exists(platform.Background))
                {
                    platform.Background = addNewFile(platform.Background, dbPlatform.Id);
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

                if (!string.IsNullOrEmpty(addedPlatform.Background))
                {
                    addedPlatform.Background = addNewFile(addedPlatform.Background, addedPlatform.Id);
                }

                database.Platforms.Add(addedPlatform);
            }
        }

        public void RemoveUnusedItems<TItem>(IList<TItem> sourceCollection, Func<Game, Guid> fieldSelector) where TItem : DatabaseObject
        {
            if (sourceCollection.Count == 0)
            {
                return;
            }

            var unused = new List<Guid>(sourceCollection.Select(a => a.Id));
            foreach (var game in database.Games)
            {
                var usedId = fieldSelector(game);
                if (unused.Contains(usedId))
                {
                    unused.Remove(usedId);
                }
            }

            if (unused.Count > 0)
            {
                if (dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCRemoveUnusedFieldsAskMessage"), unused.Count),
                    "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (var item in unused)
                    {
                        var srcItem = sourceCollection.First(a => a.Id == item);
                        sourceCollection.Remove(srcItem);
                    }
                }
            }
            else
            {
                dialogs.ShowMessage(resources.GetString("LOCRemoveUnusedFieldsNoUnusedMessage"));
            }
        }

        public void RemoveUnusedItems<TItem>(IList<TItem> sourceCollection, Func<Game, List<Guid>> fieldSelector) where TItem : DatabaseObject
        {
            if (sourceCollection.Count == 0)
            {
                return;
            }

            var unused = new List<Guid>(sourceCollection.Select(a => a.Id));
            foreach (var game in database.Games)
            {
                var usedIds = fieldSelector(game) ?? new List<Guid>();
                foreach (var item in unused.Intersect(usedIds).ToList())
                {
                    unused.Remove(item);
                }
            }

            if (unused.Count > 0)
            {
                if (dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCRemoveUnusedFieldsAskMessage"), unused.Count),
                    "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (var item in unused)
                    {
                        var srcItem = sourceCollection.First(a => a.Id == item);
                        sourceCollection.Remove(srcItem);
                    }
                }
            }
            else
            {
                dialogs.ShowMessage(resources.GetString("LOCRemoveUnusedFieldsNoUnusedMessage"));
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
                if (collection.Any(a => a.Name.Equals(res.SelectedString, StringComparison.InvariantCultureIgnoreCase)))
                {
                    dialogs.ShowErrorMessage(resources.GetString("LOCItemAlreadyExists"), "");
                }
                else
                {
                    collection.Add(typeof(TItem).CrateInstance<TItem>(res.SelectedString));
                }
            }
        }

        public void RenameItem<TItem>(IList<TItem> collection, TItem item) where TItem : DatabaseObject
        {
            var res = dialogs.SelectString(
                resources.GetString("LOCEnterNewName"),
                resources.GetString("LOCRenameItem"),
                item.Name);
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                if (collection.Any(a => a.Name.Equals(res.SelectedString, StringComparison.InvariantCultureIgnoreCase) && a.Id != item.Id))
                {
                    dialogs.ShowErrorMessage(resources.GetString("LOCItemAlreadyExists"), "");
                }
                else
                {
                    item.Name = res.SelectedString;
                }
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
            var iconPath = dialogs.SelectIconFile();
            if (string.IsNullOrEmpty(iconPath))
            {
                return;
            }

            if (iconPath.EndsWith("exe", StringComparison.OrdinalIgnoreCase))
            {
                var convertedPath = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + ".ico");
                if (IconExtractor.ExtractMainIconFromFile(iconPath, convertedPath))
                {
                    iconPath = convertedPath;
                }
                else
                {
                    iconPath = null;
                }
            }

            platform.Icon = iconPath;
        }

        public void SelectPlatformCover(Platform platform)
        {
            var path = dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(path))
            {
                platform.Cover = path;
            }
        }

        public void SelectPlatformBackground(Platform platform)
        {
            var path = dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(path))
            {
                platform.Background = path;
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

        public void RemovePlatformBackground(Platform platform)
        {
            platform.Background = null;
        }
    }
}
