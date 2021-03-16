using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public class EmulationImportViewModel : ObservableObject
    {
        private string dirToScan;
        public string DirToScan
        {
            get => dirToScan;
            set
            {
                dirToScan = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> ScanDirectoryCommand
        {
            get => new RelayCommand<object>((dir) =>
            {
                ScanDirectory(DirToScan);
            }, (a) => !DirToScan.IsNullOrEmpty());
        }

        public void ScanDirectory(string dir)
        {
        }
    }
}
