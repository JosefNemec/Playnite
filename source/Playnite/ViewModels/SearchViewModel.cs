using Playnite.Converters;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Playnite.ViewModels
{
    public class GameMenuContext : SearchContext
    {
        private readonly MainViewModelBase mainModel;
        private readonly Game game;

        public GameMenuContext(Game game, MainViewModelBase mainModel)
        {
            this.game = game;
            this.mainModel = mainModel;
            UseAutoSearch = true;
            Label = game.Name;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            return MenuItems.GetSearchGameMenuItems(game, mainModel);
        }
    }

    public class DefaultSearchContext : SearchContext
    {
        private readonly MainViewModelBase mainModel;
        private readonly List<SearchSupport> searchProviders;
        private readonly bool commandsInDefault;
        private List<SearchItem> commands;

        public DefaultSearchContext(MainViewModelBase mainModel, List<SearchSupport> searchProviders, bool commandsInDefault)
        {
            this.mainModel = mainModel;
            this.searchProviders = searchProviders;
            this.commandsInDefault = commandsInDefault;
            Description = ResourceProvider.GetString(LOC.DefaultSearchDescription);
            Hint = ResourceProvider.GetString(LOC.DefaultSearchHint);
        }

        private SearchItemAction GetGameAction(Game game, GameSearchItemAction actionSettings)
        {
            switch (actionSettings)
            {
                case GameSearchItemAction.Play:
                    return new SearchItemAction(game.IsInstalled ? LOC.PlayGame : LOC.InstallGame, () => mainModel.App.GamesEditor.StartContextAction(game));
                case GameSearchItemAction.SwitchTo:
                    return new SearchItemAction(LOC.GameSearchItemActionSwitchTo, () =>
                    {
                        if (mainModel.AppSettings.ViewSettings.GamesViewType == DesktopView.List)
                        {
                            mainModel.AppSettings.ViewSettings.GamesViewType = DesktopView.Details;
                        }
                        else if (mainModel.AppSettings.ViewSettings.GamesViewType == DesktopView.Grid)
                        {
                            if (!mainModel.AppSettings.GridViewSideBarVisible)
                            {
                                mainModel.AppSettings.GridViewSideBarVisible = true;
                            }
                        }

                        mainModel.SelectGame(game.Id, true);
                    });
                case GameSearchItemAction.OpenMenu:
                    return new ContextSwitchSearchItemAction(LOC.GameSearchItemActionOpenMenu, new GameMenuContext(game, mainModel));
                case GameSearchItemAction.Edit:
                    return new SearchItemAction(LOC.GameSearchItemActionEdit, () => mainModel.EditGame(game));
                case GameSearchItemAction.None:
                default:
                    return null;
            }
        }

        private SearchItemAction GetPrimaryGameAction(Game game)
        {
            return GetGameAction(game, mainModel.AppSettings.PrimaryGameSearchItemAction);
        }

        private SearchItemAction GetSecondaryGameAction(Game game)
        {
            return GetGameAction(game, mainModel.AppSettings.SecondaryGameSearchItemAction);
        }

        private ContextSwitchSearchItemAction GetGameMenuAction(Game game)
        {
            return new ContextSwitchSearchItemAction(LOC.GameSearchItemActionOpenMenu, new GameMenuContext(game, mainModel));
        }

        private bool GameFilter(Game game, string searchTerm, GameSearchFilterSettings settings, bool matchTargetAcronymStart)
        {
            if (game.Hidden && !settings.Hidden)
            {
                return false;
            }

            if (!game.IsInstalled && !settings.Uninstalled)
            {
                return false;
            }

            if (!SearchViewModel.MatchTextFilter(searchTerm, game.Name, matchTargetAcronymStart))
            {
                return false;
            }

            return true;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            IEnumerable<SearchItem> searchCommnands(string keyword)
            {
                if (commands == null)
                {
                    commands = mainModel.GetSearchCommands().ToList();
                }

                foreach (var command in commands.Where(a => SearchViewModel.MatchTextFilter(keyword, a.Name, true)))
                {
                    yield return command;
                }
            }

            if (args.SearchTerm.StartsWith("#"))
            {
                var commandSearch = args.SearchTerm.Substring(1).Trim();
                foreach (var cmd in searchCommnands(commandSearch))
                {
                    yield return cmd;
                }

                yield break;
            }

            if (args.SearchTerm.EndsWith(" "))
            {
                var providerTest = args.SearchTerm.Trim().TrimStart('/');
                var provider = searchProviders.FirstOrDefault(a => a.Keyword.Equals(providerTest, StringComparison.InvariantCultureIgnoreCase));
                if (provider != null)
                {
                    args.SwitchContext(provider.Context);
                    yield break;
                }
            }

            if (args.SearchTerm.StartsWith("/"))
            {
                var pluginSearch = args.SearchTerm.Substring(1).Trim();
                foreach (var provider in searchProviders.Where(a => SearchViewModel.MatchTextFilter(pluginSearch, a.Name, false)))
                {
                    yield return new SearchItem(provider.Name, new ContextSwitchSearchItemAction(LOC.Activate, provider.Context))
                    {
                        Description = "/" + provider.Keyword
                    };
                }

                yield break;
            }

            if (args.SearchTerm.IsNullOrWhiteSpace())
            {
                foreach (var game in mainModel.Database.Games.
                    Where(a => a.LastActivity != null && GameFilter(a, string.Empty, args.GameFilterSettings, true)).
                    OrderByDescending(a => a.LastActivity).
                    Take(20))
                {
                    yield return new GameSearchItem(game, GetPrimaryGameAction(game))
                    {
                        SecondaryAction = GetSecondaryGameAction(game),
                        MenuAction = GetGameMenuAction(game)
                    };
                }

                yield break;
            }

            var searchTerm = args.SearchTerm.Trim();
            if (commandsInDefault)
            {
                foreach (var cmd in searchCommnands(searchTerm))
                {
                    yield return cmd;
                }
            }

            foreach (var game in mainModel.Database.Games.
                Where(g => GameFilter(g, searchTerm, args.GameFilterSettings, true))
                .OrderBy(a => a.Name.GetLevenshteinDistanceIgnoreCase(searchTerm))
                .ThenBy(x => x.Name)
                .ThenByDescending(x => x.IsInstalled)
                .Take(60))
            {
                yield return new GameSearchItem(game, GetPrimaryGameAction(game))
                {
                    SecondaryAction = GetSecondaryGameAction(game),
                    MenuAction = GetGameMenuAction(game)
                };
            }

            foreach (var tool in mainModel.Database.SoftwareApps.Where(a => SearchViewModel.MatchTextFilter(searchTerm, a.Name, true)))
            {
                yield return new SearchItem(tool.Name, LOC.Open, () => mainModel.StartSoftwareTool(tool), tool.Icon);
            }
        }
    }

    // TODO replace this in future by exposing GamesCollectionViewEntry to SDK and let plugins create them directly.
    // Also remove all plugin and setting dependencies from View class.
    public class GameSearchItemWrapper : GameSearchItem
    {
        public GamesCollectionViewEntry GameView { get; set; }
        public List<string> AdditionalInfo { get; set; } = new List<string>();

        public GameSearchItemWrapper(GameSearchItem item, LibraryPlugin plugin, PlayniteSettings settings)
            : base(item.Game, item.PrimaryAction)
        {
            GameView = new GamesCollectionViewEntry(item.Game, plugin, settings, true);
            SecondaryAction = item.SecondaryAction;
            MenuAction = item.MenuAction;

            if (settings.SearchWindowVisibility.CompletionStatus && item.Game.CompletionStatus != null)
            {
                AdditionalInfo.Add(item.Game.CompletionStatus.Name);
            }

            if (settings.SearchWindowVisibility.PlayTime)
            {
                AdditionalInfo.Add(PlayTimeToStringConverter.Instance.Convert(item.Game.Playtime, typeof(string), settings.PlaytimeUseDaysFormat, CultureInfo.CurrentCulture) as string);
            }

            if (settings.SearchWindowVisibility.Platform && item.Game.Platforms.HasItems())
            {
                item.Game.Platforms.ForEach(a => AdditionalInfo.Add(a.Name));
            }

            if (settings.SearchWindowVisibility.ReleaseDate && item.Game.ReleaseDate != null)
            {
                AdditionalInfo.Add(item.Game.ReleaseDate.Value.Year.ToString());
            }
        }

        public override string ToString()
        {
            return GameView?.Name;
        }
    }

    public class SearchItemWrapper : SearchItem
    {
        private readonly SynchronizationContext syncContext;

        public SearchItem Item { get; }

        private object itemIcon = null;
        public object ItemIcon
        {
            get
            {
                if (itemIcon != null)
                {
                    return itemIcon;
                }

                if (Item.Icon == null)
                {
                    return null;
                }

                if (Item.Icon is string stringIcon && stringIcon.IsHttpUrl())
                {
                    Task.Run(() =>
                    {
                        itemIcon = SdkHelpers.ResolveUiItemIcon(Item.Icon, syncContext);
                        if (itemIcon == null)
                        {
                            itemIcon = DependencyProperty.UnsetValue;
                        }
                        else
                        {
                            syncContext.Send((_) => OnPropertyChanged(nameof(ItemIcon)), null);
                        }
                    });

                    return null;
                }
                else
                {
                    return SdkHelpers.ResolveUiItemIcon(Item.Icon);
                }
            }
        }

        public SearchItemWrapper(SearchItem item, SynchronizationContext syncContext)
            : base(item.Name, item.PrimaryAction)

        {
            this.syncContext = syncContext;
            Item = item;
            Description = item.Description;
            MenuAction = item.MenuAction;
            SecondaryAction = item.SecondaryAction;
        }

        public override string ToString()
        {
            return Item?.Name;
        }
    }

    public class SearchViewModel : ObservableObject
    {
        public class ItemAction : ObservableObject
        {
            private bool selected;

            public bool Selected { get => selected; set => SetValue(ref selected, value); }
            public string Name { get; set; }
            public Action Action { get; set; }
            public bool CloseView { get; set; }

            public ItemAction(string name, Action action, bool closeView)
            {
                Name = name;
                Action = action;
                CloseView = closeView;
            }

            public ItemAction(SearchItemAction action)
            {
                Name = action.Name;
                Action = action.Action;
                CloseView = action.CloseSearch;
            }
        }

        #region backing fields
        private List<SearchItem> searchResults;
        private SearchItem selectedSearchItem;
        private string currentSearchProviderDescription;
        private bool slowAnimationActive;
        private ItemAction primaryAction;
        private ItemAction secondaryAction;
        private ItemAction menuAction;
        private string currentContextHint;
        private bool contextHintVisible;
        private bool filterHintVisible = false;
        private string filterHint;
        private string currentContextLabel;
        #endregion backing fields

        private static readonly char[] textMatchSplitter = new char[] { ' ' };
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly IGameDatabaseMain database;
        private readonly ExtensionFactory extensions;
        private readonly MainViewModelBase mainModel;
        private int currentSearchDelay = 0;
        private readonly SynchronizationContext syncContext;
        private readonly System.Timers.Timer searchDelayTimer = new System.Timers.Timer { AutoReset = false };
        private readonly System.Timers.Timer longSearchTimer = new System.Timers.Timer { AutoReset = false, Interval = 700 };
        private CancellationTokenSource currentSearchToken;
        private int customProviderDeleteAttemps = 0;
        private readonly Stack<SearchContext> searchContextStack = new Stack<SearchContext>();
        private const double defaultMinimumJaronWinklerSimilarity = 0.90;
        private bool isClosing = false;

        private string searchTerm;
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                searchTerm = value;
                OnPropertyChanged();

                if (currentSearchDelay == 0)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    PerformSearch();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    searchDelayTimer.Stop();
                    searchDelayTimer.Interval = currentSearchDelay;
                    searchDelayTimer.Start();
                }
            }
        }

        public bool FilterHintVisible { get => filterHintVisible; set => SetValue(ref filterHintVisible, value); }
        public string FilterHint { get => filterHint; set => SetValue(ref filterHint, value); }
        public GameSearchFilterSettings GameFilterSettings { get; set; }
        public string CurrentContextHint { get => currentContextHint; set => SetValue(ref currentContextHint, value); }
        public string CurrentContextLabel { get => currentContextLabel; set => SetValue(ref currentContextLabel, value); }
        public bool ContextHintVisible { get => contextHintVisible; set => SetValue(ref contextHintVisible, value); }
        public bool SlowAnimationActive { get => slowAnimationActive; set => SetValue(ref slowAnimationActive, value); }
        public string CurrentSearchProviderDescription { get => currentSearchProviderDescription; set => SetValue(ref currentSearchProviderDescription, value); }
        public List<SearchItem> SearchResults { get => searchResults; set =>SetValue(ref searchResults, value); }
        public SearchItem SelectedSearchItem { get => selectedSearchItem;
            set
            {
                SetValue(ref selectedSearchItem, value);
                InitActions(selectedSearchItem);
            }
        }

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand<KeyEventArgs> TextBoxKeyDownCommand => new RelayCommand<KeyEventArgs>((keyArgs) => TextBoxKeyDown(keyArgs));
        public RelayCommand<KeyEventArgs> TextBoxKeyUpCommand => new RelayCommand<KeyEventArgs>((keyArgs) => TextBoxKeyUp(keyArgs));
        public RelayCommand<EventArgs> WindowClosedCommand => new RelayCommand<EventArgs>((_) => WindowClosed(_));
        public RelayCommand<EventArgs> WindowDeactivatedCommand => new RelayCommand<EventArgs>((_) => WindowDeactivated(_));
        public RelayCommand ToggleHintCommand => new RelayCommand(() => ToggleHint());
        public RelayCommand OpenSearchSettingsCommand => new RelayCommand(() => OpenSearchSettings());
        public RelayCommand DeactiveCurrentContextCommand => new RelayCommand(() => DeactiveCurrentContext());

        public RelayCommand PrimaryActionCommand { get; }
        public RelayCommand SecondaryActionCommand { get; }
        public RelayCommand OpenMenuCommand { get; }

        public ItemAction PrimaryAction { get => primaryAction; set => SetValue(ref primaryAction, value); }
        public ItemAction SecondaryAction { get => secondaryAction; set => SetValue(ref secondaryAction, value); }
        public ItemAction MenuAction { get => menuAction; set => SetValue(ref menuAction, value); }

        public event EventHandler SearchClosed;
        public bool Active => window.Window.IsActive;

        public SearchViewModel(
            IWindowFactory window,
            IGameDatabaseMain database,
            ExtensionFactory extensions,
            MainViewModelBase mainModel)
        {
            this.window = window;
            this.database = database;
            this.extensions = extensions;
            this.mainModel = mainModel;

            PrimaryActionCommand = new RelayCommand(() => syncContext.Send(_ =>
            {
                if (PrimaryAction.CloseView)
                {
                    Close();
                }
                PrimaryAction.Action();
            }, null));
            SecondaryActionCommand = new RelayCommand(() => syncContext.Send(_ =>
            {
                if (SecondaryAction.CloseView)
                {
                    Close();
                }
                SecondaryAction.Action();
            }, null));
            OpenMenuCommand = new RelayCommand(() => syncContext.Send(_ =>
            {
                if (MenuAction.CloseView)
                {
                    Close();
                }
                MenuAction.Action();
            }, null));

            var searchProviders = new List<SearchSupport>();
            foreach (var plugin in extensions.Plugins)
            {
                foreach (var search in plugin.Value.Plugin.Searches ?? new List<SearchSupport>())
                {
                    var searchId = plugin.Value.Description.Id + search.DefaultKeyword;
                    if (mainModel.AppSettings.CustomSearchKeywrods.TryGetValue(searchId, out var customKeywrod))
                    {
                        search.Keyword = customKeywrod;
                    }
                    else
                    {
                        search.Keyword = search.DefaultKeyword;
                    }

                    searchProviders.Add(search);
                }
            }

            syncContext = SynchronizationContext.Current;
            searchDelayTimer.Elapsed += (_, __) => syncContext.Post((___) =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                PerformSearch();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }, null);

            longSearchTimer.Elapsed += (_, __) =>
            {
                syncContext.Send((___) => SlowAnimationActive = true, null);
            };

            if (mainModel.AppSettings.SaveGlobalSearchFilterSettings)
            {
                GameFilterSettings = mainModel.AppSettings.GameSearchFilterSettings.GetClone();
            }
            else
            {
                GameFilterSettings = new GameSearchFilterSettings();
            }

            GameFilterSettings.PropertyChanged += GameFilterSettings_PropertyChanged;
            SetCurrentContext(new DefaultSearchContext(mainModel, searchProviders, mainModel.AppSettings.IncludeCommandsInDefaultSearch));
        }

        private void GameFilterSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SearchTerm = SearchTerm;
        }

        public void OpenSearch()
        {
            window.Show(this);
        }

        public void OpenSearch(string search)
        {
            window.Show(this);
            SearchTerm = search;
        }
        public void OpenSearch(SearchContext context, string search)
        {
            window.Show(this);
            SetCurrentContext(context);
            if (!search.IsNullOrEmpty())
            {
                SearchTerm = search;
            }
        }

        public void Close()
        {
            // This is used because deactivate event is called before close event
            // so WindowDeactivated and WindowClosed would conflict.
            isClosing = true;
            window.Close();
        }

        private void WindowDeactivated(EventArgs args)
        {
            // The view would get automatically closed once you switch to debugger...
            if (Debugger.IsAttached)
            {
                return;
            }

            if (isClosing)
            {
                return;
            }

            Close();
        }

        private void WindowClosed(EventArgs args)
        {
            // Don't call this in Close method because that's not invoked when closing using ALT-F4
            currentSearchToken?.Cancel();
            searchDelayTimer.Dispose();
            longSearchTimer.Dispose();
            if (mainModel.AppSettings.SaveGlobalSearchFilterSettings)
            {
                mainModel.AppSettings.GameSearchFilterSettings = GameFilterSettings.GetClone();
            }

            SearchClosed?.Invoke(this, EventArgs.Empty);
        }

        private void SetCurrentContext(SearchContext context)
        {
            syncContext.Send((_) =>
            {
                if (searchContextStack.Count == 0 || searchContextStack.Peek() != context)
                {
                    searchContextStack.Push(context);
                }

                ContextHintVisible = false;
                CurrentContextHint = context.Hint;
                CurrentSearchProviderDescription = context.Description;
                if (searchContextStack.Count == 1)
                {
                    CurrentContextLabel = null;
                }
                else
                {
                    CurrentContextLabel = context.Label.IsNullOrEmpty() ? "search" : context.Label;
                }

                customProviderDeleteAttemps = 0;
                currentSearchDelay = 0;
                searchDelayTimer.Stop();
                longSearchTimer.Stop();
                SlowAnimationActive = false;

                // Not clearing results immediately will prevent "flashing" when switching contexts
                // because results list is being cleared completely and then populated again.
                if (context.Delay > 0)
                {
                    SearchResults = null;
                }

                SearchTerm = string.Empty;

                // This is called AFTER initial search is set for case where's there's a delay set by search provider.
                // This makes it so the first search has no delay at all.
                customProviderDeleteAttemps = 1;
                currentSearchDelay = context.Delay;
            }, null);
        }

        private void DeactiveCurrentContext()
        {
            if (searchContextStack.Count == 1)
            {
                return;
            }

            searchContextStack.Pop();
            SetCurrentContext(searchContextStack.Peek());
        }

        private List<SearchItem> FilterSearchResults(List<SearchItem> toFilter, string filter, bool matchTargetAcronymStart)
        {
            var results = new List<SearchItem>();
            foreach (var item in toFilter)
            {
                if (MatchTextFilter(filter, item.Name, matchTargetAcronymStart))
                {
                    results.Add(item);
                }
            }

            return results;
        }

        public static bool MatchTextFilter(string filter, string toMatch, bool matchTargetAcronymStart, double minimumJaronWinklerSimilarity = defaultMinimumJaronWinklerSimilarity)
        {
            if (filter.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (!filter.IsNullOrWhiteSpace() && toMatch.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (filter.IsNullOrWhiteSpace() && toMatch.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (filter.GetJaroWinklerSimilarityIgnoreCase(toMatch) >= minimumJaronWinklerSimilarity)
            {
                return true;
            }

            if (filter.Length > toMatch.Length)
            {
                return false;
            }

            if (matchTargetAcronymStart && filter.IsStartOfStringAcronym(toMatch))
            {
                return true;
            }

            var filterSplit = filter.Split(textMatchSplitter, StringSplitOptions.RemoveEmptyEntries);
            var toMatchSplit = toMatch.Split(textMatchSplitter, StringSplitOptions.RemoveEmptyEntries);
            var allMatch = true;
            // This is pretty crude, but it works for most cases and provides relatively good results.
            // TODO definitely could use some improvements for better fuzzy results.
            foreach (var word in filterSplit)
            {
                if (!toMatchSplit.Any(a => a.ContainsInvariantCulture(word, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace)))
                {
                    allMatch = false;
                    break;
                }
            }

            return allMatch;
        }

        public async Task PerformSearch()
        {
            SlowAnimationActive = false;
            longSearchTimer.Stop();
            longSearchTimer.Start();

            searchDelayTimer.Stop();
            currentSearchToken?.Cancel();

            var searchToken = new CancellationTokenSource();
            currentSearchToken = searchToken;
            var searchArgs = new GetSearchResultsArgs
            {
                CancelToken = searchToken.Token,
                SearchTerm = SearchTerm,
                SwitchContextAction = SetCurrentContext,
                GameFilterSettings =  GameFilterSettings
            };

            var results = await Task.Run(() =>
            {
                try
                {
                    var context = searchContextStack.Peek();
                    if (context.UseAutoSearch)
                    {
                        if (context.AutoSearchCache == null)
                        {
                            context.AutoSearchCache = context.GetSearchResults(searchArgs)?.ToList() ?? new List<SearchItem>();
                        }

                        if (SearchTerm.IsNullOrWhiteSpace())
                        {
                            return context.AutoSearchCache;
                        }
                        else
                        {
                            return FilterSearchResults(context.AutoSearchCache, SearchTerm, true);
                        }
                    }
                    else
                    {
                        return context.GetSearchResults(searchArgs)?.ToList() ?? new List<SearchItem>();
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to get search results.");
                    return null;
                }
            });

            if (searchToken.IsCancellationRequested)
            {
                return;
            }

            if (results.HasItems())
            {
                foreach (var item in results.ToList())
                {
                    if (item is GameSearchItemWrapper || item is SearchItemWrapper)
                    {
                        continue;
                    }
                    else if (item is GameSearchItem gameItem)
                    {
                        var index = results.IndexOf(item);
                        results[index] = new GameSearchItemWrapper(gameItem, mainModel.Extensions.GetLibraryPlugin(gameItem.Game.PluginId), mainModel.AppSettings);
                    }
                    else
                    {
                        var index = results.IndexOf(item);
                        results[index] = new SearchItemWrapper(item, syncContext);
                    }
                }

                SearchResults = results;
                SelectedSearchItem = SearchResults[0];
            }
            else
            {
                SearchResults = null;
                SelectedSearchItem = null;
            }

            currentSearchToken = null;
            searchToken.Dispose();
            longSearchTimer.Stop();
            SlowAnimationActive = false;
        }

        private void TextBoxKeyDown(KeyEventArgs keyArgs)
        {
            if (!SearchResults.HasItems())
            {
                return;
            }

            if (SearchResults.Count > 0)
            {
                var selected = false;
                var curIndex = SearchResults.IndexOf(SelectedSearchItem);
                if (keyArgs.Key == Key.Up)
                {
                    if (curIndex > 0)
                    {
                        SelectedSearchItem = SearchResults[curIndex - 1];
                        selected = true;
                    }
                }
                else if (keyArgs.Key == Key.Down)
                {
                    if (curIndex < SearchResults.Count - 1)
                    {
                        SelectedSearchItem = SearchResults[curIndex + 1];
                        selected = true;
                    }
                }
                else if (keyArgs.Key == Key.PageUp)
                {
                    if (curIndex - 5 > 0)
                    {
                        SelectedSearchItem = SearchResults[curIndex - 5];
                    }
                    else
                    {
                        SelectedSearchItem = SearchResults[0];
                    }

                    selected = true;
                }
                else if (keyArgs.Key == Key.PageDown)
                {
                    if (curIndex + 5 < SearchResults.Count)
                    {
                        SelectedSearchItem = SearchResults[curIndex + 5];
                    }
                    else
                    {
                        SelectedSearchItem = SearchResults[SearchResults.Count - 1];
                    }

                    selected = true;
                }

                if (selected)
                {
                    keyArgs.Handled = true;
                    return;
                }
            }

            if (keyArgs.Key == Key.Tab)
            {
                ToggleSelectedAction();
                keyArgs.Handled = true;
                return;
            }
        }

        private void TextBoxKeyUp(KeyEventArgs keyArgs)
        {
            if ((keyArgs.Key == Key.Return || keyArgs.Key == Key.Enter) && keyArgs.KeyboardDevice.Modifiers == ModifierKeys.Shift &&
                MenuAction != null)
            {
                MenuAction.Action();
                return;
            }

            if ((keyArgs.Key == Key.Return || keyArgs.Key == Key.Enter) && keyArgs.KeyboardDevice.Modifiers == ModifierKeys.Shift &&
                MenuAction == null)
            {
                return;
            }

            if ((keyArgs.Key == Key.Return || keyArgs.Key == Key.Enter) && SelectedSearchItem != null)
            {
                InvokeSelectedAction();
                return;
            }

            if (keyArgs.Key == Key.Back && SearchTerm.IsNullOrEmpty())
            {
                if (customProviderDeleteAttemps >= 1)
                {
                    DeactiveCurrentContext();
                }
                else
                {
                    // This is to switch contexts only after user uses backspace on already cleared search.
                    // We don't want to switch contexts if user is just deleting current search.
                    customProviderDeleteAttemps++;
                }

                return;
            }

            if (keyArgs.Key == Key.F1)
            {
                ToggleHint();
                return;
            }

            if (keyArgs.Key == Key.F2)
            {
                ToggleInstalledFilter();
                return;
            }

            if (keyArgs.Key == Key.F3)
            {
                ToggleHiddenFilter();
                return;
            }

            if (!SearchTerm.IsNullOrEmpty())
            {
                customProviderDeleteAttemps = 0;
            }
        }

        private void ToggleSelectedAction()
        {
            if (PrimaryAction?.Selected == true && SecondaryAction != null)
            {
                PrimaryAction.Selected = false;
                SecondaryAction.Selected = true;
            }
            else if (PrimaryAction?.Selected == true && MenuAction != null)
            {
                PrimaryAction.Selected = false;
                MenuAction.Selected = true;
            }
            else if (SecondaryAction?.Selected == true && MenuAction != null)
            {
                SecondaryAction.Selected = false;
                MenuAction.Selected = true;
            }
            else if (SecondaryAction?.Selected == true && PrimaryAction != null)
            {
                SecondaryAction.Selected = false;
                PrimaryAction.Selected = true;
            }
            else if (MenuAction?.Selected == true && PrimaryAction != null)
            {
                MenuAction.Selected = false;
                PrimaryAction.Selected = true;
            }
            else if (MenuAction?.Selected == true && SecondaryAction != null)
            {
                MenuAction.Selected = false;
                SecondaryAction.Selected = true;
            }
        }

        private void InvokeSelectedAction()
        {
            try
            {
                syncContext.Send(_ =>
                {
                    if (PrimaryAction?.Selected == true)
                    {
                        if (PrimaryAction.CloseView)
                        {
                            Close();
                        }

                        PrimaryAction.Action();
                    }
                    else if (SecondaryAction?.Selected == true)
                    {
                        if (SecondaryAction.CloseView)
                        {
                            Close();
                        }

                        SecondaryAction.Action();
                    }
                    else if (MenuAction?.Selected == true)
                    {
                        if (MenuAction.CloseView)
                        {
                            Close();
                        }

                        MenuAction.Action();
                    }
                }, null);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to invoke search item action.");
            }
        }

        private void InitActions(SearchItem selectedSearchItem)
        {
            if (selectedSearchItem == null)
            {
                PrimaryAction = null;
                SecondaryAction = null;
                MenuAction = null;
                return;
            }

            ItemAction getAction(SearchItemAction action)
            {
                if (action != null)
                {
                    if (action is ContextSwitchSearchItemAction contextAction)
                    {
                        return new ItemAction(contextAction.Name, () => SetCurrentContext(contextAction.Context), false);
                    }
                    else
                    {
                        return new ItemAction(action);
                    }
                }
                else
                {
                    return null;
                }
            }

            PrimaryAction = getAction(selectedSearchItem.PrimaryAction);
            SecondaryAction = getAction(selectedSearchItem.SecondaryAction);
            MenuAction = getAction(selectedSearchItem.MenuAction);
            if (PrimaryAction != null)
            {
                PrimaryAction.Selected = true;
            }
            else if (SecondaryAction != null)
            {
                SecondaryAction.Selected = true;
            }
            else if (MenuAction != null)
            {
                MenuAction.Selected = true;
            }
        }

        private void ToggleHint()
        {
            var context = searchContextStack.Peek();
            if (!context.Hint.IsNullOrWhiteSpace())
            {
                ContextHintVisible = !ContextHintVisible;
            }
        }

        private void OpenSearchSettings()
        {
            Close();
            if (mainModel.App.Mode == ApplicationMode.Desktop)
            {
                mainModel.OpenSettings((int)DesktopSettingsPage.Search);
            }
            else
            {
                throw new NotSupportedInFullscreenException();
            }
        }

        private void ToggleInstalledFilter()
        {
            FilterHint = GameFilterSettings.Uninstalled ? ResourceProvider.GetString(LOC.SearchFilterUninstalledExcluded) : ResourceProvider.GetString(LOC.SearchFilterUninstalledIncluded);
            GameFilterSettings.Uninstalled = !GameFilterSettings.Uninstalled;
            FilterHintVisible = true;
            FilterHintVisible = false;
        }

        private void ToggleHiddenFilter()
        {
            FilterHint = GameFilterSettings.Hidden ? ResourceProvider.GetString(LOC.SearchFilterHiddenalledExcluded) : ResourceProvider.GetString(LOC.SearchFilterHiddenIncluded);
            GameFilterSettings.Hidden = !GameFilterSettings.Hidden;
            FilterHintVisible = true;
            FilterHintVisible = false;
        }

        public void Focus()
        {
            window.RestoreWindow();
        }
    }
}
