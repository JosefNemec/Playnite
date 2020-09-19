using Playnite.Controllers;
using Playnite.Input;
using Playnite.SDK;
using Playnite.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using Playnite.Database;
using Playnite.API;
using TheArtOfDev.HtmlRenderer;
using Playnite.Services;
using System.Windows.Input;
using System.Windows.Interop;
using System.Reflection;
using System.IO;
using Playnite.Common;
using System.ComponentModel;
using Playnite.Windows;
using Polly;
using System.Windows.Media;
using Playnite.SDK.Events;
using System.Windows.Threading;

namespace Playnite
{
    public abstract class PlayniteApplication : ObservableObject, IPlayniteApplication
    {
        private ILogger logger = LogManager.GetLogger();
        private const string instanceMuxet = "PlayniteInstaceMutex";
        private Mutex appMutex;
        private bool resourcesReleased = false;
        private PipeService pipeService;
        private PipeServer pipeServer;
        private XInputDevice xdevice;
        private System.Threading.Timer updateCheckTimer;

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
        public CmdLineOptions CmdLine { get; set; }
        public DpiScale DpiScale { get; set; } = new DpiScale(1, 1);
        public ComputerScreen CurrentScreen { get; set; } = Computer.GetPrimaryScreen();
        public DiscordManager Discord { get; set; }
        public SynchronizationContext SyncContext { get; private set; }

        public static Application CurrentNative { get; private set; }
        public static PlayniteApplication Current { get; private set; }

        public PlayniteApplication(
            Application nativeApp,
            ApplicationMode mode,
            string defaultThemeName,
            CmdLineOptions cmdLine)
        {
            if (Current != null)
            {
                throw new Exception("Only one application instance is allowed.");
            }

            SyncContext = new DispatcherSynchronizationContext(nativeApp.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            CmdLine = cmdLine;
            Mode = mode;
            Current = this;
            CurrentNative = nativeApp;
            CurrentNative.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            if (CheckOtherInstances() || CmdLine.Shutdown)
            {
                resourcesReleased = true;
                CurrentNative.Shutdown(0);
                return;
            }

            PlayniteSettings.MigrateSettingsConfig();
            AppSettings = PlayniteSettings.LoadSettings();

            var relaunchPath = string.Empty;
            if (AppSettings.StartInFullscreen && mode == ApplicationMode.Desktop && !CmdLine.StartInDesktop)
            {
                relaunchPath = PlaynitePaths.FullscreenExecutablePath;
            }

            if (CmdLine.StartInDesktop && mode != ApplicationMode.Desktop)
            {
                relaunchPath = PlaynitePaths.DesktopExecutablePath;
            }
            else if (CmdLine.StartInFullscreen && mode != ApplicationMode.Fullscreen)
            {
                relaunchPath = PlaynitePaths.FullscreenExecutablePath;
            }

            if (!relaunchPath.IsNullOrEmpty())
            {
                ProcessStarter.StartProcess(relaunchPath, CmdLine.ToString());
                CurrentNative.Shutdown(0);
                return;
            }

            CurrentNative.SessionEnding += Application_SessionEnding;
            CurrentNative.Exit += Application_Exit;
            CurrentNative.Startup += Application_Startup;
            CurrentNative.Activated += Application_Activated;
            CurrentNative.Deactivated += Application_Deactivated;

            OnPropertyChanged(nameof(AppSettings));
            var defaultTheme = new ThemeManifest()
            {
                DirectoryName = defaultThemeName,
                DirectoryPath = Path.Combine(PlaynitePaths.ThemesProgramPath, ThemeManager.GetThemeRootDir(Mode), defaultThemeName),
                Name = defaultThemeName
            };

            try
            {
                var installed = ExtensionInstaller.InstallExtensionQueue();
                var installedTheme = installed.FirstOrDefault(a => a is ThemeManifest);
                if (installedTheme != null)
                {
                    var theme = installedTheme as ThemeManifest;
                    if (theme.Mode == Mode)
                    {
                        if (theme.Mode == ApplicationMode.Desktop)
                        {
                            AppSettings.Theme = theme.DirectoryName;
                        }
                        else
                        {
                            AppSettings.Fullscreen.Theme = theme.DirectoryName;
                        }
                    }
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to finish installing extenions.");
            }

            ThemeManager.SetDefaultTheme(defaultTheme);

            // Theme must be set BEFORE default app resources are initialized for ThemeFile markup to apply custom theme's paths.
            ThemeManifest customTheme = null;
            if (CmdLine.ForceDefaultTheme || CmdLine.SafeStartup)
            {
                logger.Warn("Default theme forced by cmdline.");
            }
            else
            {
                var theme = mode == ApplicationMode.Desktop ? AppSettings.Theme : AppSettings.Fullscreen.Theme;
                if (theme != ThemeManager.DefaultTheme.Name)
                {
                    customTheme = ThemeManager.GetAvailableThemes(mode).SingleOrDefault(a => a.DirectoryName == theme);
                    if (customTheme == null)
                    {
                        logger.Error($"Failed to apply theme {theme}, theme not found.");
                        if (mode == ApplicationMode.Desktop)
                        {
                            AppSettings.Theme = "Default";
                        }
                        else
                        {
                            AppSettings.Fullscreen.Theme = "Default";
                        }

                        ThemeManager.SetCurrentTheme(defaultTheme);
                    }
                    else
                    {
                        ThemeManager.SetCurrentTheme(customTheme);
                    }
                }
            }

            InitializeNative();

            // Must be applied AFTER default app resources are initialized, otherwise custom resource dictionaries won't be properly added to application scope.
            if (customTheme != null)
            {
                if (!ThemeManager.ApplyTheme(CurrentNative, customTheme, Mode))
                {
                    ThemeManager.SetCurrentTheme(null);
                    logger.Error($"Failed to load theme {customTheme.Name}.");
                }
            }

            try
            {
                Localization.SetLanguage(AppSettings.Language);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, $"Failed to set {AppSettings.Language} langauge.");
            }

            if (mode == ApplicationMode.Desktop)
            {
                try
                {
                    if (System.Drawing.FontFamily.Families.Any(a => a.Name == AppSettings.FontFamilyName))
                    {
                        CurrentNative.Resources.Add(
                            "FontFamily", new FontFamily(AppSettings.FontFamilyName));
                    }
                    else
                    {
                        logger.Error($"Cannot set font {AppSettings.FontFamilyName}, font not found.");
                    }

                    if (AppSettings.FontSize > 0)
                    {
                        CurrentNative.Resources.Add(
                            "FontSize", AppSettings.FontSize);
                    }

                    if (AppSettings.FontSizeSmall > 0)
                    {
                        CurrentNative.Resources.Add(
                            "FontSizeSmall", AppSettings.FontSizeSmall);
                    }

                    if (AppSettings.FontSizeLarge > 0)
                    {
                        CurrentNative.Resources.Add(
                            "FontSizeLarge", AppSettings.FontSizeLarge);
                    }

                    if (AppSettings.FontSizeLarger > 0)
                    {
                        CurrentNative.Resources.Add(
                            "FontSizeLarger", AppSettings.FontSizeLarger);
                    }

                    if (AppSettings.FontSizeLargest > 0)
                    {
                        CurrentNative.Resources.Add(
                            "FontSizeLargest", AppSettings.FontSizeLargest);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to set font {AppSettings.FontFamilyName}");
                }
            }

            // Only use this for Desktop mode. Non-default options look terrible in Fullscreen because of viewport scaling.
            if (mode == ApplicationMode.Desktop)
            {
                Controls.WindowBase.SetTextRenderingOptions(AppSettings.TextFormattingMode, AppSettings.TextRenderingMode);
            }
        }

        public abstract void InstantiateApp();

        public abstract void InitializeNative();

        public abstract void Restore();

        public abstract void Minimize();

        public abstract void ShowWindowsNotification(string title, string body, Action action);

        public abstract void SwitchAppMode(ApplicationMode mode);

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            logger.Info("Shutting down application because of session ending.");
            // Don't dispose CefSharp here because of bug in CefSharp during system shutdown
            // https://github.com/JosefNemec/Playnite/issues/866
            ReleaseResources(false);
            CurrentNative.Shutdown(0);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ReleaseResources();
            appMutex?.ReleaseMutex();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            var crashInfo = Exceptions.GetExceptionInfo(exception, Extensions);
            logger.Error(exception, "Unhandled exception occured.");
            CrashHandlerViewModel crashModel = null;
            if (crashInfo.IsExtensionCrash)
            {
                crashModel = new CrashHandlerViewModel(
                    new ExtensionCrashHandlerWindowFactory(),
                    Dialogs,
                    new ResourceProvider(),
                    Mode,
                    crashInfo,
                    AppSettings);
            }
            else
            {
                crashModel = new CrashHandlerViewModel(
                    new CrashHandlerWindowFactory(),
                    Dialogs,
                    new ResourceProvider(),
                    Mode);
            }

            crashModel.OpenView();
            Process.GetCurrentProcess().Kill();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            logger.Info($"Application started from '{PlaynitePaths.ProgramPath}', with '{string.Join(",", e.Args)}' arguments.");
            SDK.Data.Markup.Init(new MarkupConverter());
            Startup();
            logger.Info($"Application {CurrentVersion} started");
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info($"Executing command \"{args.Command}\" from pipe with arguments \"{args.Args}\"");

            switch (args.Command)
            {
                case CmdlineCommand.Focus:
                    Restore();
                    break;

                case CmdlineCommand.Start:
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

                case CmdlineCommand.UriRequest:
                    (Api.UriHandler as PlayniteUriHandler).ProcessUri(args.Args);
                    break;

                case CmdlineCommand.ExtensionInstall:
                    var extPath = args.Args;
                    if (!File.Exists(extPath))
                    {
                        logger.Error($"Cannot install extension, file doesn't exists: {extPath}");
                        return;
                    }

                    var ext = Path.GetExtension(extPath).ToLower();
                    if (ext.Equals(PlaynitePaths.PackedThemeFileExtention, StringComparison.OrdinalIgnoreCase))
                    {
                        InstallThemeFile(extPath);
                    }
                    else if (ext.Equals(PlaynitePaths.PackedExtensionFileExtention, StringComparison.OrdinalIgnoreCase))
                    {
                        InstallExtensionFile(extPath);
                    }

                    break;

                case CmdlineCommand.SwitchMode:
                    if (args.Args == "desktop")
                    {
                        SyncContext.Post(_ => SwitchAppMode(ApplicationMode.Desktop), null);
                    }
                    else if (args.Args == "fullscreen")
                    {
                        SyncContext.Post(_ => SwitchAppMode(ApplicationMode.Fullscreen), null);
                    }
                    else
                    {
                        logger.Error($"Can't switch to uknwon application mode: {args.Args}");
                    }
                    break;

                case CmdlineCommand.Shutdown:
                    Quit();
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
                            var client = new PipeClient(PlayniteSettings.GetAppConfigValue("PipeEndpoint"));
                            if (!CmdLine.Start.IsNullOrEmpty())
                            {
                                client.InvokeCommand(CmdlineCommand.Start, CmdLine.Start);
                            }
                            else if (!CmdLine.UriData.IsNullOrEmpty())
                            {
                                client.InvokeCommand(CmdlineCommand.UriRequest, CmdLine.UriData);
                            }
                            else if (!CmdLine.InstallExtension.IsNullOrEmpty())
                            {
                                client.InvokeCommand(CmdlineCommand.ExtensionInstall, CmdLine.InstallExtension);
                            }
                            else if (CmdLine.StartInDesktop)
                            {
                                client.InvokeCommand(CmdlineCommand.SwitchMode, "desktop");
                            }
                            else if (CmdLine.StartInFullscreen)
                            {
                                client.InvokeCommand(CmdlineCommand.SwitchMode, "fullscreen");
                            }
                            else if (CmdLine.Shutdown)
                            {
                                client.InvokeCommand(CmdlineCommand.Shutdown, null);
                            }
                            else
                            {
                                client.InvokeCommand(CmdlineCommand.Focus, string.Empty);
                            }
                        });
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    MessageBox.Show(
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
                var processes = Process.GetProcesses().Where(a => a.ProcessName.StartsWith("Playnite.")).ToList();
                // In case multiple processes end up in this branch,
                // the process with highest process id gets to live.
                if (processes.Count > 1 && processes.Max(a => a.Id) != curProcess.Id)
                {
                    logger.Info("Another process instance(s) is already running, shutting down.");
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
            HtmlRendererSettings.ImageCachePath = PlaynitePaths.ImagesCachePath;
            if (AppSettings.DisableHwAcceleration || CmdLine.ForceSoftwareRender)
            {
                logger.Info("Enabling software rendering.");
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            }

            if (CmdLine.ClearWebCache)
            {
                try
                {
                    FileSystem.DeleteDirectory(PlaynitePaths.BrowserCachePath);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to clear CEF cache.");
                }
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
                SystemIntegration.SetBootupStateRegistration(AppSettings.StartOnBoot);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to register Playnite to start on boot.");
            }

            try
            {
                SystemIntegration.RegisterPlayniteUriProtocol();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to register playnite URI scheme.");
            }

            try
            {
                SystemIntegration.RegisterFileExtensions();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to register playnite extensions.");
            }
        }

        public void ProcessArguments()
        {
            (Api.UriHandler as PlayniteUriHandler).Handlers.Add("playnite", ProcessUriRequest);
            if (!CmdLine.Start.IsNullOrEmpty())
            {
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommand.Start, CmdLine.Start));
            }
            else if (!CmdLine.UriData.IsNullOrEmpty())
            {
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommand.UriRequest, CmdLine.UriData));
            }
            else if (!CmdLine.InstallExtension.IsNullOrEmpty())
            {
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommand.ExtensionInstall, CmdLine.InstallExtension));
            }
            else if (CmdLine.StartInDesktop)
            {
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommand.SwitchMode, "desktop"));
            }
            else if (CmdLine.StartInFullscreen)
            {
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommand.SwitchMode, "fullscreen"));
            }
            else if (CmdLine.Shutdown)
            {
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommand.Shutdown, null));
            }
        }

        internal void ProcessUriRequest(PlayniteUriEventArgs args)
        {
            var arguments = args.Arguments;
            if (args.Arguments.Count() == 0)
            {
                return;
            }

            var command = arguments[0];
            switch (command)
            {
                case UriCommands.CreateDiag:
                    CrashHandlerViewModel.CreateDiagPackage(Dialogs);
                    break;

                case UriCommands.StartGame:
                    if (arguments.Count() != 2)
                    {
                        return;
                    }

                    if (Guid.TryParse(arguments[1], out var gameId))
                    {
                        var game = Database.Games[gameId];
                        if (game == null)
                        {
                            logger.Error($"Cannot start game, game {arguments[1]} not found.");
                        }
                        else
                        {
                            GamesEditor.PlayGame(game);
                        }
                    }
                    else
                    {
                        logger.Error($"Can't start game, failed to parse game id: {arguments[1]}");
                    }

                    break;

                default:
                    logger.Warn($"Uknown URI command {command}");
                    break;
            }
        }

        public void SetupInputs(bool enableXinput)
        {
            if (enableXinput)
            {
                try
                {
                    xdevice = new XInputDevice(InputManager.Current, this)
                    {
                        SimulateAllKeys = false,
                        SimulateNavigationKeys = true
                    };
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed intitialize XInput");
                    Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCXInputInitErrorMessage"), "");
                }
            }
        }

        public void Quit()
        {
            logger.Info("Shutting down Playnite");
            ReleaseResources();
            CurrentNative.Shutdown(0);
        }

        public abstract void Restart();

        public abstract void Restart(CmdLineOptions options);

        public virtual void ReleaseResources(bool releaseCefSharp = true)
        {
            if (resourcesReleased)
            {
                return;
            }

            logger.Debug("Releasing Playnite resources...");
            Discord?.Dispose();
            updateCheckTimer?.Dispose();
            Extensions?.NotifiyOnApplicationStopped();
            var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), (_) =>
            {
                try
                {
                    if (GlobalTaskHandler.CancelAndWait(Common.Timer.SecondsToMilliseconds(5)) == false)
                    {
                        logger.Warn("Global task cancelation failed in time.");
                    }

                    GamesEditor?.Dispose();
                    AppSettings?.SaveSettings();
                    Controllers?.Dispose();
                    Extensions?.Dispose();
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to dispose Playnite objects.");
                }
            }, new GlobalProgressOptions("LOCClosingPlaynite"));

            progressModel.ActivateProgress();

            // This must run on main thread
            if (releaseCefSharp)
            {
                CurrentNative.Dispatcher.Invoke(() =>
                {
                    if (CefTools.IsInitialized)
                    {
                        CefTools.Shutdown();
                    }
                });
            }

            resourcesReleased = true;
        }

        private void UpdateCheckerCallback(object state)
        {
            try
            {
                var updater = new Updater(this);
                if (updater.IsUpdateAvailable)
                {
                    var updateTitle = ResourceProvider.GetString("LOCUpdaterWindowTitle");
                    var updateBody = ResourceProvider.GetString("LOCUpdateIsAvailableNotificationBody");
                    if (!Current.IsActive)
                    {
                        ShowWindowsNotification(updateTitle, updateBody, () =>
                        {
                            Restore();
                            new UpdateViewModel(
                                updater,
                                new UpdateWindowFactory(),
                                new ResourceProvider(),
                                Dialogs).OpenView();
                        });
                    }

                    Api.Notifications.Add(
                        new NotificationMessage("UpdateAvailable",
                        updateBody,
                        NotificationType.Info, () =>
                        {
                            new UpdateViewModel(
                                updater,
                                new UpdateWindowFactory(),
                                new ResourceProvider(),
                                Dialogs).OpenView();
                        }));
                    updateCheckTimer.Dispose();
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Failed to process update.");
            }
        }

        public async Task StartUpdateCheckerAsync()
        {
            if (PlayniteEnvironment.InOfflineMode)
            {
                return;
            }

            await Task.Delay(Common.Timer.SecondsToMilliseconds(5));
            if (GlobalTaskHandler.IsActive)
            {
                await GlobalTaskHandler.ProgressTask;
            }

            updateCheckTimer = new System.Threading.Timer(
                UpdateCheckerCallback,
                null,
                0,
                Common.Timer.HoursToMilliseconds(4));
        }

        public async Task SendUsageDataAsync()
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
            if (Computer.WindowsVersion == WindowsVersion.Win10 && Computer.GetWindowsReleaseId() >= 1903)
            {
                return;
            }

            try
            {
                logger.Info("Disabling DPI awareness.");
                // https://stackoverflow.com/questions/13858665/disable-dpi-awareness-for-wpf-application
                var setDpiHwnd = typeof(HwndTarget).GetField("_setDpi", BindingFlags.Static | BindingFlags.NonPublic);
                setDpiHwnd?.SetValue(null, false);

                var setProcessDpiAwareness = typeof(HwndTarget).GetProperty("ProcessDpiAwareness", BindingFlags.Static | BindingFlags.NonPublic);

                // Doesn't work
                //if (Computer.WindowsVersion == WindowsVersion.Win10 && Computer.GetWindowsReleaseId() >= 1903)
                //{
                //    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("WindowsBase"));
                //    var enumType = assembly.GetType("MS.Win32.NativeMethods+PROCESS_DPI_AWARENESS");
                //    foreach (var enumVal in Enum.GetValues(enumType))
                //    {
                //        if (enumVal.ToString() == "PROCESS_SYSTEM_DPI_AWARE")
                //        {
                //            setProcessDpiAwareness?.SetValue(null, enumVal, null);
                //            break;
                //        }
                //    }
                //}
                //else
                //{
                    setProcessDpiAwareness?.SetValue(null, 1, null);
                //}

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
                (_) =>
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

                        GameDatabase.MigrateNewDatabaseFormat(GameDatabase.GetFullDbPath(AppSettings.DatabasePath));
                    }
                    else
                    {
                        GameDatabase.MigrateNewDatabaseFormat(GameDatabase.GetFullDbPath(AppSettings.DatabasePath));
                    }
                }, new GlobalProgressOptions("LOCDBUpgradeProgress"));

                if (migrationProgress.ActivateProgress().Result != true)
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

        public void UpdateScreenInformation(Controls.WindowBase window)
        {
            try
            {
                DpiScale = VisualTreeHelper.GetDpi(window);
                CurrentScreen = window.GetScreen();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                DpiScale = new DpiScale(1, 1);
                CurrentScreen = Computer.GetPrimaryScreen();
                logger.Error(e, $"Failed to get window information for main {Mode} window.");
            }
        }

        public void InstallThemeFile(string themeFile)
        {
            try
            {
                var desc = ExtensionInstaller.GetPackedThemeManifest(themeFile);
                if (desc == null)
                {
                    throw new FileNotFoundException("Theme manifest not found.");
                }

                desc.VerifyManifest();

                if (new Version(desc.ThemeApiVersion).Major != ThemeManager.GetApiVersion(desc.Mode).Major)
                {
                    throw new Exception(ResourceProvider.GetString("LOCGeneralExtensionInstallApiVersionFails"));
                }

                if (Dialogs.ShowMessage(
                        string.Format(ResourceProvider.GetString("LOCThemeInstallPrompt"),
                            desc.Name, desc.Author, desc.Version),
                        ResourceProvider.GetString("LOCGeneralExtensionInstallTitle"),
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ExtensionInstaller.QueuePackageInstall(themeFile);
                    if (Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOCExtInstallationRestartNotif"),
                        ResourceProvider.GetString("LOCSettingsRestartTitle"),
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Restart(new CmdLineOptions()
                        {
                            SkipLibUpdate = true,
                        });
                    };
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to install theme.");
                Dialogs.ShowErrorMessage(
                    string.Format(ResourceProvider.GetString("LOCThemeInstallFail"), e.Message), "");
            }
        }

        public void InstallExtensionFile(string extensionFile)
        {
            try
            {
                var desc = ExtensionInstaller.GetPackedExtensionManifest(extensionFile);
                if (desc == null)
                {
                    throw new FileNotFoundException("Extension manifest not found.");
                }

                desc.VerifyManifest();

                if (Dialogs.ShowMessage(
                        string.Format(ResourceProvider.GetString("LOCExtensionInstallPrompt"),
                            desc.Name, desc.Author, desc.Version),
                        ResourceProvider.GetString("LOCGeneralExtensionInstallTitle"),
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ExtensionInstaller.QueuePackageInstall(extensionFile);
                    if (Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOCExtInstallationRestartNotif"),
                        ResourceProvider.GetString("LOCSettingsRestartTitle"),
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Restart(new CmdLineOptions()
                        {
                            SkipLibUpdate = true,
                        });
                    };
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to install extension.");
                Dialogs.ShowErrorMessage(
                    string.Format(ResourceProvider.GetString("LOCExtensionInstallFail"), e.Message), "");
            }
        }
    }
}