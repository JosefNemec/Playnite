using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers
{
    public enum GameStateMonitorType
    {
        Install,
        Uninstall
    }

    public interface IGameStateMonitor : IDisposable
    {
        event EventHandler GameUninstalled;

        event GameInstalledEventHandler GameInstalled;

        void StartInstallMonitoring();

        void StartUninstallMonitoring();

        void StopMonitoring();
    }
}
