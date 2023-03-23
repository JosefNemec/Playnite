namespace System;

public static class LongExtensions
{
    public static DateTime ToDateFromUnixMs(this long value)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(value).DateTime;
    }

    public static DateTime ToDateFromUnixSeconds(this long value)
    {
        return DateTimeOffset.FromUnixTimeSeconds(value).DateTime;
    }
}
