using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class DateTimes
    {
        public interface IDateTimes
        {
            DateTime Now { get; }
            DateTime Today { get; }
        }

        public class DefaultDateProvider : IDateTimes
        {
            public DateTime Now => DateTime.Now;
            public DateTime Today => DateTime.Today;
        }

        public class TempDateTime : IDisposable
        {
            public TempDateTime(IDateTimes customDates)
            {
                DateTimes.dateProvider = customDates;
            }

            public void Dispose()
            {
                DateTimes.dateProvider = DateTimes.defaultDateProvider;
            }
        }

        private static IDateTimes defaultDateProvider = new DefaultDateProvider();
        private static IDateTimes dateProvider = defaultDateProvider;

        public static DateTime Now => dateProvider.Now;
        public static DateTime Today => dateProvider.Today;

        public static IDisposable UseCustomDates(IDateTimes dates)
        {
            return new TempDateTime(dates);
        }
    }
}
