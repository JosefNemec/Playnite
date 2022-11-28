using Playnite.SDK.Models;
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

        public static string ToDisplayString(this DateTime date, DateFormattingOptions options = null)
        {
            if (options == null)
            {
                return date.ToString(Common.Constants.DateUiFormat);
            }

            if (options.PastWeekRelativeFormat)
            {
                if (date.Date == DateTime.Today)
                {
                    return LOC.Today.GetLocalized();
                }

                if (date.Date.AddDays(1) == DateTime.Today)
                {
                    return LOC.Yesterday.GetLocalized();
                }

                var currentDate = DateTime.Now;
                var diff = currentDate - date;
                if (diff.TotalDays < 7)
                {
                    switch (date.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            return LOC.Sunday.GetLocalized();
                        case DayOfWeek.Monday:
                            return LOC.Monday.GetLocalized();
                        case DayOfWeek.Tuesday:
                            return LOC.Tuesday.GetLocalized();
                        case DayOfWeek.Wednesday:
                            return LOC.Wednesday.GetLocalized();
                        case DayOfWeek.Thursday:
                            return LOC.Thursday.GetLocalized();
                        case DayOfWeek.Friday:
                            return LOC.Friday.GetLocalized();
                        case DayOfWeek.Saturday:
                            return LOC.Saturday.GetLocalized();
                    }
                }
            }

            return date.ToString(options.Format ?? Common.Constants.DateUiFormat);
        }

        public static string ToDisplayString(this ReleaseDate date, ReleaseDateFormattingOptions options = null)
        {
            if (date.Month == null && date.Day == null)
            {
                return date.Year.ToString();
            }

            if (date.Month != null &&  date.Day == null)
            {
                return date.Date.ToString(options.PartialFormat ?? Common.Constants.DefaultPartialReleaseDateTimeFormat);
            }

            return date.Date.ToDisplayString(options);
        }
    }
}
