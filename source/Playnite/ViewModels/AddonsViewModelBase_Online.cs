using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.ViewModels
{
    public class AddonsViewModelBase : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        public readonly IDialogsFactory dialogs;
        public readonly IResourceProvider resources;

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

        public RelayCommand<AddonManifest> UpdateAddonsCommand
        {
            get => new RelayCommand<AddonManifest>((a) =>
            {
                UpdateAddons();
            });
        }

        public AddonsViewModelBase(
            IDialogsFactory dialogs,
            IResourceProvider resources)
        {
            this.dialogs = dialogs;
            this.resources = resources;
        }

        public bool CheckAddonLicense(AddonManifest addon)
        {
            try
            {
                if (addon.UserAgreement != null)
                {
                    var acceptState = ExtensionInstaller.GetAddonLicenseAgreed(addon.AddonId);
                    if (acceptState == null || acceptState < addon.UserAgreement.Updated)
                    {
                        var license = HttpDownloader.DownloadString(addon.UserAgreement.AgreementUrl);
                        var licenseAgree = new LicenseAgreementViewModel(
                            new LicenseAgreementWindowFactory(),
                            license,
                            addon.Name);

                        if (licenseAgree.OpenView() == true)
                        {
                            ExtensionInstaller.AgreeAddonLicense(addon.AddonId);
                            return true;
                        }
                        else
                        {
                            ExtensionInstaller.RemoveAddonLicenseAgreement(addon.AddonId);
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to process addon license.");
                dialogs.ShowErrorMessage(LOC.AddonErrorDownloadFailed, string.Empty);
            }

            return false;
        }

        public void UpdateAddons()
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

                        if (!CheckAddonLicense(update.Item))
                        {
                            update.Status = AddonUpdateStatus.LicenseRejected;
                            continue;
                        }

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
    }
}
