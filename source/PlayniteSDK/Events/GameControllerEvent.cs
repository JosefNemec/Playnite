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
    public class GameControllerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets source controller of this event.
        /// </summary>
        public IGameController Controller
        {
            get;
        }

        /// <summary>
        /// Time in seconds for event to be finished.
        /// For example in case of Stopped event it indicates how long was game running until stopped.
        /// </summary>
        public long EllapsedTime
        {
            get;
        } = 0;

        /// <summary>
        /// Creates new instance of <see cref="GameControllerEventArgs"/>.
        /// </summary>
        public GameControllerEventArgs()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="GameControllerEventArgs"/>.
        /// </summary>
        /// <param name="controller">Source controller of this event.</param>
        /// <param name="ellapsedTime">Time in seconds for how long the operation was running.</param>
        public GameControllerEventArgs(IGameController controller, long ellapsedTime)
        {
            Controller = controller;
            EllapsedTime = ellapsedTime;
        }

        /// <summary>
        /// Creates new instance of <see cref="GameControllerEventArgs"/>.
        /// </summary>
        /// <param name="controller">Source controller of this event.</param>
        /// <param name="ellapsedTime">Time in seconds for how long the operation was running.</param>
        public GameControllerEventArgs(IGameController controller, double ellapsedTime)
            : this(controller, Convert.ToInt64(ellapsedTime))
        {
        }
    }
}
