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
        public FullscreenAppViewModel MainModel { get; }

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand ExitCommand => new RelayCommand(() => Shutdown());
        public RelayCommand SwitchToDesktopCommand => new RelayCommand(() => SwitchToDesktopMode());
        public RelayCommand OpenSettingsCommand => new RelayCommand(() => OpenSettings());
        public RelayCommand SelectRandomGameCommand => new RelayCommand(() => PlayRandomGame(), () => MainModel.Database?.IsOpen == true);
        public RelayCommand OpenPatreonCommand => new RelayCommand(() => OpenPatreon());
        public RelayCommand OpenKofiCommand => new RelayCommand(() => OpenKofi());
        public RelayCommand ShutdownSystemCommand => new RelayCommand(() => ShutdownSystem());
        public RelayCommand HibernateSystemCommand => new RelayCommand(() => HibernateSystem());
        public RelayCommand SleepSystemCommand => new RelayCommand(() => SleepSystem());
        public RelayCommand RestartSystemCommand => new RelayCommand(() => RestartSystem());
        public RelayCommand LockSystemCommand => new RelayCommand(() => LockSystem());
        public RelayCommand LogoutUserCommand => new RelayCommand(() => LogoutUser());
        public RelayCommand UpdateGamesCommand => new RelayCommand(async () =>
        {
            Close();
            await MainModel.UpdateLibrary(MainModel.AppSettings.DownloadMetadataOnImport, true, true);
        }, () => !MainModel.ProgressActive);
        public RelayCommand CancelProgressCommand => new RelayCommand(() => CancelProgress(), () => GlobalTaskHandler.CancelToken?.IsCancellationRequested == false);
        public RelayCommand OpenHelpCommand => new RelayCommand(() => OpenHelp());
        public RelayCommand MinimizeCommand => new RelayCommand(() =>
        {
            Close();
            MainModel.MinimizeWindow();
        });

        public MainMenuViewModel(
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

        public void Shutdown()
        {
            Close();
            MainModel.CloseView();
            MainModel.App.Quit();
        }

        public void SwitchToDesktopMode()
        {
            Close();
            MainModel.SwitchToDesktopMode();
        }

        public void CancelProgress()
        {
            Close();
            MainModel.CancelProgress();
        }

        public void PlayRandomGame()
        {
            Close();
            var model = new RandomGameSelectViewModel(
                MainModel.Database,
                MainModel.GamesView,
                new RandomGameSelectWindowFactory(),
                MainModel.Resources);
            model.OpenView();
            if (model.SelectedAction == RandomGameSelectAction.Play)
            {
                MainModel.SelectGame(model.SelectedGame.Id);
                MainModel.GamesEditor.PlayGame(model.SelectedGame, true);
            }
            else if (model.SelectedAction == RandomGameSelectAction.Navigate)
            {
                MainModel.ToggleGameDetailsCommand.Execute(null);
                MainModel.SelectGame(model.SelectedGame.Id);
            }
        }

        public void OpenSettings()
        {
            Close();
            var vm = new SettingsViewModel(new SettingsWindowFactory(), MainModel);
            vm.OpenView();
        }

        public void OpenHelp()
        {
            Close();
            var vm = new HelpMenuViewModel(new HelpMenuWindowFactory(), MainModel);
            vm.OpenView();
        }

        public void OpenPatreon()
        {
            Close();
            NavigateUrlCommand.Navigate(UrlConstants.Patreon);
        }

        public void OpenKofi()
        {
            Close();
            NavigateUrlCommand.Navigate(UrlConstants.Kofi);
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
                MainModel.App.QuitAndStart(Computer.ShutdownCmd.path, Computer.ShutdownCmd.args);
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
                try
                {
                    Computer.Hibernate();
                }
                catch (Exception e)
                {
                    Dialogs.ShowErrorMessage(e.Message, "");
                }
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
                try
                {
                    Computer.Sleep();
                }
                catch (Exception e)
                {
                    Dialogs.ShowErrorMessage(e.Message, "");
                }
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
                MainModel.App.QuitAndStart(Computer.RestartCmd.path, Computer.RestartCmd.args);
            }
        }

        public void LockSystem()
        {
            Close();
            if (Dialogs.ShowMessage(LOC.ConfirumationAskGeneric, LOC.MenuLockSystem, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!PlayniteEnvironment.IsDebuggerAttached)
            {
                try
                {
                    Computer.Lock();
                }
                catch (Exception e)
                {
                    Dialogs.ShowErrorMessage(e.Message, "");
                }
            }
        }

        public void LogoutUser()
        {
            Close();
            if (Dialogs.ShowMessage(LOC.ConfirumationAskGeneric, LOC.MenuLogoutUser, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!PlayniteEnvironment.IsDebuggerAttached)
            {
                try
                {
                    Computer.Logout();
                    Shutdown();
                }
                catch (Exception e)
                {
                    Dialogs.ShowErrorMessage(e.Message, "");
                }
            }
        }

        public async void CancelLibraryUpdate()
        {
            await GlobalTaskHandler.CancelAndWaitAsync();
        }
    }
}
