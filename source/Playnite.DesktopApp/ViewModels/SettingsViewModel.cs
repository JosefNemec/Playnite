using Playnite;
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
using Playnite.DesktopApp.Controls;

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
        private Dictionary<Guid, PluginSettings> loadedPluginSettings = new Dictionary<Guid, PluginSettings>();
        private bool closingHanled = false;

        public ExtensionFactory Extensions { get; set; }

        private PlayniteSettings settings;
        public PlayniteSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public List<SelectableItem<LibraryPlugin>> AutoCloseClientsList { get; } = new List<SelectableItem<LibraryPlugin>>();

        public bool ShowDpiSettings
        {
            get => Computer.WindowsVersion != WindowsVersion.Win10;
        }

        public List<LoadedPlugin> GenericPlugins
        {
            get => Extensions.Plugins.Values.Where(a => a.Description.Type == ExtensionType.GenericPlugin).ToList();
        }

        public bool AnyGenericPluginSettings
        {
            get => Extensions?.GenericPlugins.HasItems() == true;
        }

        public List<ThemeDescription> AvailableThemes
        {
            get => ThemeManager.GetAvailableThemes(ApplicationMode.Desktop);
        }

        public List<PlayniteLanguage> AvailableLanguages
        {
            get => Localization.AvailableLanguages;
        }

        public List<string> AvailableFonts
        {
            get => System.Drawing.FontFamily.Families.Where(a => !a.Name.IsNullOrEmpty()).Select(a => a.Name).ToList();
        }

        public List<SelectableTrayIcon> AvailableTrayIcons
        {
            get;
            private set;
        } = new List<SelectableTrayIcon>();

        public List<SelectablePlugin> PluginsList
        {
            get;
        }

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

        private object selectedSectionItem;
        public object SelectedSectionItem
        {
            get => selectedSectionItem;
            set
            {
                selectedSectionItem = value;
                OnPropertyChanged();
            }
        }

        private readonly Dictionary<int, UserControl> sectionViews;

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
                WindowClosing();
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

        public RelayCommand<object> SetDefaultFontSizes
        {
            get => new RelayCommand<object>((ratio) =>
            {
                Settings.FontSize = 14;
                Settings.FontSizeSmall = 12;
                Settings.FontSizeLarge = 15;
                Settings.FontSizeLarger = 20;
                Settings.FontSizeLargest = 29;
            });
        }

        public RelayCommand<RoutedPropertyChangedEventArgs<object>> SettingsTreeSelectedItemChangedCommand
        {
            get => new RelayCommand<RoutedPropertyChangedEventArgs<object>>((a) =>
            {
                SettingsTreeSelectedItemChanged(a);
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

            sectionViews = new Dictionary<int, UserControl>()
            {
                { 0, new Controls.SettingsSections.General() { DataContext = this } },
                { 1, new Controls.SettingsSections.AppearanceGeneral() { DataContext = this } },
                { 2, new Controls.SettingsSections.AppearanceAdvanced() { DataContext = this } },
                { 3, new Controls.SettingsSections.AppearanceDetailsView() { DataContext = this } },
                { 4, new Controls.SettingsSections.AppearanceGridView() { DataContext = this } },
                { 5, new Controls.SettingsSections.AppearanceLayout() { DataContext = this } },
                { 6, new Controls.SettingsSections.GeneralAdvanced() { DataContext = this } },
                { 7, new Controls.SettingsSections.Input() { DataContext = this } },
                { 8, new Controls.SettingsSections.Extensions() { DataContext = this } },
                { 9, new Controls.SettingsSections.Metadata() { DataContext = this } },
                { 10, new Controls.SettingsSections.EmptyParent() { DataContext = this } },
                { 11, new Controls.SettingsSections.Scripting() { DataContext = this } },
                { 12, new Controls.SettingsSections.ClientShutdown() { DataContext = this } },
                { 13, new Controls.SettingsSections.Performance() { DataContext = this } }
            };

            SelectedSectionView = sectionViews[0];
            foreach (var plugin in extensions.LibraryPlugins.Where(a => a.Capabilities?.CanShutdownClient == true))
            {
                AutoCloseClientsList.Add(new SelectableItem<LibraryPlugin>(plugin)
                {
                    Selected = settings.ClientAutoShutdown.ShutdownPlugins.Contains(plugin.Id)
                });
            }
        }

        private void SettingsTreeSelectedItemChanged(RoutedPropertyChangedEventArgs<object> selectedItem)
        {
            if (selectedItem.NewValue is TreeViewItem treeItem)
            {
                if (treeItem.Tag != null)
                {
                    var viewIndex = int.Parse(treeItem.Tag.ToString());
                    SelectedSectionView = sectionViews[viewIndex];
                }
                else
                {
                    SelectedSectionView = null;
                }
            }
            else if (selectedItem.NewValue is Plugin plugin)
            {
                SelectedSectionView = GetPluginSettingsView(plugin.Id);
            }
            else if (selectedItem.NewValue is LoadedPlugin ldPlugin)
            {
                SelectedSectionView = GetPluginSettingsView(ldPlugin.Plugin.Id);
            }
        }

        private UserControl GetPluginSettingsView(Guid pluginId)
        {
            if (loadedPluginSettings.TryGetValue(pluginId, out var settings))
            {
                return settings.View;
            }

            try
            {
                var plugin = Extensions.Plugins.Values.First(a => a.Plugin.Id == pluginId);
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

                    loadedPluginSettings.Add(pluginId, plugSetting);
                    return provView;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to load plugin settings, {pluginId}");
            }

            return new Controls.SettingsSections.NoSettingsAvailable();
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

        public void EndEdit()
        {
            Settings.CopyProperties(originalSettings, true, new List<string>()
            {
                nameof(PlayniteSettings.FilterSettings),
                nameof(PlayniteSettings.ViewSettings),
                nameof(PlayniteSettings.InstallInstanceId),
                nameof(PlayniteSettings.GridItemHeight),
                nameof(PlayniteSettings.WindowPositions),
                nameof(PlayniteSettings.Fullscreen)
            }, true);
        }

        public void ConfirmDialog()
        {
            foreach (var plugin in loadedPluginSettings.Values)
            {
                if (!plugin.Settings.VerifySettings(out var errors))
                {
                    logger.Error($"Plugin settings verification errors {plugin.Name}.");
                    errors?.ForEach(a => logger.Error(a));
                    if (errors == null)
                    {
                        errors = new List<string>();
                    }

                    dialogs.ShowErrorMessage(string.Join(Environment.NewLine, errors), plugin.Name);
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

            var shutdownPlugins = AutoCloseClientsList.Where(a => a.Selected == true).Select(a => a.Item.Id).ToList();
            Settings.ClientAutoShutdown.ShutdownPlugins = shutdownPlugins;

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

        public void SelectDbFile()
        {
            dialogs.ShowMessage(resources.GetString("LOCSettingsDBPathNotification"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            var path = dialogs.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
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
