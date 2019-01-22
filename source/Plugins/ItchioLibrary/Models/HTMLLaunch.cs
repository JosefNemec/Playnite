using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Models
{
    /// <summary>
    /// Ask the client to perform an HTML launch, ie. open an HTML5 game, ideally in an embedded browser.
    /// Sent during Launch.
    /// </summary>
    public class HTMLLaunch
    {
        /// <summary>
        /// Absolute path on disk to serve
        /// </summary>
        public string rootFolder;


        /// <summary>
        /// Path of index file, relative to root folder
        /// </summary>
        public string indexPath;

        /// <summary>
        /// args string[]
        /// </summary>
        public string[] args;
    }
}
