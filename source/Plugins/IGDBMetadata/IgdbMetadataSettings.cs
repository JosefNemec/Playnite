using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGDBMetadata
{
    public enum MultiImagePriority
    {
        [Description("LOCFirst")]
        First,
        [Description("LOCRandom")]
        Random,
        [Description("LOCUserSelect")]
        Select
    }

    public class IgdbMetadataSettings : ObservableObject, ISettings
    {
        private IgdbMetadataSettings editingClone;
        private readonly IgdbMetadataPlugin plugin;

        private MultiImagePriority imageSelectionPriority = MultiImagePriority.Random;
        public MultiImagePriority ImageSelectionPriority
        {
            get => imageSelectionPriority;
            set
            {
                imageSelectionPriority = value;
                OnPropertyChanged();
            }
        }

        private bool useScreenshotsIfNecessary = true;
        public bool UseScreenshotsIfNecessary
        {
            get => useScreenshotsIfNecessary;
            set
            {
                useScreenshotsIfNecessary = value;
                OnPropertyChanged();
            }
        }

        public IgdbMetadataSettings()
        {
        }

        public IgdbMetadataSettings(IgdbMetadataPlugin plugin)
        {
            this.plugin = plugin;

            var settings = plugin.LoadPluginSettings<IgdbMetadataSettings>();
            if (settings != null)
            {
                LoadValues(settings);
            }
        }

        public void BeginEdit()
        {
            editingClone = this.GetClone();
        }

        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = null;
            return true;
        }

        private void LoadValues(IgdbMetadataSettings source)
        {
            source.CopyProperties(this, false, null, true);
        }
    }
}
