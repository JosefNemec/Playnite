using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class BoolToYesNoConverter : MarkupExtension, IValueConverter
    {
        private readonly string yesString;
        private readonly string noString;

        public BoolToYesNoConverter()
        {
            yesString = ResourceProvider.GetString("LOCYesLabel");
            noString = ResourceProvider.GetString("LOCNoLabel");
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) == true ? yesString : noString;
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
