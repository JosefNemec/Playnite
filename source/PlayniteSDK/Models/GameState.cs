using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents overall game installation and running state.
    /// </summary>
    public class GameState : IComparable<GameState>
    {
        /// <summary>
        /// Gets or sets value indicating wheter a game is currently installed.
        /// </summary>
        public bool Installed
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets value indicating wheter a game is being launched.
        /// </summary>
        public bool Launching
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets value indicating wheter a game is currently running.
        /// </summary>
        public bool Running
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets value indicating wheter a game is being installed.
        /// </summary>
        public bool Installing
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets value indicating wheter a game is being uninstalled
        /// </summary>
        public bool Uninstalling
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of GameState.
        /// </summary>
        public GameState()
        {
        }

        /// <summary>
        /// Creates new instance of GameState with specific state.
        /// </summary>
        /// <param name="state"></param>
        public GameState(GameState state)
        {
            Installed = state.Installed;
            Launching = state.Launching;
            Running = state.Running;
            Installing = state.Installing;
            Uninstalling = state.Uninstalling;
        }

        /// <summary>
        /// Sets states to specific values. Use null for parameters that should keep current value.
        /// </summary>
        /// <param name="installed"></param>
        /// <param name="running"></param>
        /// <param name="installing"></param>
        /// <param name="uninstalling"></param>
        /// <param name="launching"></param>
        public void SetState(bool? installed, bool? running, bool? installing, bool? uninstalling, bool? launching)
        {
            if (installed != null)
            {
                Installed = installed.Value;
            }

            if (running != null)
            {
                Running = running.Value;
            }

            if (installing != null)
            {
                Installing = installing.Value;
            }

            if (uninstalling != null)
            {
                Uninstalling = uninstalling.Value;
            }

            if (launching != null)
            {
                Launching = launching.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var inst = Installed ? 1 : 0;
            var run = Running ? 1 : 0;
            var installing = Installing ? 1 : 0;
            var uninstalling = Uninstalling ? 1 : 0;
            var launch = Launching ? 1 : 0;
            return $"Inst:{inst}; Run:{run}; Instl:{installing}; Uninst:{uninstalling}; Lnch:{launch}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(GameState obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (Installed == obj.Installed &&
                Running == obj.Running &&
                Installing == obj.Installing &&
                Launching == obj.Launching)
            {
                return 0;
            }

            return 1;
        }
    }
}
