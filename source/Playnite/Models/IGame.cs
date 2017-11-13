using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Models
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

    public interface IGame : INotifyPropertyChanged
    {
        int Id
        {
            get; set;
        }

        string ProviderId
        {
            get; set;
        }

        DateTime? LastActivity
        {
            get;set;
        }

        Provider Provider
        {
            get; set;
        }

        ObjectId PlatformId
        {
            get; set;
        }

        string Name
        {
            get; set;
        }

        string SortingName
        {
            get; set;
        }

        string Image
        {
            get; set;
        }

        string Icon
        {
            get; set;
        }

        string BackgroundImage
        {
            get; set;
        }

        ObservableCollection<Link> Links
        {
            get; set;
        }

        string InstallDirectory
        {
            get; set;
        }

        string IsoPath
        {
            get; set;
        }

        string Description
        {
            get; set;
        }

        string DescriptionView
        {
            get;
        }

        bool IsProviderDataUpdated
        {
            get; set;
        }

        GameTask PlayTask
        {
            get; set;
        }

        ObservableCollection<GameTask> OtherTasks
        {
            get; set;
        }

        ComparableList<string> Tags
        {
            get; set;
        }

        ComparableList<string> Categories
        {
            get; set;
        }

        ComparableList<string> Genres
        {
            get; set;
        }

        DateTime? ReleaseDate
        {
            get; set;
        }

        ComparableList<string> Developers
        {
            get; set;
        }

        ComparableList<string> Publishers
        {
            get; set;
        }

        bool IsInstalled
        {
            get;
        }

        bool Hidden
        {
            get; set;
        }

        bool Favorite
        {
            get; set;
        }

        void PlayGame();

        void PlayGame(List<Emulator> emulators);

        void InstallGame();

        void UninstallGame();

        void OnPropertyChanged(string name);
    }
}
