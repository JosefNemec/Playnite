using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.DesktopApp.ViewModels
{
    public class PluginSettingsViewModel : ObservableObject
    {
        private readonly IWindowFactory window;
        private readonly IResourceProvider resources;
        private readonly ExtensionFactory extensions;
        private readonly IDialogsFactory dialogs;
        private ISettings currentSettings;
        private bool closingHanled = false;

        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private UserControl settingsView;
        public UserControl SettingsView
        {
            get => settingsView;
            set
            {
                settingsView = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> WindowClosingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                WindowClosing();
            });
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

        public PluginSettingsViewModel(
            IWindowFactory window,
            IResourceProvider resources,
            IDialogsFactory dialogs,
            ExtensionFactory extensions,
            Guid pluginId)
        {
            this.window = window;
            this.resources = resources;
            this.extensions = extensions;
            this.dialogs = dialogs;

            if (extensions.Plugins.TryGetValue(pluginId, out var plugin))
            {
                var title = plugin.Description.Name;
                if (plugin.Plugin is LibraryPlugin library)
                {
                    title = library.Name;
                }
                else if (plugin.Plugin is MetadataPlugin metadata)
                {
                    title = metadata.Name;
                }

                currentSettings = plugin.Plugin.GetSettings(false);
                var provView = plugin.Plugin.GetSettingsView(false);
                if (currentSettings != null && provView != null)
                {
                    provView.DataContext = currentSettings;
                    SettingsView = provView;
                }
                else
                {
                    SettingsView = new Controls.SettingsSections.NoSettingsAvailable();
                }

                Title = title + " " + resources.GetString("LOCSettingsLabel");
            }
            else
            {
                SettingsView = new Controls.SettingsSections.NoSettingsAvailable();
                Title = resources.GetString("LOCSettingsNoSettingsAvailable");
            }

            currentSettings?.BeginEdit();
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            currentSettings?.CancelEdit();
            closingHanled = true;
            window.Close(false);
        }

        public void ConfirmDialog()
        {
            if (currentSettings != null)
            {
                if (!currentSettings.VerifySettings(out var errors))
                {
                    dialogs.ShowErrorMessage(string.Join(Environment.NewLine, errors), "");
                    return;
                }

                currentSettings.EndEdit();
            }

            closingHanled = true;
            window.Close(true);
        }

        public void WindowClosing()
        {
            if (!closingHanled)
            {
                currentSettings?.CancelEdit();
            }
        }
    }
}
