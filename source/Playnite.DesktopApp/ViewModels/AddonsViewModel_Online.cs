using Playnite.Common;
using Playnite.Common.Web;
using Playnite.DesktopApp.Windows;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.Services;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public partial class AddonsViewModel :  Playnite.ViewModels.AddonsViewModelBase
    {
        private AddonType activeAddonSearchMode;

        private bool isUpdateSectionSelected;
        public bool IsUpdateSectionSelected
        {
            get => isUpdateSectionSelected;
            set
            {
                isUpdateSectionSelected = value;
                OnPropertyChanged();
            }
        }

        private List<AddonManifest> onlineAddonList;
        public List<AddonManifest> OnlineAddonList
        {
            get => onlineAddonList;
            set
            {
                onlineAddonList = value;
                OnPropertyChanged();
            }
        }

        private AddonManifest selectedOnlineAddon;
        public AddonManifest SelectedOnlineAddon
        {
            get => selectedOnlineAddon;
            set
            {
                selectedOnlineAddon = value;
                if (value != null)
                {
                    var progressModel = new ProgressViewViewModel(
                        new Playnite.Windows.ProgressWindowFactory(),
                            new GlobalProgressOptions(LOC.GettingsAddonInformation.GetLocalized(), true) { IsIndeterminate = true});
                    var progRes = progressModel.ActivateProgress((args) =>
                    {
                        selectedOnlineAddon.DownloadInstallerManifest(args.CancelToken);
                        if (selectedOnlineAddon.Links == null)
                        {
                            selectedOnlineAddon.Links = new Dictionary<string, string>();
                        }

                        if (!selectedOnlineAddon.SourceUrl.IsNullOrEmpty() && !selectedOnlineAddon.Links.ContainsValue(selectedOnlineAddon.SourceUrl))
                        {
                            selectedOnlineAddon.Links.AddOrUpdate("Source Repository", selectedOnlineAddon.SourceUrl);
                        }
                    }, 1500);
                    if (progRes.Canceled)
                    {
                        selectedOnlineAddon = null;
                    }
                    else if (progRes.Error != null)
                    {
                        selectedOnlineAddon = null;
                        SDK.API.Instance.Dialogs.ShowErrorMessage(progRes.Error.Message, "");
                    }
                }

                OnPropertyChanged();
            }
        }

        private int updateAddonCount = 0;
        public int UpdateAddonCount
        {
            get => updateAddonCount;
            set
            {
                updateAddonCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUpdateAvailable));
            }
        }

        public bool IsUpdateAvailable
        {
            get => UpdateAddonCount > 0;
        }

        private bool isOnlineListLoading;
        public bool IsOnlineListLoading
        {
            get => isOnlineListLoading;
            set
            {
                isOnlineListLoading = value;
                OnPropertyChanged();
            }
        }

        private bool isUpdateListLoading;
        public bool IsUpdateListLoading
        {
            get => isUpdateListLoading;
            set
            {
                isUpdateListLoading = value;
                OnPropertyChanged();
            }
        }

        private string addonSearchText;
        public string AddonSearchText
        {
            get => addonSearchText;
            set
            {
                addonSearchText = value;
                OnPropertyChanged();
                SearchAddon();
            }
        }

        public RelayCommand<object> SearchAddonCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SearchAddon();
            });
        }

        public RelayCommand<AddonManifest> InstallAddonCommand
        {
            get => new RelayCommand<AddonManifest>((a) =>
            {
                InstallAddon(a);
            });
        }

        private void InstallAddon(AddonManifest addon)
        {
            var licenseRes = addon.CheckAddonLicense();
            if (licenseRes == null)
            {
                dialogs.ShowErrorMessage(LOC.AddonErrorDownloadFailed, string.Empty);
                return;
            }

            if (licenseRes == false)
            {
                return;
            }

            AddonInstallerManifest manifest = null;
            dialogs.ActivateGlobalProgress(
                (a) => manifest = addon.InstallerManifest,
                new GlobalProgressOptions(LOC.DownloadingLabel, false));
            if (manifest == null || manifest.AddonId.IsNullOrEmpty())
            {
                dialogs.ShowErrorMessage(LOC.AddonErrorManifestDownloadError, string.Empty);
                return;
            }

            var latestPackage = manifest.GetLatestCompatiblePackage();
            if (latestPackage == null || latestPackage.PackageUrl.IsNullOrEmpty())
            {
                dialogs.ShowErrorMessage(LOC.AddonErrorNotCompatible, string.Empty);
                return;
            }

            try
            {
                var locaPath = addon.GetTargetDownloadPath();
                FileSystem.DeleteFile(locaPath);
                var res = dialogs.ActivateGlobalProgress((_) =>
                {
                    if (latestPackage.PackageUrl.IsHttpUrl())
                    {
                        FileSystem.PrepareSaveFile(locaPath);
                        HttpDownloader.DownloadFile(latestPackage.PackageUrl, locaPath);
                    }
                    else
                    {
                        File.Copy(latestPackage.PackageUrl, locaPath);
                    }
                },
                new GlobalProgressOptions(LOC.DownloadingLabel, false));

                if (res.Error != null)
                {
                    throw res.Error;
                }

                ExtensionInstaller.QueuePackageInstall(locaPath);
                IsRestartRequired = true;
                addon.OnPropertyChanged(nameof(addon.IsQueuedForInstall));
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to download addon package {latestPackage.PackageUrl}");
                dialogs.ShowErrorMessage(LOC.AddonErrorDownloadFailed, string.Empty);
            }
        }

        private void SearchAddon()
        {
            if (IsOnlineListLoading)
            {
                return;
            }

            if (PlayniteEnvironment.InOfflineMode)
            {
                return;
            }

            IsOnlineListLoading = true;
            Task.Run(() =>
            {
                try
                {
                    OnlineAddonList = serviceClient.GetAllAddons(activeAddonSearchMode, AddonSearchText).OrderBy(a => a.Name).ToList();
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to get addon list.");
                    OnlineAddonList = new List<AddonManifest>();
                }
                finally
                {
                    IsOnlineListLoading = false;
                }
            });
        }

        private void CheckUpdates()
        {
            if (PlayniteEnvironment.InOfflineMode)
            {
                return;
            }

            if (IsUpdateListLoading || UpdateAddonList != null)
            {
                return;
            }

            IsUpdateListLoading = true;
            var updateList = new List<AddonUpdate>();
            Task.Run(() =>
            {
                try
                {
                    updateList = Addons.CheckAddonUpdates(serviceClient);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to check for addon updates.");
                }
                finally
                {
                    IsUpdateListLoading = false;
                    UpdateAddonList = updateList;
                    UpdateAddonCount = updateList.Count;
                }
            });
        }
    }
}
