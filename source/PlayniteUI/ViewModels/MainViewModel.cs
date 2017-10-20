using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Commands;
using PlayniteUI.Controls;
using PlayniteUI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PlayniteUI.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //private bool importSteamCatWizard = false;
        //private ulong importSteamCatWizardId = 0;

        private static object gamesLock = new object();
        //private WindowPositionHandler positionManager;

        private GamesCollectionView gamesView;
        public GamesCollectionView GamesView
        {
            get => gamesView;
            set
            {
                gamesView = value;
                OnPropertyChanged("GamesView");
            }
        }

        private ObservableCollection<ThirdPartyTool> thirdPartyTools;
        public ObservableCollection<ThirdPartyTool> ThirdPartyTools
        {
            get => thirdPartyTools;
            set
            {
                thirdPartyTools = value;
                OnPropertyChanged("ThirdPartyTools");
            }
        }

        private bool gameAdditionAllowed = true;
        public bool GameAdditionAllowed
        {
            get => gameAdditionAllowed;
            set
            {
                gameAdditionAllowed = value;
                OnPropertyChanged("GameAdditionAllowed");
            }
        }

        private string progressStatus;
        public string ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                OnPropertyChanged("ProgressStatus");
            }
        }

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }

        private double progressTotal;
        public double ProgressTotal
        {
            get => progressTotal;
            set
            {
                progressTotal = value;
                OnPropertyChanged("ProgressTotal");
            }
        }

        private bool progressVisible = false;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                OnPropertyChanged("ProgressVisible");
            }
        }

        private FilterSelectorConfig filterSelector;
        public FilterSelectorConfig FilterSelector
        {
            get => filterSelector;
            set
            {
                filterSelector = value;
                OnPropertyChanged("FilterSelector");
            }
        }

        private Visibility visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get => visibility;
            set
            {
                visibility = value;
                OnPropertyChanged("Visibility");
            }
        }

        private WindowState windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get => windowState;
            set
            {
                if (value == WindowState.Minimized && AppSettings.MinimizeToTray && AppSettings.EnableTray)
                {
                    Visibility = Visibility.Hidden;
                }

                windowState = value;
                OnPropertyChanged("WindowState");
            }
        }

        public static Settings AppSettings
        {
            get;
            private set;
        }

#region Commands
        public RelayCommand<IGame> StartGameCommand
        {
            get => new RelayCommand<IGame>((game) =>
            {
                GamesEditor.Instance.PlayGame(game);
            });
        }

        public RelayCommand<ThirdPartyTool> ThridPartyToolOpenCommand
        {
            get => new RelayCommand<ThirdPartyTool>((tool) =>
            {
                StartThirdPartyTool(tool);
            });
        }

        public RelayCommand<object> LoadGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                LoadGames(true);
            });
        }

        public RelayCommand<object> OpenSteamFriendsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenSteamFriends();
            });
        }

        public RelayCommand<object> ReportIssueCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ReportIssue();
            });
        }

        public RelayCommand<object> ShutdownCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ShutdownApp();
            });
        }

        public RelayCommand<object> ShowWindowCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ShowWindow();
            });
        }

        public RelayCommand<CancelEventArgs> WindowClosingCommand
        {
            get => new RelayCommand<CancelEventArgs>((args) =>
            {
                OnClosing(args);
            });
        }

        public RelayCommand<object> OpenAboutCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenAboutWindow(AboutWindowFactory.Instance);
            });
        }

        public RelayCommand<object> OpenPlatformsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenPlatformsWindow(PlatformsWindowFactory.Instance);
            });
        }

        public RelayCommand<object> OpenSettingsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenSettingsWindow(SettingsWindowFactory.Instance);
            });
        }

        public RelayCommand<object> AddCustomGameCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenAddCustomGameWindow(GameEditWindowFactory.Instance);
            });
        }

        public RelayCommand<object> AddInstalledGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenAddInstalledGamesWindow(InstalledGamesWindowFactory.Instance);
            });
        }

        public RelayCommand<object> AddEmulatedGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenAddEmulatedGamesWindow(EmulatorImportWindowFactory.Instance);
            });
        }
#endregion Commands

        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private INotificationFactory notifications;

        public MainViewModel(
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            INotificationFactory notifications,
            Settings settings)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.notifications = notifications;
            AppSettings = settings;
            //positionManager = new WindowPositionHandler(this, "Main", Config);

            try
            {
                ThirdPartyTools = new ObservableCollection<ThirdPartyTool>(ThirdPartyToolsList.GetDefaultInstalledTools());
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "failed to load third party tools");
            }

            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            AppSettings.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;
            FilterSelector = new FilterSelectorConfig(new GamesStats(GameDatabase.Instance), AppSettings.FilterSettings);
        }

        private void FilterSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Active")
            {
                AppSettings.SaveSettings();
            }
        }

        private void AppSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Language")
            {
                Localization.SetLanguage(AppSettings.Language);
            }

            AppSettings.SaveSettings();
        }

        public void StartThirdPartyTool(ThirdPartyTool tool)
        {
            try
            {
                tool.Start();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to start 3rd party tool.");
            }
        }

        public void OpenSteamFriends()
        {
            System.Diagnostics.Process.Start(@"steam://open/friends");
        }

        public void ReportIssue()
        {
            System.Diagnostics.Process.Start(@"https://github.com/JosefNemec/Playnite/issues/new");
        }

        public void ShutdownApp()
        {
            Application.Current.Shutdown();
        }

        public async void LoadGames(bool updateLibrary)
        {
            if (string.IsNullOrEmpty(AppSettings.DatabasePath))
            {
                throw new Exception("Cannot load games, database path is not set.");
            }

            if (GamesLoaderHandler.ProgressTask != null && GamesLoaderHandler.ProgressTask.Status == TaskStatus.Running)
            {
                GamesLoaderHandler.CancelToken.Cancel();
                await GamesLoaderHandler.ProgressTask;
            }

            GameAdditionAllowed = false;
            var database = GameDatabase.Instance;

            try
            {
                database.OpenDatabase(AppSettings.DatabasePath);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                GameAdditionAllowed = false;
                dialogs.ShowMessage(
                    resources.FindString("DatabaseOpenError") + $" {exc.Message}",
                    resources.FindString("DatabaseErroTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            GamesView?.Dispose();
            GamesView = new GamesCollectionView(database, AppSettings);
            BindingOperations.EnableCollectionSynchronization(GamesView.Items, gamesLock);

            try
            {
                GamesEditor.Instance.UpdateJumpList();
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Failed to set update JumpList data: ");
            }

            try
            {
                if (!updateLibrary)
                {
                    return;
                }

                GamesLoaderHandler.CancelToken = new CancellationTokenSource();
                GamesLoaderHandler.ProgressTask = Task.Factory.StartNew(() =>
                {
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;
                    ProgressStatus = "Importing installed games...";
                    Thread.Sleep(2000);

                    try
                    {
                        if (AppSettings.BattleNetSettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.BattleNet);
                            notifications.RemoveMessage(NotificationCodes.BattleNetInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to import installed Battle.net games.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("BnetInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.UplaySettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.Uplay);
                            notifications.RemoveMessage(NotificationCodes.UplayInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to import installed Uplay games.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("UplayInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.GOGSettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.GOG);
                            notifications.RemoveMessage(NotificationCodes.GOGLInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to import installed GOG games.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("GOGInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.SteamSettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.Steam);
                            notifications.RemoveMessage(NotificationCodes.SteamInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to import installed Steam games.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("SteamInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.OriginSettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.Origin);
                            notifications.RemoveMessage(NotificationCodes.OriginInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to import installed Origin games.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("OriginInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    ProgressStatus = "Downloading GOG library updates...";

                    try
                    {
                        if (AppSettings.GOGSettings.IntegrationEnabled && AppSettings.GOGSettings.LibraryDownloadEnabled)
                        {
                            database.UpdateOwnedGames(Provider.GOG);
                            notifications.RemoveMessage(NotificationCodes.GOGLibDownloadError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to download GOG library updates.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("GOGLibraryImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    //ProgressStatus = "Downloading Steam library updates...";

                    //try
                    //{
                    //    if (AppSettings.SteamSettings.IntegrationEnabled && AppSettings.SteamSettings.LibraryDownloadEnabled)
                    //    {
                    //        if (AppSettings.SteamSettings.IdSource == SteamIdSource.Name)
                    //        {
                    //            database.SteamUserName = AppSettings.SteamSettings.AccountName;
                    //        }
                    //        else
                    //        {
                    //            database.SteamUserName = AppSettings.SteamSettings.AccountId.ToString();
                    //        }

                    //        database.UpdateOwnedGames(Provider.Steam);
                    //        NotificationsWin.RemoveMessage(NotificationCodes.SteamLibDownloadError);
                    //    }
                    //}
                    //catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    //{
                    //    logger.Error(e, "Failed to download Steam library updates.");
                    //    NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.SteamLibDownloadError, "Failed to download Steam library updates: " + e.Message, NotificationType.Error, () =>
                    //    {

                    //    }));
                    //}

                    //if (importSteamCatWizard && importSteamCatWizardId != 0)
                    //{
                    //    ProgressStatus = "Importing Steam categories...";

                    //    try
                    //    {
                    //        var steamLib = new SteamLibrary();
                    //        GameDatabase.Instance.ImportCategories(steamLib.GetCategorizedGames(importSteamCatWizardId));
                    //    }
                    //    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    //    {
                    //        logger.Error(e, "Failed to import Steam categories.");
                    //        NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.SteamLibDownloadError, "Failed to import Steam categories: " + e.Message, NotificationType.Error, () =>
                    //        {

                    //        }));
                    //    }
                    //}

                    ProgressStatus = "Downloading Origin library updates...";

                    try
                    {
                        if (AppSettings.OriginSettings.IntegrationEnabled && AppSettings.OriginSettings.LibraryDownloadEnabled)
                        {
                            database.UpdateOwnedGames(Provider.Origin);
                            notifications.RemoveMessage(NotificationCodes.OriginLibDownloadError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to download Origin library updates.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("OriginLibraryImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    ProgressStatus = "Downloading Battle.net library updates...";

                    try
                    {
                        if (AppSettings.BattleNetSettings.IntegrationEnabled && AppSettings.BattleNetSettings.LibraryDownloadEnabled)
                        {
                            database.UpdateOwnedGames(Provider.BattleNet);
                            notifications.RemoveMessage(NotificationCodes.BattleNetLibDownloadImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to download Battle.net library updates.");
                        notifications.AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            resources.FindString("BnetLibraryImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    ProgressStatus = "Downloading images and game details...";

                    var gamesCount = 0;
                    gamesCount = database.GamesCollection.Count(a => a.Provider != Provider.Custom && !a.IsProviderDataUpdated);
                    if (gamesCount > 0)
                    {
                        gamesCount -= 1;
                    }

                    ProgressTotal = gamesCount;

                    //var tasks = new List<Task>
                    //{
                    //        // Steam metada download thread
                    //        Task.Factory.StartNew(() =>
                    //        {
                    //            DownloadMetadata(database, Provider.Steam, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                    //        }),
                    //        // Origin metada download thread
                    //        Task.Factory.StartNew(() =>
                    //        {
                    //            DownloadMetadata(database, Provider.Origin, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                    //        }),
                    //        // GOG metada download thread
                    //        Task.Factory.StartNew(() =>
                    //        {
                    //            DownloadMetadata(database, Provider.GOG, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                    //        }),
                    //        // Uplay metada download thread
                    //        Task.Factory.StartNew(() =>
                    //        {
                    //            DownloadMetadata(database, Provider.Uplay, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                    //        }),
                    //        // BattleNet metada download thread
                    //        Task.Factory.StartNew(() =>
                    //        {
                    //            DownloadMetadata(database, Provider.BattleNet, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                    //        })
                    //};

                    //Task.WaitAll(tasks.ToArray());


                    ProgressStatus = "Library update finished";
                    Thread.Sleep(2000);
                    ProgressVisible = false;
                });

                await GamesLoaderHandler.ProgressTask;            
            }
            finally
            {
                GameAdditionAllowed = true;
            }
        }

        public void ShowWindow()
        {
            window.RestoreWindow();
        }

        public void OpenAddCustomGameWindow(IWindowFactory window)
        {
            var newGame = new Game()
            {
                Name = "New Game",
                Provider = Provider.Custom
            };

            GameDatabase.Instance.AddGame(newGame);
            if (GamesEditor.Instance.EditGame(newGame) == true)
            {
                // TODO
                //var viewEntry = GamesView.Items.First(a => a.Game.ProviderId == newGame.ProviderId);
                //switch (Settings.Instance.GamesViewType)
                //{
                //    case ViewType.List:
                //        ListGamesView.ListGames.SelectedItem = viewEntry;
                //        ListGamesView.ListGames.ScrollIntoView(viewEntry);
                //        break;

                //    case ViewType.Grid:
                //        GridGamesView.GridGames.SelectedItem = viewEntry;
                //        GridGamesView.GridGames.ScrollIntoView(viewEntry);
                //        break;
                //}
            }
            else
            {
                GameDatabase.Instance.DeleteGame(newGame);
            }
        }

        public void OpenAddInstalledGamesWindow(IWindowFactory window)
        {
            window.CreateAndOpenDialog(null);
        }

        public void OpenAddEmulatedGamesWindow(IWindowFactory window)
        {
            window.CreateAndOpenDialog(null);
        }

        public void OpenAboutWindow(IWindowFactory window)
        {            
            window.CreateAndOpenDialog(null);
        }

        public void OpenSettingsWindow(IWindowFactory window)
        {
            window.CreateAndOpenDialog(AppSettings);

            // TODO
            //if (configWindow.ProviderIntegrationChanged || configWindow.DatabaseLocationChanged)
            //{
            //    LoadGames(true);
            //}
        }

        public void OpenPlatformsWindow(IWindowFactory window)
        {
            var model = new PlatformsViewModel(GameDatabase.Instance);
            model.OpenPlatformsConfiguration();
        }

        public void SetViewType(ViewType type)
        {
            AppSettings.GamesViewType = type;
        }

        private void OnClosing(CancelEventArgs args)
        {
            if (AppSettings.CloseToTray && AppSettings.EnableTray)
            {
                Visibility = Visibility.Hidden;
                args.Cancel = true;
            }
            else
            {
                ShutdownApp();
            }
        }

        private void DownloadMetadata(GameDatabase database, Provider provider, CancellationToken token)
        {   
            var games = database.GamesCollection.Find(a => a.Provider == provider && !a.IsProviderDataUpdated).ToList();

            foreach (var game in games)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue++;

                try
                {
                    database.UpdateGameWithMetadata(game);
                }
                catch (Exception e)
                {
                    logger.Error(e, string.Format("Failed to download metadata for id:{0}, provider:{1}.", game.ProviderId, game.Provider));
                }
            }
        }
    }
}
