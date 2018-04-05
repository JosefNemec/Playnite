using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class GameState
    {
        public bool Installed
        {
            get; set;
        }

        public bool Launching
        {
            get; set;
        }

        public bool Running
        {
            get; set;
        }

        public bool Installing
        {
            get; set;
        }

        public bool Uninstalling
        {
            get; set;
        }

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

        public override string ToString()
        {
            var inst = Installed ? 1 : 0;
            var run = Running ? 1 : 0;
            var installing = Installing ? 1 : 0;
            var uninstalling = Uninstalling ? 1 : 0;
            var launch = Launching ? 1 : 0;
            return $"Inst:{inst}; Run:{run}; Instl:{installing}; Uninst:{uninstalling}; Lnch:{launch}";
        }
    }
}
