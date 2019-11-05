using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents errors related to object references.
    /// </summary>
    public class ReferenceException : Exception
    {
        /// <summary>
        /// Creates new instance of <see cref="ReferenceException"/>.
        /// </summary>
        public ReferenceException()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="ReferenceException"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ReferenceException(string message) : base(message)
        {
        }
    }
}
