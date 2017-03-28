using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public static class GamesLoaderHandler
    {
        public static CancellationTokenSource CancelToken = new CancellationTokenSource();
        public static Task ProgressTask;
    }
}
