using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface ILogProvider
    {
        ILogger GetLogger(string loggerName);
    }

    public static class LogManager
    {
        // TODO add null logger without init
        private static ILogProvider logManager;

        public static void Init(ILogProvider manager)
        {
            logManager = manager;
        }

        public static ILogger GetLogger()
        {
            var className = (new StackFrame(1)).GetMethod().DeclaringType.Name;
            return logManager.GetLogger(className);
        }

        public static ILogger GetLogger(string loggerName)
        {
            return logManager.GetLogger(loggerName);
        }
    }
}
