using Playnite.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.Emulators
{

    public class EmulatorDefinitionProfile
    {
        public string Name
        {
            get; set;
        }

        public string DefaultArguments
        {
            get; set;
        }

        public List<string> Platforms
        {
            get; set;
        }

        public List<string> ImageExtensions
        {
            get; set;
        }

        public List<string> RequiredFiles
        {
            get; set;
        }

        public string ExecutableLookup
        {
            get; set;
        }

        public EmulatorProfile ToEmulatorConfig()
        {
            return new EmulatorProfile()
            {
                Arguments = DefaultArguments,
                ImageExtensions = ImageExtensions,
                Name = Name                
            };
        }
    }

    public class EmulatorDefinition
    {
        public string Name
        {
            get; set;
        }
        public string Website
        {
            get; set;
        }

        public List<EmulatorDefinitionProfile> Profiles
        {
            get; set;
        }

        public List<string> AllPlatforms
        {
            get
            {
                return Profiles?.SelectMany(a => a.Platforms).Distinct().OrderBy(a => a).ToList();
            }
        }

        public static string DefinitionsPath
        {
            get => Path.Combine(Paths.ProgramPath, "Emulators", "Definitions.yaml");
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
            var definitions = deserializer.Deserialize<List<EmulatorDefinition>>(File.ReadAllText(DefinitionsPath)).OrderBy(a => a.Name).ToList();
            return definitions;
        }
    }
}
