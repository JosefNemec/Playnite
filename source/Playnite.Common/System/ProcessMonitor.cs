using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using System.IO;
using Playnite.Common.System;

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

        public async void WatchProcessTree(Process process)
        {
            await WatchProcess(process);
        }

        public async void WatchDirectoryProcesses(string directory, bool alreadyRunning)
        {
            await WatchDirectory(directory, alreadyRunning);
        }

        public void StopWatching()
        {
            watcherToken?.Cancel();
        }

        private async Task WatchDirectory(string directory, bool alreadyRunning)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Cannot watch directory processes, {directory} not found.");
            }

            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
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
                    var processes = Process.GetProcesses().Where(a => a.SessionId != 0);
                    foreach (var process in processes)
                    {
                        if (process.TryGetMainModuleFileName(out var procPath))
                        {
                            if (procPath.IndexOf(directory, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                processFound = true;
                                processStarted = true;
                                break;
                            }
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
            });
        }            

        private async Task WatchProcess(Process process)
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

                    var processes = Process.GetProcesses().Where(a => a.SessionId != 0);
                    var runningIds = new List<int>();
                    foreach (var proc in processes)
                    {
                        if (proc.TryGetParentId(out var parent))
                        {
                            if (ids.Contains(parent) && !ids.Contains(proc.Id))
                            {
                                ids.Add(proc.Id);
                            }
                        }

                        if (ids.Contains(proc.Id))
                        {
                            runningIds.Add(proc.Id);
                        }
                    }

                    ids = runningIds;
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
