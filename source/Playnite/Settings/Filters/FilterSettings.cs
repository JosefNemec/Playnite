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
                    (IsInstalled != ThreeStateFilterEnum.Disable) ||
                    (IsUnInstalled != ThreeStateFilterEnum.Disable) ||
                    (Hidden != ThreeStateFilterEnum.Disable) ||
                    (Favorite != ThreeStateFilterEnum.Disable) ||
                    !string.IsNullOrEmpty(Name) ||
                    !string.IsNullOrEmpty(ReleaseDate) ||
                    !string.IsNullOrEmpty(Series) ||
                    !string.IsNullOrEmpty(Source) ||
                    !string.IsNullOrEmpty(AgeRating) ||
                    !string.IsNullOrEmpty(Region) ||
                    Genres?.Any() == true ||
                    Publishers?.Any() == true ||
                    Developers?.Any() == true ||
                    Categories?.Any() == true ||
                    Tags?.Any() == true ||
                    Platforms?.Any() == true ||
                    Libraries?.Any() == true;
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

        private List<string> genres;
        public List<string> Genres
        {
            get
            {
                return genres;
            }

            set
            {
                genres = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Genres));
                OnPropertyChanged(nameof(Active));
            }
        }

        private List<string> platforms;
        public List<string> Platforms
        {
            get
            {
                return platforms;
            }

            set
            {
                platforms = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Platforms));
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


        private List<string> publishers;
        public List<string> Publishers
        {
            get
            {
                return publishers;
            }

            set
            {
                publishers = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Publishers));
                OnPropertyChanged(nameof(Active));
            }
        }

        private List<string> developers;
        public List<string> Developers
        {
            get
            {
                return developers;
            }

            set
            {
                developers = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Developers));
                OnPropertyChanged(nameof(Active));
            }
        }

        private List<string> categories;
        public List<string> Categories
        {
            get
            {
                return categories;
            }

            set
            {
                categories = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Categories));
                OnPropertyChanged(nameof(Active));
            }
        }

        private List<string> tags;
        public List<string> Tags
        {
            get
            {
                return tags;
            }

            set
            {
                tags = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Tags));
                OnPropertyChanged(nameof(Active));
            }
        }

        private ThreeStateFilterEnum isInstalled;

        public ThreeStateFilterEnum IsInstalled
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

        private ThreeStateFilterEnum isUnInstalled;

        public ThreeStateFilterEnum IsUnInstalled
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

        private ThreeStateFilterEnum hidden;
        public ThreeStateFilterEnum Hidden
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

        private ThreeStateFilterEnum favorite;
        public ThreeStateFilterEnum Favorite
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

        private string series;
        public string Series
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

        private string region;
        public string Region
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

        private string source;
        public string Source
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

        private string ageRating;
        public string AgeRating
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

        private List<Guid> libraries;
        public List<Guid> Libraries
        {
            get
            {
                return libraries;
            }

            set
            {
                libraries = value;
                OnPropertyChanged();
                OnFilterChanged(nameof(Libraries));
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

            if (Genres != null)
            {
                Genres = null;
                filterChanges.Add(nameof(Genres));
            }

            if (Platforms != null)
            {
                Platforms = null;
                filterChanges.Add(nameof(Platforms));
            }

            if (ReleaseDate != null)
            {
                ReleaseDate = null;
                filterChanges.Add(nameof(ReleaseDate));
            }

            if (Publishers != null)
            {
                Publishers = null;
                filterChanges.Add(nameof(Publishers));
            }

            if (Developers != null)
            {
                Developers = null;
                filterChanges.Add(nameof(Developers));
            }

            if (Categories != null)
            {
                Categories = null;
                filterChanges.Add(nameof(Categories));
            }

            if (Tags != null)
            {
                Tags = null;
                filterChanges.Add(nameof(Tags));
            }

            if (IsInstalled != ThreeStateFilterEnum.Disable)
            {
                IsInstalled = ThreeStateFilterEnum.Disable;
                filterChanges.Add(nameof(IsInstalled));
            }

            if (IsUnInstalled != ThreeStateFilterEnum.Disable)
            {
                IsUnInstalled = ThreeStateFilterEnum.Disable;
                filterChanges.Add(nameof(IsUnInstalled));
            }

            if (Hidden != ThreeStateFilterEnum.Disable)
            {
                Hidden = ThreeStateFilterEnum.Disable;
                filterChanges.Add(nameof(Hidden));
            }

            if (Favorite != ThreeStateFilterEnum.Disable)
            {
                Favorite = ThreeStateFilterEnum.Disable;
                filterChanges.Add(nameof(Favorite));
            }

            if (Series != null)
            {
                Series = null;
                filterChanges.Add(nameof(Series));
            }

            if (Region != null)
            {
                Region = null;
                filterChanges.Add(nameof(Region));
            }

            if (Source != null)
            {
                Source = null;
                filterChanges.Add(nameof(Source));
            }

            if (AgeRating != null)
            {
                AgeRating = null;
                filterChanges.Add(nameof(AgeRating));
            }

            if (Libraries != null)
            {
                Libraries = null;
                filterChanges.Add(nameof(Libraries));
            }

            suppressFilterChanges = false;
            OnFilterChanged(filterChanges);
        }
    }
}
