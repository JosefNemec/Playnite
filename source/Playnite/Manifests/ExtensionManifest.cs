using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite
{
    public enum ExtensionType
    {
        GenericPlugin,
        GameLibrary,
        Script,
        MetadataProvider
    }

    public class BaseExtensionManifest
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public List<Link> Links { get; set; }

        [YamlIgnore]
        public string DirectoryPath { get; set; }

        [YamlIgnore]
        public string DirectoryName { get; set; }

        [YamlIgnore]
        public string DescriptionPath { get; set; }

        public void VerifyManifest()
        {
            if (!System.Version.TryParse(Version, out var extver))
            {
                throw new Exception("Extension version string must be a real version!");
            }
        }
    }

    public class ExtensionManifest : BaseExtensionManifest
    {
        [YamlIgnore]
        public bool IsExternalDev { get; set; }

        //[YamlIgnore]
        //public bool IsCompatible { get; } = false;

        public string Module { get; set; }

        public string Icon { get; set; }

        public ExtensionType Type { get; set; }

        public ExtensionManifest()
        {
        }

        public static ExtensionManifest FromFile(string descriptorPath)
        {
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            var description = deserializer.Deserialize<ExtensionManifest>(File.ReadAllText(descriptorPath));
            description.DescriptionPath = descriptorPath;
            description.DirectoryPath = Path.GetDirectoryName(descriptorPath);
            description.DirectoryName = Path.GetFileName(description.DirectoryPath);
            return description;
        }
    }
}