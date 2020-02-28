using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Exceptions
{
    /// <summary>
    /// Represents exception from scripting runtime.
    /// </summary>
    public class ScriptRuntimeException : Exception
    {
        /// <summary>
        /// Gets script runtime stack trace.
        /// </summary>
        public string ScriptStackTrace { get; private set; }

        /// <summary>
        /// Creates new instance of <see cref="ScriptRuntimeException"/>.
        /// </summary>
        public ScriptRuntimeException()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="ScriptRuntimeException"/>.
        /// </summary>
        /// <param name="message"></param>
        public ScriptRuntimeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="ScriptRuntimeException"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        public ScriptRuntimeException(string message, string stackTrace) : base(message)
        {
            ScriptStackTrace = stackTrace;
        }
    }
}
