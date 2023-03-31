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
                var today = Today;
                var dayDiff = (today - date.Date).TotalDays;

                if (dayDiff == 0)
                {
                    return LOC.Today.GetLocalized();
                }
                
                if (dayDiff == 1)
                {
                    return LOC.Yesterday.GetLocalized();
                }
                
                if (dayDiff > 1 && dayDiff < 7)
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

            if (date.Month != null && date.Day == null)
            {
                return date.Date.ToString(options.PartialFormat ?? Common.Constants.DefaultPartialReleaseDateTimeFormat);
            }

            return date.Date.ToDisplayString(options);
        }
    }
}
