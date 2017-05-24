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
                    Hidden ||
                    Favorite ||
                    Provider.Any(a => a.Value == true) ||
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

        private ObservableConcurrentDictionary<Provider, bool> provider = new ObservableConcurrentDictionary<Provider, bool>()
        {
            { Models.Provider.Custom, false },
            { Models.Provider.GOG, false },
            { Models.Provider.Steam, false },
            { Models.Provider.Origin, false }
        };

        public ObservableConcurrentDictionary<Provider, bool> Provider
        {
            get
            {
                return provider;
            }

            set
            {
                if (provider != null)
                {
                    provider.PropertyChanged -= Providers_PropertyChanged;
                }

                provider = value;
                provider.PropertyChanged += Providers_PropertyChanged;
                OnPropertyChanged("Provider");
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

        public FilterSettings()
        {
            Provider.PropertyChanged += Providers_PropertyChanged;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
