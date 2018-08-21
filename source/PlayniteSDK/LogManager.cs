using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public class NullLoggger : ILogger
    {
        public void Debug(string message)
        {
        }

        public void Debug(Exception exception, string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(Exception exception, string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Info(Exception exception, string message)
        {
        }

        public void Warn(string message)
        {
        }

        public void Warn(Exception exception, string message)
        {
        }
    }

    public interface ILogProvider
    {
        ILogger GetLogger(string loggerName);
    }

    public static class LogManager
    {
        private static ILogProvider logManager;

        public static void Init(ILogProvider manager)
        {
            logManager = manager;
        }

        public static ILogger GetLogger()
        {
            var className = (new StackFrame(1)).GetMethod().DeclaringType.Name;
            if (logManager != null)
            {
                return logManager.GetLogger(className);
            }
            else
            {
                return new NullLoggger();
            }
        }

        public static ILogger GetLogger(string loggerName)
        {
            return logManager.GetLogger(loggerName);
        }
    }
}
