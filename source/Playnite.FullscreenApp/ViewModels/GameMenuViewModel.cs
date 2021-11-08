using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Playnite.FullscreenApp.ViewModels.GameDetailsViewModel;

namespace Playnite.FullscreenApp.ViewModels
{
    public class GameActionItem
    {
        public RelayCommandBase Command { get; set; }
        public object CommandParameter { get; set; }
        public string Title { get; set; }
        public object Template { get; set; }

        public GameActionItem(RelayCommandBase command, string title)
        {
            Command = command;
            Title = title;
        }

        public GameActionItem(RelayCommandBase command, string title, string templateName)
        {
            Command = command;
            Title = title;
            Template = ResourceProvider.GetResource(templateName) ?? DependencyProperty.UnsetValue;
        }

        public GameActionItem(RelayCommandBase command, object commandParameter, string title)
        {
            Command = command;
            CommandParameter = commandParameter;
            Title = title;
        }

        public GameActionItem(RelayCommandBase command, object commandParameter, string title, string templateName)
        {
            Command = command;
            CommandParameter = commandParameter;
            Title = title;
            Template = ResourceProvider.GetResource(templateName) ?? DependencyProperty.UnsetValue;
        }
    }

    public class GameMenuViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly FullscreenAppViewModel mainModel;
        private readonly GamesEditor gamesEditor;

        private GameDetailsViewModel gameDetails;
        public GameDetailsViewModel GameDetails
        {
            get => gameDetails;
            set
            {
                gameDetails = value;
                OnPropertyChanged();
            }
        }

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

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand StartGameCommand => new RelayCommand(() => StartGame());
        public RelayCommand InstallGameCommand => new RelayCommand(() => InstallGame());
        public RelayCommand UninstallGameCommand => new RelayCommand(() => UninstallGame());
        public RelayCommand ToggleFavoritesCommand => new RelayCommand(() => ToggleFavorites());
        public RelayCommand ToggleVisibilityCommand => new RelayCommand(() => ToggleVisibility());
        public RelayCommand RemoveGameCommand => new RelayCommand(() => RemoveGame());
        public RelayCommand<GameAction> ActivateActionCommand => new RelayCommand<GameAction>((a) => ActivateAction(a));

        public GameMenuViewModel(
            IWindowFactory window,
            FullscreenAppViewModel mainModel,
            GameDetailsViewModel gameDetails,
            GamesEditor gamesEditor)
        {
            this.window = window;
            this.mainModel = mainModel;
            this.gamesEditor = gamesEditor;
            GameDetails = gameDetails;
            var items = new List<GameActionItem>();
            if (GameDetails.Game.IsInstalled)
            {
                items.Add(new GameActionItem(StartGameCommand, ResourceProvider.GetString(LOC.PlayGame), "GameMenuPlayButtonTemplate"));
            }
            else
            {
                items.Add(new GameActionItem(InstallGameCommand, ResourceProvider.GetString(LOC.InstallGame), "GameMenuInstallButtonTemplate"));
            }

            gameDetails.Game.GameActions?.Where(a => !a.IsPlayAction).ForEach(a => items.Add(new GameActionItem(ActivateActionCommand, a, a.Name, "GameMenuCustomActionButtonTemplate")));

            items.Add(new GameActionItem(ToggleFavoritesCommand, GameDetails.Game.Favorite ? ResourceProvider.GetString(LOC.RemoveFavoriteGame) : ResourceProvider.GetString(LOC.FavoriteGame), "GameMenuFavoriesButtonTemplate"));
            items.Add(new GameActionItem(ToggleVisibilityCommand, GameDetails.Game.Hidden ? ResourceProvider.GetString(LOC.UnHideGame) : ResourceProvider.GetString(LOC.HideGame), "GameMenuVisibilityButtonTemplate"));
            items.Add(new GameActionItem(RemoveGameCommand, ResourceProvider.GetString(LOC.RemoveGame), "GameMenuRemoveButtonTemplate"));

            if (!GameDetails.Game.IsCustomGame && GameDetails.Game.IsInstalled)
            {
                items.Add(new GameActionItem(UninstallGameCommand, ResourceProvider.GetString(LOC.UninstallGame), "GameMenuUninstallButtonTemplate"));
            }

            GameItems = items;
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void Close()
        {
            window.Close(true);
        }

        public void ActivateAction(GameAction action)
        {
            Close();
            gamesEditor.ActivateAction(gameDetails.Game.Game, action);
        }

        public void StartGame()
        {
            Close();
            gamesEditor.PlayGame(gameDetails.Game.Game);
        }

        public void InstallGame()
        {
            Close();
            gamesEditor.InstallGame(gameDetails.Game.Game);
        }

        public void UninstallGame()
        {
            Close();
            gamesEditor.UnInstallGame(gameDetails.Game.Game);
        }

        public void ToggleFavorites()
        {
            Close();
            gamesEditor.ToggleFavoriteGame(gameDetails.Game.Game);
        }

        public void ToggleVisibility()
        {
            Close();
            gamesEditor.ToggleHideGame(gameDetails.Game.Game);
        }

        public void RemoveGame()
        {
            Close();
            gamesEditor.RemoveGame(gameDetails.Game.Game);
        }
    }
}
