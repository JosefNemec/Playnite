﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using System.IO;
using Playnite.Common.System;
using Playnite.SDK;

namespace Playnite
{
    public class ProcessMonitor : IDisposable
    {
        public delegate void ProcessMonitorEventHandler(object sender, EventArgs args);
        public event ProcessMonitorEventHandler TreeStarted;
        public event ProcessMonitorEventHandler TreeDestroyed;

        private SynchronizationContext execContext;
        private CancellationTokenSource watcherToken;
        private static ILogger logger = LogManager.GetLogger();

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

        public async void WatchDirectoryProcesses(string directory, bool alreadyRunning, bool byProcessNames = false)
        {
            // Get real path in case that original path is symlink or junction point
            var realPath = directory;
            try
            {
                realPath = Paths.GetFinalPathName(directory);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get target path for a directory {directory}");
            }

            if (byProcessNames)
            {
                await WatchDirectoryByProcessNames(realPath, alreadyRunning);
            }
            else
            {
                await WatchDirectory(realPath, alreadyRunning);
            }
        }

        public void StopWatching()
        {
            watcherToken?.Cancel();
        }

        private async Task WatchDirectoryByProcessNames(string directory, bool alreadyRunning)
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
            var startedCalled = false;
            var processStarted = false;
            var failCount = 0;

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var processFound = false;
                try
                {
                    var processes = Process.GetProcesses().Where(a => a.SessionId != 0);
                    foreach (var process in processes)
                    {
                        if (process.TryGetMainModuleFileName(out var procPath))
                        {
                            if (procNames.Contains(Path.GetFileName(procPath)))
                            {
                                processFound = true;
                                processStarted = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e) when (failCount < 5)
                {
                    // This shouldn't happen, but there were some crash reports from Process.GetProcesses
                    failCount++;
                    logger.Error(e, "Watch process.");
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

                await Task.Delay(2000);
            }
        }

        private async Task WatchDirectory(string directory, bool alreadyRunning)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Cannot watch directory processes, {directory} not found.");
            }

            watcherToken = new CancellationTokenSource();      
            var startedCalled = false;
            var processStarted = false;
            var failCount = 0;

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var processFound = false;
                try
                {
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
                }                
                catch (Exception e) when (failCount < 5)
                {
                    // This shouldn't happen, but there were some crash reports from Process.GetProcesses
                    failCount++;
                    logger.Error(e, "Watch process.");
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

                await Task.Delay(2000);
            }
        }            

        private async Task WatchProcess(Process process)
        {
            watcherToken = new CancellationTokenSource();            
            var ids = new List<int>() { process.Id };
            var failCount = 0;

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

                try
                {
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
                }
                catch (Exception e) when (failCount < 5)
                {
                    // This shouldn't happen, but there were some crash reports from Process.GetProcesses
                    failCount++;
                    logger.Error(e, "Watch process.");
                }

                await Task.Delay(500);
            }
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
