using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Events
{
    /// <summary>
    /// Respresents arguments for Playnite URI execution event.
    /// </summary>
    public class PlayniteUriEventArgs
    {
        /// <summary>
        /// Gets or sets url arguments.
        /// </summary>
        public string[] Arguments { get; set; }
    }

}
