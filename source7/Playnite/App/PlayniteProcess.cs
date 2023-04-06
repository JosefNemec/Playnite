using System.Diagnostics;

namespace Playnite;

public class PlayniteProcess
{
    public static long WorkingSetMemory
    {
        get => Process.GetCurrentProcess().WorkingSet64;
    }

    public static string? Path
    {
        get => Environment.ProcessPath;
    }

    public static string? Cmdline
    {
        get => Process.GetCurrentProcess().GetCommandLine();
    }
}