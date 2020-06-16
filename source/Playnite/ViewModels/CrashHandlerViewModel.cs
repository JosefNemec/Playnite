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

        private string exception;
        public string Exception
        {
            get => exception;
            set
            {
                exception = value;
                OnPropertyChanged();
            }
        }

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

        public CrashHandlerViewModel(IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources, ApplicationMode mode)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.mode = mode;
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

            var genResult = ProgressViewViewModel.ActivateProgress(() =>
                Diagnostic.CreateDiagPackage(diagPath, crashDescription, packageInfo),
                "LOCDiagGenerating",
                out var genExc);
            if (genResult != true)
            {
                logger.Error(genExc, "Failed to created diagnostics package.");
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
            var uploadResult = ProgressViewViewModel.ActivateProgress(() =>
                uploadedId = new ServicesClient().UploadDiagPackage(diagPath),
                "LOCDiagUploading",
                out var updExc);
            if (uploadResult == true)
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
                logger.Error(updExc, "Failed to upload diag package.");
                dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCDiagPackageUploadError"), "");
                if (mode == ApplicationMode.Desktop)
                {
                    Explorer.NavigateToFileSystemEntry(diagPath);
                }
            }
        }
    }
}
