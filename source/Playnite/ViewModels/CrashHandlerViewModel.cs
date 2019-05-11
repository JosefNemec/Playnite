using Playnite;
using Playnite.Common;
using Playnite.SDK;
using Playnite.Services;
using Playnite.Settings;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite.Windows;

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

        public RelayCommand<object> CreateDiagPackageCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CreateDiagPackage();
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

        public void ReportIssue()
        {
            Process.Start(@"https://github.com/JosefNemec/Playnite/issues");
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

        public void CreateDiagPackage()
        {
            var diagPath = Path.Combine(PlaynitePaths.TempPath, "diag.zip");

            try
            {
                Diagnostic.CreateDiagPackage(diagPath);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to created diagnostics package.");
                dialogs.ShowErrorMessage(resources.GetString("LOCDiagPackageCreationError"), "");
                return;
            }

            if (PlayniteEnvironment.InOfflineMode && mode == ApplicationMode.Desktop)
            {
                Explorer.NavigateToFileSystemEntry(diagPath);
                return;
            }

            try
            {
                var uploadedId = new ServicesClient().UploadDiagPackage(diagPath);
                if (mode == ApplicationMode.Desktop)
                {
                    dialogs.ShowSelectableString(resources.GetString("LOCDiagPackageCreationSuccess"), "", uploadedId.ToString());
                }
                else
                {
                    dialogs.ShowMessage(resources.GetString("LOCDiagPackageSentSuccess"));
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to upload diag package.");
                dialogs.ShowErrorMessage(resources.GetString("LOCDiagPackageUploadError"), "");
                if (mode == ApplicationMode.Desktop)
                {
                    Explorer.NavigateToFileSystemEntry(diagPath);
                }
            }
        }
    }
}
