using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class NullToBoolConverter : MarkupExtension, IValueConverter
{
    public static NullToBoolConverter Instance { get; } = new NullToBoolConverter();

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return false;
        }

        return true;
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
