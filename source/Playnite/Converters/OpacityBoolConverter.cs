using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class OpacityBoolConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter != null && ((bool)parameter) == true)
                    boolValue = !boolValue;

                return boolValue ? 1.0 : 0.5;
            }

            return 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null)
                throw new NotSupportedException();

            var val = (double)value;
            if (val <= 0.5)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
