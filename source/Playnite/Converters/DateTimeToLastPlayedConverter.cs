using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class DateTimeToLastPlayedConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var lastPlayed = (DateTime?)value;
            if (lastPlayed == null)
            {
                return ResourceProvider.GetString("LOCNever");
            }
            else
            {
                if (lastPlayed.Value.Date == DateTime.Today)
                {
                    return ResourceProvider.GetString("LOCToday");
                }

                if (lastPlayed.Value.Date.AddDays(1) == DateTime.Today)
                {
                    return ResourceProvider.GetString("LOCYesterday");
                }

                var currentDate = DateTime.Now;
                var diff = currentDate - lastPlayed.Value;
                if (diff.TotalDays < 7)
                {
                    switch (lastPlayed.Value.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            return ResourceProvider.GetString("LOCSunday");
                        case DayOfWeek.Monday:
                            return ResourceProvider.GetString("LOCMonday");
                        case DayOfWeek.Tuesday:
                            return ResourceProvider.GetString("LOCTuesday");
                        case DayOfWeek.Wednesday:
                            return ResourceProvider.GetString("LOCWednesday");
                        case DayOfWeek.Thursday:
                            return ResourceProvider.GetString("LOCThursday");
                        case DayOfWeek.Friday:
                            return ResourceProvider.GetString("LOCFriday");
                        case DayOfWeek.Saturday:
                            return ResourceProvider.GetString("LOCSaturday");
                        default:
                            return lastPlayed.Value.ToString(Common.Constants.DateUiFormat);
                    }
                }
                else
                {
                    return lastPlayed.Value.ToString(Common.Constants.DateUiFormat);
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
