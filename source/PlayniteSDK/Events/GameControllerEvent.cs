using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Events
{
    public class GameControllerEventArgs : EventArgs
    {
        public IGameController Controller
        {
            get; set;
        }

        /// <summary>
        /// Time in seconds for event to be finished.
        /// For example in case of Stopped event it indicates how long was game running until stopped.
        /// </summary>
        public long EllapsedTime
        {
            get; set;
        } = 0;

        public GameControllerEventArgs()
        {
        }

        public GameControllerEventArgs(IGameController controller, long ellapsedTime)
        {
            Controller = controller;
            EllapsedTime = ellapsedTime;
        }

        public GameControllerEventArgs(IGameController controller, double ellapsedTime)
            : this(controller, Convert.ToInt64(ellapsedTime))
        {
        }
    }

    public delegate void GameControllerEventHandler(object sender, GameControllerEventArgs controller);
}
