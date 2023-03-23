using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Playnite;

/// <summary>
/// Describes logger object used to write message into log file.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Writes message to log with Info severity.
    /// </summary>
    /// <param name="message">Message to be added into log.</param>
    void Info(string message);

    /// <summary>
    /// Writes message to log with Info severity, including parsed exception.
    /// </summary>
    /// <param name="exception">Message to be added into log.</param>
    /// <param name="message">Exception to be added into log.</param>
    void Info(Exception exception, string message);

    /// <summary>
    /// Writes message to log with Debug severity.
    /// </summary>
    /// <param name="message">Message to be added into log.</param>
    void Debug(string message);

    /// <summary>
    /// Writes message to log with Debug severity, including parsed exception.
    /// </summary>
    /// <param name="exception">Message to be added into log.</param>
    /// <param name="message">Exception to be added into log.</param>
    void Debug(Exception exception, string message);

    /// <summary>
    /// Writes message to log with Warning severity.
    /// </summary>
    /// <param name="message">Message to be added into log.</param>
    void Warn(string message);

    /// <summary>
    /// Writes message to log with Warning severity, including parsed exception.
    /// </summary>
    /// <param name="exception">Message to be added into log.</param>
    /// <param name="message">Exception to be added into log.</param>
    void Warn(Exception exception, string message);

    /// <summary>
    /// Writes message to log with Error severity.
    /// </summary>
    /// <param name="message">Message to be added into log.</param>
    void Error(string message);

    /// <summary>
    /// Writes message to log with Error severity, including parsed exception.
    /// </summary>
    /// <param name="exception">Message to be added into log.</param>
    /// <param name="message">Exception to be added into log.</param>
    void Error(Exception exception, string message);

    /// <summary>
    /// Writes message to log with Trace severity.
    /// </summary>
    /// <param name="message">Message to be added into log.</param>
    void Trace(string message);

    /// <summary>
    /// Writes message to log with Trace severity, including parsed exception.
    /// </summary>
    /// <param name="exception">Message to be added into log.</param>
    /// <param name="message">Exception to be added into log.</param>
    void Trace(Exception exception, string message);
}

/// <summary>
/// Describes logger not logging anywhere.
/// </summary>
public class NullLoggger : ILogger
{
    /// <inheritdoc />
    public void Debug(string message)
    {
    }

    /// <inheritdoc />
    public void Debug(Exception exception, string message)
    {
    }

    /// <inheritdoc />
    public void Error(string message)
    {
    }

    /// <inheritdoc />
    public void Error(Exception exception, string message)
    {
    }

    /// <inheritdoc />
    public void Info(string message)
    {
    }

    /// <inheritdoc />
    public void Info(Exception exception, string message)
    {
    }

    /// <inheritdoc />
    public void Warn(string message)
    {
    }

    /// <inheritdoc />
    public void Warn(Exception exception, string message)
    {
    }

    /// <inheritdoc />
    public void Trace(string message)
    {
    }

    /// <inheritdoc />
    public void Trace(Exception exception, string message)
    {
    }
}

/// <summary>
/// Describes log provider.
/// </summary>
public interface ILogProvider
{
    /// <summary>
    /// Gets new logger.
    /// </summary>
    /// <param name="loggerName">Logger name.</param>
    /// <returns>Logger.</returns>
    ILogger GetLogger(string loggerName);
}

/// <summary>
/// Represents log manager.
/// </summary>
public static class LogManager
{
    private static ILogProvider? logManager;

    /// <summary>
    /// Initializes log manager using specific log provider.
    /// </summary>
    /// <param name="manager"></param>
    public static void Init(ILogProvider manager)
    {
        logManager = manager;
    }

    /// <summary>
    /// Gets logger with name of calling class.
    /// </summary>
    /// <returns>Logger.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static ILogger GetLogger()
    {
        if (logManager != null)
        {
            var asmName = Assembly.GetCallingAssembly().GetName().Name;
            var isCore = asmName == "Playnite.DesktopApp" || asmName == "Playnite.FullscreenApp" || asmName == "Playnite";
            var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "uknown";
            if (isCore)
            {
                return logManager.GetLogger(className);
            }
            else
            {
                return logManager.GetLogger($"{asmName}#{className}");
            }
        }
        else
        {
            return new NullLoggger();
        }
    }

    /// <summary>
    /// Gets logger with specific name.
    /// </summary>
    /// <param name="loggerName">Logger name.</param>
    /// <returns>Logger.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static ILogger GetLogger(string loggerName)
    {
        if (string.IsNullOrEmpty(loggerName))
        {
            throw new ArgumentNullException(nameof(loggerName));
        }

        if (logManager != null)
        {
            var asmName = Assembly.GetCallingAssembly().GetName().Name;
            var isCore = asmName == "Playnite.DesktopApp" || asmName == "Playnite.FullscreenApp" || asmName == "Playnite";
            if (isCore || loggerName.Contains("#", StringComparison.Ordinal))
            {
                return logManager.GetLogger(loggerName);
            }
            else
            {
                return logManager.GetLogger($"{asmName}#{loggerName}");
            }
        }
        else
        {
            return new NullLoggger();
        }
    }
}