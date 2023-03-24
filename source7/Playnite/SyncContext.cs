using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Playnite;

public class SyncContext
{
    [AllowNull] private static SynchronizationContext mainContext;

    internal static void SetMainContext(SynchronizationContext context)
    {
        mainContext = context;
    }

    public static void Post(SendOrPostCallback d, object? state)
    {
        mainContext.Post(d, state);
    }

    public static void Post(SendOrPostCallback d)
    {
        mainContext.Post(d, null);
    }

    public static void Send(SendOrPostCallback d, object? state)
    {
        mainContext.Send(d, state);
    }

    public static void Send(SendOrPostCallback d)
    {
        mainContext.Send(d, null);
    }
}