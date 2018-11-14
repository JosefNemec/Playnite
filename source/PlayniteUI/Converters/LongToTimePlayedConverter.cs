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
    public class LongToTimePlayedConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var seconds = (long)value;
            if (seconds == 0)
            {
                return DefaultResourceProvider.FindString("LOCPlayedNone");
            }

            var time = TimeSpan.FromSeconds(seconds);
            if (time.TotalSeconds < 60)
            {
                return string.Format(DefaultResourceProvider.FindString("LOCPlayedSeconds"), time.Seconds);
            }
            else if (time.TotalHours < 1)
            {
                return string.Format(DefaultResourceProvider.FindString("LOCPlayedMinutes"), time.Minutes);
            }
            else
            {
                return string.Format(DefaultResourceProvider.FindString("LOCPlayedHours"), Math.Floor(time.TotalHours), time.Minutes);
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
