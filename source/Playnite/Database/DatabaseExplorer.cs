using Playnite.Commands;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.Database
{
    public class DatabaseExplorer : ObservableObject
    {
        public class ExplorableField
        {
            public GroupableField Field { get; }

            public ExplorableField(GroupableField field)
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

        private readonly IGameDatabase database;
        private readonly ExtensionFactory extensions;
        private readonly FilterSettings filters;
        private readonly PlayniteSettings settings;
        private bool ignoreObjectSelectionChanges = false;

        public List<ExplorableField> Fields { get; set; }

        private ExplorableField selectedField;
        public ExplorableField SelectedField
        {
            get => selectedField;
            set
            {
                if (selectedField != null)
                {
                    ApplyFilter(selectedField.Field, null);
                }

                selectedField = value;
                settings.ViewSettings.SelectedExplorerField = selectedField.Field;
                LoadValues(selectedField.Field);
                OnPropertyChanged();
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

        public DatabaseExplorer(IGameDatabase database, ExtensionFactory extensions, PlayniteSettings settings)
        {
            this.database = database;
            this.extensions = extensions;
            this.filters = settings.FilterSettings;
            this.settings = settings;
            settings.PropertyChanged += Settings_PropertyChanged;

            Fields = new List<ExplorableField>();
            foreach (GroupableField val in Enum.GetValues(typeof(GroupableField)))
            {
                if (val != GroupableField.None && val != GroupableField.InstallationStatus)
                {
                    Fields.Add(new ExplorableField(val));
                }
            }

            Fields = Fields.OrderBy(a => a.Field.GetDescription()).ToList();

            SelectedField = Fields.FirstOrDefault(a => a.Field == settings.ViewSettings.SelectedExplorerField);
            if (!database.IsOpen)
            {
                database.DatabaseOpened += (s, e) => LoadValues(SelectedField.Field);
            }

            database.Games.ItemUpdated += Games_ItemUpdated;
            database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;
            database.Platforms.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Platform, e);
            database.Platforms.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Platform, e);
            database.Genres.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Genre, e);
            database.Genres.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Genre, e);
            database.AgeRatings.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.AgeRating, e);
            database.AgeRatings.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.AgeRating, e);
            database.Categories.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Category, e);
            database.Categories.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Category, e);
            database.Regions.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Region, e);
            database.Regions.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Region, e);
            database.Series.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Series, e);
            database.Series.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Series, e);
            database.Sources.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Source, e);
            database.Sources.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Source, e);
            database.Tags.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Tag, e);
            database.Tags.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Tag, e);
            database.Features.ItemCollectionChanged += (s, e) => DatabaseCollection_ItemCollectionChanged(GroupableField.Feature, e);
            database.Features.ItemUpdated += (s, e) => DatabaseCollection_ItemUpdated(GroupableField.Feature, e);
            database.Companies.ItemCollectionChanged += (s, e) =>
            {
                DatabaseCollection_ItemCollectionChanged(GroupableField.Publisher, e);
                DatabaseCollection_ItemCollectionChanged(GroupableField.Developer, e);
            };

            database.Companies.ItemUpdated += (s, e) =>
            {
                DatabaseCollection_ItemUpdated(GroupableField.Publisher, e);
                DatabaseCollection_ItemUpdated(GroupableField.Developer, e);
            };
        }

        private void DatabaseCollection_ItemUpdated<T>(GroupableField field, ItemUpdatedEventArgs<T> e) where T : DatabaseObject
        {
            if (settings.ExplorerPanelVisible && SelectedField.Field == field)
            {
                foreach (var item in FieldValues)
                {
                    item.OnPropertyChanged(nameof(SelectionObject.Name));
                }
            }
        }

        private void DatabaseCollection_ItemCollectionChanged<T>(GroupableField field, ItemCollectionChangedEventArgs<T> e) where T : DatabaseObject
        {
            if (settings.ExplorerPanelVisible && SelectedField.Field == field)
            {
                var oldSelection = SelectedFieldObject;
                ignoreObjectSelectionChanges = true;
                LoadValues(field);
                if (oldSelection != null)
                {
                    SelectedFieldObject = FieldValues.FirstOrDefault(a => a.Value.Equals(oldSelection.Value));
                }

                ignoreObjectSelectionChanges = false;
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteSettings.ExplorerPanelVisible) && settings.ExplorerPanelVisible)
            {
                SelectedField = Fields.FirstOrDefault(a => a.Field == settings.ViewSettings.SelectedExplorerField);
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            if (settings.ExplorerPanelVisible && SelectedField.Field == GroupableField.ReleaseYear)
            {
                foreach (var change in e.AddedItems)
                {
                    if (!LoadedReleaseYears().Contains(change.ReleaseYear))
                    {
                        UpdateReleaseDateValues();
                        break;
                    }
                }
            }
        }

        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            if (settings.ExplorerPanelVisible && SelectedField.Field == GroupableField.ReleaseYear)
            {
                foreach (var change in e.UpdatedItems)
                {
                    if (change.OldData.ReleaseDate != change.NewData.ReleaseDate && !LoadedReleaseYears().Contains(change.NewData.ReleaseYear))
                    {
                        UpdateReleaseDateValues();
                        break;
                    }
                }
            }
        }

        private void UpdateReleaseDateValues()
        {
            var oldSelection = SelectedFieldObject;
            ignoreObjectSelectionChanges = true;
            LoadValues(GroupableField.ReleaseYear);
            if (oldSelection != null)
            {
                SelectedFieldObject = FieldValues.FirstOrDefault(a => a.Value.Equals(oldSelection.Value));
            }

            ignoreObjectSelectionChanges = false;
        }

        private IEnumerable<int?> LoadedReleaseYears()
        {
            foreach (var obj in FieldValues)
            {
                if (obj is SelectionObject year)
                {
                    yield return year.Value as int?;
                }
            }
        }

        private FilterItemProperites GetIdFilter(SelectionObject filter)
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
                return new FilterItemProperites(((IIdentifiable)filter.Value).Id);
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

        private StringFilterItemProperites GetStringListFilter(SelectionObject filter)
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
                return new StringFilterItemProperites(FilterSettings.MissingFieldString);
            }
            else
            {
                return new StringFilterItemProperites(filter.Value.ToString());
            }
        }

        private EnumFilterItemProperites GetEnumFilter(SelectionObject filter)
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
                return new EnumFilterItemProperites((int)filter.Value);
            }
        }

        private void ApplyFilter(GroupableField field, SelectionObject filter)
        {
            if (!settings.ExplorerPanelVisible)
            {
                return;
            }

            switch (field)
            {
                case GroupableField.Library:
                    filters.Library = GetIdFilter(filter);
                    break;
                case GroupableField.Category:
                    filters.Category = GetIdFilter(filter);
                    break;
                case GroupableField.Genre:
                    filters.Genre = GetIdFilter(filter);
                    break;
                case GroupableField.Developer:
                    filters.Developer = GetIdFilter(filter);
                    break;
                case GroupableField.Publisher:
                    filters.Publisher = GetIdFilter(filter);
                    break;
                case GroupableField.Tag:
                    filters.Tag = GetIdFilter(filter);
                    break;
                case GroupableField.Platform:
                    filters.Platform = GetIdFilter(filter);
                    break;
                case GroupableField.Series:
                    filters.Series = GetIdFilter(filter);
                    break;
                case GroupableField.AgeRating:
                    filters.AgeRating = GetIdFilter(filter);
                    break;
                case GroupableField.Region:
                    filters.Region = GetIdFilter(filter);
                    break;
                case GroupableField.Source:
                    filters.Source = GetIdFilter(filter);
                    break;
                case GroupableField.ReleaseYear:
                    filters.ReleaseYear = GetStringListFilter(filter);
                    break;
                case GroupableField.CompletionStatus:
                    filters.CompletionStatus = GetEnumFilter(filter);
                    break;
                case GroupableField.UserScore:
                    filters.UserScore = GetEnumFilter(filter);
                    break;
                case GroupableField.CommunityScore:
                    filters.CommunityScore = GetEnumFilter(filter);
                    break;
                case GroupableField.CriticScore:
                    filters.CriticScore = GetEnumFilter(filter);
                    break;
                case GroupableField.LastActivity:
                    filters.LastActivity = GetEnumFilter(filter);
                    break;
                case GroupableField.Added:
                    filters.Added = GetEnumFilter(filter);
                    break;
                case GroupableField.Modified:
                    filters.Modified = GetEnumFilter(filter);
                    break;
                case GroupableField.PlayTime:
                    filters.PlayTime = GetEnumFilter(filter);
                    break;
                case GroupableField.Feature:
                    filters.Feature = GetIdFilter(filter);
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

        private void LoadValues(GroupableField field)
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
                case GroupableField.Library:
                    var libs = extensions.LibraryPlugins.ToList();
                    libs.Add(new FakePlayniteLibraryPlugin());
                    values.AddRange(libs.Select(a => new SelectionObject(a, a.Name)));
                    break;
                case GroupableField.Category:
                    values.Add(noneDbObject);
                    values.AddRange(database.Categories.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Genre:
                    values.Add(noneDbObject);
                    values.AddRange(database.Genres.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Developer:
                    values.Add(noneDbObject);
                    values.AddRange(database.Companies.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Publisher:
                    values.Add(noneDbObject);
                    values.AddRange(database.Companies.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Tag:
                    values.Add(noneDbObject);
                    values.AddRange(database.Tags.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Platform:
                    values.Add(noneDbObject);
                    values.AddRange(database.Platforms.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Series:
                    values.Add(noneDbObject);
                    values.AddRange(database.Series.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.AgeRating:
                    values.Add(noneDbObject);
                    values.AddRange(database.AgeRatings.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Region:
                    values.Add(noneDbObject);
                    values.AddRange(database.Regions.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.Source:
                    values.Add(noneDbObject);
                    values.AddRange(database.Sources.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.ReleaseYear:
                    values.Add(noneDbObject);
                    var years = database.Games.Where(a => a.ReleaseYear != null).Select(a => a.ReleaseYear).Distinct().OrderBy(a => a.Value);
                    values.AddRange(years.Select(a => new SelectionObject(a)));
                    break;
                case GroupableField.CompletionStatus:
                    values.AddRange(GenerateEnumValues(typeof(CompletionStatus)));
                    break;
                case GroupableField.UserScore:
                case GroupableField.CriticScore:
                case GroupableField.CommunityScore:
                    values.AddRange(GenerateEnumValues(typeof(ScoreGroup)));
                    break;
                case GroupableField.LastActivity:
                case GroupableField.Added:
                case GroupableField.Modified:
                    values.AddRange(GenerateEnumValues(typeof(PastTimeSegment)));
                    break;
                case GroupableField.PlayTime:
                    values.AddRange(GenerateEnumValues(typeof(PlaytimeCategory)));
                    break;
                case GroupableField.Feature:
                    values.Add(noneDbObject);
                    values.AddRange(database.Features.OrderBy(a => a.Name).Select(a => new SelectionObject(a)));
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
            SelectedFieldObject = allObject;
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
