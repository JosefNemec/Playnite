using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Common.System;
using Playnite.SDK.Models;

namespace GogLibrary.Models
{
    public enum ActionType
    {
        FileTask,
        URLTask
    }

    public class GogGameActionInfo
    {
        public class Task
        {
            public bool isPrimary;
            public ActionType type;
            public string path;
            public string workingDir;
            public string arguments;
            public string link;
            public string name;

            public GameAction ConvertToGenericTask(string installDirectory)
            {
                return new GameAction()
                {
                    Arguments = arguments,
                    Name = string.IsNullOrEmpty(name) ? "Play" : name,
                    Path = type == ActionType.FileTask ? Paths.FixSeparators(Path.Combine(@"{InstallDir}", path)) : link,
                    WorkingDir = Paths.FixSeparators(Path.Combine(@"{InstallDir}", (workingDir ?? string.Empty))),
                    Type = type == ActionType.FileTask ? GameActionType.File : GameActionType.URL
                };
            }
        }

        public string gameId;
        public string rootGameId;
        public bool standalone;
        public string dependencyGameId;
        public string language;
        public string name;
        public List<Task> playTasks;
        public List<Task> supportTasks;

        public Task DefaultTask
        {
            get
            {
                return playTasks.First(a => a.isPrimary);
            }
        }

    }
}
