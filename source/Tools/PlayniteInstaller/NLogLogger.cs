using NLog;
using NLog.Config;
using NLog.Targets;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteInstaller
{
    public class NLogLogger : Playnite.SDK.ILogger
    {
        private NLog.Logger logger;

        public NLogLogger(string loggerName)
        {
            logger = NLog.LogManager.GetLogger(loggerName);
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
            logger.Trace(message);
        }

        public void Trace(Exception exception, string message)
        {
            logger.Trace(exception, message);
        }

        public static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();
#if DEBUG
            var consoleTarget = new ColoredConsoleTarget()
            {
                Layout = @"${message}${onexception:${newline}${exception}}"
            };

            config.AddTarget("console", consoleTarget);

            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);
#endif

            FileSystem.CreateDirectory(App.TempDir);
            var fileTarget = new FileTarget()
            {
                FileName = Path.Combine(App.TempDir, "installer.log"),
                Layout = "${date:format=dd-MM HH\\:mm\\:ss.fff}|${level:uppercase=true:padding=-5}|${message}${onexception:${newline}${exception:format=toString}}",
                KeepFileOpen = false,
                Encoding = Encoding.UTF8
            };

            config.AddTarget("file", fileTarget);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);
            LogManager.Configuration = config;
        }
    }

    public class NLogLogProvider : Playnite.SDK.ILogProvider
    {
        Playnite.SDK.ILogger Playnite.SDK.ILogProvider.GetLogger(string loggerName)
        {
            return new NLogLogger(loggerName);
        }
    }
}
