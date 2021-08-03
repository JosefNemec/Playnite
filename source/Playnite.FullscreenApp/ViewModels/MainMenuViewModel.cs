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
    public class MainMenuViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly FullscreenAppViewModel mainModel;

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand ExitCommand => new RelayCommand(() => Shutdown());
        public RelayCommand SwitchToDesktopCommand => new RelayCommand(() => SwitchToDesktopMode());
        public RelayCommand OpenSettingsCommand => new RelayCommand(() => OpenSettings());
        public RelayCommand SelectRandomGameCommand => new RelayCommand(() => PlayRandomGame(), () => mainModel.Database?.IsOpen == true);
        public RelayCommand OpenPatreonCommand => new RelayCommand(() => OpenPatreon());
        public RelayCommand SendFeedbackCommand => new RelayCommand(() => SendFeedback());
        public RelayCommand ShutdownSystemCommand => new RelayCommand(() => ShutdownSystem());
        public RelayCommand HibernateSystemCommand => new RelayCommand(() => HibernateSystem());
        public RelayCommand SleepSystemCommand => new RelayCommand(() => SleepSystem());
        public RelayCommand RestartSystemCommand => new RelayCommand(() => RestartSystem());
        public RelayCommand UpdateGamesCommand => new RelayCommand(async () =>
        {
            Close();
            await mainModel.UpdateDatabase(mainModel.AppSettings.DownloadMetadataOnImport);
        }, () => !mainModel.DatabaseUpdateRunning);

        public MainMenuViewModel(
            IWindowFactory window,
            FullscreenAppViewModel mainModel)
        {
            this.window = window;
            this.mainModel = mainModel;
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void Close()
        {
            window.Close(true);
        }

        public void Shutdown()
        {
            Close();
            mainModel.CloseView();
            mainModel.App.Quit();
        }

        public void SwitchToDesktopMode()
        {
            Close();
            mainModel.SwitchToDesktopMode();
        }

        public void PlayRandomGame()
        {
            Close();
            var model = new RandomGameSelectViewModel(
                mainModel.Database,
                mainModel.GamesView,
                new RandomGameSelectWindowFactory(),
                mainModel.Resources);
            if (model.OpenView() == true && model.SelectedGame != null)
            {
                var selection = mainModel.GamesView.Items.FirstOrDefault(a => a.Id == model.SelectedGame.Id);
                if (selection != null)
                {
                    mainModel.GamesEditor.PlayGame(selection.Game);
                }
            }
        }

        public void OpenSettings()
        {
            Close();
            var vm = new SettingsViewModel(new SettingsWindowFactory(), mainModel);
            vm.OpenView();
        }

        public void SendFeedback()
        {
            Close();
            NavigateUrlCommand.Navigate(PlayniteEnvironment.ReleaseChannel == ReleaseChannel.Beta ? UrlConstants.IssuesTesting : UrlConstants.Issues);
        }
        public void OpenPatreon()
        {
            Close();
            NavigateUrlCommand.Navigate(UrlConstants.Patreon);
        }

        public void ShutdownSystem()
        {
            Close();
            if (Dialogs.ShowMessage(LOC.ConfirumationAskGeneric, LOC.MenuShutdownSystem, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!PlayniteEnvironment.IsDebuggerAttached)
            {
                Computer.Shutdown();
            }
        }

        public void HibernateSystem()
        {
            Close();
            if (Dialogs.ShowMessage(LOC.ConfirumationAskGeneric, LOC.MenuHibernateSystem, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!PlayniteEnvironment.IsDebuggerAttached)
            {
                Computer.Hibernate();
            }
        }

        public void SleepSystem()
        {
            Close();
            if (Dialogs.ShowMessage(LOC.ConfirumationAskGeneric, LOC.MenuSuspendSystem, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!PlayniteEnvironment.IsDebuggerAttached)
            {
                Computer.Sleep();
            }
        }

        public void RestartSystem()
        {
            Close();
            if (Dialogs.ShowMessage(LOC.ConfirumationAskGeneric, LOC.MenuRestartSystem, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!PlayniteEnvironment.IsDebuggerAttached)
            {
                Computer.Restart();
            }
        }
    }
}
