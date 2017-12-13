using NLog;
using Playnite;
using Playnite.Database;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayniteUI.ViewModels
{
    public class FullscreenViewModel : MainViewModel
    {
        public bool IsFullScreen
        {
            get;
            private set;
        } = false;

        private double viewLeft = 0;
        public double ViewLeft
        {
            get => viewLeft;
            set
            {
                viewLeft = value;
                OnPropertyChanged("ViewLeft");
            }
        }

        private double viewTop = 0;
        public double ViewTop
        {
            get => viewTop;
            set
            {
                viewTop = value;
                OnPropertyChanged("ViewTop");
            }
        }

        private double viewWidth = 1280;
        public double ViewWidth
        {
            get => viewWidth;
            set
            {
                viewWidth = value;
                OnPropertyChanged("ViewWidth");
            }
        }

        private double viewHeight = 720;
        public double ViewHeight
        {
            get => viewHeight;
            set
            {
                viewHeight = value;
                OnPropertyChanged("ViewHeight");
            }
        }

        private bool showFilter = false;
        public bool ShowFilter
        {
            get => showFilter;
            set
            {
                showFilter = value;
                OnPropertyChanged("ShowFilter");
            }
        }

        public RelayCommand<object> BackCommand
        {
            get => new RelayCommand<object>((a) =>
            {                
                ToggleFilter();
            });
        }

        public RelayCommand<object> ToggleFilterCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleFilter();
            });
        }

        public RelayCommand<object> ToggleFullscreenCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleFullscreen();
            });
        }

        public FullscreenViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            Settings settings,
            GamesEditor gamesEditor) : base(database, window, dialogs, resources, settings, gamesEditor)
        {
        }

        public void OpenView(bool fullscreen)
        {
            Window.Show(this);
            Window.BringToForeground();
            if (fullscreen)
            {
                GoFullscreen();
            }
            else
            {
                LeaveFullscreen();
            }
        }

        public void ToggleFullscreen()
        {
            if (IsFullScreen)
            {
                LeaveFullscreen();
            }
            else
            {
                GoFullscreen();
            }
        }

        public void GoFullscreen()
        {
            IsFullScreen = true;
            ViewLeft = 0;
            ViewTop = 0;
            ViewWidth = Screen.PrimaryScreen.Bounds.Width;
            ViewHeight = Screen.PrimaryScreen.Bounds.Height;
        }

        public void LeaveFullscreen()
        {
            IsFullScreen = false;
            ViewWidth = Screen.PrimaryScreen.Bounds.Width / 1.5;
            ViewHeight = Screen.PrimaryScreen.Bounds.Height / 1.5;
            ViewLeft = (Screen.PrimaryScreen.Bounds.Width - ViewWidth) / 2;
            ViewTop = (Screen.PrimaryScreen.Bounds.Height - ViewHeight) / 2;
        }

        public void ToggleFilter()
        {
            ShowFilter = !ShowFilter;
        }
    }
}
