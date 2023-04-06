using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Playnite;

public class NLogLogger : ILogger
{
    private readonly NLog.Logger logger;

    public NLogLogger(NLog.Logger logger)
    {
        this.logger = logger;
    }

    public void Debug(string message)
    {
        logger.Debug(message);
    }

    public void Debug(Exception exception, string message)
    {
        logger.Debug(exception, message);
    }

    public void Error(string message)
    {
        logger.Error(message);
    }

    public void Error(Exception exception, string message)
    {
        logger.Error(exception, message);
    }

    public void Info(string message)
    {
        logger.Info(message);
    }

    public void Info(Exception exception, string message)
    {
        logger.Info(exception, message);
    }

    public void Warn(string message)
    {
        logger.Warn(message);
    }

    public void Warn(Exception exception, string message)
    {
        logger.Warn(exception, message);
    }

    public void Trace(string message)
    {
        if (LogManager.TraceLoggingEnabled)
        {
            logger.Trace(message);
        }
    }

    public void Trace(Exception exception, string message)
    {
        if (LogManager.TraceLoggingEnabled)
        {
            logger.Trace(exception, message);
        }
    }
}

public class PluginLogManager : ILoggerProvider
{
    private readonly string pluginLogFile;

    public PluginLogManager(string pluginDataDir)
    {
        pluginLogFile = Path.Combine(pluginDataDir, "plugin.log");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILogger GetLogger()
    {
        return LogManager.GetLogger(new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "uknown", pluginLogFile);
    }

    public ILogger GetLogger(string loggerName)
    {
        return LogManager.GetLogger(loggerName, pluginLogFile);
    }
}

public class LogManager
{
    private static readonly Dictionary<string, LogFactory> factories = new (StringComparer.OrdinalIgnoreCase);

    public static bool TraceLoggingEnabled { get; set; } = false;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static ILogger GetLogger()
    {
        return GetLogger(new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "uknown", PlaynitePaths.LogFile);
    }

    public static ILogger GetLogger(string loggerName)
    {
        return GetLogger(loggerName, PlaynitePaths.LogFile);
    }

    public static ILogger GetLogger(string loggerName, string loggerFile)
    {
        if (factories.TryGetValue(loggerFile, out var logFactory))
        {
            return new NLogLogger(logFactory.GetLogger(loggerName));
        }
        else
        {
            var config = new LoggingConfiguration();
            var fileName = Path.GetFileNameWithoutExtension(loggerFile);
            var fileTarget = new FileTarget
            {
                Name = loggerName,
                FileName = loggerFile,
                Layout = "${date:format=dd-MM HH\\:mm\\:ss.fff}|${level:uppercase=true:padding=-5}|${logger}:${message}${onexception:${newline}${exception:format=toString}}",
                KeepFileOpen = true,
                ArchiveFileName = Path.Combine(PlaynitePaths.ConfigRootDir, $"{fileName}.{{#####}}.log"),
                ArchiveAboveSize = 4_096_000,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 2,
                Encoding = Encoding.UTF8
            };

            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, fileTarget));

#if DEBUG
            var consoleTarget = new ColoredConsoleTarget
            {
                Name = "Console",
                Layout = @"${level:uppercase=true:padding=-5}|${logger}:${message}${onexception:${newline}${exception}}"
            };

            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, consoleTarget));
#endif

            logFactory = new LogFactory();
            logFactory.Configuration = config;
            factories.Add(loggerFile, logFactory);
            return new NLogLogger(logFactory.GetLogger(loggerName));
        }
    }
}