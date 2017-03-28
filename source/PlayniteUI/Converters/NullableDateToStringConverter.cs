using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayniteUI
{
    public class NullableDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var date = ((DateTime?)value).Value;
            return date.ToString(Playnite.Constants.DateUiFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                return null;
            }

            DateTime? newDate = DateTime.ParseExact(value as string, Playnite.Constants.DateUiFormat, CultureInfo.InvariantCulture);
            return newDate;
        }
    }
}
