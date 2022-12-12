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
    public class HelpMenuViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        public FullscreenAppViewModel MainModel { get; }

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand SendFeedbackCommand => new RelayCommand(() => SendFeedback());
        public RelayCommand RestartAppCommand => new RelayCommand(() => RestartApp());
        public RelayCommand RestartInSafeModeCommand => new RelayCommand(() => RestartInSafeMode());

        public HelpMenuViewModel(
            IWindowFactory window,
            FullscreenAppViewModel mainModel)
        {
            this.window = window;
            this.MainModel = mainModel;
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void Close()
        {
            window.Close(true);
        }

        public void SendFeedback()
        {
            Close();
            NavigateUrlCommand.Navigate(PlayniteEnvironment.ReleaseChannel == ReleaseChannel.Beta ? UrlConstants.IssuesTesting : UrlConstants.Issues);
        }

        public void RestartApp()
        {
            Close();
            MainModel.RestartAppSkipLibUpdate();
        }

        public void RestartInSafeMode()
        {
            Close();
            MainModel.RestartAppSafe();
        }
    }
}
