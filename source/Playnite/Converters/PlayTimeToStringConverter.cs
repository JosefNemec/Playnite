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
    public class PlayTimeToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return ResourceProvider.GetString("LOCPlayedNone");
            }

            var seconds = (ulong)value;
            if (seconds == 0)
            {
                return ResourceProvider.GetString("LOCPlayedNone");
            }

            // Can't use TimeSpan from seconds because ulong is too large for it
            if (seconds < 60)
            {
                return string.Format(ResourceProvider.GetString("LOCPlayedSeconds"), seconds);
            }

            var minutes = seconds / 60;
            if (minutes < 60)
            {
                return string.Format(ResourceProvider.GetString("LOCPlayedMinutes"), minutes);
            }

            var hours = minutes / 60;
            return string.Format(ResourceProvider.GetString("LOCPlayedHours"), hours, minutes - (hours * 60));
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
