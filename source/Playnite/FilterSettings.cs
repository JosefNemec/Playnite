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
                    Custom ||
                    !string.IsNullOrEmpty(Name) ||
                    !string.IsNullOrEmpty(ReleaseDate) ||
                    (Genres != null && Genres.Count > 0) ||
                    (Publishers != null && Publishers.Count > 0) ||
                    (Developers != null && Developers.Count > 0) ||
                    (Categories != null && Categories.Count > 0);
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
                OnPropertyChanged("Active");
            }
        }

        private void Providers_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Values")
            {
                OnPropertyChanged("Provider");
                OnPropertyChanged("Active");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
