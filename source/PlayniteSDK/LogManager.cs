using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
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
        private static ILogProvider logManager;

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
                var className = (new StackFrame(1)).GetMethod().DeclaringType.Name;
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
                if (isCore || loggerName.Contains("#"))
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
}
