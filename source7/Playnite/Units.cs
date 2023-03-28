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
}
