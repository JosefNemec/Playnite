using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class NLogLogger : ILogger
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
    }

    public class NLogLogProvider : ILogProvider
    {
        public ILogger GetLogger(string loggerName)
        {
            return new NLogLogger(loggerName);
        }
    }
}
