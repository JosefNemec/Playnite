using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Models
{
    /// <summary>
    /// Sent during Launch, when one or more prerequisites have failed to install.
    /// The user may choose to proceed with the launch anyway.
    /// </summary>
    public class PrereqsFailed
    {
        /// <summary>
        /// Short error
        /// </summary>
        public string error;

        /// <summary>
        /// Longer error(to include in logs)
        /// </summary>
        public string errorStack;
    }
}
