using Playnite.Commands;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp.ViewModels
{
    public class GameDetailsViewModel : ObservableObject, IDisposable
    {
        public class GameActionItem
        {
            public RelayCommand Command { get; set; }
            public string Title { get; set; }

            public GameActionItem(RelayCommand command, string title)
            {
                Command = command;
                Title = title;
            }
        }

        private readonly IResourceProvider resources;
        private readonly GamesEditor gamesEditor;
        private readonly FullscreenAppViewModel mainModel;

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

        #region Game Commands
        public RelayCommand<object> StartGameCommand { get; private set; }
        public RelayCommand<object> InstallGameCommand { get; private set; }
        public RelayCommand<object> UninstallGameCommand { get; private set; }
        public RelayCommand<object> ToggleFavoritesCommand { get; private set; }
        public RelayCommand<object> ToggleVisibilityCommand { get; private set; }
        public RelayCommand<object> RemoveGameCommand { get; private set; }
        #endregion

        public GameDetailsViewModel(
            GamesCollectionViewEntry gameView,
            IResourceProvider resources,
            GamesEditor gamesEditor,
            FullscreenAppViewModel mainModel)
        {
            this.resources = resources;
            this.gamesEditor = gamesEditor;
            this.mainModel = mainModel;
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
        }

        public void InitializeCommands()
        {
            StartGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.PlayGame(Game.Game);
                mainModel.GameMenuVisible = false;
            });

            InstallGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.InstallGame(Game.Game);
                mainModel.GameMenuVisible = false;
            });

            UninstallGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.UnInstallGame(Game.Game);
                mainModel.GameMenuVisible = false;
            });

            ToggleFavoritesCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.ToggleFavoriteGame(Game.Game);
                mainModel.GameMenuVisible = false;
            });

            ToggleVisibilityCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.ToggleHideGame(Game.Game);
                mainModel.GameMenuVisible = false;
            });

            RemoveGameCommand = new RelayCommand<object>((a) =>
            {
                gamesEditor.RemoveGame(Game.Game);
                mainModel.GameMenuVisible = false;
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

            items.Add(new GameActionItem(ToggleFavoritesCommand, Game.Hidden ? resources.GetString("LOCRemoveFavoriteGame") : resources.GetString("LOCFavoriteGame")));
            items.Add(new GameActionItem(ToggleVisibilityCommand, Game.Hidden ? resources.GetString("LOCUnHideGame") : resources.GetString("LOCHideGame")));
            items.Add(new GameActionItem(RemoveGameCommand, resources.GetString("LOCRemoveGame")));

            if (!Game.IsCustomGame)
            {
                items.Add(new GameActionItem(UninstallGameCommand, resources.GetString("LOCUninstallGame")));
            }

            GameItems = items;
        }
    }
}
