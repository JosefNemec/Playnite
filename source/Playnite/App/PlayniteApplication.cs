using Playnite.Controllers;
using Playnite.Input;
using Playnite.SDK;
using Playnite.Plugins;
using Playnite.Settings;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using Playnite.Database;
using Playnite.API;
using TheArtOfDev.HtmlRenderer;
using Playnite.SDK.Models;
using Playnite.WebView;
using Playnite.Services;
using System.Windows.Input;
using System.Windows.Interop;
using System.Reflection;
using System.IO;
using Playnite.Common;
using System.ComponentModel;
using Playnite.Windows;
using Polly;

namespace Playnite
{
    public enum ApplicationMode
    {
        [Description("Desktop")]
        Desktop,
        [Description("Fullscreen")]
        Fullscreen
    }

    public abstract class PlayniteApplication : ObservableObject, IPlayniteApplication
    {
        private ILogger logger = LogManager.GetLogger();
        private const string instanceMuxet = "PlayniteInstaceMutex";
        private Mutex appMutex;
        private bool resourcesReleased = false;
        private PipeService pipeService;
        private PipeServer pipeServer;
        private XInputDevice xdevice;

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                OnPropertyChanged();
            }
        }

        public System.Version CurrentVersion
        {
            get => Updater.GetCurrentVersion();
        }

        public ApplicationMode Mode { get; }
        public IDialogsFactory Dialogs { get; set; }
        public PlayniteSettings AppSettings { get; set; }
        public GamesEditor GamesEditor { get; set; }
        public ExtensionFactory Extensions { get; set; }
        public GameDatabase Database { get; set; }
        public PlayniteAPI Api { get; set; }
        public GameControllerFactory Controllers { get; set; }

        public static Application CurrentNative { get; private set; }
        public static PlayniteApplication Current { get; private set; }

        public PlayniteApplication(Application nativeApp, ApplicationMode mode, string defaultThemeName)
        {
            if (Current != null)
            {
                throw new Exception("Only one application instance is allowed.");
            }

            Mode = mode;
            Current = this;
            CurrentNative = nativeApp;
            CurrentNative.SessionEnding += Application_SessionEnding;
            CurrentNative.Exit += Application_Exit;
            CurrentNative.Startup += Application_Startup;
            CurrentNative.Activated += Application_Activated;
            CurrentNative.Deactivated += Application_Deactivated;
            CurrentNative.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            PlayniteSettings.MigrateSettingsConfig();
            AppSettings = PlayniteSettings.LoadSettings();
            OnPropertyChanged(nameof(AppSettings));

            var defaultTheme = new ThemeDescription()
            {
                DirectoryName = defaultThemeName,
                DirectoryPath = Path.Combine(PlaynitePaths.ThemesProgramPath, ThemeManager.GetThemeRootDir(Mode), defaultThemeName),
                Name = defaultThemeName
            };
            ThemeManager.SetDefaultTheme(defaultTheme);

            // Theme must be set BEFORE default app resources are initialized for ThemeFile markup to apply custom theme's paths.
            ThemeDescription customTheme = null;
            if (AppSettings.Theme != ThemeManager.DefaultTheme.Name)
            {
                customTheme = ThemeManager.GetAvailableThemes(mode).SingleOrDefault(a => a.DirectoryName == AppSettings.Theme);
                if (customTheme == null)
                {
                    logger.Error($"Failed to apply theme {AppSettings.Theme}, theme not found.");
                    ThemeManager.SetCurrentTheme(defaultTheme);
                }
                else
                {
                    ThemeManager.SetCurrentTheme(customTheme);
                }
            }

            InitializeNative();

            // Must be applied AFTER default app resources are initialized, otherwise custom resource dictionaries won't be properly added to application scope.
            if (customTheme != null)
            {
                ThemeManager.ApplyTheme(CurrentNative, customTheme, Mode);
            }
        }

        public abstract void InstantiateApp();

        public abstract void InitializeNative();

        public abstract void Restore();

        public abstract void Minimize();

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
                new CrashHandlerWindowFactory(),
                Dialogs,
                new ResourceProvider());
            model.Exception = exception.ToString();
            model.OpenView();
            Process.GetCurrentProcess().Kill();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            logger.Info($"Application started from '{PlaynitePaths.ExecutablePath}', with '{string.Join(",", e.Args)}' arguments.");
            Startup();
            logger.Info($"Application {CurrentVersion} started");
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info($"Executing command \"{args.Command}\" from pipe with arguments \"{args.Args}\"");

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                    Restore();
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

        private void Application_Activated(object sender, EventArgs e)
        {
            IsActive = true;
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            IsActive = false;
        }

        public void Run()
        {
            CurrentNative.Run();
        }

        public abstract void Startup();

        public bool CheckOtherInstances()
        {
            if (Mutex.TryOpenExisting(instanceMuxet, out var mutex))
            {
                try
                {
                    Policy.Handle<Exception>()
                        .WaitAndRetry(3, a => TimeSpan.FromSeconds(3))
                        .Execute(() =>
                        {
                            var args = Environment.GetCommandLineArgs();
                            var client = new PipeClient(PlayniteSettings.GetAppConfigValue("PipeEndpoint"));
                            if (args.Count() > 0 && args.Contains("-command"))
                            {
                                var commandArgs = args[1].Split(new char[] { ':' });
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
                    Dialogs.ShowErrorMessage(
                        ResourceProvider.GetString("LOCStartGenericError"),
                        ResourceProvider.GetString("LOCStartupError"));
                    logger.Error(exc, "Can't process communication with other instances.");
                }

                logger.Info("Application already running, shutting down.");
                resourcesReleased = true;
                CurrentNative.Shutdown(0);
                return true;
            }
            else
            {
                var curProcess = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(curProcess.ProcessName);
                if (processes.Count() > 1 && processes.OrderBy(a => a.StartTime).First().Id != curProcess.Id)
                {
                    logger.Info("Another faster instance is already running, shutting down.");
                    resourcesReleased = true;
                    CurrentNative.Shutdown(0);
                    return true;
                }

                appMutex = new Mutex(true, instanceMuxet);

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
            }

            return false;
        }

        public void ConfigureApplication()
        {
            try
            {
                Localization.SetLanguage(AppSettings.Language);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, $"Failed to set {AppSettings.Language} langauge.");
            }

            HtmlRendererSettings.ImageCachePath = PlaynitePaths.ImagesCachePath;
            if (AppSettings.DisableHwAcceleration)
            {
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            }

            try
            {
                CefTools.ConfigureCef();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to initialize CefSharp.");
                Dialogs.ShowErrorMessage(
                    ResourceProvider.GetString("LOCCefSharpInitError"),
                    ResourceProvider.GetString("LOCStartupError"));
                Quit();
                return;
            }

            try
            {
                ExtensionFactory.CreatePluginFolders();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to script and plugin directories.");
            }

            try
            {
                PlayniteSettings.SetBootupStateRegistration(AppSettings.StartOnBoot);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to register Playnite to start on boot.");
            }
        }

        public void ProcessArguments()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Count() > 0 && args.Contains("-command"))
            {
                var commandArgs = args[2].Split(new char[] { ':' });
                var command = commandArgs[0];
                var cmdArgs = commandArgs.Count() > 1 ? commandArgs[1] : string.Empty;
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(command, cmdArgs));
            }
        }

        public void SetupInputs()
        {
            if (AppSettings.EnableControllerInDesktop)
            {
                xdevice = new XInputDevice(InputManager.Current, this)
                {
                    SimulateAllKeys = true,
                    SimulateNavigationKeys = true
                };
            }
        }

        public void Quit()
        {
            logger.Info("Shutting down Playnite");
            ReleaseResources();
            CurrentNative.Shutdown(0);
        }

        public void Restart()
        {
            ReleaseResources();
            Process.Start(PlaynitePaths.ExecutablePath);
            CurrentNative.Shutdown(0);
        }        
        
        public virtual void ReleaseResources()
        {
            logger.Debug("Releasing Playnite resources...");
            if (resourcesReleased)
            {
                return;
            }

            var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), () =>
            {
                try
                {
                    GlobalTaskHandler.CancelAndWait();                    
                    GamesEditor?.Dispose();
                    AppSettings?.SaveSettings();
                    Extensions?.Dispose();
                    Controllers?.Dispose();
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to dispose Playnite objects.");
                }
            }, ResourceProvider.GetString("LOCClosingPlaynite"));

            progressModel.ActivateProgress();

            // This must run on main thread
            CurrentNative.Dispatcher.Invoke(() =>
            {
                if (CefTools.IsInitialized)
                {
                    CefTools.Shutdown();
                }
            });

            resourcesReleased = true;
        }

        public async void StartUpdateCheckerAsync()
        {
            if (PlayniteEnvironment.InOfflineMode)
            {
                return;
            }

            await Task.Delay(Common.Timer.SecondsToMilliseconds(10));
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
                            CurrentNative.Dispatcher.Invoke(() =>
                            {
                                var model = new UpdateViewModel(
                                    updater,
                                    new UpdateWindowFactory(),
                                    new ResourceProvider(),
                                    Dialogs);
                                model.OpenView();
                            });
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.Warn(exc, "Failed to process update.");
                    }
                }

                await Task.Delay(Common.Timer.HoursToMilliseconds(4));
            }
        }

        public async void SendUsageDataAsync()
        {
            if (PlayniteEnvironment.InOfflineMode)
            {
                return;
            }

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

        public void DisableDpiAwareness()
        {
            try
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
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to disable DPI awarness.");
            }
        }

        public bool MigrateDatabase()
        {
            if (GameDatabase.GetMigrationRequired(AppSettings.DatabasePath))
            {
                var migrationProgress = new ProgressViewViewModel(new ProgressWindowFactory(),
                () =>
                {
                    if (AppSettings.DatabasePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                    {
                        var newDbPath = GameDatabase.GetMigratedDbPath(AppSettings.DatabasePath);
                        var newResolvedDbPath = GameDatabase.GetFullDbPath(newDbPath);
                        if (Directory.Exists(newResolvedDbPath))
                        {
                            newDbPath += "_db";
                            newResolvedDbPath += "_db";
                        }

                        if (!File.Exists(AppSettings.DatabasePath))
                        {
                            AppSettings.DatabasePath = newDbPath;
                        }
                        else
                        {
                            var dbSize = new FileInfo(AppSettings.DatabasePath).Length;
                            if (FileSystem.GetFreeSpace(newResolvedDbPath) < dbSize)
                            {
                                throw new NoDiskSpaceException(dbSize);
                            }

                            GameDatabase.MigrateOldDatabaseFormat(AppSettings.DatabasePath);
                            GameDatabase.MigrateToNewFormat(AppSettings.DatabasePath, newResolvedDbPath);
                            FileSystem.DeleteFile(AppSettings.DatabasePath);
                            AppSettings.DatabasePath = newDbPath;
                        }
                    }
                    else
                    {
                        GameDatabase.MigrateNewDatabaseFormat(GameDatabase.GetFullDbPath(AppSettings.DatabasePath));
                    }
                }, ResourceProvider.GetString("LOCDBUpgradeProgress"));

                if (migrationProgress.ActivateProgress() != true)
                {
                    logger.Error(migrationProgress.FailException, "Failed to migrate database to new version.");
                    var message = ResourceProvider.GetString("LOCDBUpgradeFail");
                    if (migrationProgress.FailException is NoDiskSpaceException exc)
                    {
                        message = string.Format(ResourceProvider.GetString("LOCDBUpgradeEmptySpaceFail"), Units.BytesToMegaBytes(exc.RequiredSpace));
                    }

                    Dialogs.ShowErrorMessage(message, "");
                    return false;
                }
            }

            return true;
        }
    }
}
