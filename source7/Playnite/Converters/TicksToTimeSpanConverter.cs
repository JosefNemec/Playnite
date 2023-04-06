using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class TicksToTimeSpanConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int intValue)
        {
            return new TimeSpan(intValue);
        }
        else if (value is long longValue)
        {
            return new TimeSpan(longValue);
        }
        else if (value is uint uintValue)
        {
            return new TimeSpan(uintValue);
        }

        throw new NotSupportedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan.Ticks;
        }

        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
