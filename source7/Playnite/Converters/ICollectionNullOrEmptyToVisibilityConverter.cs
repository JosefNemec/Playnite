using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class ICollectionNullOrEmptyToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return Visibility.Collapsed;
        }
        else
        {
            var val = (ICollection)value;
            return val.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
