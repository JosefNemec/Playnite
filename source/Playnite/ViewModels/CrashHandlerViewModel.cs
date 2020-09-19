using Playnite.Common;
using Playnite.SDK;
using Playnite.Services;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Playnite.ViewModels
{
    public class CrashHandlerViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();

        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private ApplicationMode mode;
        private ExceptionInfo exInfo;
        private PlayniteSettings settings;

        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private bool disableExtension;
        public bool DisableExtension
        {
            get => disableExtension;
            set
            {
                disableExtension = value;
                OnPropertyChanged();
            }
        }

        public string ExtCrashDescription { get; set; }

        public RelayCommand<object> CreateDiagPackageCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CreateDiagPackage(new DiagnosticPackageInfo
                {
                    IsCrashPackage = true,
                    PlayniteVersion = Updater.GetCurrentVersion().ToString(4)
                });
            });
        }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<object> ReportIssueCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ReportIssue();
            });
        }

        public RelayCommand<object> RestartCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RestartApp();
            });
        }

        public RelayCommand<object> RestartSafeCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RestartAppSafe();
            });
        }

        public RelayCommand<object> SaveLogCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SaveLog();
            });
        }

        public CrashHandlerViewModel(
            CrashHandlerWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ApplicationMode mode)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.mode = mode;
        }

        public CrashHandlerViewModel(
            ExtensionCrashHandlerWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ApplicationMode mode,
            ExceptionInfo exInfo,
            PlayniteSettings settings)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.mode = mode;
            this.exInfo = exInfo;
            this.settings = settings;
            ExtCrashDescription = resources.
                GetString(mode == ApplicationMode.Desktop ? "LOCExtCrashDescription" : "LOCExtCrashDescriptionFS").
                Format(exInfo.CrashExtension.Name);
        }

        public void OpenView()
        {
            window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            window.Close();
        }

        public static void ReportIssue()
        {
            try
            {
                if (PlayniteEnvironment.ReleaseChannel == ReleaseChannel.Beta)
                {
                    ProcessStarter.StartUrl(UrlConstants.IssuesTesting);
                }
                else
                {
                    ProcessStarter.StartUrl(UrlConstants.Issues);
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to open report issue url.");
            }
        }

        public void RestartApp()
        {
            if (exInfo?.IsExtensionCrash == true && DisableExtension)
            {
                settings.DisabledPlugins.AddMissing(exInfo.CrashExtension.DirectoryName);
                settings.SaveSettings();
            }

            if (mode == ApplicationMode.Desktop)
            {
                Process.Start(PlaynitePaths.DesktopExecutablePath);
            }
            else
            {
                Process.Start(PlaynitePaths.FullscreenExecutablePath);
            }

            CloseView();
        }

        public void RestartAppSafe()
        {
            var options = new CmdLineOptions { SafeStartup = true };
            if (mode == ApplicationMode.Desktop)
            {
                Process.Start(PlaynitePaths.DesktopExecutablePath, options.ToString());
            }
            else
            {
                Process.Start(PlaynitePaths.FullscreenExecutablePath, options.ToString());
            }

            CloseView();
        }

        private void CreateDiagPackage(DiagnosticPackageInfo packageInfo)
        {
            CreateDiagPackage(dialogs, Description, packageInfo);
        }

        public static void CreateDiagPackage(
            IDialogsFactory dialogs,
            string crashDescription = null,
            DiagnosticPackageInfo packageInfo = null)
        {
            var diagPath = Path.Combine(PlaynitePaths.TempPath, "diag.zip");
            if (packageInfo == null)
            {
                packageInfo = new DiagnosticPackageInfo
                {
                    IsCrashPackage = false,
                    PlayniteVersion = Updater.GetCurrentVersion().ToString(4)
                };
            }

            var genResult = GlobalProgress.ActivateProgress((_) =>
                Diagnostic.CreateDiagPackage(diagPath, crashDescription, packageInfo),
                new GlobalProgressOptions("LOCDiagGenerating"));
            if (genResult.Result != true)
            {
                logger.Error(genResult.Error, "Failed to created diagnostics package.");
                dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCDiagPackageCreationError"), "");
                return;
            }

            var mode = PlayniteApplication.Current.Mode;
            if (PlayniteEnvironment.InOfflineMode && mode == ApplicationMode.Desktop)
            {
                Explorer.NavigateToFileSystemEntry(diagPath);
                return;
            }

            var uploadedId = Guid.Empty;
            var uploadResult = GlobalProgress.ActivateProgress((_) =>
                uploadedId = new ServicesClient().UploadDiagPackage(diagPath),
                new GlobalProgressOptions("LOCDiagUploading"));
            if (uploadResult.Result == true)
            {
                if (mode == ApplicationMode.Desktop)
                {
                    dialogs.ShowSelectableString(ResourceProvider.GetString("LOCDiagPackageCreationSuccess"), "", uploadedId.ToString());
                }
                else
                {
                    dialogs.ShowMessage(ResourceProvider.GetString("LOCDiagPackageSentSuccess"));
                }
            }
            else
            {
                logger.Error(uploadResult.Error, "Failed to upload diag package.");
                dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCDiagPackageUploadError"), "");
                if (mode == ApplicationMode.Desktop)
                {
                    Explorer.NavigateToFileSystemEntry(diagPath);
                }
            }
        }

        private void SaveLog()
        {
            var targetPath = dialogs.SaveFile("Log file|*.log", true);
            if (!targetPath.IsNullOrEmpty())
            {
                File.Copy(PlaynitePaths.LogPath, targetPath, true);
            }
        }
    }
}
