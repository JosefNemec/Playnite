using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.ViewModels
{
    public class SelectablePlugin : ObservableObject
    {
        public Plugin Plugin { get; set; }
        public ExtensionManifest Description { get; set; }
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

        public bool FailedLoading { get; set; }

        public SelectablePlugin()
        {
        }

        public SelectablePlugin(bool selected, Plugin plugin, ExtensionManifest description, bool failedLoading)
        {
            Selected = selected;
            Plugin = plugin;
            Description = description;
            FailedLoading = failedLoading;
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

        public override string ToString()
        {
            if (Plugin is LibraryPlugin lib)
            {
                return lib.Name;
            }
            else if (Plugin is MetadataPlugin met)
            {
                return met.Name;
            }
            else
            {
                return Description.Name;
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

    public abstract class SettingsViewModelBase : ObservableObject
    {
        internal static ILogger logger = LogManager.GetLogger();
        internal IWindowFactory window;
        internal IDialogsFactory dialogs;
        internal IResourceProvider resources;
        internal GameDatabase database;
        internal PlayniteApplication application;
        internal PlayniteSettings originalSettings;
        internal Dictionary<Guid, PluginSettings> loadedPluginSettings = new Dictionary<Guid, PluginSettings>();
        internal bool closingHanled = false;
        internal bool extUninstallQeueued = false;
        internal List<string> editedFields = new List<string>();

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

        public List<SelectablePlugin> LibraryPluginList
        {
            get;
        }

        public List<SelectablePlugin> MetadataPluginList
        {
            get;
        }

        public List<SelectablePlugin> OtherPluginList
        {
            get;
        }

        public List<ThemeManifest> DesktopThemeList
        {
            get;
        }

        public List<ThemeManifest> FullscreenThemeList
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

        public RelayCommand<SelectablePlugin> UninstallExtensionCommand
        {
            get => new RelayCommand<SelectablePlugin>((a) =>
            {
                UninstallExtension(a);
            });
        }

        public RelayCommand<SelectablePlugin> OpenExtensionDataDirCommand
        {
            get => new RelayCommand<SelectablePlugin>((plugin) =>
            {
                var extDir = string.Empty;
                if (plugin.Description.Type == ExtensionType.Script)
                {
                    if (!plugin.Description.Id.IsNullOrEmpty())
                    {
                        extDir = Path.Combine(PlaynitePaths.ExtensionsDataPath, Paths.GetSafePathName(plugin.Description.Id));
                    }
                }

                var p = Extensions.Plugins.Values.FirstOrDefault(a => a.Description.DirectoryPath == plugin.Description.DirectoryPath);
                if (p != null)
                {
                    extDir = p.Plugin.GetPluginUserDataPath();
                }

                if (!extDir.IsNullOrEmpty())
                {
                    try
                    {
                        FileSystem.CreateDirectory(extDir);
                        Process.Start(extDir);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to open dir {extDir}.");
                    }
                }
            });
        }

        public SettingsViewModelBase(
            GameDatabase database,
            PlayniteSettings settings,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            PlayniteApplication app)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.application = app;
            Extensions = extensions;
            originalSettings = settings;

            Settings = settings.GetClone();
            Settings.ImportExclusionList = settings.ImportExclusionList.GetClone();
            Settings.PropertyChanged += (s, e) => editedFields.AddMissing(e.PropertyName);

            var descriptions = ExtensionFactory.GetExtensionDescriptors();
            LibraryPluginList = descriptions
                .Where(a => a.Type == ExtensionType.GameLibrary)
                .Select(a => new SelectablePlugin(
                    Settings.DisabledPlugins?.Contains(a.DirectoryName) != true,
                    Extensions.Plugins.Values.FirstOrDefault(b => a.DescriptionPath == b.Description.DescriptionPath)?.Plugin,
                    a,
                    extensions.FailedExtensions.Any(ext => ext.DirectoryPath.Equals(a.DirectoryPath))))
                .OrderBy(a => a.Description.Name)
                .ToList();

            MetadataPluginList = descriptions
                .Where(a => a.Type == ExtensionType.MetadataProvider)
                .Select(a => new SelectablePlugin(
                    Settings.DisabledPlugins?.Contains(a.DirectoryName) != true,
                    Extensions.Plugins.Values.FirstOrDefault(b => a.DescriptionPath == b.Description.DescriptionPath)?.Plugin,
                    a,
                    extensions.FailedExtensions.Any(ext => ext.DirectoryPath.Equals(a.DirectoryPath))))
                .OrderBy(a => a.Description.Name)
                .ToList();

            OtherPluginList = descriptions
                .Where(a => a.Type == ExtensionType.GenericPlugin || a.Type == ExtensionType.Script)
                .Select(a => new SelectablePlugin(
                    Settings.DisabledPlugins?.Contains(a.DirectoryName) != true,
                    null,
                    a,
                    extensions.FailedExtensions.Any(ext => ext.DirectoryPath.Equals(a.DirectoryPath))))
                .OrderBy(a => a.Description.Name)
                .ToList();

            DesktopThemeList = ThemeManager.GetAvailableThemes(ApplicationMode.Desktop).OrderBy(a => a.Name).ToList();
            FullscreenThemeList = ThemeManager.GetAvailableThemes(ApplicationMode.Fullscreen).OrderBy(a => a.Name).ToList();
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public abstract void ConfirmDialog();
        public abstract void WindowClosing();
        public abstract void CloseView();

        internal UserControl GetPluginSettingsView(Guid pluginId)
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
                return new Controls.SettingsSections.ErrorLoading();
            }

            return new Controls.SettingsSections.NoSettingsAvailable();
        }

        private void UninstallExtension(SelectablePlugin a)
        {
            if (dialogs.ShowMessage(
                "LOCExtensionUninstallQuestion",
                string.Empty,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                extUninstallQeueued = true;
                ExtensionInstaller.QueueExtensionUninstall(a.Description.DirectoryPath);
            }
        }

        internal bool VerifyPluginSettings()
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
                    return false;
                }
            }

            return true;
        }

        internal void UpdateDisabledExtensions()
        {
            var disabledPlugs = LibraryPluginList.Where(a => !a.Selected)?.Select(a => a.Description.DirectoryName).ToList();
            disabledPlugs.AddMissing(MetadataPluginList.Where(a => !a.Selected)?.Select(a => a.Description.DirectoryName).ToList());
            disabledPlugs.AddMissing(OtherPluginList.Where(a => !a.Selected)?.Select(a => a.Description.DirectoryName).ToList());
            if (Settings.DisabledPlugins?.IsListEqual(disabledPlugs) != true)
            {
                Settings.DisabledPlugins = disabledPlugs;
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

            if (!originalSettings.ImportExclusionList.IsEqualJson(Settings.ImportExclusionList))
            {
                originalSettings.ImportExclusionList = Settings.ImportExclusionList;
            }
        }
    }
}
