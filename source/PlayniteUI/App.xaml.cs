using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using CefSharp;
using Playnite;
using Playnite.Database;
using PlayniteUI.Windows;
using PlayniteUI.ViewModels;
using System.Threading.Tasks;
using Playnite.Services;
using System.Windows.Markup;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using Playnite.Metadata;
using Playnite.API;
using PlayniteUI.API;
using Playnite.Plugins;
using Playnite.Scripting;
using Playnite.App;
using Playnite.Controllers;
using Playnite.Settings;
using Playnite.SDK;
using PlayniteUI.WebView;
using Newtonsoft.Json;
using System.Windows.Interop;
using System.Reflection;
using TheArtOfDev.HtmlRenderer;
using Polly;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged, IPlayniteApplication
    {
        private static ILogger logger = LogManager.GetLogger();
        private const string instanceMuxet = "PlayniteInstaceMutex";
        private Mutex appMutex;
        private bool resourcesReleased = false;
        private PipeService pipeService;
        private PipeServer pipeServer;
        private XInputDevice xdevice;
        private DialogsFactory dialogs;
        private GameControllerFactory controllers;

        public IWindowFactory MainViewWindow
        {
            get; private set;
        }

        public System.Version CurrentVersion
        {
            get => Updater.GetCurrentVersion();
        }

        public PlayniteAPI Api
        {
            get; set;
        }

        public ExtensionFactory Extensions
        {
            get; set;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public static MainViewModel MainModel
        {
            get;
            private set;
        }

        public static FullscreenViewModel FullscreenModel
        {
            get;
            private set;
        }

        public static GameDatabase Database
        {
            get;
            private set;
        }

        public static GamesEditor GamesEditor
        {
            get;
            private set;
        }

        public static PlayniteSettings AppSettings
        {
            get;
            private set;
        }

        public static App CurrentApp
        {
            get => Current as App;
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsActive"));
            }
        }

        public App()
        {
            InitializeComponent();
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            logger.Info("Shutting down application because of session ending.");
            Quit();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ReleaseResources();
            appMutex?.ReleaseMutex();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            logger.Error(exception, "Unhandled exception occured.");

            var model = new CrashHandlerViewModel(
                CrashHandlerWindowFactory.Instance, dialogs, new DefaultResourceProvider());
            model.Exception = exception.ToString();
            model.OpenView();
            Process.GetCurrentProcess().Kill();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            logger.Info($"Application started from '{PlaynitePaths.ExecutablePath}', with '{string.Join(",", e.Args)}' arguments.");
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            // Multi-instance checking
            if (Mutex.TryOpenExisting(instanceMuxet, out var mutex))
            {
                try
                {
                    Policy.Handle<Exception>()
                        .WaitAndRetry(3, a => TimeSpan.FromSeconds(3))
                        .Execute(() =>
                        {
                            var client = new PipeClient(PlayniteSettings.GetAppConfigValue("PipeEndpoint"));
                            if (e.Args.Count() > 0 && e.Args.Contains("-command"))
                            {
                                var commandArgs = e.Args[1].Split(new char[] { ':' });
                                var command = commandArgs[0];
                                client.InvokeCommand(command, commandArgs.Count() > 1 ? commandArgs[1] : string.Empty);
                            }
                            else
                            {
                                client.InvokeCommand(CmdlineCommands.Focus, string.Empty);
                            }
                        });
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    PlayniteMessageBox.Show(
                        DefaultResourceProvider.FindString("LOCStartGenericError"),
                        DefaultResourceProvider.FindString("LOCStartupError"), MessageBoxButton.OK, MessageBoxImage.Error);
                    logger.Error(exc, "Can't process communication with other instances.");
                }

                logger.Info("Application already running, shutting down.");
                resourcesReleased = true;
                Shutdown(0);
                return;
            }
            else
            {
                var curProcess = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(curProcess.ProcessName);
                if (processes.Count() > 1 && processes.OrderBy(a => a.StartTime).First().Id != curProcess.Id)
                {
                    logger.Info("Another faster instance is already running, shutting down.");
                    resourcesReleased = true;
                    Shutdown(0);
                    return;
                }

                appMutex = new Mutex(true, instanceMuxet);
            }

            // Migrate library configuration
            PlayniteSettings.MigrateSettingsConfig();

            Time.Instance = new Time();
            AppSettings = PlayniteSettings.LoadSettings();
            HtmlRendererSettings.ImageCachePath = PlaynitePaths.ImagesCachePath;

            try
            {
                Localization.SetLanguage(AppSettings.Language);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, $"Failed to set {AppSettings.Language} langauge.");
            }

            Resources.Remove("AsyncImagesEnabled");
            Resources.Add("AsyncImagesEnabled", AppSettings.AsyncImageLoading);
            if (AppSettings.DisableHwAcceleration)
            {
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            }

            if (AppSettings.DisableDpiAwareness)
            {
                try
                {
                    DisableDpiAwareness();
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to disable DPI awarness.");
                }
            }

            try
            {
                CefTools.ConfigureCef();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to initialize CefSharp.");
                PlayniteMessageBox.Show(
                    DefaultResourceProvider.FindString("LOCCefSharpInitError"),
                    DefaultResourceProvider.FindString("LOCStartupError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Quit();
                return;
            }

            dialogs = new DialogsFactory(AppSettings.StartInFullscreen);            

            // Create directories
            try
            {
                ExtensionFactory.CreatePluginFolders();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to script and plugin directories.");
            }

            // Initialize API
            Database = new GameDatabase();
            controllers = new GameControllerFactory(Database);
            Api = new PlayniteAPI(
                new DatabaseAPI(Database),
                dialogs,
                null,
                new PlayniteInfoAPI(),
                new PlaynitePathsAPI(),
                new WebViewFactory(),
                new DefaultResourceProvider(),
                new NotificationsAPI());
            Extensions = new ExtensionFactory(Database, controllers);

            // Load theme
            ApplyTheme(AppSettings.Skin, AppSettings.SkinColor, false);            

            // First run wizard
            bool isFirstStart = !AppSettings.FirstTimeWizardComplete;
            bool existingDb = false;
            if (!AppSettings.FirstTimeWizardComplete)
            {
                AppSettings.DatabasePath = GameDatabase.GetDefaultPath(PlayniteSettings.IsPortable);
                existingDb = Directory.Exists(AppSettings.DatabasePath);
                AppSettings.SaveSettings();
                Database.SetDatabasePath(AppSettings.DatabasePath);
                Database.OpenDatabase();

                var wizardWindow = FirstTimeStartupWindowFactory.Instance;
                var wizardModel = new FirstTimeStartupViewModel(wizardWindow, dialogs, new DefaultResourceProvider(), Extensions, Api);
                if (wizardModel.OpenView() == true)
                {
                    var settings = wizardModel.Settings;
                    AppSettings.FirstTimeWizardComplete = true;
                    AppSettings.DisabledPlugins = settings.DisabledPlugins;
                    AppSettings.SaveSettings();
                    if (wizardModel.ImportedGames?.Any() == true)
                    {
                        InstalledGamesViewModel.AddImportableGamesToDb(wizardModel.ImportedGames, Database);
                    }
                }
                else
                {
                    AppSettings.FirstTimeWizardComplete = true;
                    AppSettings.SaveSettings();
                }

                // Emulator wizard
                var model = new EmulatorImportViewModel(Database,
                   EmulatorImportViewModel.DialogType.Wizard,
                   EmulatorImportWindowFactory.Instance,
                   dialogs,
                   new DefaultResourceProvider());
                model.OpenView();
            }
            else
            {
                // This shouldn't even happen, but in case that settings file gets damanged set default path.
                if (string.IsNullOrEmpty(AppSettings.DatabasePath))
                {
                    AppSettings.DatabasePath = GameDatabase.GetDefaultPath(PlayniteSettings.IsPortable);
                }

                Database.SetDatabasePath(AppSettings.DatabasePath);
            }

            Extensions.LoadLibraryPlugins(Api, AppSettings.DisabledPlugins);
            Extensions.LoadGenericPlugins(Api, AppSettings.DisabledPlugins);
            Extensions.LoadScripts(Api, AppSettings.DisabledPlugins);
            GamesEditor = new GamesEditor(Database, controllers, AppSettings, dialogs, Extensions);
            CustomImageStringToImageConverter.SetDatabase(Database);

            // Main view startup
            if (AppSettings.StartInFullscreen)
            {
                OpenFullscreenView(true);
            }
            else
            {
                OpenNormalView(isFirstStart, existingDb);
            }

            // Update and stats
            if (!PlayniteEnvironment.InOfflineMode)
            {
                CheckUpdate();
                SendUsageData();
            }

            // Pipe server
            try
            {
                pipeService = new PipeService();
                pipeService.CommandExecuted += PipeService_CommandExecuted;
                pipeServer = new PipeServer(PlayniteSettings.GetAppConfigValue("PipeEndpoint"));
                pipeServer.StartServer(pipeService);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to start pipe service.");
            }

            var args = Environment.GetCommandLineArgs();
            if (args.Count() > 0 && args.Contains("-command"))
            {
                var commandArgs = args[2].Split(new char[] { ':' });
                var command = commandArgs[0];
                var cmdArgs = commandArgs.Count() > 1 ? commandArgs[1] : string.Empty;
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(command, cmdArgs));
            }

            // Initialize XInput
            xdevice = new XInputDevice(InputManager.Current)
            {
                SimulateAllKeys = true,
                SimulateNavigationKeys = true
            };

            // Fix bootup startup
            try
            {
                PlayniteSettings.SetBootupStateRegistration(AppSettings.StartOnBoot);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to register Playnite to start on boot.");
            }

            logger.Info($"Application {CurrentVersion} started");
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info($"Executing command \"{args.Command}\" from pipe with arguments \"{args.Args}\"");

            switch (args.Command)
            {
                case CmdlineCommands.Focus:                    
                    MainModel?.RestoreWindow();
                    FullscreenModel?.RestoreWindow();
                    break;

                case CmdlineCommands.Launch:
                    if (Guid.TryParse(args.Args, out var gameId))
                    {
                        var game = Database.Games[gameId];
                        if (game == null)
                        {
                            logger.Error($"Cannot start game, game {args.Args} not found.");
                        }
                        else
                        {
                            GamesEditor.PlayGame(game);
                        }
                    }
                    else
                    {
                        logger.Error($"Can't start game, failed to parse game id: {args.Args}");
                    }

                    break;

                default:
                    logger.Warn("Unknown command received");
                    break;
            }
        }

        private async void CheckUpdate()
        {
            await Task.Delay(Playnite.Timer.SecondsToMilliseconds(10));
            if (GlobalTaskHandler.IsActive)
            {
                await GlobalTaskHandler.ProgressTask;
            }

            var updater = new Updater(this);

            while (true)
            {
                if (!UpdateViewModel.InstanceInUse)
                {
                    try
                    {
                        if (updater.IsUpdateAvailable)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var model = new UpdateViewModel(updater, UpdateWindowFactory.Instance, new DefaultResourceProvider(), dialogs);
                                model.OpenView();
                            });
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.Warn(exc, "Failed to process update.");
                    }
                }

                await Task.Delay(Playnite.Timer.HoursToMilliseconds(4));
            }
        }

        private async void SendUsageData()
        {
            await Task.Run(() =>
            {
                try
                {
                    var client = new ServicesClient();
                    client.PostUserUsage(AppSettings.InstallInstanceId);
                }
                catch (Exception exc)
                {
                    logger.Warn(exc, "Failed to post user usage data.");
                }
            });
        }

        public void Restart()
        {
            ReleaseResources();
            Process.Start(PlaynitePaths.ExecutablePath);
            Shutdown(0);
        }

        public void Quit()
        {
            logger.Info("Shutting down Playnite");
            ReleaseResources();
            Shutdown(0);
        }

        private void ReleaseResources()
        {
            if (resourcesReleased)
            {
                return;
            }

            logger.Debug("Releasing Playnite resources...");
            var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), () =>
            {
                try
                {
                    GlobalTaskHandler.CancelAndWait();                    
                    GamesEditor?.Dispose();
                    AppSettings?.SaveSettings();
                    Extensions?.Dispose();
                    controllers?.Dispose();
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to dispose Playnite objects.");
                }
            }, DefaultResourceProvider.FindString("LOCClosingPlaynite"));

            progressModel.ActivateProgress();

            // These must run on main thread
            if (CefTools.IsInitialized)
            {
                CefTools.Shutdown();
            }

            resourcesReleased = true;
        }

        public async void OpenNormalView(bool isFirstStart, bool existingDb)
        {
            logger.Debug("Opening Desktop view");
            if (Database.IsOpen)
            {
                FullscreenModel = null;
            }

            GamesEditor.IsFullscreen = false;
            dialogs.IsFullscreen = false;
            ApplyTheme(AppSettings.Skin, AppSettings.SkinColor, false);
            MainViewWindow = new MainWindowFactory();
            MainModel = new MainViewModel(
                Database,
                MainViewWindow,
                dialogs,
                new DefaultResourceProvider(),
                AppSettings,
                GamesEditor,
                Api,
                Extensions);
            Api.MainView = new MainViewAPI(MainModel);
            MainModel.OpenView();
            Current.MainWindow = MainViewWindow.Window;
            if (AppSettings.UpdateLibStartup)
            {
                await MainModel.UpdateDatabase(AppSettings.UpdateLibStartup, !isFirstStart);
            }

            if (isFirstStart && !existingDb)
            {
                var metaSettings = new MetadataDownloaderSettings();
                metaSettings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                metaSettings.CoverImage.Source = MetadataSource.IGDBOverStore;
                metaSettings.Name = new MetadataFieldSettings(true, MetadataSource.Store);
                await MainModel.DownloadMetadata(metaSettings);
            }
        }

        public async void OpenFullscreenView(bool updateDb)
        {
            logger.Debug("Opening Fullscreen view");
            if (Database.IsOpen)
            {
                MainModel = null;
            }

            GamesEditor.IsFullscreen = true;
            dialogs.IsFullscreen = true;
            ApplyTheme(AppSettings.SkinFullscreen, AppSettings.SkinColorFullscreen, true);
            MainViewWindow = new FullscreenWindowFactory();
            FullscreenModel = new FullscreenViewModel(
                Database,
                MainViewWindow,
                dialogs,
                new DefaultResourceProvider(),
                AppSettings,
                GamesEditor,
                Api,
                Extensions);
            Api.MainView = new MainViewAPI(MainModel);
            FullscreenModel.OpenView(!PlayniteEnvironment.IsDebugBuild);
            Current.MainWindow = MainViewWindow.Window;

            if (updateDb)
            {
                await FullscreenModel.UpdateDatabase(AppSettings.UpdateLibStartup, true);
            }            
        }

        private void ApplyTheme(string name, string profile, bool fullscreen)
        {
            bool isThemeValid = true;
            string themeName = "Classic";
            string themeProfile = "Default";

            if (fullscreen)
            {
                if (Themes.CurrentFullscreenTheme == name && Themes.CurrentFullscreenColor == profile)
                {
                    return;
                }
            }
            else
            {
                if (Themes.CurrentTheme == name && Themes.CurrentColor == profile)
                {
                    return;
                }
            }

            var themeValid = Themes.IsThemeValid(name, fullscreen);
            if (themeValid.Item1 == false)
            {
                PlayniteMessageBox.Show(
                    string.Format(DefaultResourceProvider.FindString("LOCSkinApplyError"), AppSettings.Skin, AppSettings.SkinColor, themeValid.Item2),
                    DefaultResourceProvider.FindString("LOCSkinError"), MessageBoxButton.OK, MessageBoxImage.Error);
                isThemeValid = false;
            }

            var profileValid = Themes.IsColorProfileValid(name, profile, fullscreen);
            if (profileValid.Item1 == false)
            {
                PlayniteMessageBox.Show(
                    string.Format(DefaultResourceProvider.FindString("LOCSkinApplyError"), AppSettings.Skin, AppSettings.SkinColor, profileValid.Item2),
                    DefaultResourceProvider.FindString("LOCSkinError"), MessageBoxButton.OK, MessageBoxImage.Error);
                isThemeValid = false;
            }

            if (isThemeValid)
            {
                themeName = name;
                themeProfile = profile;
            }

            logger.Debug($"Applying theme {themeName}, {themeProfile}, {fullscreen}");

            if (fullscreen)
            {
                Themes.ApplyFullscreenTheme(themeName, themeProfile, true);
            }
            else
            {
                Themes.ApplyTheme(themeName, themeProfile, true);
            }
        }

        private void Application_Activated(object sender, EventArgs e)
        {
            IsActive = true;
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            IsActive = false;
        }

        private void DisableDpiAwareness()
        {
            // https://stackoverflow.com/questions/13858665/disable-dpi-awareness-for-wpf-application
            var setDpiHwnd = typeof(HwndTarget).GetField("_setDpi", BindingFlags.Static | BindingFlags.NonPublic);
            setDpiHwnd?.SetValue(null, false);
            var setProcessDpiAwareness = typeof(HwndTarget).GetProperty("ProcessDpiAwareness", BindingFlags.Static | BindingFlags.NonPublic);
            setProcessDpiAwareness?.SetValue(null, 1, null);
            var setDpi = typeof(UIElement).GetField("_setDpi", BindingFlags.Static | BindingFlags.NonPublic);
            setDpi?.SetValue(null, false);
            var setDpiXValues = (List<double>)typeof(UIElement).GetField("DpiScaleXValues", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
            setDpiXValues?.Insert(0, 1);
            var setDpiYValues = (List<double>)typeof(UIElement).GetField("DpiScaleYValues", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
            setDpiYValues?.Insert(0, 1);
        }
    }
}
