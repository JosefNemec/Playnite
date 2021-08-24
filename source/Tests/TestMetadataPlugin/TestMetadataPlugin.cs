using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    public class TestMetadataProvider : OnDemandMetadataProvider
    {
        public override List<MetadataField> AvailableFields => new List<MetadataField>
        {
            MetadataField.Links
        };

        public override List<Link> GetLinks(GetMetadataFieldArgs args)
        {
            return new List<Link>
            {
                new Link("Playnite website", "https://playnite.link/"),
                null
            };
        }
    }

    public class TestMetadataPlugin : MetadataPlugin
    {
        public override string Name => "TestMetadataPlugin";
        public override Guid Id { get; } = Guid.Parse("A51194CD-AA44-47A0-8B89-D1FD544DD9C9");
        public override List<MetadataField> SupportedFields => new List<MetadataField>
        {
            MetadataField.Links
        };

        public TestMetadataPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new TestMetadataProvider();
        }
    }
}
