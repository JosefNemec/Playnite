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

    public class FilterSettings : INotifyPropertyChanged
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
                OnPropertyChanged("Name");
                OnFilterChanged("Name");
                OnPropertyChanged("Active");
                OnPropertyChanged("SearchActive");
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
                OnPropertyChanged("Genres");
                OnFilterChanged("Genres");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Platforms");
                OnFilterChanged("Platforms");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("ReleaseDate");
                OnFilterChanged("ReleaseDate");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Publishers");
                OnFilterChanged("Publishers");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Developers");
                OnFilterChanged("Developers");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Categories");
                OnFilterChanged("Categories");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Tags");
                OnFilterChanged("Tags");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("IsInstalled");
                OnFilterChanged("IsInstalled");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("IsUnInstalled");
                OnFilterChanged("IsUnInstalled");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Hidden");
                OnFilterChanged("Hidden");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Favorite");
                OnFilterChanged("Favorite");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Series");
                OnFilterChanged("Series");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Region");
                OnFilterChanged("Region");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Source");
                OnFilterChanged("Source");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("AgeRating");
                OnFilterChanged("AgeRating");
                OnPropertyChanged("Active");
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
                OnPropertyChanged("Libraries");
                OnFilterChanged("Libraries");
                OnPropertyChanged("Active");
            }
        }

        private bool suppressFilterChanges = false;
        public event EventHandler<FilterChangedEventArgs> FilterChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
                filterChanges.Add("Name");
            }

            if (Genres != null)
            {
                Genres = null;
                filterChanges.Add("Genres");
            }

            if (Platforms != null)
            {
                Platforms = null;
                filterChanges.Add("Platforms");
            }

            if (ReleaseDate != null)
            {
                ReleaseDate = null;
                filterChanges.Add("ReleaseDate");
            }

            if (Publishers != null)
            {
                Publishers = null;
                filterChanges.Add("Publishers");
            }

            if (Developers != null)
            {
                Developers = null;
                filterChanges.Add("Developers");
            }

            if (Categories != null)
            {
                Categories = null;
                filterChanges.Add("Categories");
            }

            if (Tags != null)
            {
                Tags = null;
                filterChanges.Add("Tags");
            }

            if (IsInstalled != false)
            {
                IsInstalled = false;
                filterChanges.Add("IsInstalled");
            }

            if (IsUnInstalled != false)
            {
                IsUnInstalled = false;
                filterChanges.Add("IsUnInstalled");
            }

            if (Hidden != false)
            {
                Hidden = false;
                filterChanges.Add("Hidden");
            }

            if (Favorite != false)
            {
                Favorite = false;
                filterChanges.Add("Favorite");
            }

            if (Series != null)
            {
                Series = null;
                filterChanges.Add("Series");
            }

            if (Region != null)
            {
                Region = null;
                filterChanges.Add("Region");
            }

            if (Source != null)
            {
                Source = null;
                filterChanges.Add("Source");
            }

            if (AgeRating != null)
            {
                AgeRating = null;
                filterChanges.Add("AgeRating");
            }

            if (Libraries != null)
            {
                Libraries = null;
                filterChanges.Add("Libraries");
            }

            suppressFilterChanges = false;
            OnFilterChanged(filterChanges);
        }
    }
}
