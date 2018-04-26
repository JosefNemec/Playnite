using NLog;
using Playnite;
using Playnite.SDK;
using Playnite.Services;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.ViewModels
{
    public class AboutViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;

        public string VersionInfo
        {
            get
            {
                return "Playnite " + Update.GetCurrentVersion().ToString(2);
            }
        }

        public string SDKVersion
        {
            get
            {
                return "SDK " + Playnite.SDK.Version.SDKVersion.ToString(3);
            }
        }

        private string patronsList;
        public string PatronsList
        {
            get
            {
                if (patronsList == null)
                {
                    patronsList = string.Join(Environment.NewLine, (new ServicesClient()).GetPatrons());
                }

                return patronsList;
            }
        }

        public string PatronsListDownloading
        {
            get => resources.FindString("LOCDownloadingLabel");
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

        public RelayCommand<Uri> NavigateUrlCommand
        {
            get => new RelayCommand<Uri>((url) =>
            {
                NavigateUrl(url.AbsoluteUri);
            });
        }

        public AboutViewModel(IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
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

        public void NavigateUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        public void CreateDiagPackage()
        {
            var path = dialogs.SaveFile("ZIP Archive (*.zip)|*.zip");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    Diagnostic.CreateDiagPackage(path);
                    dialogs.ShowMessage(resources.FindString("LOCDiagPackageCreationSuccess"));
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Faild to created diagnostics package.");
                    dialogs.ShowMessage(resources.FindString("LOCDiagPackageCreationError"));
                }
            }
        }
    }
}
