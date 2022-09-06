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

        /// <summary>
        /// Gets mode of curently running application.
        /// </summary>
        ApplicationMode Mode { get; }

        /// <summary>
        /// Indicates whether application is running in portable mode.
        /// </summary>
        bool IsPortable { get; }

        /// <summary>
        /// Indicates whether application is running in offline mode.
        /// </summary>
        bool InOfflineMode { get; }

        /// <summary>
        /// Indicates whether application was built in DEBUG configuration.
        /// </summary>
        bool IsDebugBuild { get; }

        /// <summary>
        /// Indicates whether application is configured to fail for most unhandled errors.
        /// </summary>
        bool ThrowAllErrors { get; }
    }
}
