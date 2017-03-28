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

        public void Activate()
        {
            if (Type == GameTaskType.URL)
            {
                Process.Start(Path);
            }
            else
            {
                var info = new ProcessStartInfo(Path, Arguments)
                {
                    WorkingDirectory = string.IsNullOrEmpty(WorkingDir) ? (new FileInfo(Path)).Directory.FullName : WorkingDir
                };

                Process.Start(info);
            }
        }
    }
}
