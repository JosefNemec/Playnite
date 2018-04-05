using NLog;
using Playnite;
using Playnite.SDK;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI.ViewModels
{
    public class CrashHandlerViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;

        private string exception;
        public string Exception
        {
            get => exception;
            set
            {
                exception = value;
                OnPropertyChanged("Exception");
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

        public CrashHandlerViewModel(IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
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
            Process.Start(Paths.ExecutablePath);
            CloseView();
        }

        public void CreateDiagPackage()
        {
            var path = dialogs.SaveFile("ZIP Archive (*.zip)|*.zip");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    Diagnostic.CreateDiagPackage(path);
                    dialogs.ShowMessage(resources.FindString("DiagPackageCreationSuccess"));
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Faild to created diagnostics package.");
                    dialogs.ShowMessage(resources.FindString("DiagPackageCreationError"));
                }
            }
        }
    }
}
