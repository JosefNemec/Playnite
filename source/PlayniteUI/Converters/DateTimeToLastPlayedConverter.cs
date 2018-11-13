using Playnite.SDK;
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
                return DefaultResourceProvider.FindString("LOCNever");
            }
            else
            {
                var currentDate = DateTime.Now;
                var diff = currentDate - lastPlayed.Value;
                if (diff.TotalDays < 7)
                {
                    if (diff.TotalDays < 1)
                    {
                        return DefaultResourceProvider.FindString("LOCToday");
                    }
                    else if (diff.TotalDays < 2)
                    {
                        return DefaultResourceProvider.FindString("LOCYesterday");

                    }
                    else
                    {
                        switch (lastPlayed.Value.DayOfWeek)
                        {
                            case DayOfWeek.Sunday:
                                return DefaultResourceProvider.FindString("LOCSunday");
                            case DayOfWeek.Monday:
                                return DefaultResourceProvider.FindString("LOCMonday");
                            case DayOfWeek.Tuesday:
                                return DefaultResourceProvider.FindString("LOCTuesday");
                            case DayOfWeek.Wednesday:
                                return DefaultResourceProvider.FindString("LOCWednesday");
                            case DayOfWeek.Thursday:
                                return DefaultResourceProvider.FindString("LOCThursday");
                            case DayOfWeek.Friday:
                                return DefaultResourceProvider.FindString("LOCFriday");
                            case DayOfWeek.Saturday:
                                return DefaultResourceProvider.FindString("LOCSaturday");
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
