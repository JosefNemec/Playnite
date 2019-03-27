using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Playnite
{
    public class ObservableTime : ObservableObject
    {
        private SynchronizationContext context;

        public static ObservableTime Instance
        {
            get; set;
        }

        public string CurrentTime
        {
            get => DateTime.Now.ToString(Common.Constants.TimeUiFormat);
        }

        public ObservableTime()
        {
            context = SynchronizationContext.Current;
            StartTime();
        }

        private void StartTime()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    context.Post((a) => OnPropertyChanged(nameof(CurrentTime)), null);
                    await Task.Delay(10000);
                }
            });
        }
    }
}
