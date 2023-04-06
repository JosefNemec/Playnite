using System.Threading;
using System.Windows.Forms;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Playnite;

public enum BatteryChargeLevel
{
    Critical,
    Low,
    Medium,
    High
}

public class ObservablePowerStatus : ObservableObject, IDisposable
{
    private CancellationTokenSource? watcherToken;
    private Task? currentTask;

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
        if (!DesignerTools.IsInDesignMode && IsBatteryAvailable)
        {
            StartWatcher();
        }
    }

    public void StartWatcher()
    {
        watcherToken?.Cancel();
        watcherToken = new CancellationTokenSource();
        currentTask = Task.Run(async () =>
        {
            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                SyncContext.Post((_) => OnPropertyChanged(nameof(PercentCharge)));
                SyncContext.Post((_) => OnPropertyChanged(nameof(Charge)));
                SyncContext.Post((_) => OnPropertyChanged(nameof(IsCharging)));
                await Task.Delay(10000);
            }
        }, watcherToken.Token);
    }

    public void StopWatcher()
    {
        watcherToken?.Cancel();
    }

    public void Dispose()
    {
        StopWatcher();
    }
}
