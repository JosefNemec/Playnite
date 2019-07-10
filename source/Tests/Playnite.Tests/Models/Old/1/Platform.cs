using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Models.Old1
{
    public class Platform : INotifyPropertyChanged
    {
        private int id;
        [BsonId]
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private string icon;
        public string Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }

        private string cover;
        public string Cover
        {
            get => cover;
            set
            {
                cover = value;
                OnPropertyChanged("Cover");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Platform()
        {
        }

        public Platform(string name)
        {
            Name = name;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
