using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Plugins;
using Playnite.SDK;
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
    public enum AddonUpdateStatus
    {
        [Description("")]
        None,
        [Description(LOC.AddonUpdateStatusDownloaded)]
        Downloaded,
        [Description(LOC.AddonUpdateStatusFailed)]
        Failed
    }

    public class AddonUpdate : SelectableItem<AddonManifest>
    {
        public string UpdateInfo { get; set; }
        public string Changelog { get; set; }
        public AddonInstallerPackage Package { get; set; }

        private AddonUpdateStatus status = AddonUpdateStatus.None;
        public AddonUpdateStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }

        public AddonUpdate(AddonManifest item) : base(item)
        {
        }
    }

    public partial class AddonsViewModel : ObservableObject
    {
        private AddonType activeAddonSearchMode;

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

        private List<AddonUpdate> updateAddonList;
        public List<AddonUpdate> UpdateAddonList
        {
            get => updateAddonList;
            set
            {
                updateAddonList = value;
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
            }
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

        public RelayCommand<AddonManifest> UpdateAddonsCommand
        {
            get => new RelayCommand<AddonManifest>((a) =>
            {
                UpdateAddons();
            });
        }

        private Version GetApiVersion(AddonManifest addon)
        {
            switch (addon.Type)
            {
                case AddonType.Library:
                case AddonType.Metadata:
                case AddonType.Generic:
                    return SdkVersions.SDKVersion;
                case AddonType.ThemeDesktop:
                    return ThemeManager.DesktopApiVersion;
                case AddonType.ThemeFullscreen:
                    return ThemeManager.FullscreenApiVersion;
            }

            return new Version(999, 0);
        }

        private void InstallAddon(AddonManifest addon)
        {
            AddonInstallerManifest manifest = null;
            dialogs.ActivateGlobalProgress(
                (a) => manifest = addon.InstallerManifest,
                new GlobalProgressOptions(LOC.DownloadingLabel, false));
            if (manifest == null || manifest.AddonId.IsNullOrEmpty())
            {
                dialogs.ShowErrorMessage(LOC.AddonErrorManifestDownloadError, string.Empty);
                return;
            }

            var latestPackage = manifest.GetLatestCompatiblePackage(GetApiVersion(addon));
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
            IsOnlineListLoading = true;
            Task.Run(() =>
            {
                try
                {
                    OnlineAddonList = serviceClient.GetAllAddons(activeAddonSearchMode, AddonSearchText).ToList();
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

        private void UpdateAddons()
        {
            var addons = UpdateAddonList.Where(a => a.Selected == true);
            if (!addons.HasItems())
            {
                return;
            }

            dialogs.ActivateGlobalProgress((prg) =>
            {
                prg.ProgressMaxValue = addons.Count();
                prg.CurrentProgressValue = -1;
                foreach (var update in addons)
                {
                    try
                    {
                        prg.CurrentProgressValue++;
                        prg.Text = string.Format(resources.GetString(LOC.AddonDownloadingAddon), update.Item.Name);
                        Thread.Sleep(500);
                        var locaPath = update.Item.GetTargetDownloadPath();
                        FileSystem.DeleteFile(locaPath);

                        if (update.Package.PackageUrl.IsHttpUrl())
                        {
                            FileSystem.PrepareSaveFile(locaPath);
                            HttpDownloader.DownloadFile(update.Package.PackageUrl, locaPath);
                        }
                        else
                        {
                            File.Copy(update.Package.PackageUrl, locaPath);
                        }

                        ExtensionInstaller.QueuePackageInstall(locaPath);
                        update.Status = AddonUpdateStatus.Downloaded;
                        IsRestartRequired = true;
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to download addon update. {update.Item.AddonId}");
                        update.Status = AddonUpdateStatus.Failed;
                        update.StatusMessage = e.Message;
                    }
                }
            }, new GlobalProgressOptions("") { IsIndeterminate = false });
        }

        private void CheckUpdates()
        {
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
                    updateList.AddRange(CheckAddonsForUpdate(MetadataPluginList.Select(a => a.Description)));
                    updateList.AddRange(CheckAddonsForUpdate(LibraryPluginList.Select(a => a.Description)));
                    updateList.AddRange(CheckAddonsForUpdate(OtherPluginList.Select(a => a.Description)));
                    updateList.AddRange(CheckAddonsForUpdate(DesktopThemeList.Where(a => !a.IsBuiltInTheme)));
                    updateList.AddRange(CheckAddonsForUpdate(FullscreenThemeList.Where(a => !a.IsBuiltInTheme)));
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

        private List<AddonUpdate> CheckAddonsForUpdate(IEnumerable<BaseExtensionManifest> manifests)
        {
            var updateList = new List<AddonUpdate>();
            foreach (var manifest in manifests)
            {
                try
                {
                    var addonManifest = serviceClient.GetAddon(manifest.Id);
                    if (addonManifest == null)
                    {
                        continue;
                    }

                    var installer = addonManifest.InstallerManifest;
                    var package = installer.GetLatestCompatiblePackage(GetApiVersion(addonManifest));
                    var currentVersion = Version.Parse(manifest.Version);
                    var changeLog = string.Empty;
                    if (package != null && package.Version > currentVersion)
                    {
                        if (installer.Changelog.HasItems())
                        {
                            var changes = installer.Changelog.Where(a => a.Key > currentVersion && a.Key <= package.Version).ToList();
                            if (changes.HasItems())
                            {
                                changes.ForEach(a =>
                                {
                                    changeLog += a.Key.ToString();
                                    a.Value.ForEach(b => changeLog += Environment.NewLine + $"  - {b}");
                                    changeLog += Environment.NewLine;
                                });
                            }
                        }

                        updateList.Add(new AddonUpdate(addonManifest)
                        {
                            Selected = true,
                            UpdateInfo = $"{currentVersion} -> {package.Version}",
                            Changelog = changeLog,
                            Package = package
                        });
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to check for addon for update. {manifest.Id}");
                }
            }

            return updateList;
        }
    }
}
