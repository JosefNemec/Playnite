using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.ViewModels
{
    public enum RandomGameSelectAction
    {
        None,
        Play,
        Navigate
    }

    public class RandomGameSelectViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly IResourceProvider resources;
        private readonly BaseCollectionView collection;
        private readonly IGameDatabaseMain database;

        public RandomGameSelectAction SelectedAction { get; private set; } = RandomGameSelectAction.None;

        private bool isLimitedToFilter = true;
        public bool IsLimitedToFilter
        {
            get => isLimitedToFilter;
            set
            {
                isLimitedToFilter = value;
                OnPropertyChanged();
            }
        }

        private Game selectedGame;
        public Game SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> PickAnotherCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                PickGame();
            });
        }

        public RelayCommand<object> PlayGameCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                PlayGame();
            }, (a) => SelectedGame != null);
        }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand NavigateToGameCommand
        {
            get => new RelayCommand(() =>
            {
                NavigateToGame();
            }, () => SelectedGame != null);
        }

        public RandomGameSelectViewModel(
            IGameDatabaseMain database,
            BaseCollectionView collection,
            IWindowFactory window,
            IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.resources = resources;
            this.collection = collection;
        }

        public bool? OpenView()
        {
            PickGame();
            return window.CreateAndOpenDialog(this);
        }

        public void PlayGame()
        {
            SelectedAction = RandomGameSelectAction.Play;
            window.Close(true);
        }

        public void CloseView()
        {
            SelectedAction = RandomGameSelectAction.None;
            window.Close(false);
        }

        public void NavigateToGame()
        {
            SelectedAction = RandomGameSelectAction.Navigate;
            window.Close(false);
        }

        public void PickGame()
        {
            var lastSelection = SelectedGame;
            if (IsLimitedToFilter)
            {
                var count = collection.CollectionView.Count;
                if (count == 1)
                {
                    SelectedGame = (collection.CollectionView.GetItemAt(0) as GamesCollectionViewEntry).Game;
                }
                else if (count > 1)
                {
                    var newSelection = lastSelection;
                    while (newSelection == lastSelection)
                    {
                        var index = GlobalRandom.Next(0, count);
                        newSelection = (collection.CollectionView.GetItemAt(index) as GamesCollectionViewEntry).Game;
                    }

                    SelectedGame = newSelection;
                }
                else
                {
                    SelectedGame = null;
                }
            }
            else
            {
                var count = database.Games.Count;
                if (count == 1)
                {
                    SelectedGame = database.Games.First();
                }
                else if (count > 1)
                {
                    var newSelection = lastSelection;
                    while (newSelection == lastSelection)
                    {
                        var index = GlobalRandom.Next(0, count);
                        newSelection = database.Games.ElementAt(index);
                    }

                    SelectedGame = newSelection;
                }
                else
                {
                    SelectedGame = null;
                }
            }
        }
    }
}
