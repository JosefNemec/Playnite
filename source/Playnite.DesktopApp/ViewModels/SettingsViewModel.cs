﻿using Playnite;
using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Playnite.Windows;
using Playnite.DesktopApp.Markup;
using System.Text.RegularExpressions;

namespace Playnite.DesktopApp.ViewModels
{
    public class SelectableTrayIcon
    {
        public TrayIconType TrayIcon { get; }
        public object ImageSource { get; }

        public SelectableTrayIcon(TrayIconType trayIcon)
        {
            TrayIcon = trayIcon;
            ImageSource = ResourceProvider.GetResource(TrayIcon.GetDescription());
        }
    }

    public class SelectablePlugin : ObservableObject
    {
        public Plugin Plugin { get; set; }
        public ExtensionDescription Description { get; set; }
        public object PluginIcon { get; set; }

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

        public SelectablePlugin(bool selected, Plugin plugin, ExtensionDescription description)
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
                PluginIcon = ResourceProvider.GetResource("PowerShellIcon");
            }
            else if (description.Type == ExtensionType.Script && description.Module.EndsWith("py", StringComparison.OrdinalIgnoreCase))
            {
                PluginIcon = ResourceProvider.GetResource("PythonIcon");
            }
            else
            {
                PluginIcon = ResourceProvider.GetResource("CsharpIcon");
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

    public class SettingsViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;
        private PlayniteApplication application;
        private PlayniteSettings originalSettings;
        private List<string> editedFields = new List<string>();

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
                OnPropertyChanged();
            }
        }

        public bool AnyGenericPluginSettings
        {
            get => GenericPluginSettings?.Any() == true;
        }

        public List<ThemeDescription> AvailableThemes
        {
            get => ThemeManager.GetAvailableThemes(ApplicationMode.Desktop);
        }

        public List<PlayniteLanguage> AvailableLanguages
        {
            get => Localization.AvailableLanguages;
        }

        public bool DatabaseLocationChanged
        {
            get;
            private set;
        } = false;

        public List<SelectableTrayIcon> AvailableTrayIcons
        {
            get;
            private set;
        } = new List<SelectableTrayIcon>();

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

        public RelayCommand<object> WindowClosingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                WindowClosing(false);
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

        public RelayCommand<string> SetCoverArtAspectRatioCommand
        {
            get => new RelayCommand<string>((ratio) =>
            {
                SetCoverArtAspectRatio(ratio);
            });
        }

        #endregion Commands

        public SettingsViewModel(
            GameDatabase database,
            PlayniteSettings settings,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            PlayniteApplication app)
        {
            Extensions = extensions;
            originalSettings = settings;
            Settings = settings.GetClone();
            Settings.PropertyChanged += (s, e) => editedFields.Add(e.PropertyName);
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.application = app;

            AvailableTrayIcons = new List<SelectableTrayIcon>
            {
                new SelectableTrayIcon(TrayIconType.Default),
                new SelectableTrayIcon(TrayIconType.Bright),
                new SelectableTrayIcon(TrayIconType.Dark)
            };

            PluginsList = Extensions
                .GetExtensionDescriptors()
                .Select(a => new SelectablePlugin(Settings.DisabledPlugins?.Contains(a.FolderName) != true, null, a))
                .ToList();

            foreach (var provider in Extensions.LibraryPlugins)
            {
                var provSetting = provider.GetSettings(false);
                var provView = provider.GetSettingsView(false);
                if (provSetting != null && provView != null)
                {
                    provView.DataContext = provSetting;
                    provSetting.BeginEdit();
                    var plugSetting = new PluginSettings()
                    {
                        Name = provider.Name,
                        Settings = provSetting,
                        View = provView,
                        Icon = provider.LibraryIcon
                    };

                    LibraryPluginSettings.Add(provider.Id, plugSetting);
                }
            }

            foreach (var plugin in Extensions.Plugins.Values.Where(a => a.Description.Type == ExtensionType.GenericPlugin))
            {
                var provSetting = plugin.Plugin.GetSettings(false);
                var provView = plugin.Plugin.GetSettingsView(false);
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
            foreach (var provider in LibraryPluginSettings.Keys)
            {
                LibraryPluginSettings[provider].Settings.CancelEdit();
            }

            foreach (var provider in GenericPluginSettings.Keys)
            {
                GenericPluginSettings[provider].Settings.CancelEdit();
            }

            WindowClosing(true);
            window.Close(false);
        }

        public void WindowClosing(bool closingHandled)
        {
            if (closingHandled)
            {
                return;
            }
        }
 
        public void EndEdit()
        {
            Settings.CopyProperties(originalSettings, false, new List<string>()
            {
                nameof(PlayniteSettings.FilterSettings),
                nameof(PlayniteSettings.ViewSettings),
                nameof(PlayniteSettings.InstallInstanceId),
                nameof(PlayniteSettings.GridItemHeight),
                nameof(PlayniteSettings.WindowPositions),
                nameof(PlayniteSettings.Fullscreen)
            });
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

            if (editedFields.Contains(nameof(Settings.StartOnBoot)))
            {
                try
                {
                    PlayniteSettings.SetBootupStateRegistration(Settings.StartOnBoot);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to register Playnite to start on boot.");
                    dialogs.ShowErrorMessage(resources.GetString("LOCSettingsStartOnBootRegistrationError")
                        + Environment.NewLine + e.Message, "");
                }
            }

            EndEdit();
            originalSettings.SaveSettings();
            foreach (var provider in LibraryPluginSettings.Keys)
            {
                LibraryPluginSettings[provider].Settings.EndEdit();
            }

            foreach (var plugin in GenericPluginSettings.Keys)
            {
                GenericPluginSettings[plugin].Settings.EndEdit();
            }

            if (editedFields?.Any() == true)
            {
                if (editedFields.IntersectsExactlyWith(
                    new List<string>()
                    {
                        nameof(Settings.Theme),
                        nameof(Settings.AsyncImageLoading),
                        nameof(Settings.DisableHwAcceleration),
                        nameof(Settings.DisableDpiAwareness),
                        nameof(Settings.DatabasePath),
                        nameof(Settings.DisabledPlugins),
                        nameof(Settings.EnableTray),
                        nameof(Settings.TrayIcon),
                        nameof(Settings.EnableControllerInDesktop),
                        nameof(Settings.Language)
                    }))
                {
                    if (dialogs.ShowMessage(
                        resources.GetString("LOCSettingsRestartAskMessage"),
                        resources.GetString("LOCSettingsRestartTitle"),
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        application.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                    }
                }
            }

            WindowClosing(true);
            window.Close(true);
        }

        public void SelectDbFile()
        {
            var path = dialogs.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                dialogs.ShowMessage(resources.GetString("LOCSettingsDBPathNotification"));
                Settings.DatabasePath = path;
            }
        }

        public void ClearWebcache()
        {
            if (dialogs.ShowMessage(
                    resources.GetString("LOCSettingsClearCacheWarn"),
                    resources.GetString("LOCSettingsClearCacheTitle"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CefTools.Shutdown();
                Directory.Delete(PlaynitePaths.BrowserCachePath, true);
                application.Restart();
            }            
        }

        public void SetCoverArtAspectRatio(string ratio)
        {
            var regex = Regex.Match(ratio, @"(\d+):(\d+)");
            if (regex.Success)
            {

                Settings.GridItemWidthRatio = Convert.ToInt32(regex.Groups[1].Value);
                Settings.GridItemHeightRatio = Convert.ToInt32(regex.Groups[2].Value);
            }
        }
    }
}
