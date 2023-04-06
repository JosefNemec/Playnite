using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class BidirectionalEnumAndNumberConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        if (targetType.IsEnum)
        {
            // Convert int to enum
            return Enum.ToObject(targetType, value);
        }

        if (value.GetType().IsEnum)
        {
            // Convert enum to int
            return System.Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
        }

        return null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // Perform the same conversion in both directions
        return Convert(value, targetType, parameter, culture);
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

public class EnumToBooleanConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value.Equals(true) ? parameter : Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

public class EnumToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
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