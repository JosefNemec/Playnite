using Playnite.Common;
using Playnite.SDK;
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
        private static ILogger logger = LogManager.GetLogger();

        public static Process ActivateAction(GameAction action)
        {
            logger.Info($"Activating game action {action}");
            switch (action.Type)
            {
                case GameActionType.File:
                    return ProcessStarter.StartProcess(action.Path, action.Arguments, action.WorkingDir);
                case GameActionType.URL:
                    return ProcessStarter.StartUrl(action.Path);
                case GameActionType.Emulator:
                    throw new Exception("Cannot start emulated game without emulator.");
            }

            return null;
        }

        public static Process ActivateAction(GameAction action, EmulatorProfile config)
        {
            switch (action.Type)
            {
                case GameActionType.File:
                case GameActionType.URL:
                    return ActivateAction(action);
                case GameActionType.Emulator:
                    logger.Info($"Activating game task {action}");
                    if (config == null)
                    {
                        throw new Exception("Cannot start emulated game without emulator.");
                    }

                    var path = config.Executable;
                    var arguments = config.Arguments;
                    if (!string.IsNullOrEmpty(action.AdditionalArguments))
                    {
                        arguments += " " + action.AdditionalArguments;
                    }

                    if (action.OverrideDefaultArgs)
                    {
                        arguments = action.Arguments;
                    }

                    var workdir = config.WorkingDirectory;
                    try
                    {
                        return ProcessStarter.StartProcess(path, arguments, workdir);
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        logger.Error(e, "Failed to start emulator process.");
                        throw new Exception("Emulator process cannot be started, make sure that emulator paths are configured properly.");
                    }
            }

            return null;
        }

        public static EmulatorProfile GetGameActionEmulatorConfig(GameAction action, List<Emulator> emulators)
        {
            if (action.EmulatorId == Guid.Empty || emulators == null)
            {
                return null;
            }

            return emulators.FirstOrDefault(a => a.Id == action.EmulatorId)?.Profiles.FirstOrDefault(a => a.Id == action.EmulatorProfileId);
        }
    }
}
