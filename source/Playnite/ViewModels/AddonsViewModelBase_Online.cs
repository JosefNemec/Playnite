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

                        var licenseRes = update.Item.CheckAddonLicense();
                        if (licenseRes == null)
                        {
                            update.Status = AddonUpdateStatus.Failed;
                            continue;
                        }

                        if (licenseRes == false)
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
