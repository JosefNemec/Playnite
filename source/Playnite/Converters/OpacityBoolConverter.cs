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
            bool val = false;

            if (parameter != null && ((bool)parameter) == true)
            {
                val = !(bool)value;
            }
            else
            {
                val = (bool)value;
            }

            if (val)
            {
                return 1.0;
            }
            else
            {
                return 0.5;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

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
