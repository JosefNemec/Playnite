using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Playnite.Models;
using Playnite.SDK.Models;

namespace Playnite.Providers.GOG
{
    public class GogGameController : GameController
    {
        private FileSystemWatcher fileWatcher;
        private Settings settings;
        private GogLibrary gog = new GogLibrary();

        public GogGameController(Game game) : base(game)
        {
            if (game.Provider != Provider.GOG)
            {
                throw new Exception("Cannot use non-GOG game for GOG game controller.");
            }
        }

        public GogGameController(Game game, Settings settings) : this(game)
        {
            this.settings = settings;
        }

        public override void ReleaseResources()
        {
            base.ReleaseResources();
            fileWatcher?.Dispose();
        }

        public override void Play(List<Emulator> emulators)
        {
            ReleaseResources();
            if (settings?.GOGSettings.RunViaGalaxy == true)
            {
                OnStarting(this, new GameControllerEventArgs(this, 0));
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.TreeDestroyed += Monitor_TreeDestroyed;
                var args = string.Format(@"/gameId={0} /command=runGame /path=""{1}""", Game.ProviderId, Game.InstallDirectory);
                var proc = ProcessStarter.StartProcess(Path.Combine(GogSettings.InstallationPath, "GalaxyClient.exe"), args);
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
            }
            else
            {
                base.Play(emulators);
            }
        }

        public override void Install()
        {
            ReleaseResources();
            Process.Start(@"goggalaxy://openGameView/" + Game.ProviderId);
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            var uninstaller = Path.Combine(Game.InstallDirectory, "unins000.exe");
            if (!File.Exists(uninstaller))
            {
                throw new FileNotFoundException("Uninstaller not found.");
            }

            Process.Start(uninstaller);
            var infoFile = string.Format("goggame-{0}.info", Game.ProviderId);
            if (File.Exists(Path.Combine(Game.InstallDirectory, infoFile)))
            {
                fileWatcher = new FileSystemWatcher()
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Path = Game.InstallDirectory,
                    Filter = Path.GetFileName(infoFile)
                };

                fileWatcher.Deleted += FileWatcher_Deleted;
                fileWatcher.EnableRaisingEvents = true;
            }
            else
            {
                OnUninstalled(this, new GameControllerEventArgs(this, 0));
            }
        }

        private void ProcMon_TreeStarted(object sender, EventArgs args)
        {
            OnStarted(this, new GameControllerEventArgs(this, 0));
        }

        private void Monitor_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }

        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Dispose();
            OnUninstalled(this, new GameControllerEventArgs(this, 0));
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {               
                var stopWatch = Stopwatch.StartNew();
                
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var games = gog.GetInstalledGames();
                    var game = games.FirstOrDefault(a => a.ProviderId == Game.ProviderId);
                    if (game != null)
                    {
                        stopWatch.Stop();
                        Game.PlayTask = game.PlayTask;
                        Game.OtherTasks = game.OtherTasks;
                        Game.InstallDirectory = game.InstallDirectory;
                        OnInstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        return;
                    }

                    await Task.Delay(2000);
                }
            });
        }
    }
}
