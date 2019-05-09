using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes info API.
    /// </summary>
    public interface IPlayniteInfoAPI
    {
        /// <summary>
        /// Gets Playnite version.
        /// </summary>
        System.Version ApplicationVersion { get; }

        ApplicationMode Mode { get; }

        bool IsPortable { get; }

        bool InOfflineMode { get; }

        bool IsDebugBuild { get; }

        bool ThrowAllErrors { get; }
    }
}
