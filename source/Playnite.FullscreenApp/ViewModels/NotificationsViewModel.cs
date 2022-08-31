using Playnite.API;
using Playnite.Common;
using Playnite.FullscreenApp.Windows;
using Playnite.SDK;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp.ViewModels
{
    public class NotificationsViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        public FullscreenAppViewModel MainModel { get; set; }

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand ClearNotificationsCommand => new RelayCommand(() =>
        {
            MainModel.App.Notifications.RemoveAll();
            Close();
        });

        public NotificationsViewModel(
            IWindowFactory window,
            FullscreenAppViewModel mainModel)
        {
            this.window = window;
            MainModel = mainModel;
        }

        public bool? OpenView()
        {
            MainModel.App.Notifications.ActivationRequested += FullscreenAppViewModel_ActivationRequested;
            return window.CreateAndOpenDialog(this);
        }

        public void Close()
        {
            MainModel.App.Notifications.ActivationRequested -= FullscreenAppViewModel_ActivationRequested;
            window.Close(true);
        }

        private void FullscreenAppViewModel_ActivationRequested(object sender, NotificationsAPI.MessageEventArgs e)
        {
            MainModel.App.Notifications.Remove(e.Message.Id);
            Close();
            e.Message.ActivationAction();
        }
    }
}
