using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class GenericTypeConverter : MarkupExtension, IValueConverter
    {
        public string StringFormat { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var converter = TypeDescriptor.GetConverter(targetType);
            if (StringFormat.IsNullOrEmpty())
            {
                return converter.ConvertFrom(value);
            }
            else
            {
                return converter.ConvertFrom(string.Format(StringFormat, value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
