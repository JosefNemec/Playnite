using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class StringNullOrEmptyToBoolConverter : MarkupExtension, IValueConverter
{
    enum Parameters
    {
        Normal, Inverted
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string str)
        {
            throw new NotSupportedException();
        }

        var direction = parameter == null ? Parameters.Normal : (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);
        if (direction == Parameters.Inverted)
        {
            return str.IsNullOrEmpty();
        }
        else
        {
            return !str.IsNullOrEmpty();
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

public class StringNullOrEmptyToVisibilityConverter : MarkupExtension, IValueConverter
{
    enum Parameters
    {
        Normal, Inverted
    }

    public static StringNullOrEmptyToVisibilityConverter Instance { get; } = new StringNullOrEmptyToVisibilityConverter();

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string str)
        {
            throw new NotSupportedException();
        }

        var direction = parameter == null ? Parameters.Normal : (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);
        if (direction == Parameters.Inverted)
        {
            return str.IsNullOrEmpty() ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return str.IsNullOrEmpty() ? Visibility.Collapsed : Visibility.Visible;
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

public class StringToUpperCaseConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string str)
        {
            throw new NotSupportedException();
        }

        return str.ToUpperInvariant();
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
