namespace Playnite;

public static class Units
{
    public static long MegaBytesToBytes(long megaBytes)
    {
        return megaBytes * 1024 * 1024;
    }

    public static long BytesToMegaBytes(long bytes)
    {
        return bytes / 1024 / 1024;
    }

    public static int HoursToMilliseconds(int hours)
    {
        return MinutesToMilliseconds(hours * 60);
    }

    public static int MinutesToMilliseconds(int minutes)
    {
        return SecondsToMilliseconds(minutes * 60);
    }

    public static int SecondsToMilliseconds(int seconds)
    {
        return seconds * 1000;
    }
}
