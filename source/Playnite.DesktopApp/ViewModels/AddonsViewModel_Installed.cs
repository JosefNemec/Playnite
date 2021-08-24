using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public class InstalledPlugin : ObservableObject
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

        public InstalledPlugin()
        {
        }

        public InstalledPlugin(bool selected, Plugin plugin, ExtensionManifest description, bool failedLoading)
        {
            Selected = selected;
            Plugin = plugin;
            Description = description;
            FailedLoading = failedLoading;
            if (!string.IsNullOrEmpty(description.Icon))
            {
                PluginIcon = Path.Combine(Path.GetDirectoryName(description.DescriptionPath), description.Icon);
            }
            else if (description.Type == ExtensionType.Script && description.Module.Contains(".psm1", StringComparison.OrdinalIgnoreCase))
            {
                PluginIcon = ResourceProvider.GetResource("PowerShellIcon");
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

    public partial class AddonsViewModel : Playnite.ViewModels.AddonsViewModelBase
    {
        private List<InstalledPlugin> activeInstalledExtensionsList;
        public List<InstalledPlugin> ActiveInstalledExtensionsList
        {
            get => activeInstalledExtensionsList;
            set
            {
                activeInstalledExtensionsList = value;
                OnPropertyChanged();
            }
        }

        private List<ThemeManifest> activeInstalledThemeList;
        public List<ThemeManifest> ActiveInstalledThemeList
        {
            get => activeInstalledThemeList;
            set
            {
                activeInstalledThemeList = value;
                OnPropertyChanged();
            }
        }

        public List<InstalledPlugin> LibraryPluginList
        {
            get; private set;
        }

        public List<InstalledPlugin> MetadataPluginList
        {
            get; private set;
        }

        public List<InstalledPlugin> OtherPluginList
        {
            get; private set;
        }

        public List<ThemeManifest> DesktopThemeList
        {
            get; private set;
        }

        public List<ThemeManifest> FullscreenThemeList
        {
            get; private set;
        }
    }
}
