using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMetadataPlugin
{
    public class CustomMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;

        public override List<MetadataField> AvailableFields => throw new NotImplementedException();

        public CustomMetadataProvider(MetadataRequestOptions options, CustomMetadataPlugin plugin)
        {
            this.options = options;
        }

        // Override additional methods based on supported metadata fields.
        public override string GetDescription()
        {
            return options.GameData.Name + " description";
        }
    }
}
