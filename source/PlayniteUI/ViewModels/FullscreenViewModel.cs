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
    public class FullscreenViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IResourceProvider resources;
        private Settings settings;
        private GameDatabase database;

        public bool IsFullScreen
        {
            get;
            private set;
        } = false;

        private double viewLeft;
        public double ViewLeft
        {
            get => viewLeft;
            set
            {
                viewLeft = value;
                OnPropertyChanged("ViewLeft");
            }
        }

        private double viewTop;
        public double ViewTop
        {
            get => viewTop;
            set
            {
                viewTop = value;
                OnPropertyChanged("ViewTop");
            }
        }

        private double viewWidth;
        public double ViewWidth
        {
            get => viewWidth;
            set
            {
                viewWidth = value;
                OnPropertyChanged("ViewWidth");
            }
        }

        private double viewHeight;
        public double ViewHeight
        {
            get => viewHeight;
            set
            {
                viewHeight = value;
                OnPropertyChanged("ViewHeight");
            }
        }

        public RelayCommand<object> ToggleFullscreenCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleFullscreen();
            });
        }

        public FullscreenViewModel(GameDatabase database, Settings settings, IWindowFactory window, IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.resources = resources;
            this.settings = settings;
        }

        public void OpenView(bool fullscreen)
        {
            window.Show(this);
            window.BringToForeground();
            if (fullscreen)
            {
                GoFullscreen();
            }
            else
            {
                LeaveFullscreen();
            }
        }

        public void CloseView()
        {
            window.Close();
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
            ViewWidth = Screen.PrimaryScreen.Bounds.Width / 2;
            ViewHeight = Screen.PrimaryScreen.Bounds.Height / 2;
            ViewLeft = ViewWidth - (ViewWidth / 2);
            ViewTop = ViewHeight - (ViewHeight / 2);
        }
    }
}
