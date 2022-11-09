using Playnite;
using Playnite.API;
using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Playnite.Windows;
using Playnite.DesktopApp.Windows;
using Playnite.Services;
using Playnite.Common.Web;

namespace Playnite.DesktopApp.ViewModels
{
    public class FirstTimeStartupViewModel : ObservableObject
    {
        public class Pages
        {
            public const int Intro = 0;
            public const int ProviderSelect = 1;
            public const int ProviderConfig = 2;
            public const int Finish = 3;
        }

        public class RecommendedAddon : SelectableItem<string>
        {
            public string Name { get; set; }

            public RecommendedAddon(string addonId, string name) : base(addonId)
            {
                Name = name;
            }
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private ExtensionFactory extensions;
        private List<PluginSettingsItem> selectedPlugins { get; } = new List<PluginSettingsItem>();
        private int selectedPluginIndex = 0;
        private ServicesClient backendClient;
        ServicesClient.RecommendedAddons recommendedExtensions = new ServicesClient.RecommendedAddons();

        public bool ShowFinishButton
        {
            get => SelectedIndex == Pages.Finish;
        }

        private List<RecommendedAddon> recommendeLibrariesList;
        public List<RecommendedAddon> RecommendeLibrariesList { get => recommendeLibrariesList; set => SetValue(ref recommendeLibrariesList, value); }

        private PlayniteSettings settings = new PlayniteSettings();
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

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                selectedIndex = value;
                OnPropertyChanged(nameof(ShowFinishButton));
                OnPropertyChanged();
            }
        }

        private UserControl selectedProviderSettingsView;
        public UserControl SelectedProviderSettingsView
        {
            get
            {
                return selectedProviderSettingsView;
            }

            set
            {
                selectedProviderSettingsView = value;
                OnPropertyChanged();
            }
        }

        private PluginSettingsItem selectedPlugin;
        public PluginSettingsItem SelectedLibraryPlugin
        {
            get
            {
                return selectedPlugin;
            }

            set
            {
                selectedPlugin = value;
                OnPropertyChanged();
            }
        }

        public List<InstalledPlugin> LibraryPlugins
        {
            get;
        } = new List<InstalledPlugin>();

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> FinishCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(true);
            });
        }

        public RelayCommand<object> NextCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                NavigateNext();
            }, (a) => SelectedIndex < Pages.Finish);
        }

        public FirstTimeStartupViewModel(
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            ServicesClient backendClient)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.extensions = extensions;
            this.backendClient = backendClient;
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            Settings.DisabledPlugins = LibraryPlugins.Where(a => !a.Selected)?.Select(a => a.Description.Id).ToList();
            foreach (var plugin in LibraryPlugins)
            {
                plugin.Plugin.Dispose();
            }

            window.Close(result);
        }

        private void SetPluginConfiguration(PluginSettingsItem plugin)
        {
            SelectedLibraryPlugin = plugin;
            var view = plugin.View;
            view.DataContext = plugin.Settings;
            SelectedProviderSettingsView = view;
            plugin.Settings.BeginEdit();
        }

        public void NavigateNext()
        {
            if (SelectedIndex == Pages.Intro)
            {
                var listDownRes = dialogs.ActivateGlobalProgress((prg) =>
                {
                    recommendedExtensions = backendClient.GetDefaultExtensions();
                }, new GlobalProgressOptions(LOC.DefaultAddonListDownload, false) { IsIndeterminate = true });

                if (!recommendedExtensions.Libraries.HasItems())
                {
                    if (listDownRes.Error != null)
                    {
                        logger.Error(listDownRes.Error, "Failed to get list of default extensions.");
                    }

                    dialogs.ShowErrorMessage(LOC.DefaultAddonListDownloadError.GetLocalized() + $"\n\n{listDownRes.Error?.Message}", "");
                    SelectedIndex = Pages.Finish;
                    return;
                }

                RecommendeLibrariesList = recommendedExtensions.Libraries.Select(a => new RecommendedAddon(a.Value, a.Key)).ToList();
                SelectedIndex++;
                return;
            }

            if (SelectedIndex == Pages.ProviderSelect)
            {
                var selectedLibs = RecommendeLibrariesList.Where(a => a.Selected == true).ToList();
                var allPassed = true;
                dialogs.ActivateGlobalProgress((prg) =>
                {
                    prg.ProgressMaxValue = selectedLibs.Count + recommendedExtensions.Generic?.Count ?? 0;
                    prg.CurrentProgressValue = 0;

                    foreach (var lib in selectedLibs)
                    {
                        prg.CurrentProgressValue++;
                        prg.Text = resources.GetString(LOC.FirstDownloadingAddon).Format(lib.Name);
                        if (!DownloadAndInstallAddon(lib.Item))
                        {
                            allPassed = false;
                        }
                    }

                    foreach (var genericPlugin in recommendedExtensions.Generic ?? new Dictionary<string, string>())
                    {
                        prg.CurrentProgressValue++;
                        prg.Text = resources.GetString(LOC.FirstDownloadingAddon).Format(genericPlugin.Key);
                        if (!DownloadAndInstallAddon(genericPlugin.Value))
                        {
                            allPassed = false;
                        }
                    }
                }, new GlobalProgressOptions("", false) { IsIndeterminate = false });

                if (!allPassed)
                {
                    dialogs.ShowErrorMessage(LOC.FirstPluginDownloadError, "");
                }

                ExtensionInstaller.InstallExtensionQueue();
                extensions.LoadPlugins(null, false, null);
                foreach (var lib in extensions.LibraryPlugins)
                {
                    selectedPlugins.Add(new PluginSettingsItem
                    {
                        Name = lib.Name,
                        View = lib.GetSettingsView(true),
                        Settings = lib.GetSettings(true),
                        Icon = lib.LibraryIcon
                    });
                }

                if (selectedPlugins.HasItems())
                {
                    SelectedIndex++;
                    SetPluginConfiguration(selectedPlugins[0]);
                }
                else
                {
                    SelectedIndex = Pages.Finish;
                }

                return;
            }

            if (SelectedIndex == Pages.ProviderConfig && SelectedLibraryPlugin != null)
            {
                if (SelectedLibraryPlugin.Settings.VerifySettings(out var errors))
                {
                    SelectedLibraryPlugin.Settings.EndEdit();
                }
                else
                {
                    dialogs.ShowErrorMessage(string.Join(Environment.NewLine, errors), "");
                    return;
                }

                if ((selectedPluginIndex + 1) < selectedPlugins.Count)
                {
                    selectedPluginIndex++;
                    SetPluginConfiguration(selectedPlugins[selectedPluginIndex]);
                    return;
                }
            }

            SelectedIndex++;
        }

        private bool DownloadAndInstallAddon(string addonId)
        {
            try
            {
                var addon = backendClient.GetAddon(addonId);
                var man = addon.InstallerManifest;
                var package = man.GetLatestCompatiblePackage();
                if (package == null)
                {
                    logger.Error($"Can't install addon {addonId}, no compatible package found.");
                    return false;
                }

                var localPath = addon.GetTargetDownloadPath();
                FileSystem.DeleteFile(localPath);
                FileSystem.PrepareSaveFile(localPath);
                HttpDownloader.DownloadFile(package.PackageUrl, localPath);
                ExtensionInstaller.QueuePackageInstall(localPath);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to firt time setup addon {addonId}");
                return false;
            }

            return true;
        }
    }
}
