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
            MetadataField.Links,
            MetadataField.Platform,
            MetadataField.Region,
            MetadataField.Features
        };

        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {
            yield return new Link("Playnite website", "https://playnite.link/");
            yield return null;
        }

        public override IEnumerable<MetadataProperty> GetPlatforms(GetMetadataFieldArgs args)
        {
            yield return new MetadataSpecProperty("pc_windows");
            yield return new MetadataSpecProperty("Sony PlayStation Vita");
            yield return new MetadataNameProperty("Microsoft Xbox 360");
        }

        public override IEnumerable<MetadataProperty> GetRegions(GetMetadataFieldArgs args)
        {
            yield return new MetadataSpecProperty("newZealand");
            yield return new MetadataSpecProperty("hongKong");
        }

        public override IEnumerable<MetadataProperty> GetFeatures(GetMetadataFieldArgs args)
        {
            yield return new MetadataNameProperty("Test Feature");
        }
    }

    public class TestMetadataPlugin : MetadataPlugin
    {
        public override string Name => "TestMetadataPlugin";
        public override Guid Id { get; } = Guid.Parse("A51194CD-AA44-47A0-8B89-D1FD544DD9C9");
        public override List<MetadataField> SupportedFields => new List<MetadataField>
        {
            MetadataField.Links,
            MetadataField.Platform,
            MetadataField.Region,
            MetadataField.Features
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
