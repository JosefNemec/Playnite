using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.ViewModels
{
    public interface IMainViewModelBase
    {
        string ProgressStatus { get; set; }
        double ProgressValue { get; set; }
        double ProgressTotal { get; set; }
        bool ProgressVisible { get; set; }
        BaseCollectionView GamesView { get; set; }
        GamesCollectionViewEntry SelectedGame { get; set; }
        IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }
    }

    public class MainViewModelBase : ObservableObject, IMainViewModelBase
    {
        private string progressStatus;
        public string ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                OnPropertyChanged();
            }
        }

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        private double progressTotal;
        public double ProgressTotal
        {
            get => progressTotal;
            set
            {
                progressTotal = value;
                OnPropertyChanged();
            }
        }

        private bool progressVisible;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsDisposing { get; set; } = false;
        public BaseCollectionView GamesView { get; set; }
        public GamesCollectionViewEntry SelectedGame { get; set; }
        public IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }
    }
}
