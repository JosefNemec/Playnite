using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Playnite.FullscreenApp.ViewModels.GameDetailsViewModel;

namespace Playnite.FullscreenApp.ViewModels
{
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

        public bool FocusItems { get; set; } = true;

        public SimpleCommand CloseCommand => new SimpleCommand(() => Close());
        public SimpleCommand StartGameCommand => new SimpleCommand(() => StartGame());
        public SimpleCommand InstallGameCommand => new SimpleCommand(() => InstallGame());
        public SimpleCommand UninstallGameCommand => new SimpleCommand(() => UninstallGame());
        public SimpleCommand ToggleFavoritesCommand => new SimpleCommand(() => ToggleFavorites());
        public SimpleCommand ToggleVisibilityCommand => new SimpleCommand(() => ToggleVisibility());
        public SimpleCommand RemoveGameCommand => new SimpleCommand(() => RemoveGame());
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
                items.Add(new GameActionItem(StartGameCommand, ResourceProvider.GetString(LOC.PlayGame)));
            }
            else
            {
                items.Add(new GameActionItem(InstallGameCommand, ResourceProvider.GetString(LOC.InstallGame)));
            }

            gameDetails.Game.GameActions?.Where(a => !a.IsPlayAction).ForEach(a => items.Add(new GameActionItem(ActivateActionCommand, a, a.Name)));

            items.Add(new GameActionItem(ToggleFavoritesCommand, GameDetails.Game.Favorite ? ResourceProvider.GetString(LOC.RemoveFavoriteGame) : ResourceProvider.GetString(LOC.FavoriteGame)));
            items.Add(new GameActionItem(ToggleVisibilityCommand, GameDetails.Game.Hidden ? ResourceProvider.GetString(LOC.UnHideGame) : ResourceProvider.GetString(LOC.HideGame)));
            items.Add(new GameActionItem(RemoveGameCommand, ResourceProvider.GetString(LOC.RemoveGame)));

            if (!GameDetails.Game.IsCustomGame && GameDetails.Game.IsInstalled)
            {
                items.Add(new GameActionItem(UninstallGameCommand, ResourceProvider.GetString(LOC.UninstallGame)));
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
