using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers
{
    public interface IGameStateMonitor : IDisposable
    {
        event EventHandler GameUninstalled;

        event GameInstalledEventHandler GameInstalled;

        void StartMonitoring();

        void StopMonitoring();
    }
}
