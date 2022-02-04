using Playnite.Commands;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp.ViewModels
{
    public class GameDetailsViewModel : ObservableObject, IDisposable
    {
        private readonly IResourceProvider resources;
        private readonly GamesEditor gamesEditor;
        private readonly FullscreenAppViewModel mainModel;
        private IDialogsFactory dialogs;

        private GamesCollectionViewEntry game;
        public GamesCollectionViewEntry Game
        {
            get => game;
            set
            {
                game = value;
                OnPropertyChanged();
            }
        }

        public string ContextActionDescription
        {
            get
            {
                if (Game?.IsRunning == true)
                {
                    return resources.GetString("LOCGameRunning");
                }
                else if (Game?.IsLaunching == true)
                {
                    return resources.GetString("LOCGameLaunching");
                }
                else if (Game?.IsInstalling == true)
                {
                    return resources.GetString("LOCSetupRunning");
                }
                else if (Game?.IsUninstalling == true)
                {
                    return resources.GetString("LOCUninstalling");
                }
                else if (Game?.IsInstalled == false)
                {
                    return resources.GetString("LOCInstallGame");
                }
                else if (Game?.IsInstalled == true)
                {
                    return resources.GetString("LOCPlayGame");
                }

                return "<ErrorState>";
            }
        }

        #region Game Commands

        public RelayCommand<object> ContextActionCommand { get; private set; }
        #endregion

        public GameDetailsViewModel(
            GamesCollectionViewEntry gameView)
        {
            Game = gameView;
            resources = new ResourceProvider();
        }

        public GameDetailsViewModel(
            GamesCollectionViewEntry gameView,
            IResourceProvider resources,
            GamesEditor gamesEditor,
            FullscreenAppViewModel mainModel,
            IDialogsFactory dialogs)
        {
            this.resources = resources;
            this.gamesEditor = gamesEditor;
            this.mainModel = mainModel;
            this.dialogs = dialogs;
            Game = gameView;
            Game.Game.PropertyChanged += Game_PropertyChanged;
            InitializeCommands();
        }

        public void Dispose()
        {
            Game.Game.PropertyChanged -= Game_PropertyChanged;
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ContextActionDescription));
        }

        public void InitializeCommands()
        {
            ContextActionCommand = new RelayCommand<object>((a) =>
            {
                if (Game?.IsInstalling == true || Game?.IsUninstalling == true)
                {
                    CheckSetup();
                }
                else if (Game?.IsRunning == true || Game?.IsLaunching == true)
                {
                    CheckExecution();
                }
                else if (Game?.IsInstalled == false)
                {
                    gamesEditor.InstallGame(Game.Game);
                }
                else if (Game?.IsInstalled == true)
                {
                    gamesEditor.PlayGame(Game.Game);
                }
            });
        }

        public void CheckSetup()
        {
            if (dialogs.ShowMessage(
                resources.GetString("LOCCancelMonitoringSetupAsk"),
                resources.GetString("LOCCancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                gamesEditor.CancelGameMonitoring(Game.Game);
            }
        }

        public void CheckExecution()
        {
            if (dialogs.ShowMessage(
                resources.GetString("LOCCancelMonitoringExecutionAsk"),
                resources.GetString("LOCCancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                gamesEditor.CancelGameMonitoring(Game.Game);
            }
        }
    }
}
