using Playnite.Settings;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class CoversZoomToPercentageConverter : MarkupExtension, IValueConverter
    {
        private const double OneHundredPercentValue = ViewSettings.DefaultGridItemWidth;

        // raw pixel value to percentage
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (double) value;

            return Math.Round(source / OneHundredPercentValue * 100);
        }

        // percentage to raw pixel value
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (double) value;

            return Math.Round(source * OneHundredPercentValue / 100);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}