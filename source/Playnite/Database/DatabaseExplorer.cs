using Playnite.Commands;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.Database
{
    public enum ExplorerField
    {
        [Description(LOC.SettingsTopPanelFilterPresetsItem)]
        Presets,
        [Description(LOC.PlatformTitle)]
        Platform,
        [Description(LOC.GameProviderTitle)]
        Library,
        [Description(LOC.CategoryLabel)]
        Category,
        [Description(LOC.GameLastActivityTitle)]
        LastActivity,
        [Description(LOC.RecentActivityLabel)]
        RecentActivity,
        [Description(LOC.GenreLabel)]
        Genre,
        [Description(LOC.GameReleaseYearTitle)]
        ReleaseYear,
        [Description(LOC.DeveloperLabel)]
        Developer,
        [Description(LOC.PublisherLabel)]
        Publisher,
        [Description(LOC.TagLabel)]
        Tag,
        [Description(LOC.SeriesLabel)]
        Series,
        [Description(LOC.AgeRatingLabel)]
        AgeRating,
        [Description(LOC.RegionLabel)]
        Region,
        [Description(LOC.SourceLabel)]
        Source,
        [Description(LOC.TimePlayed)]
        PlayTime,
        [Description(LOC.InstallSizeLabel)]
        InstallSize,
        [Description(LOC.CompletionStatus)]
        CompletionStatus,
        [Description(LOC.UserScore)]
        UserScore,
        [Description(LOC.CriticScore)]
        CriticScore,
        [Description(LOC.CommunityScore)]
        CommunityScore,
        [Description(LOC.DateAddedLabel)]
        Added,
        [Description(LOC.DateModifiedLabel)]
        Modified,
        [Description(LOC.FeatureLabel)]
        Feature,
        [Description(LOC.GameNameTitle)]
        Name
    }

    // TODO: Rewrite this mess.
    public class DatabaseExplorer : ObservableObject
    {
        public class ExplorableField
        {
            public ExplorerField Field { get; }

            public ExplorableField(ExplorerField field)
            {
                Field = field;
            }

            public override string ToString()
            {
                return Field.GetDescription();
            }
        }

        public enum SelectionObjectType : int
        {
            All = 9998,
            None = 9999
        }

        public class SelectionObject : ObservableObject
        {
            public string DisplayName { get; }
            public object Value { get; }

            public string Name
            {
                get => DisplayName.IsNullOrEmpty() ? Value.ToString() : DisplayName;
            }

            public SelectionObject(object value, string displayName)
            {
                Value = value;
                DisplayName = displayName;
            }

            public SelectionObject(object value)
            {
                Value = value;
            }
        }

        private readonly IGameDatabaseMain database;
        private readonly ExtensionFactory extensions;
        private readonly FilterSettings filters;
        private readonly PlayniteSettings settings;
        private readonly MainViewModelBase mainModel;
        private bool ignoreObjectSelectionChanges = false;

        public List<ExplorableField> Fields { get; set; }

        private ExplorableField selectedField;
        public ExplorableField SelectedField
        {
            get => selectedField;
            set
            {
                if (value != selectedField)
                {
                    if (selectedField != null)
                    {
                        ApplyFilter(selectedField.Field, null);
                    }

                    selectedField = value;
                    if (selectedField != null)
                    {
                        settings.ViewSettings.SelectedExplorerField = selectedField.Field;
                        LoadValues(selectedField.Field);
                    }

                    OnPropertyChanged();
                }
            }
        }

        private List<SelectionObject> fieldValues;
        public List<SelectionObject> FieldValues
        {
            get => fieldValues;
            set
            {
                fieldValues = value;
                OnPropertyChanged();
            }
        }

        private SelectionObject selectedFieldObject;
        public SelectionObject SelectedFieldObject
        {
            get => selectedFieldObject;
            set
            {
                selectedFieldObject = value;
                if (selectedFieldObject != null && !ignoreObjectSelectionChanges)
                {
                    ApplyFilter(SelectedField.Field, selectedFieldObject);
                }
                OnPropertyChanged();
            }
        }

        public DatabaseExplorer(
            IGameDatabaseMain database,
            ExtensionFactory extensions,
            PlayniteSettings settings,
            MainViewModelBase mainModel)
        {
            this.database = database;
            this.extensions = extensions;
            this.filters = settings.FilterSettings;
            this.settings = settings;
            this.mainModel = mainModel;
            settings.PropertyChanged += Settings_PropertyChanged;

            Fields = new List<ExplorableField>();
            foreach (ExplorerField val in Enum.GetValues(typeof(ExplorerField)))
            {
                Fields.Add(new ExplorableField(val));
            }

            Fields = Fields.OrderBy(a => a.Field.GetDescription()).ToList();

            if (database.IsOpen)
            {
                if (settings.ExplorerPanelVisible)
                {
                    SelectedField = Fields.FirstOrDefault(a => a.Field == settings.ViewSettings.SelectedExplorerField);
                }
            }
            else
            {
                database.DatabaseOpened += (s, e) =>
                {
                    if (settings.ExplorerPanelVisible)
                    {
                        SelectedField = Fields.FirstOrDefault(a => a.Field == settings.ViewSettings.SelectedExplorerField);
                    }
                };
            }

            database.Games.ItemUpdated += Games_ItemUpdated;
            database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;
            database.Platforms.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Platform, e);
            database.Platforms.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Platform, e);
            database.Genres.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Genre, e);
            database.Genres.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Genre, e);
            database.AgeRatings.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.AgeRating, e);
            database.AgeRatings.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.AgeRating, e);
            database.Categories.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Category, e);
            database.Categories.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Category, e);
            database.Regions.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Region, e);
            database.Regions.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Region, e);
            database.Series.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Series, e);
            database.Series.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Series, e);
            database.Sources.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Source, e);
            database.Sources.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Source, e);
            database.Tags.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Tag, e);
            database.Tags.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Tag, e);
            database.Features.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Feature, e);
            database.Features.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Feature, e);
            database.FilterPresets.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(ExplorerField.Presets, e);
            database.FilterPresets.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(ExplorerField.Presets, e);
            (database.FilterPresets as FilterPresetsCollection).OnSettingsUpdated += Database_OnFilterSettingsUpdated;
            database.Companies.ItemCollectionChanged += (s, e) =>
            {
                DatabaseCollection_ItemCollectionChanged(ExplorerField.Publisher, e);
                DatabaseCollection_ItemCollectionChanged(ExplorerField.Developer, e);
            };

            database.Companies.ItemUpdated += (s, e) =>
            {
                DatabaseCollection_ItemUpdated(ExplorerField.Publisher, e);
                DatabaseCollection_ItemUpdated(ExplorerField.Developer, e);
            };

            database.AgeRatingsInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.AgeRating);
            database.CategoriesInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Category);
            database.DevelopersInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Developer);
            database.FeaturesInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Feature);
            database.GenresInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Genre);
            database.PlatformsInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Platform);
            database.PublishersInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Publisher);
            database.RegionsInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Region);
            database.SeriesInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Series);
            database.SourcesInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Source);
            database.TagsInUseUpdated += (_, __) => Database_DatabaseCollectionInUseUpdated(ExplorerField.Tag);
        }

        private void Database_OnFilterSettingsUpdated(object sender, FilterPresetsSettingsUpdateEvent e)
        {
            if (SelectedField?.Field != ExplorerField.Presets)
            {
                return;
            }

            LoadValues(ExplorerField.Presets);
        }

        private void Database_DatabaseCollectionInUseUpdated(ExplorerField field)
        {
            if (!settings.UsedFieldsOnlyOnFilterLists)
            {
                return;
            }

            if (settings.ExplorerPanelVisible && SelectedField.Field == field)
            {
                var oldSelection = SelectedFieldObject;
                ignoreObjectSelectionChanges = true;
                var refreshSelection = false;
                LoadValues(field);
                if (oldSelection != null && FieldValues.FirstOrDefault(a => a.Value.Equals(oldSelection.Value)) != null)
                {
                    SelectedFieldObject = FieldValues.FirstOrDefault(a => a.Value.Equals(oldSelection.Value));
                }
                else
                {
                    refreshSelection = true;
                }

                ignoreObjectSelectionChanges = false;
                if (refreshSelection)
                {
                    SelectedFieldObject = FieldValues[0];
                }
            }
        }

        private void DatabaseCollection_ItemUpdated<T>(ExplorerField field, ItemUpdatedEventArgs<T> e) where T : DatabaseObject
        {
            if (settings.ExplorerPanelVisible && SelectedField.Field == field)
            {
                foreach (var item in FieldValues)
                {
                    item.OnPropertyChanged(nameof(SelectionObject.Name));
                }
            }
        }

        private void DatabaseCollection_ItemCollectionChanged<T>(ExplorerField field, ItemCollectionChangedEventArgs<T> e) where T : DatabaseObject
        {
            if (settings.UsedFieldsOnlyOnFilterLists && field != ExplorerField.Presets)
            {
                return;
            }

            if (settings.ExplorerPanelVisible && SelectedField.Field == field)
            {
                var oldSelection = SelectedFieldObject;
                ignoreObjectSelectionChanges = true;
                var refreshSelection = false;
                LoadValues(field);
                if (oldSelection != null && FieldValues.FirstOrDefault(a => a.Value.Equals(oldSelection.Value)) != null)
                {
                    SelectedFieldObject = FieldValues.FirstOrDefault(a => a.Value.Equals(oldSelection.Value));
                }
                else
                {
                    refreshSelection = true;
                }

                ignoreObjectSelectionChanges = false;
                if (refreshSelection)
                {
                    if (FieldValues.HasItems())
                    {
                        SelectedFieldObject = FieldValues[0];
                    }
                    else
                    {
                        SelectedFieldObject = null;
                    }
                }
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteSettings.ExplorerPanelVisible))
            {
                if (settings.ExplorerPanelVisible)
                {
                    SelectedField = Fields.FirstOrDefault(a => a.Field == settings.ViewSettings.SelectedExplorerField);
                }
                else
                {
                    SelectedField = null;
                }
            }
            else if (e.PropertyName == nameof(PlayniteSettings.UsedFieldsOnlyOnFilterLists) && settings.ExplorerPanelVisible)
            {
                DatabaseCollection_ItemCollectionChanged<DatabaseObject>(SelectedField.Field, null);
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            if (e.AddedItems.HasItems())
            {
                ProcessGameDataChages(e.AddedItems);
            }
        }

        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            ProcessGameDataChages(e.UpdatedItems.Select(a => a.NewData));
        }

        private void ProcessGameDataChages(IEnumerable<Game> gameUpdates)
        {
            if (!settings.ExplorerPanelVisible)
            {
                return;
            }

            if (SelectedField.Field == ExplorerField.ReleaseYear)
            {
                foreach (var change in gameUpdates)
                {
                    if (!IsReleaseYearLoaded(change.ReleaseYear))
                    {
                        UpdateReleaseDateValues();
                        break;
                    }
                }
            }
        }

        private bool IsRelevantGameFieldLoaded(Game game)
        {
            var relevantFieldChanged = false;
            switch (SelectedField.Field)
            {
                case ExplorerField.Category:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.CategoryIds);
                    break;
                case ExplorerField.Genre:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.GenreIds);
                    break;
                case ExplorerField.Developer:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.DeveloperIds);
                    break;
                case ExplorerField.Publisher:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.PublisherIds);
                    break;
                case ExplorerField.Tag:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.TagIds);
                    break;
                case ExplorerField.Platform:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.PlatformIds);
                    break;
                case ExplorerField.Series:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.SeriesIds);
                    break;
                case ExplorerField.AgeRating:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.AgeRatingIds);
                    break;
                case ExplorerField.Region:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.RegionIds);
                    break;
                case ExplorerField.Source:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.SourceId);
                    break;
                case ExplorerField.ReleaseYear:
                    relevantFieldChanged = IsReleaseYearLoaded(game.ReleaseYear);
                    break;
                case ExplorerField.Feature:
                    relevantFieldChanged = IsExplorableDbObjectLoaded(game.FeatureIds);
                    break;
            }

            return relevantFieldChanged;
        }

        private bool IsExplorableDbObjectLoaded(Guid id)
        {
            foreach (var val in FieldValues)
            {
                if (val.Value is DatabaseObject obj)
                {
                    if (obj.Id == id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsExplorableDbObjectLoaded(List<Guid> ids)
        {
            if (!ids.HasItems())
            {
                return true;
            }

            return FieldValues.Where(a => a.Value is DatabaseObject).Select(a => ((DatabaseObject)a.Value).Id).Contains(ids);
        }

        private void UpdateReleaseDateValues()
        {
            var oldSelection = SelectedFieldObject;
            ignoreObjectSelectionChanges = true;
            LoadValues(ExplorerField.ReleaseYear);
            if (oldSelection != null)
            {
                SelectedFieldObject = FieldValues.FirstOrDefault(a => a.Value.Equals(oldSelection.Value));
            }

            ignoreObjectSelectionChanges = false;
        }

        private bool IsReleaseYearLoaded(int? year)
        {
            if (year == null)
            {
                return true;
            }

            foreach (var obj in FieldValues)
            {
                if (obj is SelectionObject y)
                {
                    if (y.Value == null && year == null)
                    {
                        return true;
                    }

                    if (int.TryParse(y.Value.ToString(), out var parsedYear))
                    {
                        if (parsedYear == year)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private IdItemFilterItemProperties GetIdFilter(SelectionObject filter)
        {
            if (filter?.Value == null)
            {
                return null;
            }
            else if (filter.Value is SelectionObjectType flt && flt == SelectionObjectType.All)
            {
                return null;
            }
            else
            {
                return new IdItemFilterItemProperties(((IIdentifiable)filter.Value).Id);
            }
        }

        private string GetStringFilter(SelectionObject filter)
        {
            if (filter == null)
            {
                return null;
            }
            else if (filter.Value is SelectionObjectType ftl && ftl == SelectionObjectType.All)
            {
                return null;
            }
            else if (filter.Value is DatabaseObject obj && obj.Id == Guid.Empty)
            {
                return FilterSettings.MissingFieldString;
            }
            else
            {
                return filter.Value.ToString();
            }
        }

        private StringFilterItemProperties GetStringListFilter(SelectionObject filter)
        {
            if (filter == null)
            {
                return null;
            }
            else if (filter.Value is SelectionObjectType flt && flt == SelectionObjectType.All)
            {
                return null;
            }
            else if (filter.Value is DatabaseObject obj && obj.Id == Guid.Empty)
            {
                return new StringFilterItemProperties(FilterSettings.MissingFieldString);
            }
            else
            {
                return new StringFilterItemProperties(filter.Value.ToString());
            }
        }

        private EnumFilterItemProperties GetEnumFilter(SelectionObject filter)
        {
            if (filter == null)
            {
                return null;
            }
            else if (filter is SelectionObject flt && (SelectionObjectType)flt.Value == SelectionObjectType.All)
            {
                return null;
            }
            else
            {
                return new EnumFilterItemProperties((int)filter.Value);
            }
        }

        private void ApplyFilter(ExplorerField field, SelectionObject filter)
        {
            if (!settings.ExplorerPanelVisible)
            {
                return;
            }

            switch (field)
            {
                case ExplorerField.Library:
                    filters.Library = GetIdFilter(filter);
                    break;
                case ExplorerField.Category:
                    filters.Category = GetIdFilter(filter);
                    break;
                case ExplorerField.Genre:
                    filters.Genre = GetIdFilter(filter);
                    break;
                case ExplorerField.Developer:
                    filters.Developer = GetIdFilter(filter);
                    break;
                case ExplorerField.Publisher:
                    filters.Publisher = GetIdFilter(filter);
                    break;
                case ExplorerField.Tag:
                    filters.Tag = GetIdFilter(filter);
                    break;
                case ExplorerField.Platform:
                    filters.Platform = GetIdFilter(filter);
                    break;
                case ExplorerField.Series:
                    filters.Series = GetIdFilter(filter);
                    break;
                case ExplorerField.AgeRating:
                    filters.AgeRating = GetIdFilter(filter);
                    break;
                case ExplorerField.Region:
                    filters.Region = GetIdFilter(filter);
                    break;
                case ExplorerField.Source:
                    filters.Source = GetIdFilter(filter);
                    break;
                case ExplorerField.ReleaseYear:
                    filters.ReleaseYear = GetStringListFilter(filter);
                    break;
                case ExplorerField.CompletionStatus:
                    filters.CompletionStatuses = GetIdFilter(filter);
                    break;
                case ExplorerField.UserScore:
                    filters.UserScore = GetEnumFilter(filter);
                    break;
                case ExplorerField.CommunityScore:
                    filters.CommunityScore = GetEnumFilter(filter);
                    break;
                case ExplorerField.CriticScore:
                    filters.CriticScore = GetEnumFilter(filter);
                    break;
                case ExplorerField.LastActivity:
                    filters.LastActivity = GetEnumFilter(filter);
                    break;
                case ExplorerField.RecentActivity:
                    filters.RecentActivity = GetEnumFilter(filter);
                    break;
                case ExplorerField.Added:
                    filters.Added = GetEnumFilter(filter);
                    break;
                case ExplorerField.Modified:
                    filters.Modified = GetEnumFilter(filter);
                    break;
                case ExplorerField.PlayTime:
                    filters.PlayTime = GetEnumFilter(filter);
                    break;
                case ExplorerField.InstallSize:
                    filters.InstallSize = GetEnumFilter(filter);
                    break;
                case ExplorerField.Feature:
                    filters.Feature = GetIdFilter(filter);
                    break;
                case ExplorerField.Presets:
                    if (filter?.Value != null)
                    {
                        mainModel.ActiveFilterPreset = filter.Value as FilterPreset;
                    }
                    else
                    {
                        mainModel.ActiveFilterPreset = null;
                    }
                    break;
                case ExplorerField.Name:
                    filters.Name = GetStringFilter(filter);
                    break;
                default:
                    if (PlayniteEnvironment.ThrowAllErrors)
                    {
                        throw new NotSupportedException();
                    }
                    else
                    {
                        break;
                    }
            }
        }

        private SelectionObject allObject = new SelectionObject(SelectionObjectType.All, ResourceProvider.GetString("LOCAll"));
        private SelectionObject noneDbObject = new SelectionObject(
            new DatabaseObject()
            {
                Id = Guid.Empty,
                Name = ResourceProvider.GetString("LOCNone")
            }, ResourceProvider.GetString("LOCNone"));
        private SelectionObject noneObject = new SelectionObject(SelectionObjectType.None, ResourceProvider.GetString("LOCNone"));

        private void LoadValues(ExplorerField field)
        {
            if (!database.IsOpen || !settings.ExplorerPanelVisible)
            {
                return;
            }

            var values = new List<SelectionObject>()
            {
                allObject
            };

            switch (field)
            {
                case ExplorerField.Library:
                    var libs = extensions.LibraryPlugins.ToList();
                    libs.Add(new FakePlayniteLibraryPlugin());
                    values.AddRange(libs.Select(a => new SelectionObject(a, a.Name)));
                    break;
                case ExplorerField.Category:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedCategories.Select(a => database.Categories[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Categories.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Genre:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedGenres.Select(a => database.Genres[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Genres.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Developer:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedDevelopers.Select(a => database.Companies[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Companies.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Publisher:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedPublishers.Select(a => database.Companies[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Companies.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Tag:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedTags.Select(a => database.Tags[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Tags.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Platform:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedPlatforms.Select(a => database.Platforms[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Platforms.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Series:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedSeries.Select(a => database.Series[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Series.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.AgeRating:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedAgeRatings.Select(a => database.AgeRatings[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.AgeRatings.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Region:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedRegions.Select(a => database.Regions[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Regions.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Source:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedSources.Select(a => database.Sources[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Sources.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.ReleaseYear:
                    values.Add(noneDbObject);
                    var years = database.Games.Where(a => a.ReleaseYear != null).Select(a => a.ReleaseYear).Distinct().OrderBy(a => a.Value);
                    values.AddRange(years.Select(a => new SelectionObject(a)));
                    break;
                case ExplorerField.CompletionStatus:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedCompletionStatuses.Select(a => database.CompletionStatuses[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.CompletionStatuses.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.UserScore:
                case ExplorerField.CriticScore:
                case ExplorerField.CommunityScore:
                    values.AddRange(GenerateEnumValues(typeof(ScoreGroup)));
                    break;
                case ExplorerField.LastActivity:
                case ExplorerField.RecentActivity:
                case ExplorerField.Added:
                case ExplorerField.Modified:
                    values.AddRange(GenerateEnumValues(typeof(PastTimeSegment)));
                    break;
                case ExplorerField.PlayTime:
                    values.AddRange(GenerateEnumValues(typeof(PlaytimeCategory)));
                    break;
                case ExplorerField.InstallSize:
                    values.AddRange(GenerateEnumValues(typeof(InstallSizeGroup)));
                    break;
                case ExplorerField.Feature:
                    values.Add(noneDbObject);
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        values.AddRange(database.UsedFeastures.Select(a => database.Features[a]).OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    else
                    {
                        values.AddRange(database.Features.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    }
                    break;
                case ExplorerField.Presets:
                    values.Clear();
                    values.AddRange(database.GetSortedFilterPresets().Select(a => new SelectionObject(a)));
                    break;
                case ExplorerField.Name:
                    values.Add(new SelectionObject("^#", "#"));
                    values.AddRange(Enumerable.Range('A', 26).Select(a => new SelectionObject("^" + ((char)a).ToString(), ((char)a).ToString())));
                    break;
                default:
                    if (PlayniteEnvironment.ThrowAllErrors)
                    {
                        throw new NotSupportedException();
                    }
                    else
                    {
                        break;
                    }
            }

            FieldValues = values;
            if (field != ExplorerField.Presets)
            {
                SelectedFieldObject = allObject;
            }
        }

        public IEnumerable<SelectionObject> GenerateEnumValues(Type enumType)
        {
            foreach (Enum status in Enum.GetValues(enumType))
            {
                yield return new SelectionObject(status, status.GetDescription());
            }
        }
    }
}
