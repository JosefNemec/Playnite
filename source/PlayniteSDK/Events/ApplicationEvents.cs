using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Events
{
    /// <summary>
    /// Application wide events.
    /// </summary>
    public enum ApplicationEvent
    {
        /// <summary>
        ///
        /// </summary>
        OnApplicationStarted,
        /// <summary>
        ///
        /// </summary>
        OnApplicationStopped,
        /// <summary>
        ///
        /// </summary>
        OnLibraryUpdated,
        /// <summary>
        ///
        /// </summary>
        OnGameStarting,
        /// <summary>
        ///
        /// </summary>
        OnGameStarted,
        /// <summary>
        ///
        /// </summary>
        OnGameStopped,
        /// <summary>
        ///
        /// </summary>
        OnGameInstalled,
        /// <summary>
        ///
        /// </summary>
        OnGameUninstalled,
        /// <summary>
        ///
        /// </summary>
        OnGameSelected
    }
}
