using Playnite.Plugins;
using Playnite.SDK;
using Playnite.Services;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp.ViewModels
{
    public class AddonsViewModel : Playnite.ViewModels.AddonsViewModelBase
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private ServicesClient serviceClient;
        private PlayniteSettings settings;
        private PlayniteApplication application;

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public new RelayCommand<AddonManifest> UpdateAddonsCommand
        {
            get => new RelayCommand<AddonManifest>((a) =>
            {
                UpdateAddons();
                if (UpdateAddonList.All(add => add.Status == AddonUpdateStatus.Downloaded))
                {
                    if (dialogs.ShowMessage(
                       LOC.SettingsRestartAskMessage,
                       LOC.SettingsRestartTitle,
                       MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        application.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                    }
                    else
                    {
                        window.Close(true);
                    }
                }
            });
        }

        public AddonsViewModel(
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ServicesClient serviceClient,
            ExtensionFactory extensions,
            PlayniteSettings settings,
            PlayniteApplication application,
            List<AddonUpdate> addonUpdates) : base(dialogs, resources)
        {
            this.window = window;
            this.serviceClient = serviceClient;
            this.settings = settings;
            this.application = application;
            UpdateAddonList = addonUpdates;
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void ConfirmDialog()
        {
            if (IsRestartRequired)
            {
                if (dialogs.ShowMessage(
                       LOC.SettingsRestartAskMessage,
                       LOC.SettingsRestartTitle,
                       MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    application.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                }
            }

            window.Close(true);
        }
    }
}
