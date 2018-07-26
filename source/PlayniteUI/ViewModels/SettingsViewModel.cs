using CefSharp;
using Playnite;
using Playnite.API;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.Settings;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlayniteUI.ViewModels
{
    public class SettingsViewModel : ObservableObject, IDisposable
    {
        public class PluginSetting
        {
            public IEditableObject Settings { get; set; }
            public UserControl View { get; set; }
            public string Name { get; set; }
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;        

        public PlayniteAPI PlayniteApi { get; set; }

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

        public List<PluginDescription> PluginsList { get; }

        public Dictionary<Guid, PluginSetting> PluginSettings { get; } = new Dictionary<Guid, PluginSetting>();

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
            PlayniteAPI playniteApi)
        {
            PlayniteApi = playniteApi;
            Settings = settings;
            Settings.BeginEdit();
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            PluginsList = PluginFactory.GetPluginDescriptors();
            foreach (var provider in PlayniteApi.LibraryPlugins)
            {
                var provSetting = provider.Settings;
                var provView = provider.SettingsView;
                if (provSetting != null && provView != null)
                {
                    provView.DataContext = provSetting;
                    provSetting.BeginEdit();
                    var plugSetting = new PluginSetting()
                    {
                        Name = provider.Name,
                        Settings = provSetting,
                        View = provView
                    };

                    PluginSettings.Add(provider.Id, plugSetting);
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
            foreach (var provider in PluginSettings.Keys)
            {
                PluginSettings[provider].Settings.CancelEdit();
            }

            window.Close(false);
            Dispose();
        }

        public void Dispose()
        {
        }

        public void ConfirmDialog()
        {
            if (!Paths.GetValidFilePath(Settings.DatabasePath))
            {
                dialogs.ShowMessage(resources.FindString("LOCSettingsInvalidDBLocation"),
                    resources.FindString("LOCInvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Settings.EndEdit();
            Settings.SaveSettings();
            foreach (var provider in PluginSettings.Keys)
            {
                PluginSettings[provider].Settings.EndEdit();
            }

            if (Settings.EditedFields?.Any() == true)
            {
                if (Settings.EditedFields.IntersectsExactlyWith(
                    new List<string>() { "Skin", "AsyncImageLoading", "DisableHwAcceleration", "DatabasePath" }))
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
            var path = dialogs.SelectFile("Database file (*.db)|*.db");
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
