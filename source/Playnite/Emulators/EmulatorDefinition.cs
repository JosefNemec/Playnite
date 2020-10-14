using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private static readonly ILogger logger = LogManager.GetLogger();

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

        public static string DefinitionsDir
        {
            get => Path.Combine(PlaynitePaths.ProgramPath, "Emulators");
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<EmulatorDefinition> GetDefinitions()
        {
            if (!Directory.Exists(DefinitionsDir))
            {
                return new List<EmulatorDefinition>();
            }

            var defs = new List<EmulatorDefinition>();
            foreach (var file in Directory.GetFiles(DefinitionsDir, "*.yaml", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetFileName(file).Equals("Definitions.yaml", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    defs.Add(Serialization.FromYamlFile<EmulatorDefinition>(file));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to read emulator definition from {file}");
                }
            }

            return defs;
        }
    }
}
