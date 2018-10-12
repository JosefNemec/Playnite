using Playnite;
using Playnite.App;
using Playnite.SDK;
using Playnite.Services;
using Playnite.Settings;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.ViewModels
{
    public class AboutViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;

        public string VersionInfo
        {
            get
            {
                return "Playnite " + Updater.GetCurrentVersion().ToString(2);
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
                try
                {
                    if (patronsList == null)
                    {
                        patronsList = string.Join(Environment.NewLine, (new ServicesClient()).GetPatrons());
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to get patron list.");
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
            var model = new CrashHandlerViewModel(null, dialogs, resources);
            model.CreateDiagPackage();
        }
    }
}
