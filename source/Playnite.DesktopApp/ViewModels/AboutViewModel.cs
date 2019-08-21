using Playnite.SDK;
using Playnite.Services;
using Playnite.Settings;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Windows;
using Playnite.Common;
using Playnite.ViewModels;

namespace Playnite.DesktopApp.ViewModels
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
                return "SDK: " + Playnite.SDK.SdkVersions.SDKVersion.ToString(3);
            }
        }

        public string ThemeApiVersion
        {
            get
            {
                return "Theme API:\n" +
                    $"Desktop: {ThemeManager.DesktopApiVersion.ToString(3)}\n" +
                    $"Fullscreen: {ThemeManager.FullscreenApiVersion.ToString(3)}\n";
            }
        }

        public string Contributors
        {
            get
            {
                return Resources.ReadFileFromResource("Playnite.DesktopApp.Resources.contributors.txt");
            }
        }

        private string patronsList;
        public string PatronsList
        {
            get
            {
                if (PlayniteEnvironment.InOfflineMode)
                {
                    return string.Empty;
                }

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
            get => resources.GetString("LOCDownloadingLabel");
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

        public RelayCommand<object> NavigateUrlCommand => GlobalCommands.NavigateUrlCommand;

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

        public void CreateDiagPackage()
        {
            var model = new CrashHandlerViewModel(null, dialogs, resources, ApplicationMode.Desktop);
            model.CreateDiagPackage();
        }
    }
}
