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
        public class GameActionItem
        {
            public RelayCommand Command { get; set; }
            public object CommandParameter { get; set; }
            public string Title { get; set; }

            public GameActionItem(RelayCommand command, string title)
            {
                Command = command;
                Title = title;
            }

            public GameActionItem(RelayCommand command, object commandParameter, string title)
            {
                Command = command;
                CommandParameter = commandParameter;
                Title = title;
            }
        }

        private readonly IResourceProvider resources;
        private readonly GamesEditor gamesEditor;
        private readonly FullscreenAppViewModel mainModel;
        private IDialogsFactory dialogs;

        public GamesCollectionViewEntry Game { get; set; }

        private List<GameActionItem> gameItems;
        public List<GameActionItem> GameItems
        {
            get => gameItems;
            set
            {
                gameItems = value;
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
                else if (Game?.IsUnistalling == true)
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
        public RelayCommand<object> StartGameCommand { get; private set; }
        public RelayCommand<object> InstallGameCommand { get; private set; }
        public RelayCommand<object> UninstallGameCommand { get; private set; }
        public RelayCommand<object> ToggleFavoritesCommand { get; private set; }
        public RelayCommand<object> ToggleVisibilityCommand { get; private set; }
        public RelayCommand<object> RemoveGameCommand { get; private set; }
        public RelayCommand<object> ContextActionCommand { get; private set; }
        public RelayCommand<GameAction> ActivateActionCommand { get; private set; }
        #endregion

        public GameDetailsViewModel(
            GamesCollectionViewEntry gameView)
        {
            Game = gameView;
            resources = new ResourceProvider();
            InitializeItems();
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
            InitializeItems();
        }

        public void Dispose()
        {
            Game.Game.PropertyChanged -= Game_PropertyChanged;
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InitializeItems();
            OnPropertyChanged(nameof(ContextActionDescription));
        }

        public void InitializeCommands()
        {
            ActivateActionCommand = new RelayCommand<GameAction>((a) =>
            {
                gamesEditor.ActivateAction(Game.Game, a);
                mainModel.ToggleGameOptionsCommand.Execute(null);
            }, (a) => a != null);

            StartGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.PlayGame(Game.Game);
                mainModel.ToggleGameOptionsCommand.Execute(null);
            });

            InstallGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.InstallGame(Game.Game);
                mainModel.ToggleGameOptionsCommand.Execute(null);
            });

            UninstallGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.UnInstallGame(Game.Game);
                mainModel.ToggleGameOptionsCommand.Execute(null);
            });

            ToggleFavoritesCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.ToggleFavoriteGame(Game.Game);
                mainModel.ToggleGameOptionsCommand.Execute(null);
            });

            ToggleVisibilityCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.ToggleHideGame(Game.Game);
                mainModel.ToggleGameOptionsCommand.Execute(null);
            });

            RemoveGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.RemoveGame(Game.Game);
                mainModel.ToggleGameOptionsCommand.Execute(null);
            });

            ContextActionCommand = new RelayCommand<object>((a) =>
            {
                if (Game?.IsInstalling == true || Game?.IsUnistalling == true)
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

        public void InitializeItems()
        {
            var items = new List<GameActionItem>();
            if (Game.IsInstalled)
            {
                items.Add(new GameActionItem(StartGameCommand, resources.GetString("LOCPlayGame")));
            }
            else
            {
                items.Add(new GameActionItem(InstallGameCommand, resources.GetString("LOCInstallGame")));
            }

            Game.OtherActions.ForEach(a => items.Add(new GameActionItem(ActivateActionCommand, a, a.Name)));

            items.Add(new GameActionItem(ToggleFavoritesCommand, Game.Favorite ? resources.GetString("LOCRemoveFavoriteGame") : resources.GetString("LOCFavoriteGame")));
            items.Add(new GameActionItem(ToggleVisibilityCommand, Game.Hidden ? resources.GetString("LOCUnHideGame") : resources.GetString("LOCHideGame")));
            items.Add(new GameActionItem(RemoveGameCommand, resources.GetString("LOCRemoveGame")));

            if (!Game.IsCustomGame && Game.IsInstalled)
            {
                items.Add(new GameActionItem(UninstallGameCommand, resources.GetString("LOCUninstallGame")));
            }

            GameItems = items;
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
