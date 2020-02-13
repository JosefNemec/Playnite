using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _namespace_
{
    public class _name_Provider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;
        private readonly _name_ plugin;

        public override List<MetadataField> AvailableFields => throw new NotImplementedException();

        public _name_Provider(MetadataRequestOptions options, _name_ plugin)
        {
            this.options = options;
            this.plugin = plugin;
        }

        // Override additional methods based on supported metadata fields.
        public override string GetDescription()
        {
            return options.GameData.Name + " description";
        }
    }
}