using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class GameExtensions
    {
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
                var file = new FileInfo(prog.Path);
                var versionInfo = FileVersionInfo.GetVersionInfo(prog.Path);
                var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : new DirectoryInfo(file.DirectoryName).Name;
                game.Name = programName;
                game.InstallDirectory = prog.WorkDir;
                game.PlayAction = new GameAction()
                {
                    Type = GameActionType.File,
                    WorkingDir = ExpandableVariables.InstallationDirectory,
                    Path = prog.Path.Substring(game.InstallDirectory.Length).Trim(Path.DirectorySeparatorChar),
                    Arguments = prog.Arguments
                };
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
