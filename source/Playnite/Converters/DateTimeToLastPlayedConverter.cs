using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class DateTimeToLastPlayedConverter : MarkupExtension, IValueConverter
    {
        public static readonly DateTimeToLastPlayedConverter Instance = new DateTimeToLastPlayedConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var lastPlayed = (DateTime?)value;
            if (lastPlayed == null)
            {
                return LOC.Never.GetLocalized();
            }

            if (parameter is DateFormattingOptions options)
            {
                return lastPlayed.Value.ToDisplayString(options);
            }

            return lastPlayed.Value.ToString(Common.Constants.DateUiFormat);
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
