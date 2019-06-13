using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Playnite.Installer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private Window windowHost;

        private string destinationFolder;
        public string DestionationFolder
        {
            get => destinationFolder;
            set
            {
                destinationFolder = value;
                OnPropertyChanged();
            }
        }

        private bool portable;
        public bool Portable
        {
            get => portable;
            set
            {
                portable = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> BrowseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Browse();
            });
        }

        public RelayCommand<object> InstallCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Install();
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Cancel();
            });
        }

        public MainViewModel(Window window)
        {
            DestionationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Playnite");
            windowHost = window;
        }

        public void Browse()
        {
            var dialog = new FolderBrowserDialog()
            {                
                Description = "Select Destination Folder...",
                ShowNewFolderButton = true
            };            
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DestionationFolder = dialog.SelectedPath;
            }
        }

        public void Install()
        {


            windowHost.Close();
        }

        public void Cancel()
        {
            windowHost.Close();
        }
    }
}
