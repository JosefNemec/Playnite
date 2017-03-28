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
                return Installed || Hidden || Providers.Any(a => a.Value == true);
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
