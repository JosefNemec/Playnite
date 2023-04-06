using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class ObjectToStringConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is Enum enumVar)
        {
            return enumVar.GetDescription();
        }
        else
        {
            return value.ToString() ?? string.Empty;
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
