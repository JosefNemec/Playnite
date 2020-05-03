using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite
{
    public static class GameExtensions
    {
        public static string GetDefaultIcon(this Game game, PlayniteSettings settings, GameDatabase database, LibraryPlugin plugin)
        {
            if (settings.DefaultIconSource == DefaultIconSourceOptions.None)
            {
                return null;
            }
            else if (settings.DefaultIconSource == DefaultIconSourceOptions.Library && plugin?.LibraryIcon.IsNullOrEmpty() == false)
            {
                return plugin.LibraryIcon;
            }
            else if (settings.DefaultIconSource == DefaultIconSourceOptions.Platform && game.Platform?.Icon.IsNullOrEmpty() == false)
            {
                return database.GetFullFilePath(game.Platform.Icon);
            }

            return null;
        }

        public static Game GetGameFromExecutable(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Cannot create game from executable, {path} not found.");
            }

            var game = new Game();
            if (string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase))
            {
                var prog = Programs.ParseShortcut(path);
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

                game.Name = Path.GetFileNameWithoutExtension(path);
                game.InstallDirectory = prog.WorkDir.IsNullOrEmpty() ? fileInfo.Directory.FullName : prog.WorkDir;
                game.PlayAction = new GameAction()
                {
                    Type = GameActionType.File,
                    WorkingDir = ExpandableVariables.InstallationDirectory,
                    Path = fileInfo.FullName.Substring(game.InstallDirectory.Length).Trim(Path.DirectorySeparatorChar),
                    Arguments = prog.Arguments
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
            else
            {
                var file = new FileInfo(path);
                var versionInfo = FileVersionInfo.GetVersionInfo(path);
                var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : new DirectoryInfo(file.DirectoryName).Name;
                game.Name = programName;
                game.InstallDirectory = file.DirectoryName;
                game.PlayAction = new GameAction()
                {
                    Type = GameActionType.File,
                    WorkingDir = ExpandableVariables.InstallationDirectory,
                    Path = file.Name
                };
            };

            game.IsInstalled = true;
            return game;
        }

        public static string ExpandVariables(this Game game, string inputString, bool fixSeparators = false)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }

            var result = inputString;
            result = result.Replace(ExpandableVariables.InstallationDirectory, game.InstallDirectory);
            result = result.Replace(ExpandableVariables.InstallationDirName, Path.GetFileName(Path.GetDirectoryName(game.InstallDirectory)));
            result = result.Replace(ExpandableVariables.ImagePath, game.GameImagePath);
            result = result.Replace(ExpandableVariables.ImageNameNoExtension, Path.GetFileNameWithoutExtension(game.GameImagePath));
            result = result.Replace(ExpandableVariables.ImageName, Path.GetFileName(game.GameImagePath));
            result = result.Replace(ExpandableVariables.PlayniteDirectory, PlaynitePaths.ProgramPath);
            result = result.Replace(ExpandableVariables.Name, game.Name);
            result = result.Replace(ExpandableVariables.Platform, game.Platform?.Name);
            result = result.Replace(ExpandableVariables.PluginId, game.PluginId.ToString());
            result = result.Replace(ExpandableVariables.GameId, game.GameId);
            result = result.Replace(ExpandableVariables.DatabaseId, game.Id.ToString());
            result = result.Replace(ExpandableVariables.Version, game.Version);
            return fixSeparators ? Paths.FixSeparators(result) : result;
        }

        public static string ExpandVariables(this GameInfo game, string inputString, bool fixSeparators = false)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }

            var result = inputString;
            result = result.Replace(ExpandableVariables.InstallationDirectory, game.InstallDirectory);
            result = result.Replace(ExpandableVariables.InstallationDirName, Path.GetFileName(Path.GetDirectoryName(game.InstallDirectory)));
            result = result.Replace(ExpandableVariables.ImagePath, game.GameImagePath);
            result = result.Replace(ExpandableVariables.ImageNameNoExtension, Path.GetFileNameWithoutExtension(game.GameImagePath));
            result = result.Replace(ExpandableVariables.ImageName, Path.GetFileName(game.GameImagePath));
            result = result.Replace(ExpandableVariables.PlayniteDirectory, PlaynitePaths.ProgramPath);
            result = result.Replace(ExpandableVariables.Name, game.Name);
            result = result.Replace(ExpandableVariables.Platform, game.Platform);
            result = result.Replace(ExpandableVariables.GameId, game.GameId);
            result = result.Replace(ExpandableVariables.Version, game.Version);
            return fixSeparators ? Paths.FixSeparators(result) : result;
        }

        public static string GetIdentifierInfo(this Game game)
        {
            return $"{game.Name}, {game.Id}, {game.GameId}, {game.PluginId}";
        }

        public static string GetRawExecutablePath(this Game game)
        {
            if (game.PlayAction == null)
            {
                return null;
            }

            var playAction = game.PlayAction.ExpandVariables(game);
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
    }
}
