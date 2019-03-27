using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.ViewModels
{
    public class MainViewModelBase : ObservableObject
    {
        public string ProgressStatus { get; set; }
        public double ProgressValue { get; set; }
        public double ProgressTotal { get; set; }
        public bool ProgressVisible { get; set; }
        public GamesCollectionView GamesView { get; set; }
        public GamesCollectionViewEntry SelectedGame { get; set; }
        public IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }
    }
}
