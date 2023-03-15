using Playnite.DesktopApp.Windows;
using Playnite.SDK;
using Playnite.SDK.Exceptions;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Playnite.DesktopApp.ViewModels
{
    public partial class DesktopAppViewModel
    {
        public RelayCommand<object> ToggleExplorerPanelCommand { get; private set; }
        public RelayCommand<object> ToggleFilterPanelCommand { get; private set; }
        public RelayCommand<object> OpenFilterPanelCommand { get; private set; }
        public RelayCommand<object> CloseFilterPanelCommand { get; private set; }
        public RelayCommand<object> CloseNotificationPanelCommand { get; private set; }
        public RelayCommand<ThirdPartyTool> ThirdPartyToolOpenCommand { get; private set; }
        public RelayCommand<object> UpdateGamesCommand { get; private set; }
        public RelayCommand<object> OpenSteamFriendsCommand { get; private set; }
        public RelayCommand<object> ReportIssueCommand { get; private set; }
        public RelayCommand<object> ShutdownCommand { get; private set; }
        public RelayCommand<object> ShowWindowCommand { get; private set; }
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; private set; }
        public RelayCommand<DragEventArgs> FileDroppedCommand { get; private set; }
        public RelayCommand<object> OpenAboutCommand { get; private set; }
        public RelayCommand<object> OpenEmulatorsCommand { get; private set; }
        public RelayCommand<object> OpenSettingsCommand { get; private set; }
        public RelayCommand<object> OpenAddonsCommand { get; private set; }
        public RelayCommand<object> AddCustomGameCommand { get; private set; }
        public RelayCommand<object> AddInstalledGamesCommand { get; private set; }
        public RelayCommand<object> AddEmulatedGamesCommand { get; private set; }
        public RelayCommand<object> AddWindowsStoreGamesCommand { get; private set; }
        public RelayCommand<object> OpenFullScreenCommand { get; private set; }
        public RelayCommand<object> OpenFullScreenFromControllerCommand { get; private set; }
        public RelayCommand<object> ClearMessagesCommand { get; private set; }
        public RelayCommand<object> DownloadMetadataCommand { get; private set; }
        public RelayCommand<object> OpenSoftwareToolsCommand { get; private set; }
        public RelayCommand<object> ClearFiltersCommand { get; private set; }
        public RelayCommand<object> RemoveGameSelectionCommand { get; private set; }
        public RelayCommand<ExtensionFunction> InvokeExtensionFunctionCommand { get; private set; }
        public RelayCommand<object> ReloadScriptsCommand { get; private set; }
        public RelayCommand<GamesCollectionViewEntry> ShowGameSideBarCommand { get; private set; }
        public RelayCommand<object> CloseGameSideBarCommand { get; private set; }
        public RelayCommand<object> OpenSearchCommand { get; private set; }
        public RelayCommand OpenGlobalSearchCommand { get; private set; }
        public RelayCommand<object> CheckForUpdateCommand { get; private set; }
        public RelayCommand<object> OpenDbFieldsManagerCommand { get; private set; }
        public RelayCommand<object> OpenLibraryIntegrationsConfigCommand { get; private set; }
        public RelayCommand<LibraryPlugin> UpdateLibraryCommand { get; private set; }
        public RelayCommand UpdateEmulationDirsCommand { get; private set; }
        public RelayCommand<GameScannerConfig> UpdateEmulationDirCommand { get; private set; }
        public RelayCommand<Guid> OpenPluginSettingsCommand { get; private set; }

        public RelayCommand<Game> StartGameCommand { get; private set; }
        public RelayCommand<AppSoftware> StartSoftwareToolCommand { get; private set; }
        public RelayCommand<Game> InstallGameCommand { get; private set; }
        public RelayCommand<Game> UninstallGameCommand { get; private set; }
        public RelayCommand<object> StartSelectedGameCommand { get; private set; }
        public RelayCommand<object> EditSelectedGamesCommand { get; private set; }
        public RelayCommand<object> RemoveSelectedGamesCommand { get; private set; }
        public RelayCommand<Game> EditGameCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> EditGamesCommand { get; private set; }
        public RelayCommand<Game> OpenGameLocationCommand { get; private set; }
        public RelayCommand<Game> UpdateGameInstallSizeWithDialogCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> UpdateGamesAllInstallSizeWithDialogCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> UpdateGamesMissingInstallSizeWithDialogCommand { get; private set; }
        public RelayCommand<Game> CreateDesktopShortcutCommand { get; private set; }
        public RelayCommand<List<Game>> CreateDesktopShortcutsCommand { get; private set; }
        public RelayCommand<Game> OpenManualCommand { get; private set; }
        public RelayCommand<Game> ToggleFavoritesCommand { get; private set; }
        public RelayCommand<Game> ToggleVisibilityCommand { get; private set; }
        public RelayCommand<Game> ToggleHdrCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsHiddensCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsHiddensCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> EnableHdrCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> DisableHdrCommand { get; private set; }

        public RelayCommand<Game> AssignGameCategoryCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> AssignGamesCategoryCommand { get; private set; }
        public RelayCommand<Tuple<Game, CompletionStatus>> SetGameCompletionStatusCommand { get; private set; }
        public RelayCommand<Tuple<IEnumerable<Game>, CompletionStatus>> SetGamesCompletionStatusCommand { get; private set; }
        public RelayCommand<Game> RemoveGameCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveGamesCommand { get; private set; }
        public RelayCommand<object> SelectRandomGameCommand { get; private set; }
        public RelayCommand<object> ViewSelectRandomGameCommand { get; private set; }
        public RelayCommand<SidebarWrapperItem> SelectSidebarViewCommand { get; private set; }

        public RelayCommand<object> SwitchDetailsViewCommand { get; private set; }
        public RelayCommand<object> SwitchGridViewCommand { get; private set; }
        public RelayCommand<object> SwitchListViewCommand { get; private set; }

        private void InitializeCommands()
        {
            OpenSearchCommand = new RelayCommand<object>((_) => OpenSearch(), new KeyGesture(Key.F, ModifierKeys.Control));
            OpenGlobalSearchCommand = new RelayCommand(() => OpenGlobalSearch());

            ToggleExplorerPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.ExplorerPanelVisible = !AppSettings.ExplorerPanelVisible;
            }, new KeyGesture(Key.E, ModifierKeys.Control));

            ToggleFilterPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = !AppSettings.FilterPanelVisible;
            }, new KeyGesture(Key.G, ModifierKeys.Control));

            OpenFilterPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = true;
            });

            CloseFilterPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = false;
            });

            CloseNotificationPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.NotificationPanelVisible = false;
            });

            ThirdPartyToolOpenCommand = new RelayCommand<ThirdPartyTool>((tool) =>
            {
                StartThirdPartyTool(tool);
            });

            UpdateGamesCommand = new RelayCommand<object>((a) =>
            {
#pragma warning disable CS4014
                UpdateLibrary(AppSettings.DownloadMetadataOnImport, true, true);
#pragma warning restore CS4014
            }, (a) => GameAdditionAllowed,
            new KeyGesture(Key.F5));

            OpenSteamFriendsCommand = new RelayCommand<object>((a) =>
            {
                OpenSteamFriends();
            });

            ReportIssueCommand = new RelayCommand<object>((a) =>
            {
                ReportIssue();
            });

            ShutdownCommand = new RelayCommand<object>((a) =>
            {
                if (GlobalTaskHandler.IsActive)
                {
                    if (Dialogs.ShowMessage(
                        Resources.GetString("LOCBackgroundProgressCancelAskExit"),
                        Resources.GetString("LOCCrashClosePlaynite"),
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                ignoreCloseActions = true;
                ShutdownApp();
            }, new KeyGesture(Key.Q, ModifierKeys.Alt));

            ShowWindowCommand = new RelayCommand<object>((a) =>
            {
                RestoreWindow();
            });

            WindowClosingCommand = new RelayCommand<CancelEventArgs>((args) =>
            {
                OnClosing(args);
            });

            FileDroppedCommand = new RelayCommand<DragEventArgs>((args) =>
            {
                OnFileDropped(args);
            });

            OpenAboutCommand = new RelayCommand<object>((a) =>
            {
                OpenAboutWindow(new AboutViewModel(new AboutWindowFactory(), Dialogs, Resources, App.ServicesClient));
            }, new KeyGesture(Key.F1));

            OpenEmulatorsCommand = new RelayCommand<object>((a) =>
            {
                ConfigureEmulators(
                    new EmulatorsViewModel(Database,
                    new EmulatorsWindowFactory(),
                    Dialogs,
                    Resources));
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.T, ModifierKeys.Control));

            OpenSoftwareToolsCommand = new RelayCommand<object>((a) =>
            {
                ConfigureSoftwareTools(new ToolsConfigViewModel(
                    Database,
                    new ToolsConfigWindowFactory(),
                    Dialogs,
                    Resources));
            }, (a) => Database?.IsOpen == true);

            AddCustomGameCommand = new RelayCommand<object>((a) =>
            {
                AddCustomGame(new GameEditWindowFactory());
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.Insert));

            AddInstalledGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportInstalledGames(
                    new InstalledGamesViewModel(
                    new InstalledGamesWindowFactory(),
                    Dialogs,
                    Database), null);
            }, (a) => Database?.IsOpen == true);

            AddEmulatedGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportEmulatedGames(
                    new EmulatedGamesImportViewModel(
                        Database,
                        new EmulatedGameImportWindowFactory(),
                        Dialogs,
                        Resources));
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.Q, ModifierKeys.Control));

            AddWindowsStoreGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportWindowsStoreGames(
                    new InstalledGamesViewModel(
                    new InstalledGamesWindowFactory(),
                    Dialogs,
                    Database));
            }, (a) => Database?.IsOpen == true);

            OpenFullScreenCommand = new RelayCommand<object>((a) =>
            {
                SwitchToFullscreenMode();
            }, new KeyGesture(Key.F11));

            OpenFullScreenFromControllerCommand = new RelayCommand<object>((a) =>
            {
                if (AppSettings.GuideButtonOpensFullscreen)
                {
                    SwitchToFullscreenMode();
                }
            }, new KeyGesture(Key.F11));

            ClearMessagesCommand = new RelayCommand<object>((a) =>
            {
                ClearMessages();
            }, (a) => App?.Notifications?.Count > 0);

            DownloadMetadataCommand = new RelayCommand<object>((a) =>
            {
                DownloadMetadata(new MetadataDownloadViewModel(new MetadataDownloadWindowFactory()));
            }, (a) => GameAdditionAllowed,
            new KeyGesture(Key.D, ModifierKeys.Control));

            ClearFiltersCommand = new RelayCommand<object>((a) =>
            {
                ClearFilters();
            });

            CheckForUpdateCommand = new RelayCommand<object>((a) =>
            {
                CheckForUpdate();
            });

            OpenDbFieldsManagerCommand = new RelayCommand<object>((a) =>
            {
                ConfigureDatabaseFields(
                        new DatabaseFieldsManagerViewModel(
                            Database,
                            new DatabaseFieldsManagerWindowFactory(),
                            Dialogs,
                            Resources));
            }, (a) => GameAdditionAllowed,
            new KeyGesture(Key.W, ModifierKeys.Control));

            OpenLibraryIntegrationsConfigCommand = new RelayCommand<object>((a) =>
            {
                OpenIntegrationSettings(
                    new LibraryIntegrationsViewModel(
                        new LibraryIntegrationsWindowFactory(),
                        Dialogs,
                        Resources,
                        Extensions));
            });

            UpdateLibraryCommand = new RelayCommand<LibraryPlugin>((a) =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                UpdateLibrary(a);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }, (a) => GameAdditionAllowed);

            RemoveGameSelectionCommand = new RelayCommand<object>((a) =>
            {
                RemoveGameSelection();
            });

            InvokeExtensionFunctionCommand = new RelayCommand<ExtensionFunction>((f) =>
            {
                if (!Extensions.InvokeExtension(f, out var error))
                {
                    Dialogs.ShowMessage(
                         error.Message,
                         Resources.GetString("LOCScriptError"),
                         MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            ReloadScriptsCommand = new RelayCommand<object>((f) =>
            {
                Extensions.LoadScripts(AppSettings.DisabledPlugins, App.CmdLine.SafeStartup, AppSettings.DevelExtenions.Where(a => a.Selected == true).Select(a => a.Item).ToList());
            }, new KeyGesture(Key.F12));

            ShowGameSideBarCommand = new RelayCommand<GamesCollectionViewEntry>((f) =>
            {
                AppSettings.GridViewSideBarVisible = true;
                if (SelectedGame?.Game.Id != f.Id)
                {
                    SelectedGames = new List<GamesCollectionViewEntry> { f };
                }
            });

            CloseGameSideBarCommand = new RelayCommand<object>((f) =>
            {
                AppSettings.GridViewSideBarVisible = false;
            });

            OpenSettingsCommand = new RelayCommand<object>((a) =>
            {
                OpenSettings();
            }, new KeyGesture(Key.F4));

            OpenAddonsCommand = new RelayCommand<object>((a) =>
            {
                new AddonsViewModel(
                    new AddonsWindowFactory(),
                    Dialogs,
                    Resources,
                    App.ServicesClient,
                    Extensions,
                    AppSettings,
                    App).OpenView();
            }, new KeyGesture(Key.F9));

            StartGameCommand = new RelayCommand<Game>((game) =>
            {
                if (game != null)
                {
                    StartGame(game);
                }
                else if (SelectedGame != null)
                {
                    StartGame(SelectedGame.Game);
                }
            });

            StartSoftwareToolCommand = new RelayCommand<AppSoftware>((app) =>
            {
                StartSoftwareTool(app);
            });

            InstallGameCommand = new RelayCommand<Game>((game) =>
            {
                if (game != null)
                {
                    GamesEditor.InstallGame(game);
                }
                else if (SelectedGame != null)
                {
                    GamesEditor.InstallGame(SelectedGame.Game);
                }
            });

            UninstallGameCommand = new RelayCommand<Game>((game) =>
            {
                if (game != null)
                {
                    GamesEditor.UnInstallGame(game);
                }
                else if (SelectedGame != null)
                {
                    GamesEditor.UnInstallGame(SelectedGame.Game);
                }
            });

            EditSelectedGamesCommand = new RelayCommand<object>((a) =>
            {
                if (SelectedGames?.Count() > 1)
                {
                    ignoreSelectionChanges = true;
                    try
                    {
                        GamesEditor.EditGames(SelectedGames.Select(g => g.Game).ToList());
                    }
                    finally
                    {
                        ignoreSelectionChanges = false;
                    }
                }
                else
                {
                    GamesEditor.EditGame(SelectedGame.Game);
                }
            },
            (a) => SelectedGame != null,
            new KeyGesture(Key.F3));

            StartSelectedGameCommand = new RelayCommand<object>((a) =>
            {
                GamesEditor.PlayGame(SelectedGame.Game);
            },
            (a) => SelectedGames?.Count() == 1,
            new KeyGesture(Key.Enter));

            RemoveSelectedGamesCommand = new RelayCommand<object>((a) =>
            {
                if (SelectedGames?.Count() > 1)
                {
                    GamesEditor.RemoveGames(SelectedGames.Select(g => g.Game).Distinct().ToList());
                }
                else
                {
                    GamesEditor.RemoveGame(SelectedGame.Game);
                }
            },
            (a) => SelectedGame != null,
            new KeyGesture(Key.Delete));

            EditGameCommand = new RelayCommand<Game>((a) => EditGame(a));

            EditGamesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                ignoreSelectionChanges = true;
                try
                {
                    GamesEditor.EditGames(a.ToList());
                }
                finally
                {
                    ignoreSelectionChanges = false;
                }
            });

            OpenGameLocationCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.OpenGameLocation(a);
            });

            CreateDesktopShortcutCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.CreateDesktopShortcut(a);
            });

            UpdateGameInstallSizeWithDialogCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.UpdateGameSizeWithDialog(a, false, true);
            });

            CreateDesktopShortcutsCommand = new RelayCommand<List<Game>>((a) =>
            {
                GamesEditor.CreateDesktopShortcut(a);
            });

            OpenManualCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.OpenManual(a);
            });

            ToggleFavoritesCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.ToggleFavoriteGame(a);
            });

            ToggleVisibilityCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.ToggleHideGame(a);
            });

            ToggleHdrCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.ToggleHdrGame(a);
            });

            AssignGameCategoryCommand = new RelayCommand<Game>((a) => AssignCategories(a));

            AssignGamesCategoryCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetGamesCategories(a.ToList());
            });

            SetGameCompletionStatusCommand = new RelayCommand<Tuple<Game, CompletionStatus>>((a) =>
            {
                GamesEditor.SetCompletionStatus(a.Item1, a.Item2);
            });

            SetGamesCompletionStatusCommand = new RelayCommand<Tuple<IEnumerable<Game>, CompletionStatus>>((a) =>
            {
                GamesEditor.SetCompletionStatus(a.Item1.ToList(), a.Item2);
            });

            RemoveGameCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.RemoveGame(a);
            },
            new KeyGesture(Key.Delete));

            RemoveGamesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.RemoveGames(a.Distinct().ToList());
            },
            new KeyGesture(Key.Delete));

            SetAsFavoritesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetFavoriteGames(a.ToList(), true);
            });

            RemoveAsFavoritesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetFavoriteGames(a.ToList(), false);
            });

            UpdateGamesAllInstallSizeWithDialogCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.UpdateGamesSizeWithDialog(a.ToList(), false);
            });

            UpdateGamesMissingInstallSizeWithDialogCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.UpdateGamesSizeWithDialog(a.ToList(), true);
            });

            SetAsHiddensCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHideGames(a.ToList(), true);
            });

            RemoveAsHiddensCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHideGames(a.ToList(), false);
            });

            EnableHdrCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHdrSupport(a.ToList(), true);
            });

            DisableHdrCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHdrSupport(a.ToList(), false);
            });

            SelectRandomGameCommand = new RelayCommand<object>((a) =>
            {
                PlayRandomGame();
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.F6));

            ViewSelectRandomGameCommand = new RelayCommand<object>((a) =>
            {
                ViewSelectRandomGame();
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.F7));

            SelectSidebarViewCommand = new RelayCommand<SidebarWrapperItem>((a) =>
            {
                a.Command.Execute(null);
            });

            SwitchDetailsViewCommand = new RelayCommand<object>((_) => AppSettings.ViewSettings.GamesViewType = DesktopView.Details);
            SwitchGridViewCommand = new RelayCommand<object>((_) => AppSettings.ViewSettings.GamesViewType = DesktopView.Grid);
            SwitchListViewCommand = new RelayCommand<object>((_) => AppSettings.ViewSettings.GamesViewType = DesktopView.List);

            UpdateEmulationDirCommand = new RelayCommand<GameScannerConfig>((a) =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                UpdateEmulationLibrary(a);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }, (a) => GameAdditionAllowed);

            UpdateEmulationDirsCommand = new RelayCommand(() =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                UpdateEmulationLibrary();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }, () => GameAdditionAllowed);

            OpenPluginSettingsCommand = new RelayCommand<Guid>((pluginId) => OpenPluginSettings(pluginId));
        }
    }
}
