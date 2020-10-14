using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents exception supporting localized message strings.
    /// </summary>
    public class LocalizedException : Exception
    {
        /// <summary>
        /// Creates new instance of <see cref="LocalizedException"/>.
        /// </summary>
        public LocalizedException()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="LocalizedException"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public LocalizedException(string message) : base(message.StartsWith("LOC") ? ResourceProvider.GetString(message) : message)
        {
        }
    }
}
