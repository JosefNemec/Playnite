using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CustomMetadataPlugin
{
    public class CustomMetadataPlugin : MetadataPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private CustomMetadataSettings settings { get; set; }

        // Change this ID to unique non-empty GUID
        public override Guid Id { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
                
        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.Description
            // Include addition fields if supported by the metadata source
        };

        // Change to something more appropriate
        public override string Name => "Custom Metadata";

        public CustomMetadataPlugin(IPlayniteAPI api) : base(api)
        {
            settings = new CustomMetadataSettings(this);
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new CustomMetadataProvider(options, this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CustomMetadataSettingsView();
        }
    }
}
