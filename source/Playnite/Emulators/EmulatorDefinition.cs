using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.Emulators
{
    public class EmulatorDefinition
    {
        public string Name
        {
            get; set;
        }

        public List<string> Platforms
        {
            get; set;
        }

        public string DefaultArguments
        {
            get; set;
        }

        public string ExecutableLookup
        {
            get; set;
        }       

        public static string DefinitionsPath
        {
            get => Path.Combine(Paths.ProgramFolder, "Emulators", "Definitions.yaml");
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<EmulatorDefinition> GetDefinitions()
        {
            if (!File.Exists(DefinitionsPath))
            {
                throw new Exception("Emulator definitions file not found.");
            }

            var deserializer = new DeserializerBuilder().Build();
            var definitions = deserializer.Deserialize<List<EmulatorDefinition>>(File.ReadAllText(DefinitionsPath));
            return definitions;
        }
    }
}
