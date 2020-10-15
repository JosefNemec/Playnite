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
    public class LibraryIntegrationsViewModel : SettingsViewModelBase
    {
        private UserControl pluginListView;

        public RelayCommand<SelectionChangedEventArgs> LibraryItemChangedCommand
        {
            get => new RelayCommand<SelectionChangedEventArgs>((a) =>
            {
                LibraryItemChanged(a);
            });
        }

        public List<object> OptionsList { get; set; }

        public LibraryIntegrationsViewModel(
            GameDatabase database,
            PlayniteSettings settings,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            PlayniteApplication app) : base(database, settings, window, dialogs, resources, extensions, app)
        {
            OptionsList = new List<object>()
            {
                new DatabaseObject { Name = ResourceProvider.GetString(LOC.Libraries) },
                new Separator()
            };
            OptionsList.AddRange(Extensions.LibraryPlugins);

            pluginListView = new Controls.SettingsSections.ExtensionsLibraries() { DataContext = this };
            SelectedSectionView = pluginListView;
        }

        private void LibraryItemChanged(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 0)
            {
                return;
            }

            var item = args.AddedItems[0];
            if (item is DatabaseObject)
            {
                SelectedSectionView = pluginListView;
            }
            else if (item is Plugin plugin)
            {
                SelectedSectionView = GetPluginSettingsView(plugin.Id);
            }
        }

        public override void CloseView()
        {
            foreach (var plugin in loadedPluginSettings.Values)
            {
                plugin.Settings.CancelEdit();
            }

            closingHanled = true;
            window.Close(false);
        }

        public override void WindowClosing()
        {
            if (!closingHanled)
            {
                foreach (var plugin in loadedPluginSettings.Values)
                {
                    plugin.Settings.CancelEdit();
                }
            }
        }

        public override void ConfirmDialog()
        {
            if (!VerifyPluginSettings())
            {
                return;
            }

            UpdateDisabledExtensions();
            EndEdit();
            originalSettings.SaveSettings();
            foreach (var plugin in loadedPluginSettings.Values)
            {
                plugin.Settings.EndEdit();
            }

            if (editedFields?.Any(a => typeof(PlayniteSettings).HasPropertyAttribute<RequiresRestartAttribute>(a)) == true)
            {
                if (dialogs.ShowMessage(
                    resources.GetString("LOCSettingsRestartAskMessage"),
                    resources.GetString("LOCSettingsRestartTitle"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    application.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                }
            }

            closingHanled = true;
            window.Close(true);
        }
    }
}
