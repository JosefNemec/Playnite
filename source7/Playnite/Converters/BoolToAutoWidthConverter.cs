using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class BoolToAutoWidthConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolVal)
        {
            return boolVal ? double.NaN : (double)0;
        }

        return new NotSupportedException();
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
