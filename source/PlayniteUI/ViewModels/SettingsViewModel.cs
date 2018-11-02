using CefSharp;
using Playnite;
using Playnite.API;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlayniteUI.ViewModels
{
    public class SelectablePlugin : ObservableObject
    {
        public IPlugin Plugin { get; set; }
        public ExtensionDescription Description { get; set; }
        public string PluginIcon { get; set; } = string.Empty;

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        public SelectablePlugin()
        {
        }

        public SelectablePlugin(bool selected, IPlugin plugin, ExtensionDescription description)
        {
            Selected = selected;
            Plugin = plugin;
            Description = description;
            if (!string.IsNullOrEmpty(description.Icon))
            {
                PluginIcon = Path.Combine(Path.GetDirectoryName(description.DescriptionPath), description.Icon);
            }
            else if (description.Type == ExtensionType.Script && description.Module.EndsWith("ps1", StringComparison.OrdinalIgnoreCase))
            {
                PluginIcon = @"/Images/powershell.ico";
            }
            else if (description.Type == ExtensionType.Script && description.Module.EndsWith("py", StringComparison.OrdinalIgnoreCase))
            {
                PluginIcon = @"/Images/python.ico";
            }
            else
            {
                PluginIcon = @"/Images/csharp.ico";
            }
        }
    }

    public class PluginSettings
    {
        public ISettings Settings { get; set; }
        public UserControl View { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class SettingsViewModel : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;        

        public ExtensionFactory Extensions { get; set; }

        private PlayniteSettings settings;
        public PlayniteSettings Settings
        {
            get
            {
                return settings;
            }

            set
            {
                settings = value;
                OnPropertyChanged("Settings");
            }
        }

        public bool AnyGenericPluginSettings
        {
            get => GenericPluginSettings?.Any() == true;
        }

        public List<Theme> AvailableSkins
        {
            get => Themes.AvailableThemes;
        }

        public List<PlayniteLanguage> AvailableLanguages
        {
            get => Localization.AvailableLanguages;
        }

        public List<Theme> AvailableFullscreenSkins
        {
            get => Themes.AvailableFullscreenThemes;
        }

        public bool ProviderIntegrationChanged
        {
            get;
            private set;
        } = false;

        public bool DatabaseLocationChanged
        {
            get;
            private set;
        } = false;

        public List<SelectablePlugin> PluginsList
        {
            get;
        }

        public Dictionary<Guid, PluginSettings> LibraryPluginSettings
        {
            get;
        } = new Dictionary<Guid, PluginSettings>();

        public Dictionary<Guid, PluginSettings> GenericPluginSettings
        {
            get;
        } = new Dictionary<Guid, PluginSettings>();

        #region Commands

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

        public RelayCommand<object> DisposeCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Dispose();
            });
        }

        public RelayCommand<object> SelectDbFileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectDbFile();
            });
        }

        public RelayCommand<object> ClearWebCacheCommand
        {
            get => new RelayCommand<object>((url) =>
            {
                ClearWebcache();
            });
        }

        #endregion Commands

        public SettingsViewModel(
            GameDatabase database,
            PlayniteSettings settings,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions)
        {
            Extensions = extensions;
            Settings = settings;
            Settings.BeginEdit();
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            PluginsList = Extensions
                .GetExtensionDescriptors()
                .Select(a => new SelectablePlugin(Settings.DisabledPlugins?.Contains(a.FolderName) != true, null, a))
                .ToList();

            foreach (var provider in Extensions.LibraryPlugins.Values)
            {
                var provSetting = provider.Plugin.Settings;
                var provView = provider.Plugin.SettingsView;
                if (provSetting != null && provView != null)
                {
                    provView.DataContext = provSetting;
                    provSetting.BeginEdit();
                    var plugSetting = new PluginSettings()
                    {
                        Name = provider.Plugin.Name,
                        Settings = provSetting,
                        View = provView,
                        Icon = provider.Plugin.LibraryIcon
                    };

                    LibraryPluginSettings.Add(provider.Plugin.Id, plugSetting);
                }
            }

            foreach (var plugin in Extensions.GenericPlugins.Values)
            {
                var provSetting = plugin.Plugin.Settings;
                var provView = plugin.Plugin.SettingsView;
                if (provSetting != null && provView != null)
                {
                    provView.DataContext = provSetting;
                    provSetting.BeginEdit();
                    var plugSetting = new PluginSettings()
                    {
                        Name = plugin.Description.Name,
                        Settings = provSetting,
                        View = provView
                    };

                    GenericPluginSettings.Add(plugin.Plugin.Id, plugSetting);
                }
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            Settings.CancelEdit();
            foreach (var provider in LibraryPluginSettings.Keys)
            {
                LibraryPluginSettings[provider].Settings.CancelEdit();
            }

            foreach (var provider in GenericPluginSettings.Keys)
            {
                GenericPluginSettings[provider].Settings.CancelEdit();
            }

            window.Close(false);
            Dispose();
        }

        public void Dispose()
        {
        }

        public void ConfirmDialog()
        {
            foreach (var provider in LibraryPluginSettings.Keys)
            {
                if (!LibraryPluginSettings[provider].Settings.VerifySettings(out var errors))
                {
                    dialogs.ShowErrorMessage(string.Join(Environment.NewLine, errors), LibraryPluginSettings[provider].Name);
                    return;
                }
            }

            foreach (var plugin in GenericPluginSettings.Keys)
            {
                if (!GenericPluginSettings[plugin].Settings.VerifySettings(out var errors))
                {
                    dialogs.ShowErrorMessage(string.Join(Environment.NewLine, errors), GenericPluginSettings[plugin].Name);
                    return;
                }
            }

            var disabledPlugs = PluginsList.Where(a => !a.Selected)?.Select(a => a.Description.FolderName).ToList();
            if (Settings.DisabledPlugins?.IsListEqual(disabledPlugs) != true)
            {
                Settings.DisabledPlugins = PluginsList.Where(a => !a.Selected)?.Select(a => a.Description.FolderName).ToList();
            }

            if (Settings.EditedFields.Contains("StartOnBoot"))
            {
                PlayniteSettings.SetBootupStateRegistration(Settings.StartOnBoot);
            }

            Settings.EndEdit();
            Settings.SaveSettings();
            foreach (var provider in LibraryPluginSettings.Keys)
            {
                LibraryPluginSettings[provider].Settings.EndEdit();
            }

            foreach (var plugin in GenericPluginSettings.Keys)
            {
                GenericPluginSettings[plugin].Settings.EndEdit();
            }

            if (Settings.EditedFields?.Any() == true)
            {
                if (Settings.EditedFields.IntersectsExactlyWith(
                    new List<string>() { "Skin", "AsyncImageLoading", "DisableHwAcceleration", "DatabasePath", "DisabledPlugins" }))
                {
                    if (dialogs.ShowMessage(
                        resources.FindString("LOCSettingsRestartAskMessage"),
                        resources.FindString("LOCSettingsRestartTitle"),
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        App.CurrentApp.Restart();
                    }
                }
            }

            window.Close(true);
            Dispose();
        }

        public void SelectDbFile()
        {
            var path = dialogs.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                dialogs.ShowMessage(resources.FindString("LOCSettingsDBPathNotification"));
                Settings.DatabasePath = path;
                Settings.OnPropertyChanged("DatabasePath", true);
            }
        }

        public void ClearWebcache()
        {
            if (dialogs.ShowMessage(
                    resources.FindString("LOCSettingsClearCacheWarn"),
                    resources.FindString("LOCSettingsClearCacheTitle"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Cef.Shutdown();
                System.IO.Directory.Delete(PlaynitePaths.BrowserCachePath, true);
                App.CurrentApp.Restart();
            }            
        }
    }
}
