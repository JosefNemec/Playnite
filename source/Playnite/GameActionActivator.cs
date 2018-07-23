using NLog;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class GameActionActivator
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Process ActivateTask(GameAction task)
        {
            logger.Info($"Activating game task {task}");
            switch (task.Type)
            {
                case GameActionType.File:
                    return ProcessStarter.StartProcess(task.Path, task.Arguments, task.WorkingDir);
                case GameActionType.URL:
                    return ProcessStarter.StartUrl(task.Path);
                case GameActionType.Emulator:
                    throw new Exception("Cannot start emulated game without emulator.");
            }

            return null;
        }

        public static Process ActivateTask(GameAction task, Game gameData)
        {
            logger.Info($"Activating game task {task}");
            switch (task.Type)
            {
                case GameActionType.File:
                    var path = gameData.ExpandVariables(task.Path);
                    var arguments = gameData.ExpandVariables(task.Arguments);
                    var workdir = gameData.ExpandVariables(task.WorkingDir);
                    return ProcessStarter.StartProcess(path, arguments, workdir);
                case GameActionType.URL:
                    var url = gameData.ExpandVariables(task.Path);
                    return ProcessStarter.StartUrl(url);
                case GameActionType.Emulator:
                    throw new Exception("Cannot start emulated game without emulator.");
            }

            return null;
        }

        public static Process ActivateTask(GameAction task, Game gameData, EmulatorProfile config)
        {
            logger.Info($"Activating game task {task}");
            switch (task.Type)
            {
                case GameActionType.File:
                case GameActionType.URL:
                    return ActivateTask(task, gameData);
                case GameActionType.Emulator:
                    if (config == null)
                    {
                        throw new Exception("Cannot start emulated game without emulator.");
                    }

                    var path = gameData.ExpandVariables(config.Executable);
                    var arguments = gameData.ExpandVariables(config.Arguments);
                    if (!string.IsNullOrEmpty(task.AdditionalArguments))
                    {
                        arguments += " " + gameData.ExpandVariables(task.AdditionalArguments);
                    }

                    if (task.OverrideDefaultArgs)
                    {
                        arguments = gameData.ExpandVariables(task.Arguments);
                    }

                    var workdir = gameData.ExpandVariables(config.WorkingDirectory);
                    return ProcessStarter.StartProcess(path, arguments, workdir);
            }

            return null;
        }

        public static Process ActivateTask(GameAction task, Game gameData, List<Emulator> emulators)
        {
            return ActivateTask(task, gameData, GetGameTaskEmulatorConfig(task, emulators));
        }

        public static EmulatorProfile GetGameTaskEmulatorConfig(GameAction task, List<Emulator> emulators)
        {
            if (task.EmulatorId == null || emulators == null)
            {
                return null;
            }

            return emulators.FirstOrDefault(a => a.Id == task.EmulatorId)?.Profiles.FirstOrDefault(a => a.Id == task.EmulatorProfileId);
        }
    }
}
