using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.API
{
    public enum PluginType
    {
        Generic,
        GameLibrary
    }

    public class PluginDescription
    {
        public string Path { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string Assembly { get; set; }

        public PluginType Type { get; set; }

        public PluginDescription()
        {
        }

        public static PluginDescription FromFile(string descriptorPath)
        {
            var deserializer = new DeserializerBuilder().Build();
            var description = deserializer.Deserialize<PluginDescription>(File.ReadAllText(descriptorPath));
            description.Path = descriptorPath;
            return description;
        }
    }
}
