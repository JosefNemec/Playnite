using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class ListSizeToBoolConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is IList<dynamic> list)
        {
            return list.Count > 0;
        }
        else
        {
            return false;
        }
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

public class ListSizeToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is System.Collections.IList list)
        {
            return list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return Visibility.Collapsed;
        }
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

public class NiceListToStringConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is IEnumerable<dynamic>)
        {
            return string.Join(", ", (IEnumerable<object>)value);
        }
        else
        {
            return value.ToString() ?? string.Empty;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string stringVal)
        {
            return null;
        }

        if (stringVal.IsNullOrWhiteSpace())
        {
            return null;
        }

        var converted = stringVal.Split(',').Select(a => a.Trim());
        return converted.ToList();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

public class ListToStringConverter : MarkupExtension, IValueConverter
{
    private const string defaultSeperator = ",";

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is IEnumerable<dynamic>)
        {
            return string.Join(parameter is string customSep ? customSep : defaultSeperator, (IEnumerable<object>)value);
        }
        else
        {
            return value.ToString() ?? string.Empty;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string stringVal)
        {
            return null;
        }

        if (stringVal.IsNullOrWhiteSpace())
        {
            return null;
        }

        var converted = stringVal.Split(parameter is string customSep ? customSep : defaultSeperator, StringSplitOptions.None);
        if (targetType == typeof(ObservableCollection<string>))
        {
            return new ObservableCollection<string>(converted);
        }
        else
        {
            return converted.ToList();
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

public class ListToMultilineStringConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is IEnumerable<dynamic>)
        {
            return string.Join("\n", (IEnumerable<object>)value);
        }
        else
        {
            return value.ToString() ?? string.Empty;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string stringVal)
        {
            return null;
        }

        if (stringVal.IsNullOrWhiteSpace())
        {
            return null;
        }

        var converted = stringVal.Split('\n', StringSplitOptions.None).Select(a => a.Trim('\r')).ToArray();
        if (targetType == typeof(ObservableCollection<string>))
        {
            return new ObservableCollection<string>(converted);
        }
        else
        {
            return converted.ToList();
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
