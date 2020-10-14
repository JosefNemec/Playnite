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
using System.Diagnostics;

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

    public class SettingsViewModel : SettingsViewModelBase
    {
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

        public List<ThemeManifest> AvailableThemes
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

        private readonly Dictionary<int, UserControl> sectionViews;

        #region Commands

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

        public RelayCommand<IList<object>> RemoveImmportExclusionItemCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (ImportExclusionItem item in items.ToList())
                {
                    Settings.ImportExclusionList.Items.Remove(item);
                }
            }, (items) => items != null && items.Count > 0);
        }

        public RelayCommand<RoutedPropertyChangedEventArgs<object>> SettingsTreeSelectedItemChangedCommand
        {
            get => new RelayCommand<RoutedPropertyChangedEventArgs<object>>((a) =>
            {
                SettingsTreeSelectedItemChanged(a);
            });
        }

        public RelayCommand<ThemeManifest> UninstallThemeCommand
        {
            get => new RelayCommand<ThemeManifest>((a) =>
            {
                UninstallTheme(a);
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
            PlayniteApplication app) : base(database, settings, window, dialogs, resources, extensions, app)
        {
            AvailableTrayIcons = new List<SelectableTrayIcon>
            {
                new SelectableTrayIcon(TrayIconType.Default),
                new SelectableTrayIcon(TrayIconType.Bright),
                new SelectableTrayIcon(TrayIconType.Dark)
            };

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
                { 13, new Controls.SettingsSections.Performance() { DataContext = this } },
                { 14, new Controls.SettingsSections.ImportExlusionList() { DataContext = this } },
                { 15, new Controls.SettingsSections.ExtensionsLibraries() { DataContext = this } },
                { 16, new Controls.SettingsSections.ExtensionsMetadata() { DataContext = this } },
                { 17, new Controls.SettingsSections.ExtensionsOther() { DataContext = this } },
                { 18, new Controls.SettingsSections.ExtensionsThemes() { DataContext = this } }
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

            if (editedFields.Contains(nameof(Settings.StartOnBoot)))
            {
                try
                {
                    SystemIntegration.SetBootupStateRegistration(Settings.StartOnBoot);
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

            if (editedFields?.Any(a => typeof(PlayniteSettings).HasPropertyAttribute<RequiresRestartAttribute>(a)) == true ||
                extUninstallQeueued)
            {
                if (dialogs.ShowMessage(
                    resources.GetString("LOCSettingsRestartAskMessage"),
                    resources.GetString("LOCSettingsRestartTitle"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    application.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                }
            }

            if (editedFields.Contains(nameof(Settings.DiscordPresenceEnabled)) && application.Discord != null)
            {
                application.Discord.IsPresenceEnabled = Settings.DiscordPresenceEnabled;
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
                application.Restart(new CmdLineOptions
                {
                    SkipLibUpdate = true,
                    ClearWebCache = true
                });
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

        private void UninstallTheme(ThemeManifest a)
        {
            if (dialogs.ShowMessage(
               "LOCThemeUninstallQuestion",
               string.Empty,
               MessageBoxButton.YesNo,
               MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                extUninstallQeueued = true;
                ExtensionInstaller.QueueExtensionUninstall(a.DirectoryPath);
            }
        }
    }
}
