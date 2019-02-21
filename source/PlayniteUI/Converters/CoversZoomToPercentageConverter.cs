using System;
using System.Globalization;
using System.Windows.Data;
using Playnite;

namespace PlayniteUI
{
    public class CoversZoomToPercentageConverter : IValueConverter
    {
        private const double OneHundredPercentValue = ViewSettings.DefaultCoversZoom;

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
    }
}