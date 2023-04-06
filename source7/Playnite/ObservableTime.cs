using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Windows;

namespace Playnite;

public class ObservableTime : ObservableObject
{
    private CancellationTokenSource? watcherToken;
    private Task? currentTask;

    public string Time
    {
        get => DateTime.Now.ToString(Constants.TimeUiFormat);
    }

    public ObservableTime()
    {
        if (!DesignerTools.IsInDesignMode)
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
        watcherToken?.Dispose();

        watcherToken = new CancellationTokenSource();
        currentTask = Task.Run(async () =>
        {
            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                SyncContext.Post((a) => OnPropertyChanged(nameof(Time)), null);
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

        watcherToken?.Dispose();
    }

    public void Dispose()
    {
        StopWatcher();
    }
}
