using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.Models;

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
        public bool Active
        {
            get
            {
                return
                    IsInstalled ||
                    IsUnInstalled ||
                    Hidden ||
                    Favorite ||
                    Steam ||
                    Origin ||
                    GOG ||
                    Uplay ||
                    BattleNet ||
                    Custom ||
                    !string.IsNullOrEmpty(Name) ||
                    !string.IsNullOrEmpty(ReleaseDate) ||
                    (Genres != null && Genres.Count > 0) ||
                    (Publishers != null && Publishers.Count > 0) ||
                    (Developers != null && Developers.Count > 0) ||
                    (Categories != null && Categories.Count > 0) ||
                    (Tags != null && Tags.Count > 0) ||
                    (Platforms != null && Platforms.Count > 0);
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

        private bool steam;
        public bool Steam
        {
            get
            {
                return steam;
            }

            set
            {
                steam = value;
                OnPropertyChanged("Steam");
                OnFilterChanged("Steam");
                OnPropertyChanged("Active");
            }
        }

        private bool origin;
        public bool Origin
        {
            get
            {
                return origin;
            }

            set
            {
                origin = value;
                OnPropertyChanged("Origin");
                OnFilterChanged("Origin");
                OnPropertyChanged("Active");
            }
        }

        private bool gog;
        public bool GOG
        {
            get
            {
                return gog;
            }

            set
            {
                gog = value;
                OnPropertyChanged("GOG");
                OnFilterChanged("GOG");
                OnPropertyChanged("Active");
            }
        }

        private bool uplay;
        public bool Uplay
        {
            get
            {
                return uplay;
            }

            set
            {
                uplay = value;
                OnPropertyChanged("Uplay");
                OnFilterChanged("Uplay");
                OnPropertyChanged("Active");
            }
        }

        private bool battleNet;
        public bool BattleNet
        {
            get
            {
                return battleNet;
            }

            set
            {
                battleNet = value;
                OnPropertyChanged("BattleNet");
                OnFilterChanged("BattleNet");
                OnPropertyChanged("Active");
            }
        }

        private bool custom;
        public bool Custom
        {
            get
            {
                return custom;
            }

            set
            {
                custom = value;
                OnPropertyChanged("Custom");
                OnFilterChanged("Custom");
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

            if (Steam != false)
            {
                Steam = false;
                filterChanges.Add("Steam");
            }

            if (Origin != false)
            {
                Origin = false;
                filterChanges.Add("Origin");
            }

            if (GOG != false)
            {
                GOG = false;
                filterChanges.Add("GOG");
            }

            if (Uplay != false)
            {
                Uplay = false;
                filterChanges.Add("Uplay");
            }

            if (BattleNet != false)
            {
                BattleNet = false;
                filterChanges.Add("BattleNet");
            }

            if (Custom != false)
            {
                Custom = false;
                filterChanges.Add("Custom");
            }

            suppressFilterChanges = false;
            OnFilterChanged(filterChanges);
        }
    }
}
