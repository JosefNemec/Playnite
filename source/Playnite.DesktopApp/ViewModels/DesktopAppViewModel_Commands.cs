﻿using Playnite.DesktopApp.Windows;
using Playnite.SDK;
using Playnite.SDK.Exceptions;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
        public RelayCommand<object> AddCustomGameCommand { get; private set; }
        public RelayCommand<object> AddInstalledGamesCommand { get; private set; }
        public RelayCommand<object> AddEmulatedGamesCommand { get; private set; }
        public RelayCommand<object> AddWindowsStoreGamesCommand { get; private set; }
        public RelayCommand<object> OpenFullScreenCommand { get; private set; }
        public RelayCommand<object> OpenFullScreenFromControllerCommand { get; private set; }
        public RelayCommand<object> CancelProgressCommand { get; private set; }
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
        public RelayCommand<object> CheckForUpdateCommand { get; private set; }
        public RelayCommand<object> OpenDbFieldsManagerCommand { get; private set; }
        public RelayCommand<object> OpenLibraryIntegrationsConfigCommand { get; private set; }
        public RelayCommand<LibraryPlugin> UpdateLibraryCommand { get; private set; }
        public RelayCommand<object> RestartInSafeMode { get; private set; }

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
        public RelayCommand<Game> CreateDesktopShortcutCommand { get; private set; }
        public RelayCommand<List<Game>> CreateDesktopShortcutsCommand { get; private set; }
        public RelayCommand<Game> OpenManualCommand { get; private set; }
        public RelayCommand<Game> ToggleFavoritesCommand { get; private set; }
        public RelayCommand<Game> ToggleVisibilityCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsHiddensCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsHiddensCommand { get; private set; }
        public RelayCommand<Game> AssignGameCategoryCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> AssignGamesCategoryCommand { get; private set; }
        public RelayCommand<Tuple<Game, CompletionStatus>> SetGameCompletionStatusCommand { get; private set; }
        public RelayCommand<Tuple<IEnumerable<Game>, CompletionStatus>> SetGamesCompletionStatusCommand { get; private set; }
        public RelayCommand<Game> RemoveGameCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveGamesCommand { get; private set; }
        public RelayCommand<object> SelectRandomGameCommand { get; private set; }
        public RelayCommand<SidebarWrapperItem> SelectSidebarViewCommand { get; private set; }

        private void InitializeCommands()
        {
            OpenSearchCommand = new RelayCommand<object>((game) =>
            {
                if (SearchOpened)
                {
                    // The binding sometimes breaks when main window is restored from minimized state.
                    // This fixes it.
                    SearchOpened = false;
                }

                SearchOpened = true;
            }, new KeyGesture(Key.F, ModifierKeys.Control));

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
                UpdateDatabase(AppSettings.DownloadMetadataOnImport);
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
                OpenAboutWindow(new AboutViewModel(new AboutWindowFactory(), Dialogs, Resources));
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
                    Dialogs), null);
            }, (a) => Database?.IsOpen == true);

            AddEmulatedGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportEmulatedGames(
                    new EmulatorImportViewModel(Database,
                    EmulatorImportViewModel.DialogType.GameImport,
                    new EmulatorImportWindowFactory(),
                    Dialogs,
                    Resources));
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.Q, ModifierKeys.Control));

            AddWindowsStoreGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportWindowsStoreGames(
                    new InstalledGamesViewModel(
                    new InstalledGamesWindowFactory(),
                    Dialogs));
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

            CancelProgressCommand = new RelayCommand<object>((a) =>
            {
                CancelProgress();
            }, (a) => GlobalTaskHandler.CancelToken?.IsCancellationRequested == false);

            ClearMessagesCommand = new RelayCommand<object>((a) =>
            {
                ClearMessages();
            }, (a) => PlayniteApi?.Notifications?.Count > 0);

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
                    new LibraryIntegrationsViewModel(Database,
                    AppSettings,
                    new LibraryIntegrationsWindowFactory(),
                    Dialogs,
                    Resources,
                    Extensions,
                    application));
            });

            UpdateLibraryCommand = new RelayCommand<LibraryPlugin>((a) =>
            {
                UpdateLibrary(a);
            }, (a) => GameAdditionAllowed);

            RemoveGameSelectionCommand = new RelayCommand<object>((a) =>
            {
                RemoveGameSelection();
            });

            InvokeExtensionFunctionCommand = new RelayCommand<ExtensionFunction>((f) =>
            {
                if (!Extensions.InvokeExtension(f, out var error))
                {
                    var message = error.Message;
                    if (error is ScriptRuntimeException err)
                    {
                        message = err.Message + "\n\n" + err.ScriptStackTrace;
                    }

                    Dialogs.ShowMessage(
                         message,
                         Resources.GetString("LOCScriptError"),
                         MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            ReloadScriptsCommand = new RelayCommand<object>((f) =>
            {
                Extensions.LoadScripts(PlayniteApi, AppSettings.DisabledPlugins, application.CmdLine.SafeStartup);
            }, new KeyGesture(Key.F12));

            ShowGameSideBarCommand = new RelayCommand<GamesCollectionViewEntry>((f) =>
            {
                AppSettings.GridViewSideBarVisible = true;
                SelectedGame = f;
            });

            CloseGameSideBarCommand = new RelayCommand<object>((f) =>
            {
                AppSettings.GridViewSideBarVisible = false;
            });

            OpenSettingsCommand = new RelayCommand<object>((a) =>
            {
                OpenSettings(
                    new SettingsViewModel(Database,
                    AppSettings,
                    new SettingsWindowFactory(),
                    Dialogs,
                    Resources,
                    Extensions,
                    application));
            }, new KeyGesture(Key.F4));

            StartGameCommand = new RelayCommand<Game>((game) =>
            {
                if (game != null)
                {
                    GamesEditor.PlayGame(game);
                }
                else if (SelectedGame != null)
                {
                    GamesEditor.PlayGame(SelectedGame.Game);
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
                    GamesEditor.RemoveGames(SelectedGames.Select(g => g.Game).ToList());
                }
                else
                {
                    GamesEditor.RemoveGame(SelectedGame.Game);
                }
            },
            (a) => SelectedGame != null,
            new KeyGesture(Key.Delete));

            EditGameCommand = new RelayCommand<Game>((a) =>
            {
                if (GamesEditor.EditGame(a) == true)
                {
                    SelectedGame = GamesView.Items.FirstOrDefault(g => g.Id == a.Id);
                }
            });

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

            AssignGameCategoryCommand = new RelayCommand<Game>((a) =>
            {
                if (GamesEditor.SetGameCategories(a) == true)
                {
                    SelectedGame = GamesView.Items.FirstOrDefault(g => g.Id == a.Id);
                }
            });

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
                GamesEditor.RemoveGames(a.ToList());
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

            SetAsHiddensCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHideGames(a.ToList(), true);
            });

            RemoveAsHiddensCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHideGames(a.ToList(), false);
            });

            SelectRandomGameCommand = new RelayCommand<object>((a) =>
            {
                PlayRandomGame();
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.F6));

            RestartInSafeMode = new RelayCommand<object>((a) =>
            {
                RestartAppSafe();
            });

            SelectSidebarViewCommand = new RelayCommand<SidebarWrapperItem>((a) =>
            {
                a.Command.Execute(null);
            });
        }
    }
}
