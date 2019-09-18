using Newtonsoft.Json;
using NLog;
using Playnite.Common;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Emulators
{
    public class ScannedEmulatorProfile : EmulatorProfile
    {
        [JsonIgnore]
        public EmulatorDefinitionProfile ProfileDefinition
        {
            get; set;
        }

        public ScannedEmulatorProfile() : base()
        {
        }
    }

    public class ScannedEmulator : Emulator
    {
        public new ObservableCollection<ScannedEmulatorProfile> Profiles
        {
            get; set;
        }

        public ScannedEmulator(string name, IEnumerable<ScannedEmulatorProfile> profiles)
        {
            Profiles = new ObservableCollection<ScannedEmulatorProfile>(profiles);
            Name = name;
        }
    }

    public class EmulatorFinder
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static async Task<List<ScannedEmulator>> SearchForEmulators(DirectoryInfoBase path, List<EmulatorDefinition> definitions, CancellationTokenSource cancelToken = null)
        {
            return await Task.Run(() =>
            {
                logger.Info($"Looking for emulators in {path}, using {definitions.Count} definitions.");
                var emulators = new Dictionary<EmulatorDefinition, List<ScannedEmulatorProfile>>();

                var fileEnumerator = new SafeFileEnumerator(path, "*.exe", SearchOption.AllDirectories);
                foreach (var file in fileEnumerator)
                {
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        return null;
                    }

                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    foreach (var definition in definitions)
                    {
                        foreach (var defProfile in definition.Profiles)
                        {
                            var reqMet = true;
                            var folder = Path.GetDirectoryName(file.FullName);
                            var regex = new Regex(defProfile.ExecutableLookup, RegexOptions.IgnoreCase);
                            if (regex.IsMatch(file.Name))
                            {
                                if (defProfile.RequiredFiles?.Any() == true)
                                {
                                    foreach (var reqFile in defProfile.RequiredFiles)
                                    {
                                        if (!File.Exists(Path.Combine(folder, reqFile)))
                                        {
                                            reqMet = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                reqMet = false;
                            }

                            if (reqMet)
                            {
                                var emuProfile = new ScannedEmulatorProfile()
                                {
                                    Name = defProfile.Name,
                                    Arguments = defProfile.DefaultArguments,
                                    Executable = file.FullName,
                                    WorkingDirectory = folder,
                                    ImageExtensions = defProfile.ImageExtensions,
                                    ProfileDefinition = defProfile
                                };

                                if (!emulators.ContainsKey(definition))
                                {
                                    emulators.Add(definition, new List<ScannedEmulatorProfile>());
                                }

                                emulators[definition].Add(emuProfile);
                            }
                        }
                    }
                }

                var result = new List<ScannedEmulator>();
                foreach (var key in emulators.Keys)
                {
                    result.Add(new ScannedEmulator(key.Name, emulators[key]));
                }

                return result;
            });
        }

        public static async Task<List<ScannedEmulator>> SearchForEmulators(string path, List<EmulatorDefinition> definitions, CancellationTokenSource cancelToken = null)
        {
            return await SearchForEmulators(new DirectoryInfo(path), definitions, cancelToken);
        }

        public static async Task<List<Game>> SearchForGames(DirectoryInfoBase path, EmulatorProfile profile, CancellationTokenSource cancelToken = null)
        {
            return await Task.Run(() =>
            {
                logger.Info($"Looking for games in {path}, using {profile.Name} emulator profile.");
                if (!profile.ImageExtensions.HasNonEmptyItems())
                {
                    throw new Exception("Cannot scan for games, emulator doesn't support any file types.");
                }

                var games = new List<Game>();
                var fileEnumerator = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);
                foreach (var file in fileEnumerator)
                {
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        return null;
                    }

                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    foreach (var extension in profile.ImageExtensions)
                    {
                        if (extension.IsNullOrEmpty())
                        {
                            continue;
                        }

                        if (string.Equals(file.Extension.TrimStart('.'), extension.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            var newGame = new Game()
                            {
                                Name = StringExtensions.NormalizeGameName(StringExtensions.GetPathWithoutAllExtensions(Path.GetFileName(file.Name))),
                                GameImagePath = file.FullName,
                                InstallDirectory = Path.GetDirectoryName(file.FullName)
                            };

                            games.Add(newGame);
                        }
                    }
                }

                return games;
            });
        }

        public static async Task<List<Game>> SearchForGames(string path, EmulatorProfile profile, CancellationTokenSource cancelToken = null)
        {
            return await SearchForGames(new DirectoryInfo(path), profile, cancelToken);
        }
    }
}
