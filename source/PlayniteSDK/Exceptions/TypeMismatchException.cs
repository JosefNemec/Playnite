using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents errors related to type mismatch use.
    /// </summary>
    public class TypeMismatchException : Exception
    {
        /// <summary>
        /// Creates new instance of <see cref="TypeMismatchException"/>.
        /// </summary>
        public TypeMismatchException()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="TypeMismatchException"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public TypeMismatchException(string message) : base(message)
        {
        }
    }
}
