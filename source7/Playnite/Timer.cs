using System.Diagnostics;

namespace Playnite;

public class ExecutionTimer : IDisposable
{
    private static readonly ILogger logger = LogManager.GetLogger();
    private readonly string name;
    private readonly Stopwatch watch = new Stopwatch();

    public ExecutionTimer(string name)
    {
        this.name = name;
        watch.Start();
    }

    public void Dispose()
    {
        watch.Stop();
        logger.Debug($"--- Timer '{name}', {watch.ElapsedMilliseconds} ms to complete.");
    }
}

public class Timer
{
    public static IDisposable TimeExecution(string name)
    {
        return new ExecutionTimer(name);
    }
}
