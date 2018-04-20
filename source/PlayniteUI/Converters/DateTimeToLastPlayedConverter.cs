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
                return ResourceProvider.Instance.FindString("LOCNever");
            }
            else
            {
                var currentDate = DateTime.Now;
                var diff = currentDate - lastPlayed.Value;
                if (diff.TotalDays < 7)
                {
                    if (diff.TotalDays < 1)
                    {
                        return ResourceProvider.Instance.FindString("LOCToday");
                    }
                    else if (diff.TotalDays < 2)
                    {
                        return ResourceProvider.Instance.FindString("LOCYesterday");

                    }
                    else
                    {
                        switch (lastPlayed.Value.DayOfWeek)
                        {
                            case DayOfWeek.Sunday:
                                return ResourceProvider.Instance.FindString("LOCSunday");
                            case DayOfWeek.Monday:
                                return ResourceProvider.Instance.FindString("LOCMonday");
                            case DayOfWeek.Tuesday:
                                return ResourceProvider.Instance.FindString("LOCTuesday");
                            case DayOfWeek.Wednesday:
                                return ResourceProvider.Instance.FindString("LOCWednesday");
                            case DayOfWeek.Thursday:
                                return ResourceProvider.Instance.FindString("LOCThursday");
                            case DayOfWeek.Friday:
                                return ResourceProvider.Instance.FindString("LOCFriday");
                            case DayOfWeek.Saturday:
                                return ResourceProvider.Instance.FindString("LOCSaturday");
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
