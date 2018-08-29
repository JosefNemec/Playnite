using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes library client application.
    /// </summary>
    public interface ILibraryClient
    {
        /// <summary>
        /// Gets value indicating wheter the client is installed.
        /// </summary>
        bool IsInstalled { get; }

        /// <summary>
        /// Open client application.
        /// </summary>
        void Open();
    }
}
