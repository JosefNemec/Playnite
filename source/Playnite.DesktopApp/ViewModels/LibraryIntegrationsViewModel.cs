using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.ViewModels
{
    public class LibraryIntegrationsViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private readonly ExtensionFactory extensions;
        private bool closingHanled = false;

        private Dictionary<Guid, PluginSettingsItem> loadedPluginSettings = new Dictionary<Guid, PluginSettingsItem>();

        public RelayCommand<SelectionChangedEventArgs> LibraryItemChangedCommand
        {
            get => new RelayCommand<SelectionChangedEventArgs>((a) =>
            {
                LibraryItemChanged(a);
            });
        }

        public List<LibraryPlugin> LibraryPlugins { get; set; }

        private UserControl selectedSectionView;
        public UserControl SelectedSectionView
        {
            get => selectedSectionView;
            set
            {
                selectedSectionView = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<object> WindowClosingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                WindowClosing();
            });
        }

        public LibraryIntegrationsViewModel(
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.extensions = extensions;
            LibraryPlugins = extensions.LibraryPlugins.OrderBy(a => a.Name).ToList();
            SelectedSectionView = new Controls.SettingsSections.LibrariesConfigWindowInfo() { DataContext = this }; ;
        }

        private void LibraryItemChanged(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 0)
            {
                return;
            }

            var item = args.AddedItems[0];
            if (item is Plugin plugin)
            {
                SelectedSectionView = PluginSettingsHelper.GetPluginSettingsView(plugin.Id, extensions, loadedPluginSettings);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            foreach (var plugin in loadedPluginSettings.Values)
            {
                plugin.Settings.CancelEdit();
            }

            closingHanled = true;
            window.Close(false);
        }

        public void WindowClosing()
        {
            if (!closingHanled)
            {
                foreach (var plugin in loadedPluginSettings.Values)
                {
                    plugin.Settings.CancelEdit();
                }
            }
        }

        public void ConfirmDialog()
        {
            var verResult = PluginSettingsHelper.VerifyPluginSettings(loadedPluginSettings);
            if (!verResult.Item1)
            {
                dialogs.ShowErrorMessage(string.Join(Environment.NewLine, verResult.Item2), "");
                return;
            }

            foreach (var plugin in loadedPluginSettings.Values)
            {
                plugin.Settings.EndEdit();
            }

            closingHanled = true;
            window.Close(true);
        }
    }
}
