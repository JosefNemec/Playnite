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

namespace Playnite.API
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

        [YamlIgnore]
        public string LegacyDirId
        {
            get
            {
                if (Name.IsNullOrEmpty())
                {
                    return string.Empty;
                }
                else
                {
                    return Paths.GetSafePathName(Name).Replace(" ", string.Empty) + "_" + (Name + Author).MD5();
                }
            }
        }

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
        public bool IsBuiltInExtension { get; set; }

        [YamlIgnore]
        public bool IsCustomExtension => !IsBuiltInExtension;

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
            if (description.Type == ExtensionType.Script)
            {
                description = deserializer.Deserialize<ScriptExtensionDescription>(File.ReadAllText(descriptorPath));
            }

            description.DescriptionPath = descriptorPath;
            description.DirectoryPath = Path.GetDirectoryName(descriptorPath);
            description.DirectoryName = Path.GetFileNameWithoutExtension(description.DirectoryPath);
            description.IsBuiltInExtension = BuiltinExtensions.BuiltinExtensionFolders.Contains(description.DirectoryName);
            return description;
        }
    }
}