using Playnite;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class OriginClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => Origin.Icon;

        public override bool IsInstalled => Origin.IsInstalled;

        public override void Open()
        {
            Origin.StartClient();
        }

        public override void Shutdown()
        {
            var mainProc = Process.GetProcessesByName("Origin").FirstOrDefault();
            if (mainProc == null)
            {
                logger.Info("Origin client is no longer running, no need to shut it down.");
                return;
            }
            
            var res = Interop.SendMessage(mainProc.MainWindowHandle, Interop.WM_QUERYENDSESSION, 0, new IntPtr(Interop.ENDSESSION_CLOSEAPP));
            if ((uint)res == 0)
            {
                logger.Error("Failed to close Origin gracefully, shutting it down.");
                var procRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {mainProc.Id}", null, out var stdOut, out var stdErr);
                if (procRes != 0)
                {
                    logger.Error($"Failed to close Origin client: {procRes}, {stdErr}");
                }
            }
            else
            {
                Interop.SendMessage(mainProc.MainWindowHandle, Interop.WM_ENDSESSION, 0, new IntPtr(Interop.ENDSESSION_CLOSEAPP));
            }
        }
    }
}
