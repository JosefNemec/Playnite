using Playnite.FullscreenApp.Windows;
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

        public Game Game { get; }
        public List<GameActionItem> GameItems { get; }

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand StartGameCommand => new RelayCommand(() => StartGame());
        public RelayCommand InstallGameCommand => new RelayCommand(() => InstallGame());
        public RelayCommand UninstallGameCommand => new RelayCommand(() => UninstallGame());
        public RelayCommand ToggleFavoritesCommand => new RelayCommand(() => ToggleFavorites());
        public RelayCommand ToggleVisibilityCommand => new RelayCommand(() => ToggleVisibility());
        public RelayCommand ToggleHdrCommand => new RelayCommand(() => ToggleHdr());
        public RelayCommand RemoveGameCommand => new RelayCommand(() => RemoveGame());
        public RelayCommand<GameAction> ActivateActionCommand => new RelayCommand<GameAction>((a) => ActivateAction(a));
        public RelayCommand SetFieldsCommand => new RelayCommand(() => SetFields());

        public GameMenuViewModel(
            IWindowFactory window,
            FullscreenAppViewModel mainModel,
            Game game,
            GamesEditor gamesEditor)
        {
            this.window = window;
            this.mainModel = mainModel;
            this.gamesEditor = gamesEditor;
            Game = game;
            var items = new List<GameActionItem>();
            if (game.IsInstalled)
            {
                items.Add(new GameActionItem(StartGameCommand, ResourceProvider.GetString(LOC.PlayGame), "GameMenuPlayButtonTemplate"));
            }
            else
            {
                items.Add(new GameActionItem(InstallGameCommand, ResourceProvider.GetString(LOC.InstallGame), "GameMenuInstallButtonTemplate"));
            }

            game.GameActions?.Where(a => !a.IsPlayAction).ForEach(a => items.Add(new GameActionItem(ActivateActionCommand, a, a.Name, "GameMenuCustomActionButtonTemplate")));

            items.Add(new GameActionItem(ToggleFavoritesCommand, game.Favorite ? ResourceProvider.GetString(LOC.RemoveFavoriteGame) : ResourceProvider.GetString(LOC.FavoriteGame), "GameMenuFavoriesButtonTemplate"));
            items.Add(new GameActionItem(ToggleVisibilityCommand, game.Hidden ? ResourceProvider.GetString(LOC.UnHideGame) : ResourceProvider.GetString(LOC.HideGame), "GameMenuVisibilityButtonTemplate"));
            if (HdrUtilities.IsHdrSupported())
            {
                items.Add(new GameActionItem(ToggleHdrCommand, game.EnableSystemHdr ? ResourceProvider.GetString(LOC.DisableHdr) : ResourceProvider.GetString(LOC.EnableHdr), "GameMenuHdrButtonTemplate"));
            }
            items.Add(new GameActionItem(SetFieldsCommand, ResourceProvider.GetString(LOC.MenuSetFields), "GameMenuSetFieldsTemplate"));
            items.Add(new GameActionItem(RemoveGameCommand, ResourceProvider.GetString(LOC.RemoveGame), "GameMenuRemoveButtonTemplate"));

            if (!game.IsCustomGame && game.IsInstalled)
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
            gamesEditor.ActivateAction(Game, action);
        }

        public void StartGame()
        {
            Close();
            gamesEditor.PlayGame(Game, true);
        }

        public void InstallGame()
        {
            Close();
            gamesEditor.InstallGame(Game);
        }

        public void UninstallGame()
        {
            Close();
            gamesEditor.UnInstallGame(Game);
        }

        public void ToggleFavorites()
        {
            Close();
            gamesEditor.ToggleFavoriteGame(Game);
        }

        public void ToggleVisibility()
        {
            Close();
            gamesEditor.ToggleHideGame(Game);
        }

        public void ToggleHdr()
        {
            Close();
            gamesEditor.ToggleHdrGame(Game);
        }

        public void RemoveGame()
        {
            Close();
            gamesEditor.RemoveGame(Game);
        }

        private void SelectSingleAndSet<T>(IItemCollection<T> dbCollection, string header, Action<Guid> setter, Guid preselectedItem) where T : DatabaseObject, new()
        {
            var items = dbCollection.Select(a => new SelectableNamedObject<T>(a, a.Name)).OrderBy(a => a.Name).ToList();
            items.Insert(0, new SelectableNamedObject<T>(new T() { Id = Guid.Empty }, ResourceProvider.GetString(LOC.None)));
            items.ForEach(a => a.Selected = preselectedItem == a.Value.Id);
            var passed = ItemSelector.SelectSingle(header, "", items, out var selectedItem);
            if (passed)
            {
                setter(selectedItem.Id);
                mainModel.Database.Games.Update(Game);
            }
        }

        private void SelectMultiAndSet<T>(IItemCollection<T> dbCollection, string header, Action<List<Guid>> setter, List<Guid> preselectedItems) where T : DatabaseObject
        {
            var passed = ItemSelector.SelectMultiple(
               header,
               "",
               dbCollection.Select(a => new SelectableNamedObject<T>(a, a.Name, preselectedItems?.Contains(a.Id) == true)).OrderByDescending(a => a.Selected).ThenBy(a => a.Name).ToList(),
               out var selectedItems);
            if (passed)
            {
                setter(selectedItems.HasItems() ? selectedItems.Select(a => a.Id).ToList() : null);
                mainModel.Database.Games.Update(Game);
            }
        }

        private void SetFields()
        {
            Close();
            var selected = ItemSelector.SelectSingle(
                LOC.MenuSetFields,
                "",
                new List<SelectableNamedObject<GameField>>
                {
                    new SelectableNamedObject<GameField>(GameField.CompletionStatus, ResourceProvider.GetString(LOC.CompletionStatus)),
                    new SelectableNamedObject<GameField>(GameField.Categories,ResourceProvider.GetString(LOC.CategoryLabel)),
                    new SelectableNamedObject<GameField>(GameField.Tags, ResourceProvider.GetString(LOC.TagLabel)),
                    new SelectableNamedObject<GameField>(GameField.Features, ResourceProvider.GetString(LOC.FeatureLabel)),
                    new SelectableNamedObject<GameField>(GameField.Platforms, ResourceProvider.GetString(LOC.PlatformTitle)),
                    new SelectableNamedObject<GameField>(GameField.Genres, ResourceProvider.GetString(LOC.GenreLabel)),
                    new SelectableNamedObject<GameField>(GameField.Developers, ResourceProvider.GetString(LOC.DeveloperLabel)),
                    new SelectableNamedObject<GameField>(GameField.Publishers, ResourceProvider.GetString(LOC.PublisherLabel)),
                    new SelectableNamedObject<GameField>(GameField.Series, ResourceProvider.GetString(LOC.SeriesLabel)),
                    new SelectableNamedObject<GameField>(GameField.AgeRatings, ResourceProvider.GetString(LOC.AgeRatingLabel)),
                    new SelectableNamedObject<GameField>(GameField.Regions, ResourceProvider.GetString(LOC.RegionLabel)),
                    new SelectableNamedObject<GameField>(GameField.Source, ResourceProvider.GetString(LOC.SourceLabel)),
                },
                out var selectedField);
            if (selected)
            {
                switch (selectedField)
                {
                    case GameField.CompletionStatus:
                        SelectSingleAndSet(mainModel.Database.CompletionStatuses, LOC.CompletionStatuses, (val) => Game.CompletionStatusId = val, Game.CompletionStatusId);
                        break;
                    case GameField.Categories:
                        SelectMultiAndSet(mainModel.Database.Categories, LOC.CategoriesLabel, (val) => Game.CategoryIds = val, Game.CategoryIds);
                        break;
                    case GameField.Tags:
                        SelectMultiAndSet(mainModel.Database.Tags, LOC.TagsLabel, (val) => Game.TagIds = val, Game.TagIds);
                        break;
                    case GameField.Features:
                        SelectMultiAndSet(mainModel.Database.Features, LOC.FeaturesLabel, (val) => Game.FeatureIds = val, Game.FeatureIds);
                        break;
                    case GameField.Platforms:
                        SelectMultiAndSet(mainModel.Database.Platforms, LOC.PlatformsTitle, (val) => Game.PlatformIds = val, Game.PlatformIds);
                        break;
                    case GameField.Genres:
                        SelectMultiAndSet(mainModel.Database.Genres, LOC.GenresLabel, (val) => Game.GenreIds = val, Game.GenreIds);
                        break;
                    case GameField.Developers:
                        SelectMultiAndSet(mainModel.Database.Companies, LOC.DevelopersLabel, (val) => Game.DeveloperIds = val, Game.DeveloperIds);
                        break;
                    case GameField.Publishers:
                        SelectMultiAndSet(mainModel.Database.Companies, LOC.PublishersLabel, (val) => Game.PublisherIds = val, Game.PublisherIds);
                        break;
                    case GameField.Series:
                        SelectMultiAndSet(mainModel.Database.Series, LOC.SeriesLabel, (val) => Game.SeriesIds = val, Game.SeriesIds);
                        break;
                    case GameField.AgeRatings:
                        SelectMultiAndSet(mainModel.Database.AgeRatings, LOC.AgeRatingsLabel, (val) => Game.AgeRatingIds = val, Game.AgeRatingIds);
                        break;
                    case GameField.Regions:
                        SelectMultiAndSet(mainModel.Database.Regions, LOC.RegionsLabel, (val) => Game.RegionIds = val, Game.RegionIds);
                        break;
                    case GameField.Source:
                        SelectSingleAndSet(mainModel.Database.Sources, LOC.SourceLabel, (val) => Game.SourceId = val, Game.SourceId);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
