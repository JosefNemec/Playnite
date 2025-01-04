using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using System.IO;
using Playnite.SDK;

namespace Playnite.Common
{
    public class MonitorProcess
    {
        private readonly Process process;

        public MonitorProcess(Process process)
        {
            this.process = process;
        }

        public bool IsProcessRunning()
        {
            return !process.HasExited;
        }
    }

    public class MonitorProcessTree
    {
        private List<int> relatedIds = new List<int>();

        public MonitorProcessTree(int originalId)
        {
            relatedIds.Add(originalId);
        }

        public bool IsProcessTreeRunning()
        {
            if (relatedIds.Count == 0)
            {
                return false;
            }

            var runningIds = new List<int>();
            foreach (var proc in Process.GetProcesses().Where(a => a.SessionId != 0))
            {
                if (proc.TryGetParentId(out var parent))
                {
                    if (relatedIds.Contains(parent) && !relatedIds.Contains(proc.Id))
                    {
                        relatedIds.Add(proc.Id);
                    }
                }

                if (relatedIds.Contains(proc.Id))
                {
                    runningIds.Add(proc.Id);
                }
            }

            relatedIds = runningIds;
            return relatedIds.Count > 0;
        }
    }

    public class MonitorProcessNames
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private readonly List<string> procNames = new List<string>();
        private readonly List<string> procNamesNoExt = new List<string>();

        public MonitorProcessNames(string directory)
        {
            var dir = directory;
            try
            {
                dir = Paths.GetFinalPathName(directory);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get target path for a directory {directory}");
            }

            if (FileSystem.DirectoryExists(dir))
            {
                var executables = Directory.GetFiles(dir, "*.exe", SearchOption.AllDirectories);
                procNames = executables.Select(a => Path.GetFileName(a)).ToList();
                procNamesNoExt = executables.Select(a => Path.GetFileNameWithoutExtension(a)).ToList();
            }
        }

        public bool IsTrackable()
        {
            return procNames.Count > 0;
        }

        public int IsProcessRunning()
        {
            foreach (var process in Process.GetProcesses().Where(a => a.SessionId != 0))
            {
                if (process.TryGetMainModuleFileName(out var procPath))
                {
                    if (procNames.Contains(Path.GetFileName(procPath)))
                    {
                        return process.Id; ;
                    }
                }
                else if (procNamesNoExt.Contains(process.ProcessName))
                {
                    return process.Id;
                }
            }

            return 0;
        }
    }

    public class MonitorDirectory
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private readonly string dir;

        public MonitorDirectory(string directory)
        {
            dir = directory;

            try
            {
                dir = Paths.GetFinalPathName(directory);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get target path for a directory {directory}");
            }
        }

        public bool IsTrackable()
        {
            if (dir.IsNullOrWhiteSpace())
            {
                return false;
            }

            return FileSystem.DirectoryExists(dir);
        }

        public int IsProcessRunning()
        {
            foreach (var process in Process.GetProcesses().Where(a => a.SessionId != 0))
            {
                if (process.TryGetMainModuleFileName(out var procPath) &&
                    procPath.IndexOf(dir, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return process.Id;
                }
            }

            return 0;
        }
    }
}
