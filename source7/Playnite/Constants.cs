using System.Globalization;

namespace Playnite;

public static class Constants
{
    public const string DefaultDateTimeFormat = "d";
    public const string DefaultPartialReleaseDateTimeFormat = "y";

    public static string DateUiFormat { get; } = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
    public static string TimeUiFormat { get; } = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

    public static char[] ListSeparators { get; } = new char[] { ListSeparator };
    public const char ListSeparator = ',';
}
