using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.API
{
    public class ScriptFunctionDescription
    {
        public string Description { get; set; }
        public string FunctionName { get; set; }
    }

    public class ScriptExtensionDescription : ExtensionManifest
    {
        public List<ScriptFunctionDescription> Functions { get; set; }

        public ScriptExtensionDescription()
        {
        }

        public new static ScriptExtensionDescription FromFile(string descriptorPath)
        {
            var deserializer = new DeserializerBuilder().Build();
            var description = deserializer.Deserialize<ScriptExtensionDescription>(File.ReadAllText(descriptorPath));
            description.DescriptionPath = descriptorPath;
            description.DirectoryName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(descriptorPath));
            return description;
        }
    }
}
