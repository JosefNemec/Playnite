using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Playnite
{
    public class FilterChangedEventArgs : EventArgs
    {
        public List<string> Fields
        {
            get; set;
        }

        public FilterChangedEventArgs()
        {
        }

        public FilterChangedEventArgs(List<string> fields)
        {
            Fields = fields;
        }
    }

    // TODO store as null if not set
    public class FilterItemProperites
    {
        [JsonIgnore]
        public bool IsSet => !Text.IsNullOrEmpty() || Ids?.Any() == true;
        public List<Guid> Ids { get; set; }
        public string Text { get; set; }
    }

    public class FilterSettings : ObservableObject
    {
        [JsonIgnore]
        public bool SearchActive
        {
            get => !string.IsNullOrEmpty(Name);
        }

        [JsonIgnore]
        public bool Active
        {
            get
            {
                return
                    IsInstalled ||
                    IsUnInstalled ||
                    Hidden ||
                    Favorite ||
                    !string.IsNullOrEmpty(Name) ||
                    !string.IsNullOrEmpty(ReleaseDate) ||
                    Series?.IsSet == true ||
                    Source?.IsSet == true ||
                    AgeRating?.IsSet == true ||
                    Region?.IsSet == true ||
                    Genre?.IsSet == true ||
                    Publisher?.IsSet == true ||
                    Developer?.IsSet == true ||
                    Category?.IsSet == true ||
                    Tag?.IsSet == true ||
                    Platform?.IsSet == true ||
                    Library?.IsSet == true;
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Name));
                OnPropertyChanged(nameof(Active));
                OnPropertyChanged(nameof(SearchActive));
            }
        }

        private FilterItemProperites genres;
        public FilterItemProperites Genre
        {
            get
            {
                return genres;
            }

            set
            {
                genres = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Genre));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites platforms;
        public FilterItemProperites Platform
        {
            get
            {
                return platforms;
            }

            set
            {
                platforms = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Platform));
                OnPropertyChanged(nameof(Active));
            }
        }

        private string releaseDate;
        public string ReleaseDate
        {
            get
            {
                return releaseDate;
            }

            set
            {
                releaseDate = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(ReleaseDate));
                OnPropertyChanged(nameof(Active));
            }
        }


        private FilterItemProperites publishers;
        public FilterItemProperites Publisher
        {
            get
            {
                return publishers;
            }

            set
            {
                publishers = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Publisher));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites developers;
        public FilterItemProperites Developer
        {
            get
            {
                return developers;
            }

            set
            {
                developers = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Developer));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites categories;
        public FilterItemProperites Category
        {
            get
            {
                return categories;
            }

            set
            {
                categories = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Category));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites tags;
        public FilterItemProperites Tag
        {
            get
            {
                return tags;
            }

            set
            {
                tags = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Tag));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites series;
        public FilterItemProperites Series
        {
            get
            {
                return series;
            }

            set
            {
                series = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Series));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites region;
        public FilterItemProperites Region
        {
            get
            {
                return region;
            }

            set
            {
                region = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Region));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites source;
        public FilterItemProperites Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Source));
                OnPropertyChanged(nameof(Active));
            }
        }

        private FilterItemProperites ageRating;
        public FilterItemProperites AgeRating
        {
            get
            {
                return ageRating;
            }

            set
            {
                ageRating = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(AgeRating));
                OnPropertyChanged(nameof(Active));
            }
        }

        private bool isInstalled;
        public bool IsInstalled
        {
            get
            {
                return isInstalled;
            }

            set
            {
                isInstalled = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(IsInstalled));
                OnPropertyChanged(nameof(Active));
            }
        }

        private bool isUnInstalled;
        public bool IsUnInstalled
        {
            get
            {
                return isUnInstalled;
            }

            set
            {
                isUnInstalled = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(IsUnInstalled));
                OnPropertyChanged(nameof(Active));
            }
        }

        private bool hidden;
        public bool Hidden
        {
            get
            {
                return hidden;
            }

            set
            {
                hidden = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Hidden));
                OnPropertyChanged(nameof(Active));
            }
        }

        private bool favorite;
        public bool Favorite
        {
            get
            {
                return favorite;
            }

            set
            {
                favorite = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Favorite));
                OnPropertyChanged(nameof(Active));
            }
        }
        
        private FilterItemProperites library;
        public FilterItemProperites Library
        {
            get
            {
                return library;
            }

            set
            {
                library = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Library));
                OnPropertyChanged(nameof(Active));
            }
        }

        private bool suppressFilterChanges = false;
        public event EventHandler<FilterChangedEventArgs> FilterChanged;

        public void OnFilterChanged(string field)
        {
            if (!suppressFilterChanges)
            {
                FilterChanged?.Invoke(this, new FilterChangedEventArgs(new List<string>() { field }));
            }
        }

        public void OnFilterChanged(List<string> fields)
        {
            if (!suppressFilterChanges)
            {
                FilterChanged?.Invoke(this, new FilterChangedEventArgs(fields));
            }
        }

        public void ClearFilters()
        {
            suppressFilterChanges = true;
            var filterChanges = new List<string>();

            if (Name != null)
            {
                Name = null;
                filterChanges.Add(nameof(Name));
            }

            if (Genre?.IsSet == true)
            {
                Genre = null;
                filterChanges.Add(nameof(Genre));
            }

            if (Platform?.IsSet == true)
            {
                Platform = null;
                filterChanges.Add(nameof(Platform));
            }

            if (ReleaseDate != null)
            {
                ReleaseDate = null;
                filterChanges.Add(nameof(ReleaseDate));
            }

            if (Publisher?.IsSet == true)
            {
                Publisher = null;
                filterChanges.Add(nameof(Publisher));
            }

            if (Developer?.IsSet == true)
            {
                Developer = null;
                filterChanges.Add(nameof(Developer));
            }

            if (Category?.IsSet == true)
            {
                Category = null;
                filterChanges.Add(nameof(Category));
            }

            if (Tag?.IsSet == true)
            {
                Tag = null;
                filterChanges.Add(nameof(Tag));
            }

            if (IsInstalled != false)
            {
                IsInstalled = false;
                filterChanges.Add(nameof(IsInstalled));
            }

            if (IsUnInstalled != false)
            {
                IsUnInstalled = false;
                filterChanges.Add(nameof(IsUnInstalled));
            }

            if (Hidden != false)
            {
                Hidden = false;
                filterChanges.Add(nameof(Hidden));
            }

            if (Favorite != false)
            {
                Favorite = false;
                filterChanges.Add(nameof(Favorite));
            }

            if (Series?.IsSet == true)
            {
                Series = null;
                filterChanges.Add(nameof(Series));
            }

            if (Region?.IsSet == true)
            {
                Region = null;
                filterChanges.Add(nameof(Region));
            }

            if (Source?.IsSet == true)
            {
                Source = null;
                filterChanges.Add(nameof(Source));
            }

            if (AgeRating?.IsSet == true)
            {
                AgeRating = null;
                filterChanges.Add(nameof(AgeRating));
            }

            if (Library != null)
            {
                Library = null;
                filterChanges.Add(nameof(Library));
            }

            suppressFilterChanges = false;
            OnFilterChanged(filterChanges);
        }
    }
}
