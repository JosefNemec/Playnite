using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public static class GlobalTaskHandler
    {
        public static CancellationTokenSource CancelToken = new CancellationTokenSource();
        public static Task ProgressTask;

        public static bool IsActive
        {
            get => ProgressTask?.Status == TaskStatus.Running || ProgressTask?.Status == TaskStatus.WaitingForActivation;
        }

        public static void CancelAndWait()
        {
            if (IsActive)
            {
                CancelToken?.Cancel();
                ProgressTask?.Wait();
            }
        }

        public async static Task CancelAndWaitAsync()
        {
            if (IsActive)
            {
                CancelToken?.Cancel();
                await ProgressTask;
            }
        }
    }
}
