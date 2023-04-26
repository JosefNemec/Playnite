using Playnite.Common;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite
{
    public static class EmulatorProfileExtensions
    {
        public static CustomEmulatorProfile ExpandVariables(this CustomEmulatorProfile profile, Game game, string emulatorDir, string romPath)
        {
            var g = game.GetCopy();
            g.Roms = new System.Collections.ObjectModel.ObservableCollection<GameRom> { new GameRom("", romPath) };
            var expaded = profile.GetCopy();
            expaded.Arguments = g.ExpandVariables(expaded.Arguments, false, emulatorDir);
            expaded.WorkingDirectory = g.ExpandVariables(expaded.WorkingDirectory, true, emulatorDir);
            expaded.Executable = g.ExpandVariables(expaded.Executable, true, emulatorDir);
            expaded.TrackingPath = g.ExpandVariables(expaded.TrackingPath, true, emulatorDir);
            return expaded;
        }
    }

    public static class GameActionExtensions
    {
        public static GameAction ExpandVariables(this GameAction action, Game game)
        {
            var expaded = action.GetCopy();
            expaded.AdditionalArguments = game.ExpandVariables(expaded.AdditionalArguments);
            expaded.Arguments = game.ExpandVariables(expaded.Arguments);
            expaded.WorkingDir = game.ExpandVariables(expaded.WorkingDir, true);
            expaded.TrackingPath = game.ExpandVariables(expaded.TrackingPath, true);
            if (expaded.Type != GameActionType.URL)
            {
                expaded.Path = game.ExpandVariables(expaded.Path, true);
            }

            return expaded;
        }
    }

    public static class GameExtensions
    {
        private static ILogger logger = LogManager.GetLogger();

        public static Game GetGameFromExecutable(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Cannot create game from executable, {path} not found.");
            }

            var game = new Game();
            if (string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase))
            {
                var prog = Programs.GetLnkShortcutData(path);
                var fileInfo = new FileInfo(prog.Path);
                if (!fileInfo.Exists && prog.Path.Contains("Program Files (x86)"))
                {
                    var newPath = prog.Path.Replace("Program Files (x86)", "Program Files");
                    if (File.Exists(newPath))
                    {
                        fileInfo = new FileInfo(newPath);
                        if (prog.WorkDir.Contains("Program Files (x86)"))
                        {
                            prog.WorkDir.Replace("Program Files (x86)", "Program Files");
                        }
                    }
                }

                game.GameId = path.MD5();
                game.Name = Path.GetFileNameWithoutExtension(path);
                game.InstallDirectory = prog.WorkDir.IsNullOrEmpty() ? fileInfo.Directory.FullName : prog.WorkDir;
                game.GameActions = new System.Collections.ObjectModel.ObservableCollection<GameAction>
                {
                    new GameAction()
                    {
                        Type = GameActionType.File,
                        WorkingDir = ExpandableVariables.InstallationDirectory,
                        Path = fileInfo.FullName.Replace(game.InstallDirectory.EndWithDirSeparator(), ExpandableVariables.InstallationDirectory.EndWithDirSeparator()),
                        Arguments = prog.Arguments,
                        IsPlayAction = true,
                        Name = game.Name
                    }
                };

                if (!prog.Icon.IsNullOrEmpty())
                {
                    var iconPath = Regex.Replace(prog.Icon, @",\d+$", "");
                    if (File.Exists(iconPath))
                    {
                        game.Icon = iconPath;
                    }
                    else if (iconPath.Contains("Program Files (x86)"))
                    {
                        iconPath = iconPath.Replace("Program Files (x86)", "Program Files");
                        if (File.Exists(iconPath))
                        {
                            game.Icon = iconPath;
                        }
                    }
                }
            }
            else if (string.Equals(Path.GetExtension(path), ".url", StringComparison.OrdinalIgnoreCase))
            {
                var urlData = IniParser.Parse(File.ReadAllLines(path));
                var shortcut = urlData["InternetShortcut"];
                if (shortcut == null)
                {
                    throw new Exception("URL file doesn't have shortcut definition section.");
                }

                game.Name = Path.GetFileNameWithoutExtension(path);
                game.Icon = shortcut["IconFile"];
                game.GameActions = new System.Collections.ObjectModel.ObservableCollection<GameAction>
                {
                    new GameAction()
                    {
                        Type = GameActionType.URL,
                        Path = shortcut["URL"],
                        IsPlayAction = true,
                        Name = game.Name
                    }
                };
            }
            else
            {
                var file = new FileInfo(path);
                var versionInfo = FileVersionInfo.GetVersionInfo(path);
                var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : new DirectoryInfo(file.DirectoryName).Name;
                game.Name = programName;
                game.InstallDirectory = file.DirectoryName;
                game.GameActions = new System.Collections.ObjectModel.ObservableCollection<GameAction>
                {
                    new GameAction()
                    {
                        Type = GameActionType.File,
                        WorkingDir = ExpandableVariables.InstallationDirectory,
                        Path = file.FullName.Replace(game.InstallDirectory.EndWithDirSeparator(), ExpandableVariables.InstallationDirectory.EndWithDirSeparator()),
                        IsPlayAction = true,
                        Name = game.Name
                    }
                };
            };

            game.IsInstalled = true;
            return game;
        }

        public static Game ExpandGame(this Game game, bool fixSeparators = false, string emulatorDir = null, string romPath = null)
        {
            var g = game.GetCopy();
            g.InstallDirectory = g.StringExpand(g.InstallDirectory, fixSeparators, emulatorDir, romPath);
            g.Roms.ForEach(rom => rom.Path = g.StringExpand(rom.Path, fixSeparators, emulatorDir, romPath));
            return g;
        }

        public static GameMetadata ExpandGame(this GameMetadata game)
        {
            var g = game.GetClone();
            g.InstallDirectory = g.StringExpand(g.InstallDirectory);
            g.Roms.ForEach(rom => rom.Path = g.StringExpand(rom.Path));
            return g;
        }

        public static string ExpandVariables(this Game game, string inputString, bool fixSeparators = false, string emulatorDir = null, string romPath = null)
        {
            var g = game.ExpandGame(fixSeparators, emulatorDir, romPath);
            return StringExpand(g, inputString, fixSeparators, emulatorDir, romPath);
        }

        // TODO rework this whole mess into something better and more maintainable :|
        private static string StringExpand(this Game game, string inputString, bool fixSeparators = false, string emulatorDir = null, string romPath = null)
        {
            if (string.IsNullOrWhiteSpace(inputString) || !inputString.Contains('{'))
            {
                return inputString;
            }

            var result = inputString;
            if (!game.InstallDirectory.IsNullOrWhiteSpace())
            {
                result = result.Replace(ExpandableVariables.InstallationDirectory, game.InstallDirectory);
                result = result.Replace(ExpandableVariables.InstallationDirName, game.InstallDirectory.Split(Paths.DirectorySeparators, StringSplitOptions.RemoveEmptyEntries).LastOrDefault());
            }

            if (romPath.IsNullOrEmpty() && game.Roms.HasItems())
            {
                var customPath = game.Roms[0].Path;
                if (!customPath.IsNullOrEmpty())
                {
                    result = result.Replace(ExpandableVariables.ImagePath, customPath);
                    result = result.Replace(ExpandableVariables.ImageNameNoExtension, Path.GetFileNameWithoutExtension(customPath));
                    result = result.Replace(ExpandableVariables.ImageName, Path.GetFileName(customPath));
                }
            }
            else if (!romPath.IsNullOrEmpty())
            {
                result = result.Replace(ExpandableVariables.ImagePath, romPath);
                result = result.Replace(ExpandableVariables.ImageNameNoExtension, Path.GetFileNameWithoutExtension(romPath));
                result = result.Replace(ExpandableVariables.ImageName, Path.GetFileName(romPath));
            }

            result = result.Replace(ExpandableVariables.PlayniteDirectory, PlaynitePaths.ProgramPath);
            result = result.Replace(ExpandableVariables.Name, game.Name);
            result = result.Replace(ExpandableVariables.PluginId, game.PluginId.ToString());
            result = result.Replace(ExpandableVariables.GameId, game.GameId);
            result = result.Replace(ExpandableVariables.DatabaseId, game.Id.ToString());
            result = result.Replace(ExpandableVariables.Version, game.Version);
            result = result.Replace(ExpandableVariables.EmulatorDirectory, emulatorDir ?? string.Empty);
            var plats = game.Platforms;
            if (plats.HasItems())
            {
                result = result.Replace(ExpandableVariables.Platform, plats?[0].Name);
            }

            return fixSeparators ? Paths.FixSeparators(result) : result;
        }

        public static string ExpandVariables(this GameMetadata game, string inputString, bool fixSeparators = false)
        {
            var g = game.ExpandGame();
            return StringExpand(g, inputString, fixSeparators);
        }

        private static string StringExpand(this GameMetadata game, string inputString, bool fixSeparators = false)
        {
            if (string.IsNullOrEmpty(inputString) || !inputString.Contains('{'))
            {
                return inputString;
            }

            var result = inputString;
            if (!game.InstallDirectory.IsNullOrWhiteSpace())
            {
                result = result.Replace(ExpandableVariables.InstallationDirectory, game.InstallDirectory);
                result = result.Replace(ExpandableVariables.InstallationDirName, game.InstallDirectory.Split(Paths.DirectorySeparators, StringSplitOptions.RemoveEmptyEntries).LastOrDefault());
            }

            if (game.Roms.HasItems())
            {
                var romPath = game.Roms[0].Path;
                if (!romPath.IsNullOrEmpty())
                {
                    result = result.Replace(ExpandableVariables.ImagePath, romPath);
                    result = result.Replace(ExpandableVariables.ImageNameNoExtension, Path.GetFileNameWithoutExtension(romPath));
                    result = result.Replace(ExpandableVariables.ImageName, Path.GetFileName(romPath));
                }
            }

            result = result.Replace(ExpandableVariables.PlayniteDirectory, PlaynitePaths.ProgramPath);
            result = result.Replace(ExpandableVariables.Name, game.Name);
            result = result.Replace(ExpandableVariables.GameId, game.GameId);
            result = result.Replace(ExpandableVariables.Version, game.Version);
            if (game.Platforms.HasItems() && game.Platforms.First() is MetadataNameProperty prop)
            {
                result = result.Replace(ExpandableVariables.Platform, prop.Name);
            }

            return fixSeparators ? Paths.FixSeparators(result) : result;
        }

        public static string GetIdentifierInfo(this Game game)
        {
            return $"{game.Name}, {game.Id}, {game.GameId}, {game.PluginId}";
        }

        public static string GetRawExecutablePath(this Game game)
        {
            try
            {
                var playAction = game.GameActions?.FirstOrDefault(a => a.IsPlayAction && a.Type == GameActionType.File);
                if (playAction == null)
                {
                    return null;
                }

                playAction = playAction.ExpandVariables(game);
                if (playAction.Type == GameActionType.File)
                {
                    if (string.IsNullOrEmpty(playAction.WorkingDir))
                    {
                        if (Paths.IsValidFilePath(playAction.Path))
                        {
                            return Path.GetFullPath(playAction.Path);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (Path.IsPathRooted(playAction.Path))
                        {
                            return playAction.Path;
                        }
                        else
                        {
                            var combined = Path.Combine(playAction.WorkingDir, playAction.Path);
                            return Path.GetFullPath(combined);
                        }
                    }
                }
                else if (playAction.Type == GameActionType.URL)
                {
                    return playAction.Path;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to get executable from game data.");
                return null;
            }
        }

        public static Dictionary<Emulator, List<EmulatorProfile>> GetCompatibleEmulators(this Game game, GameDatabase database)
        {
            var emulators = new Dictionary<Emulator, List<EmulatorProfile>>();
            if (!game.Platforms.HasItems())
            {
                return emulators;
            }

            foreach (var emulator in database.Emulators)
            {
                var profiles = game.GetCompatibleProfiles(emulator);
                if (profiles.HasItems())
                {
                    emulators.Add(emulator, new List<EmulatorProfile>(profiles));
                }
            }

            return emulators;
        }

        public static List<EmulatorProfile> GetCompatibleProfiles(this Game game, Emulator emulator)
        {
            var profiles = new List<EmulatorProfile>();
            if (!game.Platforms.HasItems())
            {
                return profiles;
            }

            foreach (var profile in emulator.CustomProfiles ?? new ObservableCollection<CustomEmulatorProfile>())
            {
                if (profile.Platforms?.Intersect(game.PlatformIds).HasItems() == true)
                {
                    profiles.Add(profile);
                }
            }

            foreach (var profile in emulator.BuiltinProfiles ?? new ObservableCollection<BuiltInEmulatorProfile>())
            {
                var profDef = Emulation.GetProfile(emulator.BuiltInConfigId, profile.BuiltInProfileName);
                if (profDef == null)
                {
                    continue;
                }

                if (game.Platforms.Where(a => !a.SpecificationId.IsNullOrEmpty()).Any(a => profDef.Platforms.Contains(a.SpecificationId)))
                {
                    profiles.Add(profile);
                }
            }

            return profiles;
        }
    }
}
