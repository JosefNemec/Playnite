using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Models
{
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

        string Name
        {
            get; set;
        }

        string DefaultImage
        {
            get;
        }

        string Image
        {
            get; set;
        }

        string DefaultIcon
        {
            get;
        }

        string Icon
        {
            get; set;
        }

        string DefaultBackgroundImage
        {
            get;
        }

        string BackgroundImage
        {
            get; set;
        }

        string CommunityHubUrl
        {
            get; set;
        }

        string StoreUrl
        {
            get; set;
        }

        string WikiUrl
        {
            get; set;
        }

        string InstallDirectory
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

        List<string> Categories
        {
            get; set;
        }

        List<string> Genres
        {
            get; set;
        }

        DateTime? ReleaseDate
        {
            get; set;
        }

        List<string> Developers
        {
            get; set;
        }

        List<string> Publishers
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

        void PlayGame();

        void InstallGame();

        void UninstallGame();

        void OnPropertyChanged(string name);
    }
}
