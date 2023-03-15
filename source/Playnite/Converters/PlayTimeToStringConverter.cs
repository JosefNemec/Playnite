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
        public static PlayTimeToStringConverter Instance { get; } = new PlayTimeToStringConverter();

        private static string LOCPlayedNoneString;
        private static string LOCPlayedNone;
        private static string LOCPlayedSeconds;
        private static string LOCPlayedMinutes;
        private static string LOCPlayedHours;
        private static string LOCPlayedDays;

        private static void CacheStrings()
        {
            if (LOCPlayedNoneString != null)
            {
                return;
            }

            LOCPlayedNoneString = ResourceProvider.GetString("LOCPlayedNoneString");
            LOCPlayedNone = ResourceProvider.GetString("LOCPlayedNone");
            LOCPlayedSeconds = ResourceProvider.GetString("LOCPlayedSeconds");
            LOCPlayedMinutes = ResourceProvider.GetString("LOCPlayedMinutes");
            LOCPlayedHours = ResourceProvider.GetString("LOCPlayedHours");
            LOCPlayedDays = ResourceProvider.GetString("LOCPlayedDays");
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CacheStrings();
            if (value == null)
            {
                return LOCPlayedNone;
            }

            var seconds = (ulong)value;
            if (seconds == 0)
            {
                return LOCPlayedNone;
            }

            // Can't use TimeSpan from seconds because ulong is too large for it
            if (seconds < 60)
            {
                return string.Format(LOCPlayedSeconds, seconds);
            }

            var minutes = seconds / 60;
            if (minutes < 60)
            {
                return string.Format(LOCPlayedMinutes, minutes);
            }

            var hours = minutes / 60;
            if (parameter is bool formatToDays && formatToDays && hours >= 24)
            {
                var days = hours / 24;
                var remainingHours = hours % 24;
                var remainingMinutes = minutes % 60;

                return string.Format(LOCPlayedDays, days, remainingHours, remainingMinutes);
            }

            return string.Format(LOCPlayedHours, hours, minutes - (hours * 60));
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
