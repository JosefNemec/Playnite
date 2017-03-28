using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayniteUI
{
    public class BidirectionalEnumAndNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (targetType.IsEnum)
            {
                // Convert int to enum
                return Enum.ToObject(targetType, value);
            }

            if (value.GetType().IsEnum)
            {
                // Convert enum to int
                return System.Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Perform the same conversion in both directions
            return Convert(value, targetType, parameter, culture);
        }
    }
}
