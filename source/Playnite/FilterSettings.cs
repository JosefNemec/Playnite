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
                    Installed ||
                    Hidden ||
                    Favorite ||
                    Providers.Any(a => a.Value == true) ||
                    !string.IsNullOrEmpty(Name) ||
                    !string.IsNullOrEmpty(ReleaseDate) ||
                    (Genre != null && Genre.Count > 0) ||
                    (Publisher != null && Publisher.Count > 0) ||
                    (Developer != null && Developer.Count > 0) ||
                    (Category != null && Category.Count > 0);
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

        private List<string> genre;
        public List<string> Genre
        {
            get
            {
                return genre;
            }

            set
            {
                genre = value;
                OnPropertyChanged("Genre");
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


        private List<string> publisher;
        public List<string> Publisher
        {
            get
            {
                return publisher;
            }

            set
            {
                publisher = value;
                OnPropertyChanged("Publisher");
                OnPropertyChanged("Active");
            }
        }

        private List<string> developer;
        public List<string> Developer
        {
            get
            {
                return developer;
            }

            set
            {
                developer = value;
                OnPropertyChanged("Developer");
                OnPropertyChanged("Active");
            }
        }

        private List<string> category;
        public List<string> Category
        {
            get
            {
                return category;
            }

            set
            {
                category = value;
                OnPropertyChanged("Category");
                OnPropertyChanged("Active");
            }
        }

        private bool installed;
        public bool Installed
        {
            get
            {
                return installed;
            }

            set
            {
                installed = value;
                OnPropertyChanged("Installed");
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

        private ObservableConcurrentDictionary<Provider, bool> providers = new ObservableConcurrentDictionary<Provider, bool>()
        {
            { Provider.Custom, false },
            { Provider.GOG, false },
            { Provider.Steam, false },
            { Provider.Origin, false }
        };

        public ObservableConcurrentDictionary<Provider, bool> Providers
        {
            get
            {
                return providers;
            }

            set
            {
                if (providers != null)
                {
                    providers.PropertyChanged -= Providers_PropertyChanged;
                }

                providers = value;
                providers.PropertyChanged += Providers_PropertyChanged;
                OnPropertyChanged("Providers");
                OnPropertyChanged("Active");
            }
        }

        private void Providers_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Values")
            {
                OnPropertyChanged("Providers");
                OnPropertyChanged("Active");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FilterSettings()
        {
            Providers.PropertyChanged += Providers_PropertyChanged;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
