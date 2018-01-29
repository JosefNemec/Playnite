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
            CancelToken?.Cancel();
            ProgressTask?.Wait();
        }

        public static void Wait()
        {
            ProgressTask?.Wait();
        }
    }
}
