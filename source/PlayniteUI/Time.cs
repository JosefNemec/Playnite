using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PlayniteUI
{
    public class Time : ObservableObject
    {
        private SynchronizationContext context;

        public static Time Instance
        {
            get; set;
        }

        public string CurrentTime
        {
            get => DateTime.Now.ToString(Playnite.Constants.TimeUiFormat);
        }

        public Time()
        {
            context = SynchronizationContext.Current;
            StartTime();
        }

        private void StartTime()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    context.Post((a) => OnPropertyChanged("CurrentTime"), null);
                    Thread.Sleep(10000);
                }
            });
        }
    }
}
