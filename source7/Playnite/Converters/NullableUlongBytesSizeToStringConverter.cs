using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite;

public class NullableUlongBytesSizeToStringConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not ulong bytes)
        {
            return string.Empty;
        }

        string suffix;
        double readable;
        if (bytes >= 0x1000000000000000) // Exabyte
        {
            suffix = "EB";
            readable = (bytes >> 50);
        }
        else if (bytes >= 0x4000000000000) // Petabyte
        {
            suffix = "PB";
            readable = bytes >> 40;
        }
        else if (bytes >= 0x10000000000) // Terabyte
        {
            suffix = "TB";
            readable = bytes >> 30;
        }
        else if (bytes >= 0x40000000) // Gigabyte
        {
            suffix = "GB";
            readable = bytes >> 20;
        }
        else if (bytes >= 0x100000) // Megabyte
        {
            suffix = "MB";
            readable = bytes >> 10;
        }
        else if (bytes >= 0x400) // Kilobyte
        {
            suffix = "KB";
            readable = bytes;
        }
        else
        {
            return bytes.ToString("0 B"); // Byte
        }

        readable /= 1024;
        return readable.ToString("0.00# ") + suffix;
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