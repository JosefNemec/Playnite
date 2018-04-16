using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PlayniteUI
{
    public class DateTimeToLastPlayedConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var lastPlayed = (DateTime?)value;
            if (lastPlayed == null)
            {
                return ResourceProvider.Instance.FindString("Never");
            }
            else
            {
                var currentDate = DateTime.Now;
                var diff = currentDate - lastPlayed.Value;
                if (diff.TotalDays < 7)
                {
                    if (diff.TotalDays < 1)
                    {
                        return ResourceProvider.Instance.FindString("Today");
                    }
                    else if (diff.TotalDays < 2)
                    {
                        return ResourceProvider.Instance.FindString("Yesterday");

                    }
                    else
                    {
                        switch (lastPlayed.Value.DayOfWeek)
                        {
                            case DayOfWeek.Sunday:
                                return ResourceProvider.Instance.FindString("Sunday");
                            case DayOfWeek.Monday:
                                return ResourceProvider.Instance.FindString("Monday");
                            case DayOfWeek.Tuesday:
                                return ResourceProvider.Instance.FindString("Tuesday");
                            case DayOfWeek.Wednesday:
                                return ResourceProvider.Instance.FindString("Wednesday");
                            case DayOfWeek.Thursday:
                                return ResourceProvider.Instance.FindString("Thursday");
                            case DayOfWeek.Friday:
                                return ResourceProvider.Instance.FindString("Friday");
                            case DayOfWeek.Saturday:
                                return ResourceProvider.Instance.FindString("Saturday");
                            default:
                                return lastPlayed.Value.ToString(Playnite.Constants.DateUiFormat);
                        }

                    }
                }
                else
                {
                    return lastPlayed.Value.ToString(Playnite.Constants.DateUiFormat);
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
