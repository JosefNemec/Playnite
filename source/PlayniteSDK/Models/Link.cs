using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class Link : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        private string url;
        public string Url
        {
            get => url;
            set
            {
                url = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Url"));
            }
        }

        public Link()
        {
        }

        public Link(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
