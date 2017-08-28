using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Models
{
    public enum GameTaskType : int
    {
        File = 0,
        URL = 1
    }

    public class GameTask
    {
        public GameTaskType Type
        {
            get;set;
        }

        public bool IsPrimary
        {
            get;set;
        }

        public string Arguments
        {
            get;set;
        }

        public string Path
        {
            get;set;
        }

        public string WorkingDir
        {
            get;set;
        }

        public string Name
        {
            get;set;
        }

        public bool IsBuiltIn
        {
            get;set;
        }

        private void Activate(string path, string arguments, string workDir)
        {
            if (Type == GameTaskType.URL)
            {
                Process.Start(path);
            }
            else
            {
                var info = new ProcessStartInfo(path, arguments)
                {
                    WorkingDirectory = string.IsNullOrEmpty(workDir) ? (new FileInfo(path)).Directory.FullName : workDir
                };

                Process.Start(info);
            }
        }

        public void Activate()
        {
            Activate(Path, Arguments, WorkingDir);
        }

        public void Activate(Game gameData)
        {
            var path = gameData.ResolveVariables(Path);
            var args = gameData.ResolveVariables(Arguments);
            var workDir = gameData.ResolveVariables(WorkingDir);
            Activate(path, args, workDir);
        }
    }
}
