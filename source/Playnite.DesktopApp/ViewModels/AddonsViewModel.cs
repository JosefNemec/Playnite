using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Services;
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
    public partial class AddonsViewModel : ObservableObject
    {
        private enum View : int
        {
            InstalledLibraries = 0,
            InstalledMetadata = 1,
            InstalledGeneric = 2,
            InstalledThemesDesktop = 3,
            InstalledThemesFullscreen = 4,
            BrowseLibraries = 5,
            BrowseMetadata = 6,
            BrowseGeneric = 7,
            BrowseThemesDesktop = 8,
            BrowseThemesFullscreen = 9,
            Updates = 10
        }

        private static ILogger logger = LogManager.GetLogger();
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private IWindowFactory window;
        private IPlayniteAPI api;
        private ServicesClient serviceClient;
        private PlayniteSettings settings;
        private readonly Dictionary<View, UserControl> sectionViews;
        private PlayniteApplication application;

        public ExtensionFactory Extensions { get; set; }

        private bool isRestartRequired = false;
        public bool IsRestartRequired
        {
            get => isRestartRequired;
            set
            {
                isRestartRequired = value;
                OnPropertyChanged();
            }
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

        public RelayCommand<InstalledPlugin> UninstallExtensionCommand
        {
            get => new RelayCommand<InstalledPlugin>((a) =>
            {
                UninstallExtension(a);
            });
        }

        public RelayCommand<ThemeManifest> UninstallThemeCommand
        {
            get => new RelayCommand<ThemeManifest>((a) =>
            {
                UninstallTheme(a);
            });
        }

        public RelayCommand<InstalledPlugin> OpenExtensionDataDirCommand
        {
            get => new RelayCommand<InstalledPlugin>((plugin) =>
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

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<RoutedPropertyChangedEventArgs<object>> SectionChangedChangedCommand
        {
            get => new RelayCommand<RoutedPropertyChangedEventArgs<object>>((a) =>
            {
                SectionChanged(a);
            });
        }

        public AddonsViewModel(
            IWindowFactory window,
            IPlayniteAPI api,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ServicesClient serviceClient,
            ExtensionFactory extensions,
            PlayniteSettings settings,
            PlayniteApplication application)
        {
            this.window = window;
            this.api = api;
            this.dialogs = dialogs;
            this.resources = resources;
            this.serviceClient = serviceClient;
            this.settings = settings;
            this.application = application;
            Extensions = extensions;

            sectionViews = new Dictionary<View, UserControl>()
            {
                { View.InstalledLibraries, new Controls.AddonsSections.InstalledExtensions() { DataContext = this } },
                { View.InstalledMetadata, new Controls.AddonsSections.InstalledExtensions() { DataContext = this } },
                { View.InstalledGeneric, new Controls.AddonsSections.InstalledExtensions() { DataContext = this } },
                { View.InstalledThemesDesktop, new Controls.AddonsSections.InstalledThemes() { DataContext = this } },
                { View.InstalledThemesFullscreen, new Controls.AddonsSections.InstalledThemes() { DataContext = this } },
                { View.BrowseLibraries, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseMetadata, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseGeneric, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseThemesDesktop, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseThemesFullscreen, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.Updates, new Controls.AddonsSections.AddonUpdates() { DataContext = this } },
            };

            var descriptions = ExtensionFactory.GetExtensionDescriptors();
            LibraryPluginList = descriptions
                .Where(a => a.Type == ExtensionType.GameLibrary)
                .Select(a => new InstalledPlugin(
                    settings.DisabledPlugins?.Contains(a.DirectoryName) != true,
                    Extensions.Plugins.Values.FirstOrDefault(b => a.DescriptionPath == b.Description.DescriptionPath)?.Plugin,
                    a,
                    extensions.FailedExtensions.Any(ext => ext.DirectoryPath.Equals(a.DirectoryPath))))
                .OrderBy(a => a.Description.Name)
                .ToList();

            MetadataPluginList = descriptions
                .Where(a => a.Type == ExtensionType.MetadataProvider)
                .Select(a => new InstalledPlugin(
                    settings.DisabledPlugins?.Contains(a.DirectoryName) != true,
                    Extensions.Plugins.Values.FirstOrDefault(b => a.DescriptionPath == b.Description.DescriptionPath)?.Plugin,
                    a,
                    extensions.FailedExtensions.Any(ext => ext.DirectoryPath.Equals(a.DirectoryPath))))
                .OrderBy(a => a.Description.Name)
                .ToList();

            OtherPluginList = descriptions
                .Where(a => a.Type == ExtensionType.GenericPlugin || a.Type == ExtensionType.Script)
                .Select(a => new InstalledPlugin(
                    settings.DisabledPlugins?.Contains(a.DirectoryName) != true,
                    null,
                    a,
                    extensions.FailedExtensions.Any(ext => ext.DirectoryPath.Equals(a.DirectoryPath))))
                .OrderBy(a => a.Description.Name)
                .ToList();

            DesktopThemeList = ThemeManager.GetAvailableThemes(ApplicationMode.Desktop).OrderBy(a => a.Name).ToList();
            FullscreenThemeList = ThemeManager.GetAvailableThemes(ApplicationMode.Fullscreen).OrderBy(a => a.Name).ToList();
            CheckUpdates();
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        private void SectionChanged(RoutedPropertyChangedEventArgs<object> selectedItem)
        {
            int viewIndex = -1;
            if (selectedItem.NewValue is TreeViewItem treeItem)
            {
                if (treeItem.Tag != null)
                {
                    viewIndex = int.Parse(treeItem.Tag.ToString());
                }
            }

            if (viewIndex == -1)
            {
                return;
            }

            var view = (View)viewIndex;
            SelectedSectionView = sectionViews[view];
            switch (view)
            {
                case View.BrowseLibraries:
                    activeAddonSearchMode = AddonType.Library;
                    SearchAddon();
                    break;
                case View.BrowseMetadata:
                    activeAddonSearchMode = AddonType.Metadata;
                    SearchAddon();
                    break;
                case View.BrowseGeneric:
                    activeAddonSearchMode = AddonType.Generic;
                    SearchAddon();
                    break;
                case View.BrowseThemesDesktop:
                    activeAddonSearchMode = AddonType.ThemeDesktop;
                    SearchAddon();
                    break;
                case View.BrowseThemesFullscreen:
                    activeAddonSearchMode = AddonType.ThemeFullscreen;
                    SearchAddon();
                    break;
                case View.InstalledLibraries:
                    ActiveInstalledExtensionsList = LibraryPluginList;
                    break;
                case View.InstalledMetadata:
                    ActiveInstalledExtensionsList = MetadataPluginList;
                    break;
                case View.InstalledGeneric:
                    ActiveInstalledExtensionsList = OtherPluginList;
                    break;
                case View.InstalledThemesDesktop:
                    ActiveInstalledThemeList = DesktopThemeList;
                    break;
                case View.InstalledThemesFullscreen:
                    ActiveInstalledThemeList = FullscreenThemeList;
                    break;
                case View.Updates:
                    CheckUpdates();
                    break;
                default:
                    break;
            }
        }

        private void UninstallExtension(InstalledPlugin a)
        {
            if (dialogs.ShowMessage(
                LOC.ExtensionUninstallQuestion,
                string.Empty,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                IsRestartRequired = true;
                ExtensionInstaller.QueueExtensionUninstall(a.Description.DirectoryPath);
            }
        }

        private void UninstallTheme(ThemeManifest a)
        {
            if (dialogs.ShowMessage(
               "LOCThemeUninstallQuestion",
               string.Empty,
               MessageBoxButton.YesNo,
               MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                IsRestartRequired = true;
                ExtensionInstaller.QueueExtensionUninstall(a.DirectoryPath);
            }
        }

        internal void UpdateDisabledExtensions()
        {
            var disabledPlugs = LibraryPluginList.Where(a => !a.Selected)?.Select(a => a.Description.DirectoryName).ToList();
            disabledPlugs.AddMissing(MetadataPluginList.Where(a => !a.Selected)?.Select(a => a.Description.DirectoryName).ToList());
            disabledPlugs.AddMissing(OtherPluginList.Where(a => !a.Selected)?.Select(a => a.Description.DirectoryName).ToList());
            if (settings.DisabledPlugins?.IsListEqual(disabledPlugs) != true)
            {
                IsRestartRequired = true;
                settings.DisabledPlugins = disabledPlugs;
            }
        }

        public void ConfirmDialog()
        {
            UpdateDisabledExtensions();
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
