using Playnite.Plugins;
using Playnite.SDK;
using Playnite.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public enum AddonUpdateStatus
    {
        [Description("")]
        None,
        [Description(LOC.AddonUpdateStatusDownloaded)]
        Downloaded,
        [Description(LOC.AddonUpdateStatusFailed)]
        Failed,
        [Description(LOC.AddonUpdateStatusLicenseRejected)]
        LicenseRejected,
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

    public static class Addons
    {
        private static ILogger logger = LogManager.GetLogger();

        private static List<AddonUpdate> CheckAddonsForUpdate(IEnumerable<BaseExtensionManifest> manifests, ServicesClient serviceClient)
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
                    var package = installer.GetLatestCompatiblePackage(addonManifest.Type);
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

        public static List<AddonUpdate> CheckAddonUpdates(ServicesClient serviceClient)
        {
            var updateList = new List<AddonUpdate>();
            var descriptions = ExtensionFactory.GetInstalledManifests();
            updateList.AddRange(CheckAddonsForUpdate(descriptions.Where(a => a.Type == ExtensionType.MetadataProvider), serviceClient));
            updateList.AddRange(CheckAddonsForUpdate(descriptions.Where(a => a.Type == ExtensionType.GameLibrary), serviceClient));
            updateList.AddRange(CheckAddonsForUpdate(descriptions.Where(a => a.Type == ExtensionType.GenericPlugin), serviceClient));
            updateList.AddRange(CheckAddonsForUpdate(ThemeManager.GetAvailableThemes(ApplicationMode.Desktop).Where(a => !a.IsBuiltInTheme), serviceClient));
            updateList.AddRange(CheckAddonsForUpdate(ThemeManager.GetAvailableThemes(ApplicationMode.Fullscreen).Where(a => !a.IsBuiltInTheme), serviceClient));
            return updateList;
        }
    }
}
