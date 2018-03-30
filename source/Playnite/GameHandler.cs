using NLog;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class GameHandler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Process ActivateTask(GameTask task)
        {
            switch (task.Type)
            {
                case GameTaskType.File:
                    logger.Info($"Starting process: {task.Path}, {task.Arguments}, {task.WorkingDir}");
                    return ProcessStarter.StartProcess(task.Path, task.Arguments, task.WorkingDir);
                case GameTaskType.URL:
                    logger.Info($"Opening URL {task.Path}");
                    return ProcessStarter.StartUrl(task.Path);
                case GameTaskType.Emulator:
                    throw new Exception("Cannot start emulated game without emulator.");
            }

            return null;
        }

        public static Process ActivateTask(GameTask task, Game gameData)
        {
            switch (task.Type)
            {
                case GameTaskType.File:
                    var path = gameData.ResolveVariables(task.Path);
                    var arguments = gameData.ResolveVariables(task.Arguments);
                    var workdir = gameData.ResolveVariables(task.WorkingDir);
                    logger.Info($"Starting process: {path}, {arguments}, {workdir}");
                    return ProcessStarter.StartProcess(path, arguments, workdir);
                case GameTaskType.URL:
                    var url = gameData.ResolveVariables(task.Path);
                    logger.Info($"Opening URL {url}");
                    return ProcessStarter.StartUrl(url);
                case GameTaskType.Emulator:
                    throw new Exception("Cannot start emulated game without emulator.");
            }

            return null;
        }

        public static Process ActivateTask(GameTask task, Game gameData, EmulatorProfile config)
        {
            switch (task.Type)
            {
                case GameTaskType.File:
                case GameTaskType.URL:
                    return ActivateTask(task, gameData);                    
                case GameTaskType.Emulator:
                    if (config == null)
                    {
                        throw new Exception("Cannot start emulated game without emulator.");
                    }

                    var path = gameData.ResolveVariables(config.Executable);
                    var arguments = gameData.ResolveVariables(config.Arguments);
                    if (!string.IsNullOrEmpty(task.AdditionalArguments))
                    {
                        arguments += " " + gameData.ResolveVariables(task.AdditionalArguments);
                    }

                    if (task.OverrideDefaultArgs)
                    {
                        arguments = gameData.ResolveVariables(task.Arguments);
                    }

                    var workdir = gameData.ResolveVariables(config.WorkingDirectory);
                    logger.Info($"Starting emulator: {path}, {arguments}, {workdir}");
                    return ProcessStarter.StartProcess(path, arguments, workdir);
            }

            return null;
        }

        public static Process ActivateTask(GameTask task, Game gameData, List<Emulator> emulators)
        {
            return ActivateTask(task, gameData, GetGameTaskEmulatorConfig(task, emulators));
        }

        public static EmulatorProfile GetGameTaskEmulatorConfig(GameTask task, List<Emulator> emulators)
        {
            if (task.EmulatorId == null || emulators == null)
            {
                return null;
            }

            return emulators.FirstOrDefault(a => a.Id == task.EmulatorId)?.Profiles.FirstOrDefault(a => a.Id == task.EmulatorProfileId);
        }

        public static Game GetMultiGameEditObject(IEnumerable<IGame> games)
        {
            var dummyGame = new Game();
            if (games?.Any() != true)
            {
                return dummyGame;
            }

            var firstGame = games.First();

            var firstName = firstGame.Name;
            if (games.All(a => a.Name == firstName) == true)
            {
                dummyGame.Name = firstName;
            }

            var firstSortingName = firstGame.SortingName;
            if (games.All(a => a.SortingName == firstSortingName) == true)
            {
                dummyGame.SortingName = firstSortingName;
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

            var firstCat = firstGame.Categories;
            if (games.All(a => a.Categories.IsListEqual(firstCat) == true))
            {
                dummyGame.Categories = firstCat;
            }

            var firstTag = firstGame.Tags;
            if (games.All(a => a.Tags.IsListEqual(firstTag) == true))
            {
                dummyGame.Tags = firstTag;
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

            var firstLastActivity = firstGame.LastActivity;
            if (games.All(a => a.LastActivity == firstLastActivity) == true)
            {
                dummyGame.LastActivity = firstLastActivity;
            }

            var firstPlaytime = firstGame.Playtime;
            if (games.All(a => a.Playtime == firstPlaytime) == true)
            {
                dummyGame.Playtime = firstPlaytime;
            }

            var firstAdded = firstGame.Added;
            if (games.All(a => a.Added == firstAdded) == true)
            {
                dummyGame.Added = firstAdded;
            }

            var firstPlayCount = firstGame.PlayCount;
            if (games.All(a => a.PlayCount == firstPlayCount) == true)
            {
                dummyGame.PlayCount = firstPlayCount;
            }

            var firstSeries = firstGame.Series;
            if (games.All(a => a.Series == firstSeries) == true)
            {
                dummyGame.Series = firstSeries;
            }

            var firstVersion = firstGame.Version;
            if (games.All(a => a.Version == firstVersion) == true)
            {
                dummyGame.Version = firstVersion;
            }

            var firstAgeRating = firstGame.AgeRating;
            if (games.All(a => a.AgeRating == firstAgeRating) == true)
            {
                dummyGame.AgeRating = firstAgeRating;
            }

            var firstRegion = firstGame.Region;
            if (games.All(a => a.Region == firstRegion) == true)
            {
                dummyGame.Region = firstRegion;
            }

            var firstSource = firstGame.Source;
            if (games.All(a => a.Source == firstSource) == true)
            {
                dummyGame.Source = firstSource;
            }

            return dummyGame;
        }
    }
}
