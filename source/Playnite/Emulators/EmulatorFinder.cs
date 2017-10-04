using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Emulators
{
    public class ScannedEmulator
    {
        public Emulator Emulator
        {
            get; set;
        }

        public EmulatorDefinition Definition
        {
            get; set;
        }

        public ScannedEmulator(Emulator emulator, EmulatorDefinition definition)
        {
            Emulator = emulator;
            Definition = definition;
        }
    }

    public class EmulatorFinder
    {
        public static List<ScannedEmulator> SearchForEmulators(string path, List<EmulatorDefinition> definitions)
        {
            var emulators = new List<ScannedEmulator>();
            var fileEnumerator = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);
            foreach (var file in fileEnumerator)
            {
                foreach (var definition in definitions)
                {
                    var regex = new Regex(definition.ExecutableLookup, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(file.Name))
                    {
                        var folder = Path.GetDirectoryName(file.FullName);
                        emulators.Add(new ScannedEmulator(new Emulator(Path.GetFileName(Path.GetDirectoryName(file.FullName)))
                        {
                            WorkingDirectory = folder,
                            Executable = file.FullName,
                            Arguments = definition.DefaultArguments,
                            Name = definition.Name,
                            ImageExtensions = definition.ImageExtensions
                        }, definition));
                    }
                }
            }

            return emulators;
        }

        public static List<IGame> SearchForGames(string path, Emulator emulator)
        {
            if (emulator.ImageExtensions == null)
            {
                throw new Exception("Cannot scan for games, emulator doesn't support any file types.");
            }

            var games = new List<IGame>();
            var fileEnumerator = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);
            foreach (var file in fileEnumerator)
            {
                foreach (var extension in emulator.ImageExtensions)
                {
                    if (string.Equals(file.Extension, extension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var newGame = new Game()
                        {
                            Name = Path.GetFileNameWithoutExtension(file.Name),
                            IsoPath = file.FullName,
                            InstallDirectory = Path.GetDirectoryName(file.FullName)                            
                        };

                        games.Add(newGame);
                    }
                }
            }

            return games;
        }
    }
}
