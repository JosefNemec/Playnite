using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Playnite.Common;

namespace Playnite
{
    public enum BatteryChargeLevel
    {
        Critical,
        Low,
        Medium,
        High
    }

    public class ObservablePowerStatus : ObservableObject, IDisposable
    {
        private SynchronizationContext context;
        private CancellationTokenSource watcherToken;
        private Task currentTask;

        public int PercentCharge
        {
            get => (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
        }

        public BatteryChargeLevel Charge
        {
            get
            {
                var charge = PercentCharge;
                if (charge > 85)
                {
                    return BatteryChargeLevel.High;
                }
                else if (charge > 40)
                {
                    return BatteryChargeLevel.Medium;
                }
                else if (charge > 10)
                {
                    return BatteryChargeLevel.Low;
                }
                else
                {
                    return BatteryChargeLevel.Critical;
                }
            }
        }

        public bool IsCharging
        {
            get => SystemInformation.PowerStatus.BatteryChargeStatus.HasFlag(BatteryChargeStatus.Charging);
        }

        public bool IsBatteryAvailable
        {
            get => !SystemInformation.PowerStatus.BatteryChargeStatus.HasFlag(BatteryChargeStatus.NoSystemBattery);
        }

        public ObservablePowerStatus()
        {
            context = SynchronizationContext.Current;
            if (!DesignerTools.IsInDesignMode && IsBatteryAvailable)
            {
                StartWatcher();
            }
        }

        public async void StartWatcher()
        {
            watcherToken?.Cancel();
            if (currentTask != null)
            {
                await currentTask;
            }

            watcherToken = new CancellationTokenSource();
            currentTask = Task.Run(async () =>
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    context.Post((a) => OnPropertyChanged(nameof(PercentCharge)), null);
                    context.Post((a) => OnPropertyChanged(nameof(Charge)), null);
                    context.Post((a) => OnPropertyChanged(nameof(IsCharging)), null);
                    await Task.Delay(10000);
                }
            }, watcherToken.Token);
        }

        public async void StopWatcher()
        {
            watcherToken?.Cancel();
            if (currentTask != null)
            {
                await currentTask;
            }
        }

        public void Dispose()
        {
            StopWatcher();
        }
    }
}
