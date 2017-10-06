using NLog;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class GameHandler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void ActivateTask(GameTask task)
        {
            switch (task.Type)
            {
                case GameTaskType.File:
                    logger.Info($"Starting process: {task.Path}, {task.Arguments}, {task.WorkingDir}");
                    ProcessStarter.StartProcess(task.Path, task.Arguments, task.WorkingDir);
                    break;
                case GameTaskType.URL:
                    logger.Info($"Opening URL {task.Path}");
                    ProcessStarter.StartUrl(task.Path);
                    break;
                case GameTaskType.Emulator:
                    throw new Exception("Cannot start emulated game without emulator.");
            }
        }

        public static void ActivateTask(GameTask task, Game gameData)
        {
            switch (task.Type)
            {
                case GameTaskType.File:
                    var path = gameData.ResolveVariables(task.Path);
                    var arguments = gameData.ResolveVariables(task.Arguments);
                    var workdir = gameData.ResolveVariables(task.WorkingDir);
                    logger.Info($"Starting process: {path}, {arguments}, {workdir}");
                    ProcessStarter.StartProcess(path, arguments, workdir);
                    break;
                case GameTaskType.URL:
                    var url = gameData.ResolveVariables(task.Path);
                    logger.Info($"Opening URL {url}");
                    ProcessStarter.StartUrl(url);
                    break;
                case GameTaskType.Emulator:
                    throw new Exception("Cannot start emulated game without emulator.");
            }
        }

        public static void ActivateTask(GameTask task, Game gameData, Emulator emulator)
        {
            switch (task.Type)
            {
                case GameTaskType.File:
                case GameTaskType.URL:
                    ActivateTask(task, gameData);
                    break;
                case GameTaskType.Emulator:
                    if (emulator == null)
                    {
                        throw new Exception("Cannot start emulated game without emulator.");
                    }

                    var path = gameData.ResolveVariables(emulator.Executable);
                    var arguments = gameData.ResolveVariables(emulator.Arguments);
                    if (!string.IsNullOrEmpty(task.AdditionalArguments))
                    {
                        arguments += " " + gameData.ResolveVariables(task.AdditionalArguments);
                    }

                    if (task.OverrideDefaultArgs)
                    {
                        arguments = gameData.ResolveVariables(task.Arguments);
                    }

                    var workdir = gameData.ResolveVariables(emulator.WorkingDirectory);
                    logger.Info($"Starting emulator: {path}, {arguments}, {workdir}");
                    ProcessStarter.StartProcess(path, arguments, workdir);
                    break;
            }
        }

        public static void ActivateTask(GameTask task, Game gameData, List<Emulator> emulators)
        {
            ActivateTask(task, gameData, GetGameTaskEmulator(task, emulators));
        }

        public static Emulator GetGameTaskEmulator(GameTask task, List<Emulator> emulators)
        {
            if (task.EmulatorId == 0 || emulators == null)
            {
                return null;
            }

            return emulators.FirstOrDefault(a => a.Id == task.EmulatorId);
        }

        public static Game GetMultiGameEditObject(IEnumerable<IGame> games)
        {
            var dummyGame = new Game();
            var firstGame = games.First();

            var firstName = firstGame.Name;
            if (games.All(a => a.Name == firstName) == true)
            {
                dummyGame.Name = firstName;
            }

            var firstGenres = firstGame.Genres;
            if (games.All(a => a.Genres.IsListEqual(firstGenres) == true))
            {
                dummyGame.Genres = firstGenres;
            }

            var firstReleaseDate = firstGame.ReleaseDate;
            if (games.All(a => a.ReleaseDate == firstReleaseDate) == true)
            {
                dummyGame.ReleaseDate = firstReleaseDate;
            }

            var firstDeveloper = firstGame.Developers;
            if (games.All(a => a.Developers.IsListEqual(firstDeveloper) == true))
            {
                dummyGame.Developers = firstDeveloper;
            }

            var firstPublisher = firstGame.Publishers;
            if (games.All(a => a.Publishers.IsListEqual(firstPublisher) == true))
            {
                dummyGame.Publishers = firstPublisher;
            }

            var firstTag = firstGame.Categories;
            if (games.All(a => a.Categories.IsListEqual(firstTag) == true))
            {
                dummyGame.Categories = firstTag;
            }

            var firstDescription = firstGame.Description;
            if (games.All(a => a.Description == firstDescription) == true)
            {
                dummyGame.Description = firstDescription;
            }

            var firstPlatform = firstGame.PlatformId;
            if (games.All(a => a.PlatformId == firstPlatform) == true)
            {
                dummyGame.PlatformId = firstPlatform;
            }

            return dummyGame;
        }
    }
}
