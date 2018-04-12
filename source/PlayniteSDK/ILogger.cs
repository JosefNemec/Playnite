using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes logger object used to write message into log file.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes message to log with Info serverity.
        /// </summary>
        /// <param name="message">Message to be added into log.</param>
        void Info(string message);

        /// <summary>
        /// Writes message to log with Info serverity, including parsed exception.
        /// </summary>
        /// <param name="exception">Message to be added into log.</param>
        /// <param name="message">Exception to be added into log.</param>
        void Info(Exception exception, string message);

        /// <summary>
        /// Writes message to log with Debug serverity.
        /// </summary>
        /// <param name="message">Message to be added into log.</param>
        void Debug(string message);

        /// <summary>
        /// Writes message to log with Debug serverity, including parsed exception.
        /// </summary>
        /// <param name="exception">Message to be added into log.</param>
        /// <param name="message">Exception to be added into log.</param>
        void Debug(Exception exception, string message);

        /// <summary>
        /// Writes message to log with Warning serverity.
        /// </summary>
        /// <param name="message">Message to be added into log.</param>
        void Warn(string message);

        /// <summary>
        /// Writes message to log with Warning serverity, including parsed exception.
        /// </summary>
        /// <param name="exception">Message to be added into log.</param>
        /// <param name="message">Exception to be added into log.</param>
        void Warn(Exception exception, string message);

        /// <summary>
        /// Writes message to log with Error serverity.
        /// </summary>
        /// <param name="message">Message to be added into log.</param>
        void Error(string message);

        /// <summary>
        /// Writes message to log with Error serverity, including parsed exception.
        /// </summary>
        /// <param name="exception">Message to be added into log.</param>
        /// <param name="message">Exception to be added into log.</param>
        void Error(Exception exception, string message);
    }
}
