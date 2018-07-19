using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using System.IO;

namespace Playnite
{
    public class ProcessMonitor : IDisposable
    {
        public delegate void ProcessMonitorEventHandler(object sender, EventArgs args);
        public event ProcessMonitorEventHandler TreeStarted;
        public event ProcessMonitorEventHandler TreeDestroyed;

        private SynchronizationContext execContext;
        private CancellationTokenSource watcherToken;

        public ProcessMonitor()
        {
            execContext = SynchronizationContext.Current;
        }

        public void Dispose()
        {
            StopWatching();
        }

        public void WatchProcessTree(Process process)
        {
            WatchProcess(process);
        }

        public void WatchDirectoryProcesses(string directory, bool alreadyRunning)
        {
            WatchDirectory(directory, alreadyRunning);
        }

        public void StopWatching()
        {
            watcherToken?.Cancel();
        }

        private async void WatchDirectory(string directory, bool alreadyRunning)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Cannot watch directory processes, {directory} not found.");
            }

            var executables = Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories);
            if (executables.Count() == 0)
            {
                throw new Exception($"Cannot watch directory processes {directory}, no executables found.");
            }

            var procNames = executables.Select(a => Path.GetFileName(a)).ToList();
            watcherToken = new CancellationTokenSource();            

            await Task.Run(async () =>
            {
                var query = $"Select * From Win32_Process";
                using (var mos = new ManagementObjectSearcher(query))
                {
                    var startedCalled = false;
                    var processStarted = false;

                    while (true)
                    {
                        if (watcherToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var processFound = false;
                        foreach (ManagementObject mo in mos.Get())
                        {
                            var name = mo["Name"].ToString();
                            if (procNames.Contains(name))
                            {
                                processFound = true;
                                processStarted = true;
                                break;
                            }
                        }
                        
                        if (!alreadyRunning && processFound && !startedCalled)
                        {
                            OnTreeStarted();
                            startedCalled = true;
                        }

                        if (!processFound && processStarted)
                        {
                            OnTreeDestroyed();
                            return;
                        }

                        await Task.Delay(3000);
                    }
                }
            });
        }            

        private async void WatchProcess(Process process)
        {
            watcherToken = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                var ids = new List<int>() { process.Id };

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (ids.Count == 0)
                    {
                        OnTreeDestroyed();
                        return;
                    }

                    // Check for existing childs
                    foreach (var id in ids.ToList())
                    {
                        var query = $"Select * From Win32_Process Where ParentProcessID={id}";
                        using (var mos = new ManagementObjectSearcher(query))
                        {
                            foreach (ManagementObject mo in mos.Get())
                            {
                                var childId = Convert.ToInt32(mo["ProcessID"]);
                                if (!ids.Contains(childId))
                                {
                                    ids.Add(childId);
                                }

                                mo.Dispose();
                            }
                        }
                    }

                    // Check if processes are still running
                    foreach (var id in ids.ToList())
                    {
                        var query = $"Select * From Win32_Process Where ProcessID={id}";
                        using (var mos = new ManagementObjectSearcher(query))
                        {
                            using (var procs = mos.Get())
                            {
                                if (procs.Count == 0)
                                {
                                    ids.Remove(id);
                                }
                            }
                        }
                    }

                    await Task.Delay(500);
                }
            });
        }

        private void OnTreeStarted()
        {
            execContext.Post((a) => TreeStarted?.Invoke(this, EventArgs.Empty), null);
        }

        private void OnTreeDestroyed()
        {
            execContext.Post((a) => TreeDestroyed?.Invoke(this, EventArgs.Empty), null);
        }
    }
}
