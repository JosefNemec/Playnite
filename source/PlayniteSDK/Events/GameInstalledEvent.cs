using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Events
{
    /// <summary>
    /// Represents game event.
    /// </summary>
    public class GameInstalledEventArgs : GameControllerEventArgs
    {
        /// <summary>
        /// Gets or sets data for newly installed game.
        /// </summary>
        public GameInfo InstalledInfo { get; }

        /// <summary>
        /// Creates new instance of <see cref="GameInstalledEventArgs"/>.
        /// </summary>
        /// <param name="controller">Source controller of this event.</param>
        /// <param name="ellapsedTime">Time in seconds for how long the operation was running.</param>
        /// <param name="installedInfo">Newly installed game.</param>
        public GameInstalledEventArgs(GameInfo installedInfo, IGameController controller, long ellapsedTime) : base(controller, ellapsedTime)
        {
            InstalledInfo = installedInfo;
        }

        /// <summary>
        /// Creates new instance of <see cref="GameInstalledEventArgs"/>.
        /// </summary>
        /// <param name="controller">Source controller of this event.</param>
        /// <param name="ellapsedTime">Time in seconds for how long the operation was running.</param>
        /// <param name="installedInfo">Newly installed game.</param>
        public GameInstalledEventArgs(GameInfo installedInfo, IGameController controller, double ellapsedTime)
            : this(installedInfo, controller, Convert.ToInt64(ellapsedTime))
        {
        }
    }
}
